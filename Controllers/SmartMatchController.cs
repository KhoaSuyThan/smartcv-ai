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
        /// API: Phân tích batch - So khớp tất cả CV công khai với 1 Job Description
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AnalyzeJob(int jobId)
        {
            var job = await _context.Jobs.Include(j => j.Company).FirstOrDefaultAsync(j => j.JobID == jobId);
            if (job == null)
                return Json(new { success = false, message = "Không tìm thấy tin tuyển dụng." });

            // Kiểm tra quyền sở hữu nếu là Recruiter
            if (User.IsInRole("Recruiter"))
            {
                var companyIdClaim = User.FindFirst("CompanyID")?.Value;
                if (companyIdClaim == null || !int.TryParse(companyIdClaim, out int cid) || job.CompanyID != cid)
                    return Json(new { success = false, message = "Bạn không có quyền phân tích tin này." });
            }

            // Lấy CV công khai có nội dung — giới hạn tối đa 10 để tránh quá tải API
            var publicResumes = await _context.Resumes
                .Include(r => r.User)
                .Where(r => r.IsPublic && !string.IsNullOrEmpty(r.JsonContent))
                .OrderByDescending(r => r.UpdatedAt)
                .Take(10)
                .ToListAsync();

            if (!publicResumes.Any())
                return Json(new { success = false, message = "Chưa có ứng viên nào công khai CV." });

            // Xóa kết quả cũ cho job này
            var oldResults = await _context.CVMatchResults.Where(r => r.JobID == jobId).ToListAsync();
            if (oldResults.Any())
            {
                _context.CVMatchResults.RemoveRange(oldResults);
                await _context.SaveChangesAsync();
            }

            // Tạo JD text
            string jdText = $"Vị trí: {job.Title}\nCông ty: {job.Company?.Name ?? "N/A"}\nMô tả: {job.Description}\nYêu cầu: {job.Requirements}\nMức lương: {job.Salary}";

            var results = new List<object>();
            int successCount = 0;

            // Kiểm tra rate limit & cấu hình
            var configData = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
            int userId = CurrentUserId;
            var userInfo = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);
            bool isPro = userInfo?.IsPro ?? false || User.IsInRole("Admin");

            int current = 0;
            foreach (var resume in publicResumes)
            {
                current++;
                try
                {
                    Console.WriteLine($"[SmartMatch] Đang phân tích CV {current}/{publicResumes.Count}: {resume.FullName ?? "N/A"} (ID={resume.ResumeID})");

                    // Trích xuất nội dung CV từ JSON
                    string cvText = ExtractCVText(resume);
                    if (string.IsNullOrWhiteSpace(cvText)) continue;

                    // Tạo prompt phân tích
                    string prompt = BuildMatchPrompt(cvText, jdText);

                    // Gọi AI
                    var aiResult = await _aiService.GenerateContent(prompt, isPro);

                    // Parse JSON kết quả
                    var matchData = ParseMatchResult(aiResult);

                    if (matchData != null)
                    {
                        var matchResult = new CVMatchResult
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

                        _context.CVMatchResults.Add(matchResult);
                        successCount++;

                        // Ghi log AI
                        int estimatedTokens = (prompt.Length / 4) + (aiResult.Length / 4);
                        _context.AILogs.Add(new AILog
                        {
                            UserID = userId,
                            RequestType = "smart_match",
                            InputText = $"Job #{jobId} vs Resume #{resume.ResumeID}",
                            OutputText = aiResult.Length > 500 ? aiResult.Substring(0, 500) + "..." : aiResult,
                            UsedTokens = estimatedTokens,
                            CreatedAt = DateTime.Now
                        });

                        // Cập nhật token tích lũy
                        if (configData != null)
                        {
                            var trackedConfig = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == configData.Id);
                            if (trackedConfig != null) trackedConfig.TotalTokensUsed += estimatedTokens;
                        }
                    }

                    // Delay 2 giây giữa mỗi lần gọi AI để tránh rate limit 429
                    if (current < publicResumes.Count)
                        await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SmartMatch] Error analyzing Resume #{resume.ResumeID}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã phân tích {successCount}/{publicResumes.Count} ứng viên thành công!",
                analyzed = successCount,
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
                                        if (item.description != null) sb.AppendLine($"Mô tả: {item.description}");
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
            return $@"Bạn là hệ thống AI chuyên phân tích và so khớp CV với Job Description cho các công ty tuyển dụng IT tại Việt Nam.

=== MÔ TẢ CÔNG VIỆC (JD) ===
{jdText}

=== CV ỨNG VIÊN ===
{cvText}

YÊU CẦU PHÂN TÍCH CHI TIẾT:
1. matchScore (số nguyên 0-100): Dựa trên kỹ năng kỹ thuật (35%), kinh nghiệm liên quan (25%), trình độ học vấn (20%), kỹ năng mềm (20%).
2. matchedSkills: Mảng các kỹ năng/từ khóa trong CV đã khớp với JD.
3. missingSkills: Mảng các yêu cầu trong JD mà CV chưa có.
4. strengths: Mảng 3 điểm mạnh nổi bật nhất của ứng viên cho vị trí này.
5. suggestions: Mảng 3 gợi ý CỤ THỂ để cải thiện CV nhằm tăng tỷ lệ trúng tuyển.
6. summary: Nhận xét tổng quan 2-3 câu bằng tiếng Việt.
7. recommendation: Một trong bốn giá trị: ""strong_match"", ""good_match"", ""partial_match"", ""weak_match"".

TUYỆT ĐỐI chỉ trả về JSON thuần (KHÔNG có markdown, KHÔNG có ```json, KHÔNG có lời dẫn hay giải thích):
{{""matchScore"": 75, ""matchedSkills"": [""C#"", "".NET""], ""missingSkills"": [""Docker""], ""strengths"": [""Nền tảng kỹ thuật vững""], ""suggestions"": [""Bổ sung Docker""], ""summary"": ""Ứng viên phù hợp..."", ""recommendation"": ""good_match""}}";
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
