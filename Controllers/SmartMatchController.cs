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
using Microsoft.AspNetCore.RateLimiting; // Sử dụng middleware Rate Limiting

namespace DoAnCS.Controllers
{
    [Authorize(Roles = "Recruiter,Admin")]
    [EnableRateLimiting("AiApiPolicy")] // Áp dụng Rate Limit theo IP
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

        // Cache bộ nhớ cho danh sách file PDF trong CV_Vector_Index.json để truy xuất cực nhanh
        private static Dictionary<int, string>? _pdfFilenameCache = null;
        private static readonly object _pdfCacheLock = new object();

        /// <summary>
        /// API: Xem tệp hồ sơ PDF gốc của ứng viên từ bộ dữ liệu benchmark (Blind Hiring)
        /// </summary>
        [HttpGet]
        public IActionResult ViewCandidatePdf(int id)
        {
            var isProClaim = User.FindFirst("IsPro")?.Value;
            bool isPro = (isProClaim == "True") || User.IsInRole("Admin");
            if (!isPro)
            {
                return Forbid();
            }

            string filename = GetCandidatePdfFilename(id);
            if (string.IsNullOrEmpty(filename))
            {
                return NotFound($"Không tìm thấy tệp hồ sơ ứng viên #{id} trong chỉ mục hệ thống.");
            }

            string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Raw_PDFs", filename);
            if (!System.IO.File.Exists(pdfPath))
            {
                return NotFound($"Tệp PDF gốc không tồn tại trên hệ thống lưu trữ: {filename}");
            }

            // Mở file PDF trực tiếp trên trình duyệt (inline preview)
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"Candidate_{id}.pdf\"");
            return PhysicalFile(pdfPath, "application/pdf");
        }

        /// <summary>
        /// API: Phân tích 2-Stage Hybrid Matching Engine (Level 3 AI Maturity):
        /// Vòng 1: Quét 3.320 CV qua Vector Cache chuẩn hóa (< 0.05s).
        /// Vòng 2: Chấm điểm ATS Scorer chuyên sâu 4 trụ cột & chống nhồi kỹ năng.
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

            // Chuẩn bị thông tin JD
            string cleanDesc = StripHTML(job.Description ?? "");
            string cleanReq = StripHTML(job.Requirements ?? "");
            string jdText = $"Vị trí: {job.Title}\nCông ty: {job.Company?.Name ?? "N/A"}\nMô tả: {cleanDesc}\nYêu cầu: {cleanReq}\nMức lương: {job.Salary ?? "Thỏa thuận"}";

            // Trích xuất tự động kỹ năng, cấp bậc và số năm kinh nghiệm từ JD
            var (hardSkills, expYears, level) = ExtractJobMetadata(job.Title, cleanDesc, cleanReq);

            var configData = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
            var pythonBaseUrl = _config["PythonAI:BaseUrl"] ?? "http://localhost:8000";
            int finalTopK = (configData?.TopCandidatesCount >= 10) ? configData.TopCandidatesCount : 10;

            Console.WriteLine($"[SmartMatch Hybrid] Bắt đầu phân tích Job #{jobId} ({job.Title}) với 3.320 CVs...");

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Payload gửi sang Python FastAPI Hybrid Matcher
                    var hybridPayload = new
                    {
                        jd_text = jdText,
                        hard_skills = hardSkills,
                        experience_years_required = expYears,
                        level = level,
                        title = job.Title,
                        filter_top_k = 25,
                        final_top_k = finalTopK
                    };

                    var response = await client.PostAsJsonAsync($"{pythonBaseUrl}/api/match/hybrid-match", hybridPayload);
                    if (!response.IsSuccessStatusCode)
                    {
                        string errText = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[SmartMatch Error] Python AI trả về mã lỗi {response.StatusCode}: {errText}");
                        return Json(new { success = false, message = "Dịch vụ Python AI phản hồi không thành công. Kiểm tra lại server Python cổng 8000." });
                    }

                    string rawJson = await response.Content.ReadAsStringAsync();
                    var matchResponse = JsonConvert.DeserializeObject<HybridMatchResponse>(rawJson);
                    if (matchResponse == null || matchResponse.top_candidates == null || !matchResponse.top_candidates.Any())
                    {
                        return Json(new { success = false, message = "Không tìm thấy ứng viên phù hợp trong cơ sở dữ liệu." });
                    }

                    int recruiterId = CurrentUserId;
                    var savedResumeIds = await _context.SavedCandidates
                        .Where(s => s.RecruiterId == recruiterId)
                        .Select(s => s.ResumeId)
                        .ToListAsync();

                    // Chuẩn hóa danh sách kết quả gửi về UI
                    var candidates = matchResponse.top_candidates.Select((c, idx) =>
                    {
                        // Parse số năm kinh nghiệm từ chuỗi experience_eval
                        float detectedYears = 0;
                        if (!string.IsNullOrEmpty(c.experience_eval))
                        {
                            var m = Regex.Match(c.experience_eval, @"(\d+(?:\.\d+)?)");
                            if (m.Success) float.TryParse(m.Groups[1].Value, out detectedYears);
                        }

                        // Parse strengths thành list
                        var strengthList = new List<string>();
                        if (!string.IsNullOrWhiteSpace(c.strengths))
                        {
                            strengthList = c.strengths.Split(new[] { "; ", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        // Parse suggestions thành list
                        var suggestionList = new List<string>();
                        if (!string.IsNullOrWhiteSpace(c.suggestions))
                        {
                            suggestionList = c.suggestions.Split(new[] { "; ", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        return new
                        {
                            rank = idx + 1,
                            resumeId = c.id,
                            fullName = $"Ứng viên #{c.id}",
                            jobTitle = job.Title,
                            avatarUrl = (string?)null,
                            hasPdf = true,
                            matchScore = (int)Math.Round(c.ats_score),
                            semanticScore = Math.Round(c.semantic_score * 100, 1),
                            matchedSkills = c.matched_skills ?? new List<string>(),
                            missingSkills = c.missing_skills ?? new List<string>(),
                            provenSkills = c.proven_skills ?? new List<string>(),
                            unverifiedSkills = c.unverified_skills ?? new List<string>(),
                            knockoutPassed = c.knockout_passed,
                            seniorityFit = c.seniority_fit ?? "Phù hợp",
                            experienceYearsDetected = detectedYears,
                            experienceEval = c.experience_eval ?? "Phù hợp yêu cầu",
                            strengths = strengthList,
                            suggestions = suggestionList,
                            summary = c.summary ?? "Hồ sơ ứng viên được đánh giá qua công nghệ lọc ATS và mô hình ngôn ngữ ngữ nghĩa.",
                            recommendation = c.recommendation ?? "good_match",
                            isSaved = savedResumeIds.Contains(c.id)
                        };
                    }).ToList();

                    // Ghi nhận AILog phục vụ thống kê & kiểm toán
                    try
                    {
                        var log = new AILog
                        {
                            UserID = CurrentUserId,
                            RequestType = "hybrid_smart_match",
                            InputText = $"Job #{jobId} ({job.Title}) | Exp: {expYears}y | Level: {level}",
                            OutputText = $"Quét: {matchResponse.total_scanned} CVs | Top: {candidates.Count} | Total Latency: {matchResponse.latency?.total_latency_ms}ms",
                            UsedTokens = 1500,
                            ApiProvider = "Local MiniLM + ATS Scorer",
                            CreatedAt = DateTime.Now
                        };
                        _context.AILogs.Add(log);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception logEx)
                    {
                        Console.WriteLine($"[SmartMatch Log Error] {logEx.Message}");
                    }

                    // Lưu cache kết quả vào tệp JSON đệm theo JobID
                    SaveJobMatchResultsCache(jobId, job.Title, job.Company?.Name ?? "N/A", candidates, matchResponse.latency, matchResponse.total_scanned);

                    return Json(new
                    {
                        success = true,
                        message = $"Phân tích Hybrid AI hoàn tất: Đã quét {matchResponse.total_scanned:N0} CV trong kho dữ liệu và chọn lọc Top {candidates.Count} ứng viên sáng giá nhất.",
                        jobTitle = job.Title,
                        companyName = job.Company?.Name,
                        totalCandidates = candidates.Count,
                        totalScanned = matchResponse.total_scanned,
                        latency = matchResponse.latency,
                        avgScore = candidates.Any() ? Math.Round(candidates.Average(x => x.matchScore), 1) : 0,
                        candidates = candidates
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartMatch Exception] Lỗi kết nối dịch vụ Python AI: {ex.Message}");
                return Json(new { success = false, message = $"Không thể kết nối đến Dịch vụ Python AI (FastAPI tại cổng 8000): {ex.Message}" });
            }
        }

        /// <summary>
        /// API: Lấy kết quả xếp hạng cho 1 Job (Ưu tiên đọc cache tệp JSON nhanh, fallback DB)
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

            int recruiterId = CurrentUserId;
            var savedResumeIds = await _context.SavedCandidates
                .Where(s => s.RecruiterId == recruiterId)
                .Select(s => s.ResumeId)
                .ToListAsync();

            // 1. Kiểm tra cache kết quả từ tệp JSON đệm theo JobID
            string cachePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "JobMatchResults", $"{jobId}.json");
            if (System.IO.File.Exists(cachePath))
            {
                try
                {
                    string jsonStr = await System.IO.File.ReadAllTextAsync(cachePath);
                    var cachedData = JsonConvert.DeserializeObject<dynamic>(jsonStr);
                    if (cachedData != null && cachedData.candidates != null)
                    {
                        // Cập nhật trạng thái isSaved cho recruiter hiện tại
                        foreach (var c in cachedData.candidates)
                        {
                            int resId = (int)(c.resumeId ?? 0);
                            c.isSaved = savedResumeIds.Contains(resId);
                        }
                        cachedData.success = true;

                        // Dùng Newtonsoft.Json serialize chuỗi JSON chuẩn và trả thẳng cho client
                        string cleanJson = JsonConvert.SerializeObject(cachedData);
                        return Content(cleanJson, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SmartMatch Cache Read Error] {ex.Message}");
                }
            }

            // 2. Fallback: Đọc từ bảng CVMatchResults trong SQL Server (nếu có dữ liệu cũ)
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

            var data = results.Select((r, index) => new
            {
                rank = index + 1,
                resumeId = r.ResumeID,
                userId = r.Resume?.UserID,
                fullName = r.Resume?.User?.FullName ?? r.Resume?.FullName ?? $"Ứng viên #{r.ResumeID}",
                jobTitle = r.Resume?.JobTitle ?? "Chưa rõ",
                avatarUrl = !string.IsNullOrWhiteSpace(r.Resume?.AvatarUrl) ? r.Resume.AvatarUrl
                           : (!string.IsNullOrWhiteSpace(r.Resume?.User?.AvatarUrl) ? r.Resume.User.AvatarUrl : null),
                hasPdf = false,
                matchScore = r.MatchScore,
                matchedSkills = SafeParseJsonArray(r.MatchedSkills),
                missingSkills = SafeParseJsonArray(r.MissingSkills),
                provenSkills = new List<string>(),
                unverifiedSkills = new List<string>(),
                seniorityFit = "Phù hợp",
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
                totalScanned = results.Count,
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
                        // 1. Kiểm tra nếu là CV tải lên từ file PDF đã được bóc tách và che PII
                        if (json.is_pdf_upload == true && json.clean_text != null)
                        {
                            string cleanText = json.clean_text.ToString();
                            sb.AppendLine("\n[Nội dung hồ sơ PDF]");
                            sb.AppendLine(cleanText);
                            return sb.ToString().Trim();
                        }

                        // 2. Trích xuất sections từ JSON (dành cho CV tạo bằng Vue Builder)
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

        /// <summary>Lưu bộ nhớ đệm kết quả phân tích theo JobID vào tệp JSON</summary>
        private void SaveJobMatchResultsCache(int jobId, string jobTitle, string companyName, object candidates, LatencyDto? latency, int totalScanned)
        {
            try
            {
                string cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "JobMatchResults");
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }

                string cachePath = Path.Combine(cacheDir, $"{jobId}.json");
                var cachePayload = new
                {
                    success = true,
                    jobId = jobId,
                    jobTitle = jobTitle,
                    companyName = companyName,
                    totalScanned = totalScanned,
                    totalCandidates = ((System.Collections.IList)candidates).Count,
                    avgScore = ((System.Collections.IEnumerable)candidates).Cast<dynamic>().Any() 
                        ? Math.Round(((System.Collections.IEnumerable)candidates).Cast<dynamic>().Average(x => (double)x.matchScore), 1) : 0,
                    latency = latency,
                    analyzedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    candidates = candidates
                };

                string jsonStr = JsonConvert.SerializeObject(cachePayload, Formatting.Indented);
                System.IO.File.WriteAllText(cachePath, jsonStr, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartMatch Cache Save Error] {ex.Message}");
            }
        }

        /// <summary>Ánh xạ ID ứng viên sang tên file PDF vật lý từ bộ chỉ mục CV_Vector_Index.json</summary>
        private string GetCandidatePdfFilename(int candidateId)
        {
            lock (_pdfCacheLock)
            {
                if (_pdfFilenameCache == null)
                {
                    _pdfFilenameCache = new Dictionary<int, string>();
                    try
                    {
                        string indexPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "CV_Vector_Index.json");
                        if (System.IO.File.Exists(indexPath))
                        {
                            string json = System.IO.File.ReadAllText(indexPath);
                            var indexList = JsonConvert.DeserializeObject<List<IndexEntryDto>>(json);
                            if (indexList != null)
                            {
                                foreach (var entry in indexList)
                                {
                                    if (!string.IsNullOrEmpty(entry.filename))
                                    {
                                        _pdfFilenameCache[entry.id] = entry.filename;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SmartMatch PDF Cache Error] {ex.Message}");
                    }
                }

                if (_pdfFilenameCache.TryGetValue(candidateId, out var filename))
                {
                    return filename;
                }
                return string.Empty;
            }
        }

        /// <summary>Trích xuất kỹ năng, số năm kinh nghiệm và cấp bậc từ JD</summary>
        private (List<string> hardSkills, float expYears, string level) ExtractJobMetadata(string title, string desc, string req)
        {
            string fullText = $"{title} {desc} {req}".ToLower();
            
            // 1. Phân loại Cấp bậc (Level)
            string level = "Middle";
            if (fullText.Contains("senior") || fullText.Contains("chuyên viên cao cấp") || fullText.Contains("chính"))
                level = "Senior";
            else if (fullText.Contains("lead") || fullText.Contains("trưởng nhóm") || fullText.Contains("manager") || fullText.Contains("quản lý"))
                level = "Lead";
            else if (fullText.Contains("junior") || fullText.Contains("fresher") || fullText.Contains("thực tập") || fullText.Contains("intern"))
                level = "Junior";

            // 2. Trích xuất Số năm kinh nghiệm (Experience Years)
            float expYears = level == "Senior" ? 4.0f : (level == "Lead" ? 6.0f : (level == "Junior" ? 1.0f : 2.5f));
            var expMatch = Regex.Match(fullText, @"(\d+)(?:-\d+)?\s*(?:\+)?\s*(?:năm|nam|years|year)\s*(?:kinh nghiệm|kinh nghiem|exp)?");
            if (expMatch.Success && float.TryParse(expMatch.Groups[1].Value, out float parsedYears))
            {
                expYears = parsedYears;
            }

            // 3. Trích xuất Kỹ năng cứng (Hard Skills)
            var commonSkills = new[]
            {
                "C#", ".NET", "ASP.NET", ".NET Core", "Entity Framework", "SQL Server", "MySQL", "PostgreSQL", "MongoDB",
                "Java", "Spring Boot", "Python", "Django", "FastAPI", "Flask", "React", "ReactJS", "Vue", "VueJS", "Angular",
                "NodeJS", "TypeScript", "JavaScript", "Docker", "Kubernetes", "AWS", "Azure", "GCP", "CI/CD", "Git",
                "Microservices", "RESTful API", "GraphQL", "Redis", "Kafka", "RabbitMQ", "HTML", "CSS", "Tailwind",
                "Figma", "UI/UX", "Machine Learning", "Deep Learning", "NLP", "Pandas", "Scikit-Learn", "PyTorch", "TensorFlow",
                "Data Analysis", "Power BI", "Excel", "Tableau", "Kế toán", "Thuế", "Marketing", "SEO", "Content", "HR"
            };

            var detectedSkills = new List<string>();
            foreach (var skill in commonSkills)
            {
                string pattern = $@"\b{Regex.Escape(skill.ToLower())}\b";
                if (Regex.IsMatch(fullText, pattern))
                {
                    detectedSkills.Add(skill);
                }
            }

            // Nếu không quét được kỹ năng nào, bổ sung mặc định dựa vào title
            if (!detectedSkills.Any())
            {
                if (fullText.Contains(".net") || fullText.Contains("c#"))
                    detectedSkills.AddRange(new[] { "C#", ".NET Core", "SQL Server" });
                else if (fullText.Contains("react") || fullText.Contains("frontend"))
                    detectedSkills.AddRange(new[] { "React", "JavaScript", "HTML/CSS" });
                else if (fullText.Contains("python"))
                    detectedSkills.AddRange(new[] { "Python", "FastAPI", "SQL" });
                else
                    detectedSkills.AddRange(new[] { "Communication", "Problem Solving", "Git" });
            }

            return (detectedSkills.Distinct().Take(8).ToList(), expYears, level);
        }

        private class IndexEntryDto
        {
            public int id { get; set; }
            public string filename { get; set; } = string.Empty;
        }

        public class HybridMatchResponse
        {
            public bool success { get; set; }
            public int total_scanned { get; set; }
            public int v1_passed_count { get; set; }
            public int final_count { get; set; }
            public LatencyDto? latency { get; set; }
            public List<HybridCandidateDto>? top_candidates { get; set; }
        }

        public class LatencyDto
        {
            public double v1_vector_cache_ms { get; set; }
            public double v2_ats_scoring_ms { get; set; }
            public double total_latency_ms { get; set; }
        }

        public class HybridCandidateDto
        {
            public int matrix_row { get; set; }
            public int id { get; set; }
            public string filename { get; set; } = string.Empty;
            public string language { get; set; } = string.Empty;
            public double semantic_score { get; set; }
            public double ats_score { get; set; }
            public List<string>? matched_skills { get; set; }
            public List<string>? missing_skills { get; set; }
            public List<string>? proven_skills { get; set; }
            public List<string>? unverified_skills { get; set; }
            public bool knockout_passed { get; set; }
            public string? experience_eval { get; set; }
            public string? seniority_fit { get; set; }
            public string? strengths { get; set; }
            public string? suggestions { get; set; }
            public string? summary { get; set; }
            public string? recommendation { get; set; }
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
