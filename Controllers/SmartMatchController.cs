using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using DoAnCS.Services;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace DoAnCS.Controllers
{
    [Authorize(Roles = "Recruiter,Admin")]
    public class SmartMatchController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;

        public SmartMatchController(AppDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        /// <summary>
        /// Trang chính Smart CV Matcher - Recruiter chọn JD và xem kết quả
        /// </summary>
        public async Task<IActionResult> Index()
        {
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

            // Lấy toàn bộ embeddings hiện có của các CV này
            var resumeIds = publicResumes.Select(r => r.ResumeID).ToList();
            var existingEmbeddings = await _context.CVEmbeddings
                .Where(e => resumeIds.Contains(e.ResumeID))
                .ToListAsync();

            var embeddingsDict = existingEmbeddings.ToDictionary(e => e.ResumeID, e => e);

            // BƯỚC 1: ĐỒNG BỘ VECTOR EMBEDDINGS (BATCH SYNC)
            var resumesToEmbed = new List<Resume>();
            foreach (var resume in publicResumes)
            {
                embeddingsDict.TryGetValue(resume.ResumeID, out var existing);
                // Nếu chưa có vector hoặc CV đã cập nhật mới hơn vector
                if (existing == null || existing.UpdatedAt < resume.UpdatedAt)
                {
                    resumesToEmbed.Add(resume);
                }
            }

            if (resumesToEmbed.Any())
            {
                Console.WriteLine($"[SmartMatch RAG] Đang đồng bộ Vector cho {resumesToEmbed.Count} CV mới/cập nhật...");
                // Chia batch 100 (giới hạn của Google Gemini Batch Embedding)
                for (int i = 0; i < resumesToEmbed.Count; i += 100)
                {
                    var batch = resumesToEmbed.Skip(i).Take(100).ToList();
                    var texts = batch.Select(r => ExtractCVText(r)).ToList();
                    
                    var vectors = await _aiService.GenerateEmbeddingsAsync(texts);

                    if (vectors != null && vectors.Count == batch.Count)
                    {
                        for (int j = 0; j < batch.Count; j++)
                        {
                            var resume = batch[j];
                            var vector = vectors[j];
                            string vectorJson = JsonConvert.SerializeObject(vector);

                            if (embeddingsDict.TryGetValue(resume.ResumeID, out var existing))
                            {
                                existing.VectorJson = vectorJson;
                                existing.UpdatedAt = DateTime.Now;
                            }
                            else
                            {
                                var newEmb = new CVEmbedding
                                {
                                    ResumeID = resume.ResumeID,
                                    VectorJson = vectorJson,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.CVEmbeddings.Add(newEmb);
                                embeddingsDict[resume.ResumeID] = newEmb;
                            }
                        }
                        await _context.SaveChangesAsync(); // Lưu theo batch 100 để đảm bảo an toàn dữ liệu
                    }
                }
            }

            // BƯỚC 2: NHÚNG JOB DESCRIPTION
            // [TỐI ƯU 2]: Xóa thẻ HTML khỏi Job Description trước khi nhúng và gửi cho AI để giảm cực mạnh số lượng Token.
            string jdText = $"Vị trí: {job.Title}\nCông ty: {job.Company?.Name ?? "N/A"}\nMô tả: {StripHTML(job.Description)}\nYêu cầu: {StripHTML(job.Requirements)}\nMức lương: {job.Salary}";
            var jdVector = await _aiService.GenerateEmbeddingAsync(jdText);

            if (jdVector == null || jdVector.Length == 0)
                return Json(new { success = false, message = "Lỗi tạo vector cho Job Description." });

            // BƯỚC 3: TÌM KIẾM TƯƠNG ĐỒNG COSINE (RAG FILTERING)
            Console.WriteLine("[SmartMatch RAG] Đang so khớp toán học Cosine Similarity...");
            var candidateScores = new List<(Resume Resume, double Similarity)>();

            foreach (var resume in publicResumes)
            {
                if (embeddingsDict.TryGetValue(resume.ResumeID, out var embeddingRecord) && !string.IsNullOrEmpty(embeddingRecord.VectorJson))
                {
                    try
                    {
                        var cvVector = JsonConvert.DeserializeObject<float[]>(embeddingRecord.VectorJson);
                        if (cvVector != null)
                        {
                            double similarity = CalculateCosineSimilarity(jdVector, cvVector);
                            
                            // [TỐI ƯU 1]: Lọc Vector RAG ngay từ đầu.
                            // Những CV hoàn toàn trái ngành (VD: Dược sĩ, Marketing) sẽ có độ tương đồng Vector rất thấp (< 0.5)
                            // Ta loại bỏ chúng ngay lập tức để tiết kiệm 100% token AI.
                            if (similarity >= 0.50)
                            {
                                candidateScores.Add((resume, similarity));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SmartMatch RAG] Lỗi parse vector CV #{resume.ResumeID}: {ex.Message}");
                    }
                }
            }

            // Chọn Top 10 CV xuất sắc nhất
            var topCandidates = candidateScores
                .OrderByDescending(c => c.Similarity)
                .Take(10)
                .Select(c => c.Resume)
                .ToList();

            Console.WriteLine($"[SmartMatch RAG] Đã lọc ra Top {topCandidates.Count} ứng viên. Bắt đầu gọi LLM chấm điểm song song...");

            var newMatchResults = new List<CVMatchResult>();
            var configData = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
            int userId = CurrentUserId;
            var userInfo = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);
            bool isPro = userInfo?.IsPro ?? false || User.IsInRole("Admin");

            var semaphore = new SemaphoreSlim(isPro ? 3 : 2); // Chạy tối đa 2-3 task cùng lúc để tránh Rate Limit
            var scoringTasks = topCandidates.Select(async (resume, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Thêm một chút delay lệch nhau giữa các task để không gọi API cùng 1 miligiây
                    await Task.Delay(index * (isPro ? 500 : 1000));

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

                        // Log tokens (sử dụng biến cục bộ để tránh xung đột)
                        int estimatedTokens = (prompt.Length / 4) + (aiResult.Length / 4);
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
                analyzedAt = r.AnalyzedAt.ToString("dd/MM/yyyy HH:mm")
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

        /// <summary>Trích xuất text từ JSON CV để gửi cho AI phân tích</summary>
        private string ExtractCVText(Resume resume)
        {
            var sb = new StringBuilder();

            // Thông tin cơ bản
            sb.AppendLine($"Họ tên: {resume.FullName ?? resume.User?.FullName ?? "N/A"}");
            sb.AppendLine($"Vị trí: {resume.JobTitle ?? "N/A"}");
            sb.AppendLine($"Email: {resume.Email ?? resume.User?.Email ?? "N/A"}");

            if (!string.IsNullOrEmpty(resume.Summary))
                sb.AppendLine($"Mục tiêu nghề nghiệp: {resume.Summary}");

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
                                string sectionType = section.type?.ToString() ?? "";
                                string sectionTitle = section.title?.ToString() ?? sectionType;

                                sb.AppendLine($"\n--- {sectionTitle} ---");

                                if (section.items != null)
                                {
                                    foreach (var item in section.items)
                                    {
                                        // Xử lý linh hoạt cho các loại section khác nhau
                                        if (item.company != null) sb.AppendLine($"Công ty: {item.company}");
                                        if (item.position != null) sb.AppendLine($"Vị trí: {item.position}");
                                        if (item.school != null) sb.AppendLine($"Trường: {item.school}");
                                        if (item.degree != null) sb.AppendLine($"Bằng cấp: {item.degree}");
                                        if (item.major != null) sb.AppendLine($"Chuyên ngành: {item.major}");
                                        if (item.name != null) sb.AppendLine($"Tên: {item.name}");
                                        if (item.role != null) sb.AppendLine($"Vai trò: {item.role}");
                                        if (item.description != null) sb.AppendLine($"Mô tả: {StripHTML(item.description.ToString())}"); // [TỐI ƯU 3]: Xóa HTML trong CV
                                        if (item.period != null) sb.AppendLine($"Thời gian: {item.period}");
                                        if (item.startDate != null) sb.AppendLine($"Bắt đầu: {item.startDate}");
                                        if (item.endDate != null) sb.AppendLine($"Kết thúc: {item.endDate}");
                                        if (item.technologies != null) sb.AppendLine($"Công nghệ: {item.technologies}");
                                        sb.AppendLine();
                                    }
                                }
                            }
                        }

                        // Trích xuất skills
                        if (json.skills != null)
                        {
                            sb.AppendLine("\n--- Kỹ năng ---");
                            foreach (var skill in json.skills)
                            {
                                string skillName = skill.name?.ToString() ?? skill.ToString();
                                string level = skill.level?.ToString() ?? "";
                                sb.AppendLine($"- {skillName} {(string.IsNullOrEmpty(level) ? "" : $"({level})")}");
                            }
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
                sb.AppendLine($"\nKỹ năng (Profile): {resume.User.Skills}");

            if (!string.IsNullOrEmpty(resume.User?.Summary))
                sb.AppendLine($"Giới thiệu: {resume.User.Summary}");

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
                // Loại bỏ markdown wrapping nếu có
                string cleaned = aiResult.Trim();
                if (cleaned.StartsWith("```"))
                {
                    cleaned = Regex.Replace(cleaned, @"^```\w*\s*", "");
                    cleaned = Regex.Replace(cleaned, @"\s*```$", "");
                }

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
    }
}
