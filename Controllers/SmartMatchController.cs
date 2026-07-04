using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using DoAnCS.Services;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using TiktokenSharp;

namespace DoAnCS.Controllers
{
    [Authorize(Roles = "Recruiter,Admin")]
    public class SmartMatchController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;
        private readonly IConfiguration _config;

        public SmartMatchController(AppDbContext context, IAIService aiService, IConfiguration config)
        {
            _context = context;
            _aiService = aiService;
            _config = config; // Dùng để đọc URL Python service từ biến môi trường
        }

        /// <summary>
        /// Trang chính Smart CV Matcher - Recruiter chọn JD và xem kết quả
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var isProClaim = User.FindFirst("IsPro")?.Value;
            bool isPro = (isProClaim == "True") || User.IsInRole("Admin");
            ViewBag.IsPro = isPro;

            // Lấy danh sách Jobs theo quyền
            IQueryable<Job> jobQuery = _context.Jobs.Include(j => j.Company).Where(j => j.Status == 1);

            if (User.IsInRole("Recruiter"))
            {
                var companyIdClaim = User.FindFirst("CompanyID")?.Value;
                if (companyIdClaim != null && int.TryParse(companyIdClaim, out int companyId))
                {
                    jobQuery = jobQuery.Where(j => j.CompanyID == companyId);
                }
                else
                {
                    jobQuery = jobQuery.Where(j => false);
                }
            }

            ViewBag.Jobs = await jobQuery.OrderByDescending(j => j.CreatedAt).ToListAsync();

            return View();
        }

        /// <summary>
        /// API: Phân tích RAG (Level 3 AI Maturity) - Lọc nhanh qua Vector DB và phân tích chuyên sâu bằng LLM
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AnalyzeJob(int jobId)
        {
            var isProClaim = User.FindFirst("IsPro")?.Value;
            bool isPro = (isProClaim == "True") || User.IsInRole("Admin");
            if (!isPro)
            {
                return Json(new { success = false, message = "Vui lòng nâng cấp tài khoản Pro để sử dụng tính năng Smart Match." });
            }

            var job = await _context.Jobs.Include(j => j.Company).FirstOrDefaultAsync(j => j.JobID == jobId);
            if (job == null)
                return Json(new { success = false, message = "Không tìm thấy tin tuyển dụng." });

            if (User.IsInRole("Recruiter"))
            {
                var companyIdClaim = User.FindFirst("CompanyID")?.Value;
                if (companyIdClaim == null || !int.TryParse(companyIdClaim, out int cid) || job.CompanyID != cid)
                    return Json(new { success = false, message = "Bạn không có quyền phân tích tin này." });
            }

            // Lấy toàn bộ CV công khai có nội dung
            var publicResumes = await _context.Resumes
                .Include(r => r.User)
                .Where(r => r.IsPublic && !string.IsNullOrEmpty(r.JsonContent))
                .ToListAsync();

            if (!publicResumes.Any())
                return Json(new { success = false, message = "Chưa có ứng viên nào công khai CV." });

            Console.WriteLine("[SmartMatch RAG] Gửi dữ liệu sang Python AI Service để so khớp...");

            // Chuẩn bị JD
            string jdText = $"Vị trí: {job.Title}\nCông ty: {job.Company?.Name ?? "N/A"}\nMô tả: {StripHTML(job.Description)}\nYêu cầu: {StripHTML(job.Requirements)}\nMức lương: {job.Salary}";

            // Gọi sang Python (Cổng 8000)
            var topCandidates = new List<Resume>();

            // Tải cấu hình sớm để dùng được TopCandidatesCount và thông tin model
            var configData = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
            var pythonBaseUrl = _config["PythonAI:BaseUrl"] ?? "http://localhost:8000";
            var cacheExpireDays = 7; // Vector cache hợp lệ trong 7 ngày

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30); // Tăng timeout vì có thể encode nhiều CV

                    // === BƯỚC 1: Phân loại CV có cache và CV cần encode mới ===
                    var resumeIds = publicResumes.Select(r => r.ResumeID).ToList();
                    var cutoffDate = DateTime.Now.AddDays(-cacheExpireDays);

                    // Đọc tất cả cache còn hợp lệ từ DB một lần duy nhất
                    var cachedEmbeddings = await _context.CVEmbeddings
                        .Where(e => resumeIds.Contains(e.ResumeID) && e.UpdatedAt >= cutoffDate)
                        .ToDictionaryAsync(e => e.ResumeID);

                    // Tách thành 2 nhóm: có cache và chưa có cache
                    var resumesNeedEncoding = publicResumes
                        .Where(r => !cachedEmbeddings.ContainsKey(r.ResumeID))
                        .ToList();
                    var resumesWithCache = publicResumes
                        .Where(r => cachedEmbeddings.ContainsKey(r.ResumeID))
                        .ToList();

                    Console.WriteLine($"[SmartMatch Cache] Có cache: {resumesWithCache.Count} CV | Cần encode mới: {resumesNeedEncoding.Count} CV");

                    // === BƯỚC 2: Encode các CV chưa có cache → lưu vào DB ===
                    foreach (var resume in resumesNeedEncoding)
                    {
                        try
                        {
                            var cvText = ExtractCVText(resume);
                            var embedResponse = await client.PostAsJsonAsync(
                                $"{pythonBaseUrl}/api/embed-cv",
                                new { text = cvText }
                            );

                            if (embedResponse.IsSuccessStatusCode)
                            {
                                var embedResult = await embedResponse.Content
                                    .ReadFromJsonAsync<EmbedCVResponse>();

                                if (embedResult?.vector != null)
                                {
                                    // Upsert vào CVEmbeddings (cập nhật nếu đã có, thêm mới nếu chưa có)
                                    var existing = await _context.CVEmbeddings
                                        .FirstOrDefaultAsync(e => e.ResumeID == resume.ResumeID);

                                    string vectorJson = System.Text.Json.JsonSerializer.Serialize(embedResult.vector);

                                    if (existing != null)
                                    {
                                        existing.VectorJson = vectorJson;
                                        existing.UpdatedAt = DateTime.Now;
                                    }
                                    else
                                    {
                                        _context.CVEmbeddings.Add(new CVEmbedding
                                        {
                                            ResumeID = resume.ResumeID,
                                            VectorJson = vectorJson,
                                            UpdatedAt = DateTime.Now
                                        });
                                    }

                                    // Thêm vào dictionary để dùng ngay trong lần này
                                    cachedEmbeddings[resume.ResumeID] = new CVEmbedding
                                    {
                                        ResumeID = resume.ResumeID,
                                        VectorJson = vectorJson
                                    };
                                }
                            }
                        }
                        catch (Exception embedEx)
                        {
                            Console.WriteLine($"[SmartMatch Cache] Lỗi encode CV {resume.ResumeID}: {embedEx.Message}");
                        }
                    }

                    // Lưu tất cả embedding mới vào DB một lần
                    if (resumesNeedEncoding.Any())
                        await _context.SaveChangesAsync();

                    // === BƯỚC 3: Gọi Python tính similarity từ vector đã có sẵn ===
                    var cachedCVList = publicResumes
                        .Where(r => cachedEmbeddings.ContainsKey(r.ResumeID))
                        .Select(r => new
                        {
                            id = r.ResumeID,
                            vector = System.Text.Json.JsonSerializer.Deserialize<List<float>>(
                                cachedEmbeddings[r.ResumeID].VectorJson)
                        })
                        .Where(x => x.vector != null && x.vector.Count > 0)
                        .ToList();

                    if (!cachedCVList.Any())
                    {
                        return Json(new { success = false, message = "Không thể tạo vector cho bất kỳ CV nào. Kiểm tra Python service." });
                    }

                    // Gọi endpoint mới: chỉ tính similarity, không encode lại
                    var simResponse = await client.PostAsJsonAsync(
                        $"{pythonBaseUrl}/api/similarity-from-vectors",
                        new { jd_text = jdText, cached_cvs = cachedCVList }
                    );

                    if (!simResponse.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = "Dịch vụ Python AI phản hồi lỗi. Vui lòng kiểm tra lại server Python." });
                    }

                    var pythonResult = await simResponse.Content.ReadFromJsonAsync<PythonFilterResponse>();
                    if (pythonResult != null && pythonResult.top_cvs != null)
                    {
                        // Lấy số ứng viên tối đa từ cấu hình Admin (mặc định 6 nếu chưa cài đặt)
                        int topN = (configData?.TopCandidatesCount > 0) ? configData.TopCandidatesCount : 6;
                        var topResumeIds = pythonResult.top_cvs.Select(x => x.resume_id).Take(topN).ToList();

                        // Lấy đúng các CV đã lọt Top
                        topCandidates = publicResumes.Where(r => topResumeIds.Contains(r.ResumeID)).ToList();

                        // Sắp xếp lại đúng thứ tự Python trả về (cao xuống thấp)
                        topCandidates = topCandidates.OrderBy(r => topResumeIds.IndexOf(r.ResumeID)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartMatch Error] Lỗi kết nối Python: {ex.Message}");
                return Json(new { success = false, message = "Không thể kết nối đến Dịch vụ Python AI (FastAPI tại cổng 8000). Vui lòng khởi chạy server Python trước!" });
            }

            Console.WriteLine($"[SmartMatch RAG] Đã lọc ra Top {topCandidates.Count} ứng viên. Bắt đầu gọi LLM chấm điểm song song...");

            var newMatchResults = new List<CVMatchResult>();
            int userId = CurrentUserId;
            var userInfo = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);
            isPro = userInfo?.IsPro ?? false || User.IsInRole("Admin");

            string selectedModel = isPro ? (configData?.ProModelName ?? "") : (configData?.ModelName ?? "");
            string apiProvider = selectedModel.Contains("llama") || selectedModel.Contains("mixtral") ? "Groq" : "Gemini";

            var semaphore = new SemaphoreSlim(isPro ? 2 : 1); // Giới hạn luồng chạy đồng thời để tránh Rate Limit
            var scoringTasks = topCandidates.Select(async (resume, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Đặt Delay thông minh theo nhà cung cấp API
                    if (index > 0)
                    {
                        int delayMs = isPro ? 500 : (apiProvider == "Groq" ? 1000 : 4000);
                        await Task.Delay(delayMs);
                    }

                    Console.WriteLine($"[SmartMatch RAG] AI đang đánh giá: {resume.FullName ?? "N/A"} (ID={resume.ResumeID})");

                    string cvText = ExtractCVText(resume);
                    if (string.IsNullOrWhiteSpace(cvText)) return null;

                    string prompt = BuildMatchPrompt(cvText, jdText);
                    var aiResult = await _aiService.GenerateContent(prompt, isPro);
                    var matchData = ParseMatchResult(aiResult);

                    if (matchData != null)
                    {
                        var result = new CVMatchResult
                        {
                            JobID = jobId,
                            ResumeID = resume.ResumeID,
                            MatchScore = matchData.MatchScore,
                            MatchedSkills = matchData.MatchedSkills,
                            MissingSkills = matchData.MissingSkills,
                            Suggestions = matchData.Suggestions,
                            Strengths = matchData.Strengths,
                            Summary = matchData.Summary,
                            Recommendation = matchData.Recommendation,
                            AnalyzedAt = DateTime.Now
                        };

                        // Đếm token chính xác bằng TiktokenSharp (cl100k_base, tương thích Gemini/GPT)
                        int estimatedTokens;
                        try {
                            var tikToken = TikToken.EncodingForModel("gpt-4");
                            estimatedTokens = tikToken.Encode(prompt).Count + tikToken.Encode(aiResult ?? "").Count;
                        } catch {
                            estimatedTokens = (prompt.Length / 4) + ((aiResult?.Length ?? 0) / 4);
                        }
                        lock (newMatchResults) // Lock list vì add từ nhiều thread
                        {
                            newMatchResults.Add(result);
                        }

                        // Thêm log cho Admin
                        var log = new AILog
                        {
                            UserID = userId,
                            RequestType = "smart_match_rag",
                            InputText = $"Job #{jobId} vs Resume #{resume.ResumeID}",
                            OutputText = aiResult.Length > 500 ? aiResult.Substring(0, 500) + "..." : aiResult,
                            UsedTokens = estimatedTokens,
                            ApiProvider = apiProvider,
                            CreatedAt = DateTime.Now
                        };
                        lock (_context) // DBContext không thread-safe
                        {
                            _context.AILogs.Add(log);
                        }
                        
                        return result;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SmartMatch RAG] Error LLM analyzing Resume #{resume.ResumeID}: {ex.Message}");
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            // Chờ tất cả các task hoàn tất
            await Task.WhenAll(scoringTasks);


            // BƯỚC CUỐI CÙNG: CHỈ XÓA DỮ LIỆU CŨ KHI ĐÃ CÓ DỮ LIỆU MỚI THÀNH CÔNG
            if (newMatchResults.Any())
            {
                var oldResults = await _context.CVMatchResults.Where(r => r.JobID == jobId).ToListAsync();
                if (oldResults.Any())
                {
                    _context.CVMatchResults.RemoveRange(oldResults);
                }
                _context.CVMatchResults.AddRange(newMatchResults);

                // Cập nhật tổng Token đã dùng
                if (configData != null)
                {
                    var trackedConfig = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == configData.Id);
                    if (trackedConfig != null)
                    {
                        long totalBatchTokens = newMatchResults.Count * 2000; // Ước tính trung bình 2k tokens mỗi lần match (Prompt + Result)
                        trackedConfig.TotalTokensUsed += totalBatchTokens;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                success = true,
                message = $"RAG Pipeline hoàn tất: Đã chấm điểm chuyên sâu {newMatchResults.Count}/{topCandidates.Count} ứng viên hàng đầu.",
                analyzed = newMatchResults.Count,
                total = publicResumes.Count
            });
        }

        /// <summary>
        /// API: Lấy kết quả xếp hạng cho 1 Job
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetResults(int jobId)
        {
            var isProClaim = User.FindFirst("IsPro")?.Value;
            bool isPro = (isProClaim == "True") || User.IsInRole("Admin");
            if (!isPro)
            {
                return Json(new { success = false, message = "Vui lòng nâng cấp tài khoản Pro để sử dụng tính năng Smart Match." });
            }

            var results = await _context.CVMatchResults
                .Include(r => r.Resume)
                    .ThenInclude(r => r.User)
                .Include(r => r.Job)
                    .ThenInclude(j => j.Company)
                .Where(r => r.JobID == jobId)
                .OrderByDescending(r => r.MatchScore)
                .ToListAsync();

            if (!results.Any())
                return Json(new { success = false, message = "Chưa có kết quả phân tích cho tin tuyển dụng này." });

            int recruiterId = CurrentUserId;
            var savedResumeIds = await _context.SavedCandidates
                .Where(s => s.RecruiterId == recruiterId)
                .Select(s => s.ResumeId)
                .ToListAsync();

            var data = results.Select((r, index) => new
            {
                rank = index + 1,
                resumeId = r.ResumeID,
                userId = r.Resume?.UserID,
                fullName = r.Resume?.User?.FullName ?? r.Resume?.FullName ?? "Ẩn danh",
                jobTitle = r.Resume?.JobTitle ?? "Chưa rõ",
                avatarUrl = !string.IsNullOrWhiteSpace(r.Resume?.AvatarUrl) ? r.Resume.AvatarUrl
                           : (!string.IsNullOrWhiteSpace(r.Resume?.User?.AvatarUrl) ? r.Resume.User.AvatarUrl : null),
                matchScore = r.MatchScore,
                matchedSkills = SafeParseJsonArray(r.MatchedSkills),
                missingSkills = SafeParseJsonArray(r.MissingSkills),
                suggestions = SafeParseJsonArray(r.Suggestions),
                strengths = SafeParseJsonArray(r.Strengths),
                summary = r.Summary,
                recommendation = r.Recommendation,
                analyzedAt = r.AnalyzedAt.ToString("dd/MM/yyyy HH:mm"),
                isSaved = savedResumeIds.Contains(r.ResumeID)
            });

            var job = results.First().Job;

            return Json(new
            {
                success = true,
                jobTitle = job.Title,
                companyName = job.Company?.Name,
                totalCandidates = results.Count,
                avgScore = Math.Round(results.Average(r => r.MatchScore), 1),
                candidates = data
            });
        }

        // ===========================
        // PRIVATE HELPER METHODS
        // ===========================

        /// <summary>Trích xuất text từ JSON CV để gửi cho AI phân tích (Đã được tối ưu nén Token)</summary>
        private string ExtractCVText(Resume resume)
        {
            var sb = new StringBuilder();

            // Ứng dụng Blind Hiring (Tuyển dụng mù): Loại bỏ Tên, Thông tin liên hệ, chỉ giữ lại ID và Vị trí
            sb.AppendLine($"[Ứng viên ID: {resume.ResumeID}] | Vị trí: {resume.JobTitle ?? "N/A"}");

            // Parse JsonContent nếu có
            if (!string.IsNullOrEmpty(resume.JsonContent))
            {
                try
                {
                    dynamic json = JsonConvert.DeserializeObject(resume.JsonContent);
                    if (json != null)
                    {
                        // Trích xuất sections từ JSON
                        if (json.sections != null)
                        {
                            foreach (var section in json.sections)
                            {
                                string sectionType = section.type?.ToString().ToLower() ?? "";
                                
                                // Loại bỏ rác: Sở thích, Tham chiếu, Hoạt động phụ, Thông tin liên hệ
                                if (sectionType == "hobbies" || sectionType == "references" || 
                                    sectionType == "activities" || sectionType == "additional" || 
                                    sectionType == "contact")
                                {
                                    continue;
                                }

                                string sectionTitle = section.title?.ToString() ?? sectionType;

                                sb.AppendLine($"\n[{sectionTitle}]");

                                if (section.items != null)
                                {
                                    foreach (var item in section.items)
                                    {
                                        var parts = new List<string>();
                                        
                                        // Gom nhóm thực thể chính (Công ty / Trường học / Tên dự án)
                                        if (item.company != null) parts.Add(item.company.ToString());
                                        else if (item.school != null) parts.Add(item.school.ToString());
                                        else if (item.name != null) parts.Add(item.name.ToString());

                                        // Gom nhóm Vai trò / Chuyên ngành / Bằng cấp
                                        if (item.position != null) parts.Add(item.position.ToString());
                                        if (item.role != null && item.role.ToString() != item.position?.ToString()) parts.Add(item.role.ToString());
                                        if (item.major != null) parts.Add(item.major.ToString());
                                        if (item.degree != null) parts.Add(item.degree.ToString());

                                        // Gom nhóm thời gian
                                        string dateStr = "";
                                        if (item.period != null) dateStr = item.period.ToString();
                                        else if (item.startDate != null || item.endDate != null) 
                                            dateStr = $"{item.startDate}-{item.endDate}".Trim('-');
                                        if (!string.IsNullOrWhiteSpace(dateStr)) parts.Add($"({dateStr})");

                                        // Gom nhóm công nghệ
                                        if (item.technologies != null) parts.Add($"Tech: {item.technologies}");

                                        // Gom nhóm mô tả (đã strip HTML)
                                        if (item.description != null) {
                                            string desc = StripHTML(item.description.ToString());
                                            if (!string.IsNullOrWhiteSpace(desc)) parts.Add(desc);
                                        }

                                        if (parts.Any()) sb.AppendLine("- " + string.Join(" | ", parts));
                                    }
                                }
                            }
                        }

                        // Trích xuất skills nén trên 1 dòng
                        if (json.skills != null)
                        {
                            var skillsList = new List<string>();
                            foreach (var skill in json.skills)
                            {
                                string skillName = skill.name?.ToString() ?? skill.ToString();
                                string level = skill.level?.ToString() ?? "";
                                skillsList.Add($"{skillName}{(string.IsNullOrEmpty(level) ? "" : $"({level})")}");
                            }
                            if (skillsList.Any()) sb.AppendLine($"\n[Kỹ năng] {string.Join(", ", skillsList)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SmartMatch] JSON parse error for Resume #{resume.ResumeID}: {ex.Message}");
                }
            }

            // Thêm skills từ User profile
            if (!string.IsNullOrEmpty(resume.User?.Skills))
                sb.AppendLine($"\n[Profile Skills] {resume.User.Skills}");

            return sb.ToString();
        }

        /// <summary>Tạo prompt phân tích so khớp CV với JD</summary>
        private string BuildMatchPrompt(string cvText, string jdText)
        {
            return $@"Bạn là Tech Recruiter AI cực kỳ khắt khe. Phân tích độ phù hợp của CV với Job Description (JD).

=== JD ===
{jdText}

=== CV ===
{cvText}

[LUẬT ĐÁNH GIÁ NGHIÊM NGẶT - BẮT BUỘC TUÂN THỦ]
1. KIỂM TRA CHỨC DANH (QUAN TRỌNG NHẤT): Nếu vị trí ứng viên không liên quan (VD: JD cần Backend, CV là Frontend/Security/BA), trừ điểm nặng, matchScore KHÔNG ĐƯỢC VƯỢT QUÁ 49.
2. NỀN TẢNG CỐT LÕI: Nếu CV thiếu các công nghệ lõi mà JD yêu cầu (VD: JD cần .NET, CV chỉ có Nodejs), matchScore KHÔNG ĐƯỢC VƯỢT QUÁ 49.
3. THANG ĐIỂM (matchScore): 
   - Dưới 30: Không phù hợp (Sai ngành, không có kỹ năng).
   - Từ 30 đến 49: Ít phù hợp (Thiếu nhiều kỹ năng cốt lõi).
   - Từ 50 đến 79: Phù hợp 1 phần (Đáp ứng cơ bản, còn thiếu vài kỹ năng).
   - Từ 80 đến 100: Phù hợp (Đáp ứng rất tốt yêu cầu).
4. TIẾT KIỆM TỪ NGỮ: Trả lời ngắn gọn nhất có thể để tiết kiệm token.

Output JSON thuần (KHÔNG markdown, KHÔNG text phụ):
{{
""matchScore"": <0-100, khắt khe>,
""matchedSkills"": [<các kỹ năng trùng khớp, tối đa 5>],
""missingSkills"": [<kỹ năng JD cần mà CV KHÔNG CÓ, tối đa 5>],
""strengths"": [<2 điểm mạnh cực kỳ ngắn gọn>],
""suggestions"": [<2 gợi ý CỤ THỂ để cải thiện CV>],
""summary"": ""<Nhận xét tổng quan tối đa 2 câu sắc bén>"",
""recommendation"": ""<strong_match|good_match|partial_match|weak_match>""
}}";
        }

        /// <summary>Xóa toàn bộ thẻ HTML và khoảng trắng thừa để tiết kiệm Token</summary>
        private string StripHTML(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // Xóa thẻ HTML
            var stripped = Regex.Replace(input, "<.*?>", string.Empty);
            // Chuẩn hóa khoảng trắng thừa thành 1 dấu cách
            stripped = Regex.Replace(stripped, @"\s+", " ");
            // Giới hạn độ dài tối đa 1 phần văn bản để chống tràn token (VD: max 3000 ký tự)
            if (stripped.Length > 3000) stripped = stripped.Substring(0, 3000) + "...";
            return stripped.Trim();
        }

        /// <summary>Parse JSON từ kết quả AI trả về</summary>
        private MatchResultDto ParseMatchResult(string aiResult)
        {
            try
            {
                // Tìm đoạn JSON hợp lệ bằng cách cắt từ '{' đến '}'
                int startIndex = aiResult.IndexOf('{');
                int endIndex = aiResult.LastIndexOf('}');
                
                if (startIndex < 0 || endIndex < startIndex)
                {
                    Console.WriteLine($"[SmartMatch] Không tìm thấy JSON trong phản hồi:\n{aiResult}");
                    return null;
                }

                string cleaned = aiResult.Substring(startIndex, endIndex - startIndex + 1);

                dynamic json = JsonConvert.DeserializeObject(cleaned);
                if (json == null) return null;

                return new MatchResultDto
                {
                    MatchScore = Math.Clamp((int)(json.matchScore ?? 0), 0, 100),
                    MatchedSkills = json.matchedSkills != null ? JsonConvert.SerializeObject(json.matchedSkills) : "[]",
                    MissingSkills = json.missingSkills != null ? JsonConvert.SerializeObject(json.missingSkills) : "[]",
                    Suggestions = json.suggestions != null ? JsonConvert.SerializeObject(json.suggestions) : "[]",
                    Strengths = json.strengths != null ? JsonConvert.SerializeObject(json.strengths) : "[]",
                    Summary = json.summary?.ToString() ?? "",
                    Recommendation = json.recommendation?.ToString() ?? "partial_match"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartMatch] JSON parse error: {ex.Message}\nRaw: {aiResult}");
                return null;
            }
        }

        /// <summary>An toàn parse JSON array string</summary>
        private List<string> SafeParseJsonArray(string jsonArrayStr)
        {
            if (string.IsNullOrWhiteSpace(jsonArrayStr)) return new List<string>();
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(jsonArrayStr) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>Tính toán độ tương đồng Cosine (Cosine Similarity) giữa 2 mảng vector</summary>
        private double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA == null || vectorB == null || vectorA.Length != vectorB.Length || vectorA.Length == 0)
                return 0;

            double dotProduct = 0;
            double normA = 0;
            double normB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        /// <summary>DTO tạm cho việc parse kết quả AI</summary>
        private class MatchResultDto
        {
            public int MatchScore { get; set; }
            public string MatchedSkills { get; set; }
            public string MissingSkills { get; set; }
            public string Suggestions { get; set; }
            public string Strengths { get; set; }
            public string Summary { get; set; }
            public string Recommendation { get; set; }
        }

        public class PythonFilterResponse
        {
            public List<PythonTopCV> top_cvs { get; set; }
        }

        public class PythonTopCV
        {
            public int resume_id { get; set; }
            public double similarity { get; set; }
        }

        /// <summary>Response từ endpoint /api/embed-cv của Python</summary>
        public class EmbedCVResponse
        {
            public List<float> vector { get; set; }
        }
    }
}
