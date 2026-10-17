using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using DoAnCS.Services;

namespace DoAnCS.Controllers
{
    [Authorize]
    public class InterviewController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;
        private readonly IOllamaService _ollamaService;

        public InterviewController(AppDbContext context, IAIService aiService, IOllamaService ollamaService)
        {
            _context = context;
            _aiService = aiService;
            _ollamaService = ollamaService;
        }

        /// <summary>
        /// Trích xuất chẩn đoán kỹ năng từ SmartMatch Cache (Data/JobMatchResults/{jobId}.json)
        /// hoặc từ chính nội dung CV nếu chưa từng quét qua Job cụ thể.
        /// </summary>
        private async Task<(List<string> unverifiedSkills, List<string> missingSkills, string seniority, string jobTitle)> ExtractSkillDiagnosticsAsync(Resume resume, int? jobId)
        {
            var unverifiedSkills = new List<string>();
            var missingSkills = new List<string>();
            string seniority = "Mid-level";
            string jobTitle = resume.JobTitle ?? "Kỹ sư phần mềm";

            // 1. Thử lấy từ cache SmartMatch nếu có JobID
            if (jobId.HasValue && jobId.Value > 0)
            {
                try
                {
                    string cachePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "JobMatchResults", $"{jobId.Value}.json");
                    if (System.IO.File.Exists(cachePath))
                    {
                        string jsonContent = await System.IO.File.ReadAllTextAsync(cachePath);
                        using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("jobTitle", out var jtProp) && !string.IsNullOrWhiteSpace(jtProp.GetString()))
                            {
                                jobTitle = jtProp.GetString()!;
                            }

                            if (root.TryGetProperty("candidates", out var candsProp) && candsProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var cand in candsProp.EnumerateArray())
                                {
                                    int cResumeId = 0;
                                    if (cand.TryGetProperty("resumeId", out var rIdProp))
                                    {
                                        cResumeId = rIdProp.ValueKind == JsonValueKind.Number ? rIdProp.GetInt32() : (int.TryParse(rIdProp.GetString(), out int p) ? p : 0);
                                    }

                                    if (cResumeId == resume.ResumeID)
                                    {
                                        if (cand.TryGetProperty("unverifiedSkills", out var unvProp) && unvProp.ValueKind == JsonValueKind.Array)
                                        {
                                            unverifiedSkills = unvProp.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()!;
                                        }
                                        if (cand.TryGetProperty("missingSkills", out var misProp) && misProp.ValueKind == JsonValueKind.Array)
                                        {
                                            missingSkills = misProp.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()!;
                                        }
                                        if (cand.TryGetProperty("seniorityFit", out var senProp) && !string.IsNullOrWhiteSpace(senProp.GetString()))
                                        {
                                            seniority = senProp.GetString()!;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Interview ExtractSkillDiagnostics Cache Error] {ex.Message}");
                }
            }

            // 2. Nếu chưa có unverifiedSkills từ cache, tự động trích xuất từ CV JsonContent
            if (!unverifiedSkills.Any())
            {
                var cvSkills = new List<string>();
                if (!string.IsNullOrEmpty(resume.JsonContent))
                {
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(resume.JsonContent))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("skills", out var skillsProp) && skillsProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var s in skillsProp.EnumerateArray())
                                {
                                    if (s.TryGetProperty("name", out var sName) && !string.IsNullOrWhiteSpace(sName.GetString()))
                                    {
                                        cvSkills.Add(sName.GetString()!.Trim());
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                if (cvSkills.Any())
                {
                    // Các kỹ năng có trong CV cần được phỏng vấn chuyên sâu để kiểm chứng thực tế
                    unverifiedSkills = cvSkills.Take(4).ToList();
                    if (cvSkills.Count > 4)
                    {
                        missingSkills = cvSkills.Skip(4).Take(2).ToList();
                    }
                }
                else
                {
                    // Dự phòng các kỹ năng cốt lõi theo chuyên môn
                    unverifiedSkills = new List<string> { "Xử lý hiệu năng hệ thống", "Kiến trúc ứng dụng", "Giải quyết sự cố kỹ thuật" };
                    missingSkills = new List<string> { "Tối ưu hóa cơ sở dữ liệu", "CI/CD & DevOps" };
                }
            }

            return (unverifiedSkills, missingSkills, seniority, jobTitle);
        }

        #region 1. CHỨC NĂNG LUYỆN TẬP TỰ DO (ỨNG VIÊN)

        // Trang chủ Luyện phỏng vấn tự do: Cho phép chọn CV và chế độ luyện tập
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Practice()
        {
            int userId = CurrentUserId;
            // Chỉ hiện mẫu CV đã có public trong Profile
            var resumes = await _context.Resumes
                .Where(r => r.UserID == userId && r.IsPublic == true)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            // Lấy thông tin user để kiểm tra Pro
            var user = await _context.Users.FindAsync(userId);
            bool isPro = user != null && user.IsPro;
            int maxDailySessions = isPro ? 5 : 3;

            // Đếm số lượt phỏng vấn tự do đã thực hiện trong ngày hôm nay (tổng cả 2 loại Tự luận và Trắc nghiệm)
            var today = DateTime.Today;
            var todaySessionsCount = await _context.InterviewSessions
                .CountAsync(s => s.UserID == userId && 
                                 (s.InterviewType == 0 || s.InterviewType == 1) && 
                                 s.CreatedAt >= today);

            // Lấy lịch sử các phiên phỏng vấn luyện tập
            var history = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Where(s => s.UserID == userId && s.InterviewType != 2)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            // Lấy danh sách các tin tuyển dụng đang mở để ứng viên có thể chọn phỏng vấn theo Job mục tiêu
            var targetJobs = await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.Status == 1)
                .OrderByDescending(j => j.CreatedAt)
                .Take(20)
                .Select(j => new { j.JobID, j.Title, CompanyName = j.Company != null ? j.Company.Name : "Doanh nghiệp tuyển dụng" })
                .ToListAsync();

            ViewBag.Resumes = resumes;
            ViewBag.History = history;
            ViewBag.TodaySessionsCount = todaySessionsCount;
            ViewBag.MaxDailySessions = maxDailySessions;
            ViewBag.IsPro = isPro;
            ViewBag.TargetJobs = targetJobs;
            return View();
        }



        // Bắt đầu một phiên phỏng vấn luyện tập mới
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartPracticeSession(int resumeId, int interviewType, int questionCount, int? jobId = null, string targetMode = "cv", bool enableCloudFallback = false)
        {
            int userId = CurrentUserId;

            // 1. Kiểm tra giới hạn lượt phỏng vấn hàng ngày (Free tối đa 3 lần/ngày, Pro 5 lần/ngày)
            var user = await _context.Users.FindAsync(userId);
            bool isPro = user != null && user.IsPro;
            int maxDailySessions = isPro ? 5 : 3;

            var today = DateTime.Today;
            var todaySessionsCount = await _context.InterviewSessions
                .CountAsync(s => s.UserID == userId && 
                                 (s.InterviewType == 0 || s.InterviewType == 1) && 
                                 s.CreatedAt >= today);

            if (todaySessionsCount >= maxDailySessions)
            {
                TempData["ErrorMessage"] = $"Bạn đã đạt giới hạn tối đa {maxDailySessions} lượt phỏng vấn trong ngày hôm nay. Hãy nâng cấp Pro để nhận thêm lượt hoặc quay lại vào ngày mai.";
                return RedirectToAction(nameof(Practice));
            }

            // 2. Kiểm tra CV có tồn tại và thuộc về user không
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == resumeId && r.UserID == userId);
            if (resume == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV hợp lệ.";
                return RedirectToAction(nameof(Practice));
            }

            // 3. Phân biệt rõ nguồn phỏng vấn: "Theo CV" hay "Theo JD"
            if (targetMode == "cv")
            {
                // Người dùng chọn "Theo CV": Tuyệt đối KHÔNG tự động tìm hay gán JobID, đề thi 100% bám sát hồ sơ
                jobId = null;
            }
            else if (targetMode == "jd")
            {
                // Người dùng chọn "Theo JD": KIỂM SOÁT QUYỀN TRUY CẬP JD
                if (jobId.HasValue && jobId.Value > 0)
                {
                    // Chỉ cho phép nếu: JD công khai đang mở (Status == 1) HOẶC ứng viên đã nộp đơn vào JD đó
                    bool isPublicJob = await _context.Jobs.AnyAsync(j => j.JobID == jobId.Value && j.Status == 1);
                    if (!isPublicJob)
                    {
                        bool isApplied = await _context.Applications.AnyAsync(a => a.JobID == jobId.Value && a.Resume.UserID == userId);
                        if (!isApplied)
                        {
                            jobId = null; // Từ chối truy cập JD nháp, ẩn hoặc không thuộc quyền xem
                        }
                    }
                }
                else
                {
                    // Nếu chưa chọn cụ thể, chỉ tìm JD từ đơn ứng tuyển gần nhất của ứng viên mà JD đó đang công khai
                    jobId = await _context.Applications
                        .Where(a => a.ResumeID == resumeId && a.Resume.UserID == userId && a.Job != null && a.Job.Status == 1)
                        .OrderByDescending(a => a.AppliedAt)
                        .Select(a => (int?)a.JobID)
                        .FirstOrDefaultAsync();
                }
            }

            // 4. Giới hạn số câu hỏi hợp lệ (phòng ngừa Client gửi sai lệch)
            if (interviewType == 0) // STAR
            {
                int minQ = 3;
                int maxQ = isPro ? 10 : 5;
                if (questionCount < minQ || questionCount > maxQ)
                {
                    questionCount = minQ;
                }
            }
            else if (interviewType == 1) // Trắc nghiệm
            {
                int minQ = 5;
                int maxQ = isPro ? 20 : 10;
                if (questionCount < minQ || questionCount > maxQ)
                {
                    questionCount = minQ;
                }
            }

            // Rút gọn thông tin CV theo nguyên tắc Privacy by Design (Bảo vệ dữ liệu ngay từ gốc)
            string cvSummary = "";
            var extractedSkills = new List<string>();

            if (!string.IsNullOrEmpty(resume.JsonContent))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(resume.JsonContent))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("skills", out var skillsProp) && skillsProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in skillsProp.EnumerateArray())
                            {
                                if (item.TryGetProperty("name", out var sName))
                                {
                                    extractedSkills.Add(sName.GetString() ?? "");
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            if (interviewType == 1)
            {
                // CHẾ ĐỘ TRẮC NGHIỆM: Chỉ truyền Vị trí chuyên môn và Danh sách Kỹ năng kỹ thuật
                // Tuyệt đối KHÔNG truyền tiểu sử cá nhân (Summary) và lịch sử công ty cũ (Work Experience) để bảo vệ quyền riêng tư
                cvSummary = $"Vị trí chuyên môn: {resume.JobTitle}. Kỹ năng công nghệ: {string.Join(", ", extractedSkills.Where(s => !string.IsNullOrWhiteSpace(s)))}.";
            }
            else
            {
                // CHẾ ĐỘ CHAT TỰ LUẬN 1-1: Cần thêm bối cảnh dự án để hỏi đào sâu theo chuẩn STAR (chạy trên Local AI)
                cvSummary = $"Vị trí ứng tuyển: {resume.JobTitle}. Giới thiệu: {resume.Summary}.";
                if (!string.IsNullOrEmpty(resume.JsonContent))
                {
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(resume.JsonContent))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("work", out var workProp) && workProp.ValueKind == JsonValueKind.Array)
                            {
                                cvSummary += " Kinh nghiệm làm việc: ";
                                foreach (var item in workProp.EnumerateArray())
                                {
                                    string position = item.TryGetProperty("position", out var pos) ? pos.GetString() : "";
                                    string desc = item.TryGetProperty("summary", out var sName) ? sName.GetString() : "";
                                    cvSummary += $"- Vị trí {position}: {desc}. ";
                                }
                            }
                        }
                    }
                    catch { }
                }
                cvSummary += $" Kỹ năng công nghệ: {string.Join(", ", extractedSkills)}.";
            }

            // Bắt đầu Transaction bảo vệ toàn vẹn dữ liệu: Không bao giờ để lại session dở dang nếu có lỗi
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string jobInfo = "";
                SkillDiagnosticDto? diagnostic = null;

                if (interviewType == 1)
                {
                    if (targetMode == "jd" && jobId.HasValue && jobId.Value > 0)
                    {
                        var targetJob = await _context.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.JobID == jobId.Value);
                        if (targetJob != null)
                        {
                            // CHẾ ĐỘ TRẮC NGHIỆM: Tuyệt đối KHÔNG truyền Description (chứa phần giới thiệu công ty, địa chỉ, phúc lợi...)
                            // Chỉ truyền Vị trí tuyển dụng và Yêu cầu chuyên môn kỹ thuật để bảo đảm Privacy by Design
                            jobInfo = $"Vị trí công việc: {targetJob.Title}. Yêu cầu chuyên môn: {targetJob.Requirements}";
                        }
                    }

                    // GIAI ĐOẠN 2: Chẩn đoán kỹ năng giữa hồ sơ CV và tin tuyển dụng JD
                    diagnostic = DiagnoseSkillGaps(extractedSkills, targetMode == "jd" ? jobInfo : null);
                }

                int timeLimitSec = questionCount * 60; // Chuẩn 1 phút/câu theo yêu cầu (5 câu = 5 phút, 7 câu = 7 phút,...)
                string diagJson = diagnostic != null ? JsonSerializer.Serialize(diagnostic) : "";
                var session = new InterviewSession
                {
                    UserID = userId,
                    JobID = jobId,
                    ResumeID = resumeId,
                    InterviewType = interviewType, // 0: Tự luận chat 1-1, 1: Trắc nghiệm
                    Status = 0, // Đang diễn ra
                    CreatedAt = DateTime.Now,
                    AiEvaluation = interviewType == 1 
                        ? $"TimeLimit:{timeLimitSec}|Deadline:{DateTime.Now.AddSeconds(timeLimitSec):O}|Diagnostic:{diagJson}"
                        : ("MaxQuestions:" + questionCount)
                };

                _context.InterviewSessions.Add(session);
                await _context.SaveChangesAsync();

                if (interviewType == 1)
                {
                    // Gọi động cơ sinh đề Failover (Chạy theo chính sách người dùng đã chọn)
                    var (quizQuestions, aiProvider) = await GenerateQuizQuestionsWithFailoverAsync(cvSummary, questionCount, jobInfo, enableCloudFallback: enableCloudFallback, diagnostic: diagnostic);

                    int index = 1;
                    foreach (var q in quizQuestions)
                    {
                        // Đóng gói Explanation và Topic vào cột Feedback dưới dạng JSON có version (không sửa schema DB)
                        string feedbackJson = JsonSerializer.Serialize(new QuizFeedbackMetadata
                        {
                            V = 1,
                            Topic = !string.IsNullOrWhiteSpace(q.Topic) ? q.Topic.Trim() : "Kiến thức chuyên môn",
                            Explanation = !string.IsNullOrWhiteSpace(q.Explanation) ? q.Explanation.Trim() : "Đáp án đúng theo chuẩn kỹ thuật chuyên ngành."
                        });

                        var msg = new InterviewMessage
                        {
                            SessionID = session.SessionID,
                            Role = "interviewer",
                            Content = q.Question.Trim(),
                            ChoicesJson = JsonSerializer.Serialize(q.Options),
                            SelectedAnswer = q.CorrectAnswer.Trim().ToUpper(), // Lưu đáp án đúng phía server (được bảo vệ, không gửi xuống client khi làm bài)
                            Feedback = feedbackJson,
                            AiProvider = aiProvider,
                            CreatedAt = DateTime.Now.AddSeconds(index)
                        };
                        _context.InterviewMessages.Add(msg);
                        index++;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(PracticeQuiz), new { sessionId = session.SessionID });
                }
                else
                {
                    // CHẾ ĐỘ CHAT TỰ LUẬN 1-1: Phỏng vấn chuyên sâu chuẩn STAR với Local AI (Qwen 2.5 3B)
                    var (unverifiedSkills, missingSkills, seniority, matchedJobTitle) = await ExtractSkillDiagnosticsAsync(resume, jobId);

                    // Tổng hợp danh sách kỹ năng mục tiêu (ưu tiên unverified -> missing -> technical skills)
                    var targetSkillsList = new List<string>();
                    targetSkillsList.AddRange(unverifiedSkills);
                    targetSkillsList.AddRange(missingSkills);
                    if (!targetSkillsList.Any())
                    {
                        targetSkillsList.Add("Kiến trúc ứng dụng và giải quyết bài toán kỹ thuật");
                    }

                    // Cập nhật cấu hình phiên vào AiEvaluation: MaxQuestions:X|Job:Title|Skills:Skill1,Skill2,...
                    session.AiEvaluation = $"MaxQuestions:{questionCount}|Job:{matchedJobTitle}|Skills:{string.Join(",", targetSkillsList)}";
                    await _context.SaveChangesAsync();

                    string primaryTarget = targetSkillsList.First();

                    string leadTechPrompt = $@"Bạn là Tech Lead phỏng vấn ứng viên cho vị trí {matchedJobTitle} (Cấp bậc: {seniority}).
Hồ sơ chẩn đoán kỹ năng của ứng viên:
- Kỹ năng cần kiểm chứng thực tế: {(unverifiedSkills.Any() ? string.Join(", ", unverifiedSkills) : "Chưa có bằng chứng dự án rõ ràng")}
- Lỗ hổng kỹ năng so với yêu cầu: {(missingSkills.Any() ? string.Join(", ", missingSkills) : "Cần hoàn thiện thêm")}

Nhiệm vụ: Hãy đặt MỘT câu hỏi tình huống kỹ thuật thực tế số 1/{questionCount} tập trung kiểm tra kỹ năng [{primaryTarget}].
Yêu cầu bắt buộc:
1. Đặt một câu hỏi tình huống thực tế mà kỹ sư thường gặp khi làm việc với [{primaryTarget}] và hỏi cách ứng viên giải quyết.
2. Bạn là người phỏng vấn. Tuyệt đối KHÔNG tự trả lời, KHÔNG viết bài luận mẫu, KHÔNG dùng các từ 'SITUATION', 'SITTING', 'TASK', 'ACTION', 'RESULT'.
3. Giọng điệu chuyên nghiệp, khắt khe, đi thẳng vào bản chất kỹ thuật, không vòng vo.
4. CHỈ TRẢ VỀ DUY NHẤT nội dung câu hỏi phỏng vấn bằng tiếng Việt (không thêm bất kỳ lời chào, lời dẫn hay ký tự phụ bên ngoài).";

                    string firstQuestion = "";
                    string aiProvider = "local_qwen_3b";

                    try
                    {
                        // 1. Thử gọi Local AI (Ollama Qwen 2.5 3B)
                        firstQuestion = await _ollamaService.GenerateTextAsync(leadTechPrompt);
                        if (string.IsNullOrWhiteSpace(firstQuestion))
                        {
                            throw new Exception("Local AI trả về rỗng.");
                        }
                        firstQuestion = CleanAiQuestionContent(firstQuestion);
                    }
                    catch (Exception ollamaEx)
                    {
                        // 2. Fallback sang Cloud Gemini AI nếu Ollama offline hoặc timeout
                        Console.WriteLine($"[Interview Local AI Fallback to Cloud] {ollamaEx.Message}");
                        try
                        {
                            firstQuestion = await _aiService.GenerateContent(leadTechPrompt);
                            firstQuestion = CleanAiQuestionContent(firstQuestion);
                            aiProvider = "cloud_gemini";
                        }
                        catch (Exception)
                        {
                            firstQuestion = $"Dựa trên hồ sơ của bạn với vị trí {matchedJobTitle}, bạn có thể chia sẻ một tình huống thực tế khó khăn nhất khi làm việc với {primaryTarget} mà bạn đã trực tiếp giải quyết không? Hãy nêu rõ bối cảnh, nhiệm vụ, giải pháp kỹ thuật cụ thể và kết quả đo lường đạt được.";
                            aiProvider = "system_template";
                        }
                    }

                    string skillType = unverifiedSkills.Contains(primaryTarget) ? "unverified" : (missingSkills.Contains(primaryTarget) ? "missing" : "technical");
                    string formattedFirstQuestion = $"[TARGET_SKILL:{primaryTarget}|TYPE:{skillType}]\n{firstQuestion}\n\n💡 Gợi ý trả lời theo STAR: Nêu rõ Bối cảnh (S), Nhiệm vụ (T), Hành động kỹ thuật đã làm (A), và Kết quả đo lường (R).";

                    var msg = new InterviewMessage
                    {
                        SessionID = session.SessionID,
                        Role = "interviewer",
                        Content = formattedFirstQuestion,
                        AiProvider = aiProvider,
                        CreatedAt = DateTime.Now
                    };
                    _context.InterviewMessages.Add(msg);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(PracticeChat), new { sessionId = session.SessionID });
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Không thể khởi tạo phiên phỏng vấn lúc này: " + ex.Message;
                return RedirectToAction(nameof(Practice));
            }
        }

        // Màn hình chat tự luận tương tác 1-1
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PracticeChat(int sessionId)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId);

            if (session == null)
            {
                return NotFound("Không tìm thấy phiên phỏng vấn hợp lệ.");
            }

            // Sắp xếp các tin nhắn theo thời gian gửi tăng dần
            session.Messages = session.Messages.OrderBy(m => m.CreatedAt).ToList();

            // Cơ chế Pre-warm / Pre-load Model:
            // Ngay khi người dùng truy cập vào phòng phỏng vấn, nạp trước model Qwen 2.5 vào GPU VRAM (nền ngầm)
            // Giữ model trong 5 phút để khi người dùng nhấn Gửi, AI phản hồi ngay lập tức không bị trễ nạp model
            if (session.Status == 0)
            {
                _ = Task.Run(async () => await _ollamaService.PreloadModelAsync("5m"));
            }
            else
            {
                // Nếu phiên đã hoàn thành, giải phóng model khỏi GPU VRAM để tiết kiệm tài nguyên
                _ = Task.Run(async () => await _ollamaService.UnloadModelAsync());
            }

            return View(session);
        }

        // Endpoint giải phóng AI khi người dùng thoát ra khỏi link phòng phỏng vấn
        [HttpPost]
        [Authorize(Roles = "User")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReleaseInterviewAi()
        {
            _ = Task.Run(async () => await _ollamaService.UnloadModelAsync());
            return Json(new { success = true });
        }

        // Gửi câu trả lời trong chat tự luận 1-1
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChatAnswer(int sessionId, string answer)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId);

            if (session == null || session.Status == 1)
            {
                return BadRequest("Phiên phỏng vấn không hợp lệ hoặc đã kết thúc.");
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                TempData["WarningMessage"] = "Vui lòng nhập câu trả lời của bạn.";
                return RedirectToAction(nameof(PracticeChat), new { sessionId = sessionId });
            }

            // Chống gửi lặp đúp (Double Submission) trong vòng 5 giây
            var recentCandidateMsg = session.Messages
                .Where(m => m.Role == "candidate")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (recentCandidateMsg != null && 
                recentCandidateMsg.Content == answer.Trim() && 
                (DateTime.Now - recentCandidateMsg.CreatedAt).TotalSeconds < 5)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, isCompleted = session.Status == 1, duplicate = true });
                }
                return RedirectToAction(nameof(PracticeChat), new { sessionId = sessionId });
            }

            // 1. Lưu câu trả lời của ứng viên
            var candidateMsg = new InterviewMessage
            {
                SessionID = sessionId,
                Role = "candidate",
                Content = answer.Trim(),
                CreatedAt = DateTime.Now
            };
            _context.InterviewMessages.Add(candidateMsg);
            await _context.SaveChangesAsync();

            // Lấy câu hỏi tương ứng vừa đặt ra trước đó để chấm điểm
            var lastQuestionMsg = session.Messages
                .Where(m => m.Role == "interviewer")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            string lastQuestion = lastQuestionMsg != null ? lastQuestionMsg.Content : "Hãy chia sẻ kinh nghiệm dự án của bạn.";
            string cleanQuestion = lastQuestion;
            if (cleanQuestion.Contains("]\n"))
            {
                int endTagIdx = cleanQuestion.LastIndexOf("]\n");
                cleanQuestion = cleanQuestion.Substring(endTagIdx + 2).Trim();
            }
            if (cleanQuestion.Contains("\n\n💡"))
            {
                int hintIdx = cleanQuestion.IndexOf("\n\n💡");
                cleanQuestion = cleanQuestion.Substring(0, hintIdx).Trim();
            }

            string evalJobTitle = session.Resume?.JobTitle ?? "Kỹ sư phần mềm";
            if (!string.IsNullOrEmpty(session.AiEvaluation))
            {
                var parts = session.AiEvaluation.Split('|');
                foreach (var part in parts)
                {
                    if (part.StartsWith("Job:"))
                    {
                        evalJobTitle = part.Substring("Job:".Length);
                        break;
                    }
                }
            }

            // 2. Chấm điểm định lượng STAR 4 trụ cột và English Polishing qua Local AI (Qwen 2.5 3B) JSON Mode
            string evaluationPrompt = $@"Bạn là Giám khảo Phỏng vấn Kỹ thuật Cấp cao (Principal Tech Lead) chuyên chấm điểm theo phương pháp STAR.
Hãy đánh giá câu trả lời của ứng viên cho câu hỏi kỹ thuật dưới đây.

Ngữ cảnh:
- Vị trí: {evalJobTitle}
- Câu hỏi của bạn: ""{cleanQuestion}""
- Câu trả lời của ứng viên: ""{answer}""

TIÊU CHÍ CHẤM ĐIỂM ĐỊNH LƯỢNG STAR (Tổng 100 điểm):
1. Situation (Tình huống / Bối cảnh dự án) [0 - 20 điểm]: Bối cảnh bài toán, quy mô hoặc khó khăn kỹ thuật gặp phải.
2. Task (Nhiệm vụ / Trách nhiệm) [0 - 20 điểm]: Mục tiêu kỹ thuật cần đạt và vai trò của ứng viên.
3. Action (Hành động kỹ thuật thực chiến) [0 - 40 điểm]: Kiến trúc, thuật toán, cách viết code/cấu hình cụ thể và xử lý lỗi (TRỌNG SỐ QUAN TRỌNG NHẤT).
4. Result (Kết quả & Đo lường) [0 - 20 điểm]: Hiệu năng đạt được, số liệu đo lường cụ thể (ms, %, RPS, dung lượng) và bài học kinh nghiệm.

YÊU CẦU KIỂM TRA SỐ LIỆU (Metric Verification):
- Kiểm tra xem câu trả lời có chứa con số đo lường định lượng thực tế hay không (ví dụ: giảm 40% latency, tăng 2x RPS, giảm 30% RAM). Nếu có số liệu cụ thể đặt hasQuantifiedMetrics = true, nếu chỉ nói chung chung đặt hasQuantifiedMetrics = false.

YÊU CẦU ENGLISH POLISHING:
- Dù ứng viên trả lời bằng Tiếng Việt hay Tiếng Anh, hãy viết lại câu trả lời thành 2 - 3 câu TIẾNG ANH KỸ THUẬT CHUYÊN NGHIỆP CẤP ĐỘ SENIOR cô đọng, súc tích (khoảng 60 - 90 từ).
- Trích xuất 2 - 4 cụm từ tiếng Anh kỹ thuật đắt giá (powerPhrases) kèm giải nghĩa tiếng Việt.

QUY TẮC BẮT BUỘC:
- TUYỆT ĐỐI KHÔNG sao chép số 84 hay các văn bản mẫu trong schema dưới đây.
- Bắt buộc phải đọc kỹ câu trả lời của ứng viên, tự tính toán điểm số chính xác theo từng trụ cột và viết nhận xét phân tích mới 100% dựa trên câu trả lời thực tế.
- Mỗi mục nhận xét trong breakdownFeedback viết ngắn gọn 1 - 2 câu súc tích đi thẳng vào bản chất kỹ thuật.

BẮT BUỘC TRẢ VỀ DUY NHẤT 1 CHUỖI JSON HỢP LỆ THEO ĐÚNG SCHEMA CẤU TRÚC SAU:
{{
  ""situationScore"": 0,
  ""taskScore"": 0,
  ""actionScore"": 0,
  ""resultScore"": 0,
  ""totalScore"": 0,
  ""hasQuantifiedMetrics"": false,
  ""breakdownFeedback"": {{
    ""situation"": ""Đánh giá ngắn gọn bối cảnh..."",
    ""task"": ""Đánh giá ngắn gọn nhiệm vụ..."",
    ""action"": ""Đánh giá ngắn gọn hành động kỹ thuật..."",
    ""result"": ""Đánh giá ngắn gọn kết quả đo lường...""
  }},
  ""generalFeedback"": ""Nhận xét tổng quan ưu điểm và điểm cần cải thiện của câu trả lời này..."",
  ""englishPolish"": {{
    ""detectedLanguage"": ""vi"",
    ""improvedResponse"": ""Câu trả lời viết lại bằng tiếng Anh chuyên ngành Senior (60 - 90 từ)..."",
    ""powerPhrases"": [
      {{ ""phrase"": ""cụm từ tiếng Anh chuyên ngành"", ""meaning"": ""nghĩa tiếng Việt"" }}
    ]
  }}
}}";

            int situationScore = 15;
            int taskScore = 15;
            int actionScore = 28;
            int resultScore = 12;
            int totalScore = 70;
            string generalFeedback = "Đã ghi nhận câu trả lời kỹ thuật của bạn.";
            string englishFeedbackJson = "{}";
            string aiEvalProvider = "local_qwen_3b";

            try
            {
                // 1. Thử gọi Local AI (Ollama Qwen 2.5 3B) JSON Mode
                string evalResponse = await _ollamaService.GenerateJsonAsync(evaluationPrompt);
                if (string.IsNullOrWhiteSpace(evalResponse)) throw new Exception("Local AI JSON rỗng");

                bool parsedSuccess = false;

                // Thử parse qua JsonDocument thông thường
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(evalResponse))
                    {
                        var root = doc.RootElement;
                        situationScore = root.TryGetProperty("situationScore", out var sProp) ? sProp.GetInt32() : 15;
                        taskScore = root.TryGetProperty("taskScore", out var tProp) ? tProp.GetInt32() : 15;
                        actionScore = root.TryGetProperty("actionScore", out var aProp) ? aProp.GetInt32() : 28;
                        resultScore = root.TryGetProperty("resultScore", out var rProp) ? rProp.GetInt32() : 12;

                        if (root.TryGetProperty("generalFeedback", out var fbProp) && !string.IsNullOrWhiteSpace(fbProp.GetString()))
                        {
                            generalFeedback = fbProp.GetString()!;
                        }
                        else if (root.TryGetProperty("feedback", out var fbProp2) && !string.IsNullOrWhiteSpace(fbProp2.GetString()))
                        {
                            generalFeedback = fbProp2.GetString()!;
                        }

                        englishFeedbackJson = evalResponse;
                        parsedSuccess = true;
                    }
                }
                catch (Exception jsonEx)
                {
                    Console.WriteLine($"[JSON Parse Soft Fail - Trích xuất cứu hộ Regex] {jsonEx.Message}");
                }

                // Nếu JsonDocument bị cắt cụt do chạm trần token, dùng Regex trích xuất cứu hộ trực tiếp từ text
                if (!parsedSuccess)
                {
                    var sMatch = Regex.Match(evalResponse, @"\""situationScore\""\s*:\s*(\d+)");
                    if (sMatch.Success) int.TryParse(sMatch.Groups[1].Value, out situationScore);

                    var tMatch = Regex.Match(evalResponse, @"\""taskScore\""\s*:\s*(\d+)");
                    if (tMatch.Success) int.TryParse(tMatch.Groups[1].Value, out taskScore);

                    var aMatch = Regex.Match(evalResponse, @"\""actionScore\""\s*:\s*(\d+)");
                    if (aMatch.Success) int.TryParse(aMatch.Groups[1].Value, out actionScore);

                    var rMatch = Regex.Match(evalResponse, @"\""resultScore\""\s*:\s*(\d+)");
                    if (rMatch.Success) int.TryParse(rMatch.Groups[1].Value, out resultScore);

                    var fbMatch = Regex.Match(evalResponse, @"\""generalFeedback\""\s*:\s*\""([^\""]+)\""");
                    if (fbMatch.Success) generalFeedback = fbMatch.Groups[1].Value;

                    englishFeedbackJson = evalResponse;
                }

                // Chuẩn hóa và cộng dồn điểm số
                situationScore = Math.Clamp(situationScore, 0, 20);
                taskScore = Math.Clamp(taskScore, 0, 20);
                actionScore = Math.Clamp(actionScore, 0, 40);
                resultScore = Math.Clamp(resultScore, 0, 20);
                totalScore = situationScore + taskScore + actionScore + resultScore;
            }
            catch (Exception localEx)
            {
                Console.WriteLine($"[Interview Evaluation Local AI Fallback] {localEx.Message}");
                try
                {
                    // 2. Fallback sang Cloud Gemini AI nếu Ollama gặp sự cố
                    string cloudEval = await _aiService.GenerateContent(evaluationPrompt, responseJson: true);
                    cloudEval = CleanJson(cloudEval);
                    aiEvalProvider = "cloud_gemini";

                    using (JsonDocument doc = JsonDocument.Parse(cloudEval))
                    {
                        var root = doc.RootElement;
                        situationScore = root.TryGetProperty("situationScore", out var sProp) ? sProp.GetInt32() : 15;
                        taskScore = root.TryGetProperty("taskScore", out var tProp) ? tProp.GetInt32() : 15;
                        actionScore = root.TryGetProperty("actionScore", out var aProp) ? aProp.GetInt32() : 28;
                        resultScore = root.TryGetProperty("resultScore", out var rProp) ? rProp.GetInt32() : 12;

                        situationScore = Math.Clamp(situationScore, 0, 20);
                        taskScore = Math.Clamp(taskScore, 0, 20);
                        actionScore = Math.Clamp(actionScore, 0, 40);
                        resultScore = Math.Clamp(resultScore, 0, 20);
                        totalScore = situationScore + taskScore + actionScore + resultScore;

                        if (root.TryGetProperty("generalFeedback", out var fbProp) && !string.IsNullOrWhiteSpace(fbProp.GetString()))
                        {
                            generalFeedback = fbProp.GetString()!;
                        }
                        englishFeedbackJson = cloudEval;
                    }
                }
                catch
                {
                    aiEvalProvider = "system_template";
                    totalScore = 70;
                }
            }

            candidateMsg.StarSituationScore = situationScore;
            candidateMsg.StarTaskScore = taskScore;
            candidateMsg.StarActionScore = actionScore;
            candidateMsg.StarResultScore = resultScore;
            candidateMsg.Score = totalScore;
            candidateMsg.Feedback = generalFeedback;
            candidateMsg.EnglishFeedback = englishFeedbackJson;
            candidateMsg.AiProvider = aiEvalProvider;
            await _context.SaveChangesAsync();

            // Đọc số câu hỏi cấu hình từ trường AiEvaluation tạm thời
            int maxQuestions = 3; // Mặc định là 3
            if (!string.IsNullOrEmpty(session.AiEvaluation) && session.AiEvaluation.StartsWith("MaxQuestions:"))
            {
                string maxPart = session.AiEvaluation.Split('|')[0];
                if (int.TryParse(maxPart.Substring("MaxQuestions:".Length), out int parsedMax))
                {
                    maxQuestions = parsedMax;
                }
            }

            // Tính số lượng câu trả lời đã gửi trong phiên này
            int answerCount = session.Messages.Count(m => m.Role == "candidate");

            if (answerCount >= maxQuestions) // Kết thúc phỏng vấn sau số lượng câu hỏi đã chọn
            {
                session.Status = 1; // Đã hoàn thành
                session.CompletedAt = DateTime.Now;

                // Tính điểm trung bình cộng 4 trụ cột STAR
                var candidateMsgs = await _context.InterviewMessages
                    .Where(m => m.SessionID == sessionId && m.Role == "candidate")
                    .ToListAsync();
                
                double avgOverall = candidateMsgs.Any() ? candidateMsgs.Average(m => m.Score ?? 0) : 0;
                double avgS = candidateMsgs.Any() ? candidateMsgs.Average(m => m.StarSituationScore ?? 0) : 0;
                double avgT = candidateMsgs.Any() ? candidateMsgs.Average(m => m.StarTaskScore ?? 0) : 0;
                double avgA = candidateMsgs.Any() ? candidateMsgs.Average(m => m.StarActionScore ?? 0) : 0;
                double avgR = candidateMsgs.Any() ? candidateMsgs.Average(m => m.StarResultScore ?? 0) : 0;

                session.OverallScore = (int)Math.Round(avgOverall);

                // Tạo nhận xét tổng quan toàn diện từ AI
                string summaryPrompt = $@"Bạn là Giám khảo Phỏng vấn Cấp cao. Hãy tổng kết toàn diện buổi phỏng vấn vị trí {evalJobTitle} của ứng viên.
Dữ liệu điểm STAR trung bình đạt được:
- Situation (Bối cảnh): {avgS:F1}/20 điểm
- Task (Nhiệm vụ): {avgT:F1}/20 điểm
- Action (Hành động kỹ thuật): {avgA:F1}/40 điểm
- Result (Kết quả & Đo lường): {avgR:F1}/20 điểm
- Tổng điểm chung cuộc: {avgOverall:F1}/100 điểm

Nội dung các câu hỏi & trả lời:
" + string.Join("\n", candidateMsgs.Select((m, i) => $"Câu {i+1} (S:{m.StarSituationScore}, T:{m.StarTaskScore}, A:{m.StarActionScore}, R:{m.StarResultScore} | Tổng: {m.Score}đ): {m.Content}")) + @"

Yêu cầu nhận xét:
1. Đánh giá thế mạnh kỹ thuật cốt lõi (Core Strengths).
2. Chỉ rõ điểm yếu trong cấu trúc trả lời hoặc lỗ hổng chuyên môn cần cải thiện.
3. Nhận xét về độ thâm niên thực tế (Junior / Middle / Senior fit).
4. Lời khuyên cụ thể để ứng viên chuẩn bị cho các đợt phỏng vấn thật.
QUY TẮC: Viết nhận xét cô đọng, súc tích trong khoảng 150 - 200 từ, đi thẳng vào bản chất kỹ thuật, không viết dài dòng.";

                string finalEvaluation = "Hoàn thành buổi phỏng vấn.";
                try
                {
                    finalEvaluation = await _ollamaService.GenerateTextAsync(summaryPrompt);
                    if (string.IsNullOrWhiteSpace(finalEvaluation)) throw new Exception("Rỗng");
                    finalEvaluation = finalEvaluation.Trim();
                }
                catch
                {
                    try
                    {
                        finalEvaluation = await _aiService.GenerateContent(summaryPrompt);
                        finalEvaluation = finalEvaluation.Trim();
                    }
                    catch
                    {
                        finalEvaluation = $"Ứng viên hoàn thành {candidateMsgs.Count} câu hỏi phỏng vấn với điểm trung bình {session.OverallScore}/100. Điểm thành phần STAR: S: {avgS:F1}/20, T: {avgT:F1}/20, A: {avgA:F1}/40, R: {avgR:F1}/20.";
                    }
                }

                session.AiEvaluation = finalEvaluation;
                await _context.SaveChangesAsync();

                // Giải phóng GPU VRAM ngay lập tức vì buổi phỏng vấn đã hoàn tất
                _ = Task.Run(async () => await _ollamaService.UnloadModelAsync());

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, isCompleted = true });
                }

                TempData["SuccessMessage"] = "Bạn đã hoàn thành buổi phỏng vấn luyện tập!";
                return RedirectToAction(nameof(PracticeChat), new { sessionId = sessionId });
            }
            else
            {
                // Sinh câu hỏi tiếp theo dựa trên danh sách lỗ hổng kỹ năng (SmartMatch Diagnostics)
                string jobTitle = session.Resume?.JobTitle ?? "Kỹ sư phần mềm";
                var targetSkillsList = new List<string>();

                // Phân tích cấu hình từ AiEvaluation: MaxQuestions:X|Job:Title|Skills:Skill1,Skill2,...
                if (!string.IsNullOrEmpty(session.AiEvaluation))
                {
                    var parts = session.AiEvaluation.Split('|');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("Job:"))
                        {
                            jobTitle = part.Substring("Job:".Length);
                        }
                        else if (part.StartsWith("Skills:"))
                        {
                            targetSkillsList = part.Substring("Skills:".Length).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                    }
                }

                // Nhận diện kỹ năng hiện tại từ câu hỏi trước đó
                string currentTargetSkill = "Kỹ năng chuyên môn";
                if (lastQuestionMsg != null && lastQuestionMsg.Content.Contains("[TARGET_SKILL:"))
                {
                    int sIdx = lastQuestionMsg.Content.IndexOf("[TARGET_SKILL:") + "[TARGET_SKILL:".Length;
                    int eIdx = lastQuestionMsg.Content.IndexOf("|", sIdx);
                    if (eIdx > sIdx)
                    {
                        currentTargetSkill = lastQuestionMsg.Content.Substring(sIdx, eIdx - sIdx).Trim();
                    }
                }

                int wordCount = answer.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                bool isLastQuestionDrillDown = lastQuestionMsg != null && lastQuestionMsg.Content.Contains("[IS_DRILLDOWN]");

                // Cơ chế Hỏi dồn (Drill-down Follow-up) nếu ứng viên trả lời quá ngắn (< 20 từ) và chưa từng hỏi dồn câu trước
                if (wordCount < 20 && !isLastQuestionDrillDown && answerCount < maxQuestions)
                {
                    string drillDownPrompt = $@"Bạn là Tech Lead kiêm Trưởng ban phỏng vấn kỹ thuật cấp cao cho vị trí {jobTitle}.
Ứng viên vừa trả lời rất ngắn gọn ({wordCount} từ) cho câu hỏi về [{currentTargetSkill}].
Câu hỏi trước: ""{lastQuestion}""
Câu trả lời của ứng viên: ""{answer}""

Nhiệm vụ: Hãy đặt một câu hỏi đào sâu kỹ thuật (Follow-up drill-down) để xoáy tiếp vào kỹ năng [{currentTargetSkill}].
Yêu cầu:
1. Nhận xét ngắn gọn rằng câu trả lời chưa làm rõ chi tiết kỹ thuật thực tế (Action) và kết quả (Result).
2. Yêu cầu ứng viên đi thẳng vào giải pháp kỹ thuật cụ thể: đã cấu hình/viết code ra sao, giải quyết bài toán/lỗi nào và kết quả đo lường được là gì.
3. Giọng điệu Tech Lead nghiêm khắc, đi thẳng vào bản chất, không vòng vo.
4. CHỈ TRẢ VỀ DUY NHẤT nội dung câu hỏi đào sâu bằng tiếng Việt.";

                    string drillDownQuestion = "";
                    string drillDownProvider = "local_qwen_3b";

                    try
                    {
                        drillDownQuestion = await _ollamaService.GenerateTextAsync(drillDownPrompt);
                        if (string.IsNullOrWhiteSpace(drillDownQuestion)) throw new Exception("Rỗng");
                        drillDownQuestion = drillDownQuestion.Trim();
                    }
                    catch
                    {
                        try
                        {
                            drillDownQuestion = await _aiService.GenerateContent(drillDownPrompt);
                            drillDownQuestion = drillDownQuestion.Trim();
                            drillDownProvider = "cloud_gemini";
                        }
                        catch
                        {
                            drillDownQuestion = $"Câu trả lời của bạn về {currentTargetSkill} còn khá ngắn và chưa cụ thể. Bạn có thể giải thích chi tiết hơn về các bước kỹ thuật (Action) bạn đã thực hiện và kết quả (Result) cụ thể đạt được không?";
                            drillDownProvider = "system_template";
                        }
                    }

                    string formattedDrillDown = $"[TARGET_SKILL:{currentTargetSkill}|TYPE:drilldown][IS_DRILLDOWN]\n{drillDownQuestion}\n\n💡 Nhắc nhở: Câu trả lời trước khá ngắn. Hãy trình bày rõ Hành động kỹ thuật (Action) và Kết quả (Result) đo lường được theo chuẩn STAR.";

                    var drillDownMsg = new InterviewMessage
                    {
                        SessionID = sessionId,
                        Role = "interviewer",
                        Content = formattedDrillDown,
                        AiProvider = drillDownProvider,
                        CreatedAt = DateTime.Now
                    };
                    _context.InterviewMessages.Add(drillDownMsg);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, isCompleted = false });
                    }

                    return RedirectToAction(nameof(PracticeChat), new { sessionId = sessionId });
                }

                // Chọn kỹ năng tiếp theo theo vòng xoay vòng
                string nextTargetSkill = "Xử lý bài toán kỹ thuật chuyên sâu";
                if (targetSkillsList.Any())
                {
                    int skillIndex = answerCount % targetSkillsList.Count;
                    nextTargetSkill = targetSkillsList[skillIndex];
                }

                string cleanLastQuestion = lastQuestion;
                if (cleanLastQuestion.Contains("]\n"))
                {
                    int endTagIdx = cleanLastQuestion.LastIndexOf("]\n");
                    cleanLastQuestion = cleanLastQuestion.Substring(endTagIdx + 2).Trim();
                }
                if (cleanLastQuestion.Contains("\n\n💡"))
                {
                    int hintIdx = cleanLastQuestion.IndexOf("\n\n💡");
                    cleanLastQuestion = cleanLastQuestion.Substring(0, hintIdx).Trim();
                }
                if (cleanLastQuestion.Length > 250)
                {
                    cleanLastQuestion = cleanLastQuestion.Substring(0, 250) + "...";
                }

                string nextQuestionPrompt = $@"Bạn là Tech Lead phỏng vấn ứng viên cho vị trí {jobTitle}.
Nhiệm vụ: Hãy đặt MỘT câu hỏi tình huống kỹ thuật thực tế số {answerCount + 1}/{maxQuestions} để kiểm tra kỹ năng [{nextTargetSkill}].
Yêu cầu bắt buộc:
1. Đặt một câu hỏi tình huống thực tế mà kỹ sư thường gặp khi làm việc với [{nextTargetSkill}] và hỏi cách ứng viên giải quyết.
2. Bạn là người phỏng vấn. Tuyệt đối KHÔNG tự trả lời, KHÔNG viết bài luận mẫu, KHÔNG dùng các từ 'SITUATION', 'SITTING', 'TASK', 'ACTION', 'RESULT', KHÔNG lặp lại câu hỏi trước.
3. Giọng điệu chuyên nghiệp, khắt khe, đi thẳng vào bản chất kỹ thuật, không vòng vo.
4. CHỈ TRẢ VỀ DUY NHẤT nội dung câu hỏi phỏng vấn bằng tiếng Việt (không thêm bất kỳ lời chào, lời dẫn hay ký tự phụ bên ngoài).";

                string nextQuestion = "";
                string aiProvider = "local_qwen_3b";

                try
                {
                    // 1. Thử gọi Local AI (Ollama Qwen 2.5 3B)
                    nextQuestion = await _ollamaService.GenerateTextAsync(nextQuestionPrompt);
                    if (string.IsNullOrWhiteSpace(nextQuestion))
                    {
                        throw new Exception("Local AI trả về rỗng.");
                    }
                    nextQuestion = CleanAiQuestionContent(nextQuestion);
                }
                catch (Exception ollamaEx)
                {
                    // 2. Fallback sang Cloud Gemini AI
                    Console.WriteLine($"[Interview Next Question Local AI Fallback] {ollamaEx.Message}");
                    try
                    {
                        nextQuestion = await _aiService.GenerateContent(nextQuestionPrompt);
                        nextQuestion = CleanAiQuestionContent(nextQuestion);
                        aiProvider = "cloud_gemini";
                    }
                    catch
                    {
                        nextQuestion = $"Hãy chia sẻ một tình huống thực tế khác liên quan đến [{nextTargetSkill}] mà bạn đã từng gặp phải, và cách bạn giải quyết nó?";
                        aiProvider = "system_template";
                    }
                }

                string formattedNextQuestion = $"[TARGET_SKILL:{nextTargetSkill}|TYPE:target]\n{nextQuestion}\n\n💡 Gợi ý trả lời theo STAR: Nêu rõ Bối cảnh (S), Nhiệm vụ (T), Hành động kỹ thuật đã làm (A), và Kết quả đo lường (R).";

                var nextMsg = new InterviewMessage
                {
                    SessionID = sessionId,
                    Role = "interviewer",
                    Content = formattedNextQuestion,
                    AiProvider = aiProvider,
                    CreatedAt = DateTime.Now
                };
                _context.InterviewMessages.Add(nextMsg);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, isCompleted = false });
                }

                return RedirectToAction(nameof(PracticeChat), new { sessionId = sessionId });
            }
        }

        // Màn hình làm bài thi trắc nghiệm luyện tập (Bảo mật đáp án: không gửi đáp án đúng xuống client khi chưa nộp bài)
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PracticeQuiz(int sessionId)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Include(s => s.Job)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId);

            if (session == null || session.InterviewType != 1)
            {
                return NotFound("Không tìm thấy phiên trắc nghiệm hợp lệ.");
            }

            var interviewerMsgs = session.Messages.Where(m => m.Role == "interviewer").OrderBy(m => m.CreatedAt).ToList();
            var candidateMsgs = session.Messages.Where(m => m.Role == "candidate").OrderBy(m => m.CreatedAt).ToList();

            DateTime? parsedDeadline = null;
            if (!string.IsNullOrEmpty(session.AiEvaluation) && session.AiEvaluation.Contains("Deadline:"))
            {
                try
                {
                    int dlIdx = session.AiEvaluation.IndexOf("Deadline:") + "Deadline:".Length;
                    string dlStr = session.AiEvaluation.Substring(dlIdx).Trim();
                    int endIdx = dlStr.IndexOf("|");
                    if (endIdx > 0) dlStr = dlStr.Substring(0, endIdx);

                    if (DateTime.TryParse(dlStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dl))
                    {
                        parsedDeadline = dl;
                    }
                }
                catch { }
            }

            // KIỂM SOÁT THỜI GIAN KHI TRUY CẬP: Tự động đóng phiên nếu người dùng rời trang và quay lại sau khi đã hết giờ
            if (session.Status == 0 && parsedDeadline.HasValue && DateTime.Now > parsedDeadline.Value.AddSeconds(15))
            {
                session.Status = 1; // Khóa phiên làm bài
                session.CompletedAt = parsedDeadline.Value;
                session.OverallScore = 0;
                session.AiEvaluation = "Bài thi đã tự động kết thúc do quá thời gian quy định (Ứng viên không nộp bài trước hạn chót).";
                await _context.SaveChangesAsync();
            }

            // Trích xuất metadata chẩn đoán kỹ năng nếu có
            SkillDiagnosticDto? skillDiagnostic = null;
            if (!string.IsNullOrEmpty(session.AiEvaluation) && session.AiEvaluation.Contains("Diagnostic:"))
            {
                try
                {
                    int diagIdx = session.AiEvaluation.IndexOf("Diagnostic:") + "Diagnostic:".Length;
                    string diagStr = session.AiEvaluation.Substring(diagIdx).Trim();
                    int endIdx = diagStr.IndexOf("|");
                    if (endIdx > 0) diagStr = diagStr.Substring(0, endIdx);
                    if (!string.IsNullOrEmpty(diagStr))
                    {
                        skillDiagnostic = JsonSerializer.Deserialize<SkillDiagnosticDto>(diagStr);
                    }
                }
                catch { }
            }

            var vm = new PracticeQuizViewModel
            {
                SessionID = session.SessionID,
                ResumeTitle = session.Resume?.Title ?? "Hồ sơ nghề nghiệp",
                JobTitle = session.Job?.Title,
                Status = session.Status,
                OverallScore = session.OverallScore,
                AiEvaluation = session.AiEvaluation,
                AiProvider = interviewerMsgs.FirstOrDefault()?.AiProvider ?? "Local AI (Ollama)",
                CreatedAt = session.CreatedAt,
                CompletedAt = session.CompletedAt,
                Deadline = parsedDeadline,
                TotalTimeSeconds = interviewerMsgs.Count * 60, // 1 phút/câu theo chuẩn mới (5 câu = 5 phút, 7 câu = 7 phút,...)
                RemainingSeconds = parsedDeadline.HasValue 
                    ? Math.Max(0, (int)(parsedDeadline.Value - DateTime.Now).TotalSeconds)
                    : interviewerMsgs.Count * 60,
                SkillDiagnostic = skillDiagnostic
            };

            // Kiểm tra xem phiên thi có bản ghi nào chứa mã định danh QID hay không
            bool hasAnyQid = candidateMsgs.Any(m => m.Feedback != null && m.Feedback.StartsWith("QID:"));

            int qIndex = 1;
            foreach (var q in interviewerMsgs)
            {
                var options = new List<string>();
                if (!string.IsNullOrEmpty(q.ChoicesJson))
                {
                    try
                    {
                        options = JsonSerializer.Deserialize<List<string>>(q.ChoicesJson) ?? new List<string>();
                    }
                    catch { }
                }

                // Tìm câu trả lời tương ứng của ứng viên:
                // 1. Với phiên mới (hasAnyQid == true): CHỈ tìm theo QID:{q.MessageID}|; tuyệt đối không fallback theo thứ tự mảng tránh ghép nhầm khi thiếu bản ghi
                // 2. Với phiên cũ lịch sử (hasAnyQid == false): Chỉ fallback theo index nếu hoàn toàn không có bản ghi QID nào
                string candidateAns = "";
                InterviewMessage? matchedCandidateMsg = null;
                if (hasAnyQid)
                {
                    matchedCandidateMsg = candidateMsgs.FirstOrDefault(m => m.Feedback != null && m.Feedback.StartsWith($"QID:{q.MessageID}|"));
                }
                else
                {
                    matchedCandidateMsg = candidateMsgs.ElementAtOrDefault(qIndex - 1);
                }

                if (matchedCandidateMsg != null)
                {
                    candidateAns = matchedCandidateMsg.SelectedAnswer ?? "";
                }

                var itemVm = new PracticeQuizItemViewModel
                {
                    MessageID = q.MessageID,
                    QuestionIndex = qIndex,
                    Question = q.Content,
                    Options = options,
                    CandidateAnswer = candidateAns
                };

                // BẢO MẬT ĐÁP ÁN: Chỉ khi phiên ĐÃ NỘP BÀI (Status == 1) mới chuyển đáp án đúng và lời giải xuống Client
                if (session.Status == 1)
                {
                    itemVm.CorrectAnswer = q.SelectedAnswer;
                    itemVm.IsCorrect = (!string.IsNullOrEmpty(candidateAns) && candidateAns.Equals(q.SelectedAnswer, StringComparison.OrdinalIgnoreCase));

                    // Giải mã Feedback JSON có version
                    if (!string.IsNullOrEmpty(q.Feedback))
                    {
                        try
                        {
                            var meta = JsonSerializer.Deserialize<QuizFeedbackMetadata>(q.Feedback);
                            if (meta != null)
                            {
                                itemVm.Topic = meta.Topic;
                                itemVm.Explanation = meta.Explanation;
                            }
                        }
                        catch
                        {
                            itemVm.Explanation = q.Feedback;
                        }
                    }
                }
                else
                {
                    // ĐANG LÀM BÀI: Triệt tiêu hoàn toàn đáp án khỏi payload Client (F12/Inspect không thể thấy)
                    itemVm.CorrectAnswer = null;
                    itemVm.Explanation = null;
                    itemVm.Topic = null;
                    itemVm.IsCorrect = null;
                }

                vm.Questions.Add(itemVm);
                qIndex++;
            }

            // GIAI ĐOẠN 4: Thống kê chi tiết & Phân tích chuyên môn theo nhóm lỗi sau khi nộp bài
            if (session.Status == 1)
            {
                bool isAutoSubmittedOverdue = !string.IsNullOrEmpty(session.AiEvaluation) && session.AiEvaluation.Contains("hết giờ");
                int durationSec = 0;
                if (session.CompletedAt.HasValue && session.CompletedAt > session.CreatedAt)
                {
                    durationSec = (int)(session.CompletedAt.Value - session.CreatedAt).TotalSeconds;
                }
                int mins = durationSec / 60;
                int secs = durationSec % 60;
                string formattedDuration = $"{(mins < 10 ? "0" + mins : mins.ToString())}:{(secs < 10 ? "0" + secs : secs.ToString())}";

                var topicStats = vm.Questions
                    .Where(q => !string.IsNullOrEmpty(q.Topic))
                    .GroupBy(q => q.Topic)
                    .Select(g => new QuizTopicStatDto
                    {
                        Topic = g.Key!,
                        Total = g.Count(),
                        Correct = g.Count(x => x.IsCorrect == true)
                    })
                    .OrderBy(t => t.Percentage)
                    .ToList();

                var strengths = topicStats.Where(t => t.IsMastered).Select(t => t.Topic).ToList();
                var weaknesses = topicStats.Where(t => !t.IsMastered).Select(t => t.Topic).ToList();
                var recommendations = GenerateQuizRecommendations(weaknesses);

                vm.ResultSummary = new QuizResultSummaryDto
                {
                    TotalQuestions = vm.Questions.Count,
                    AnsweredCount = vm.Questions.Count(x => !string.IsNullOrEmpty(x.CandidateAnswer)),
                    SkippedCount = vm.Questions.Count(x => string.IsNullOrEmpty(x.CandidateAnswer)),
                    CorrectCount = vm.Questions.Count(x => x.IsCorrect == true),
                    WrongCount = vm.Questions.Count(x => !string.IsNullOrEmpty(x.CandidateAnswer) && x.IsCorrect == false),
                    ScorePercentage = session.OverallScore ?? 0,
                    DurationSeconds = durationSec,
                    DurationFormatted = formattedDuration,
                    IsOverdue = isAutoSubmittedOverdue,
                    TopicStats = topicStats,
                    Strengths = strengths,
                    Weaknesses = weaknesses,
                    Recommendations = recommendations
                };
            }

            return View(vm);
        }

        // Nộp bài thi trắc nghiệm
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuizAnswer(int sessionId, Dictionary<int, string> answers)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId);

            if (session == null)
            {
                return BadRequest("Phiên làm bài không tồn tại.");
            }

            // GIAI ĐOẠN 4: Idempotent - Nếu phiên đã hoàn tất (do double click hoặc reload), chuyển thẳng về xem kết quả
            if (session.Status == 1)
            {
                return RedirectToAction(nameof(PracticeQuiz), new { sessionId = sessionId });
            }

            var interviewerQuestions = session.Messages.Where(m => m.Role == "interviewer").OrderBy(m => m.CreatedAt).ToList();
            int totalQuestions = interviewerQuestions.Count;

            if (totalQuestions == 0)
            {
                return BadRequest("Đề thi không có câu hỏi hợp lệ.");
            }

            // Kiểm tra giới hạn thời gian phía Server (Server-side Timer Deadline Check)
            bool isOverdue = false;
            if (!string.IsNullOrEmpty(session.AiEvaluation) && session.AiEvaluation.Contains("Deadline:"))
            {
                try
                {
                    int dlIdx = session.AiEvaluation.IndexOf("Deadline:") + "Deadline:".Length;
                    string dlStr = session.AiEvaluation.Substring(dlIdx).Trim();
                    int endIdx = dlStr.IndexOf("|");
                    if (endIdx > 0) dlStr = dlStr.Substring(0, endIdx);

                    if (DateTime.TryParse(dlStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var deadline))
                    {
                        // Cho phép thêm 15 giây đệm (Grace Buffer) để bù độ trễ mạng Internet
                        if (DateTime.Now > deadline.AddSeconds(15))
                        {
                            isOverdue = true;
                        }
                    }
                }
                catch { }
            }

            var validLabels = new HashSet<string> { "A", "B", "C", "D" };

            // YÊU CẦU: Chỉ được nộp bài khi đã chọn đủ toàn bộ câu trả lời (nếu nộp thủ công trước khi hết giờ)
            int answeredCount = answers != null ? answers.Count(a => !string.IsNullOrWhiteSpace(a.Value) && validLabels.Contains(a.Value.Trim().ToUpper())) : 0;
            if (!isOverdue && answeredCount < totalQuestions)
            {
                TempData["WarningMessage"] = $"Bạn chưa hoàn thành bài thi! Vui lòng chọn đáp án cho tất cả {totalQuestions}/{totalQuestions} câu hỏi trước khi nộp bài.";
                return RedirectToAction(nameof(PracticeQuiz), new { sessionId = sessionId });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int correctCount = 0;
                foreach (var qMsg in interviewerQuestions)
                {
                    string chosenAnswer = "";
                    if (answers != null && answers.TryGetValue(qMsg.MessageID, out var rawChosen) && !string.IsNullOrWhiteSpace(rawChosen))
                    {
                        string upperChosen = rawChosen.Trim().ToUpper();
                        if (validLabels.Contains(upperChosen))
                        {
                            chosenAnswer = upperChosen;
                        }
                    }

                    string correctAnswer = (qMsg.SelectedAnswer ?? "").Trim().ToUpper();
                    bool isAnswered = !string.IsNullOrEmpty(chosenAnswer);
                    bool isCorrect = (isAnswered && chosenAnswer == correctAnswer);
                    if (isCorrect)
                    {
                        correctCount++;
                    }

                    string feedbackText = isAnswered 
                        ? (isCorrect ? "Đúng" : $"Sai (Đáp án đúng: {correctAnswer})")
                        : "Chưa làm (0 điểm do hết thời gian)";

                    var candidateAnsMsg = new InterviewMessage
                    {
                        SessionID = sessionId,
                        Role = "candidate",
                        Content = isAnswered ? chosenAnswer : "Chưa chọn",
                        SelectedAnswer = chosenAnswer,
                        Feedback = $"QID:{qMsg.MessageID}|" + feedbackText,
                        Score = isCorrect ? 100 : 0,
                        CreatedAt = DateTime.Now
                    };
                    _context.InterviewMessages.Add(candidateAnsMsg);
                }

                session.Status = 1; // Hoàn thành
                session.CompletedAt = DateTime.Now;
                session.OverallScore = (int)Math.Round((double)correctCount / totalQuestions * 100);

                if (isOverdue)
                {
                    session.AiEvaluation = $"Kết quả bài thi trắc nghiệm (Tự động nộp khi hết giờ): Đạt {correctCount}/{totalQuestions} câu chính xác ({session.OverallScore}%).";
                    TempData["WarningMessage"] = $"Thời gian làm bài đã kết thúc! Hệ thống đã tự động lưu và chấm điểm bài thi của bạn ({correctCount}/{totalQuestions} câu đúng - {session.OverallScore}%). Những câu chưa làm được tính 0 điểm.";
                }
                else
                {
                    session.AiEvaluation = $"Kết quả bài thi trắc nghiệm: Đạt {correctCount}/{totalQuestions} câu chính xác ({session.OverallScore}%).";
                    TempData["SuccessMessage"] = $"Đã nộp bài trắc nghiệm thành công! Bạn đạt {correctCount}/{totalQuestions} câu ({session.OverallScore}%).";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(PracticeQuiz), new { sessionId = sessionId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Không thể ghi nhận kết quả bài thi lúc này: " + ex.Message;
                return RedirectToAction(nameof(PracticeQuiz), new { sessionId = sessionId });
            }
        }

        #endregion

        #region 2. CHỨC NĂNG LỜI MỜI PHÒNG VẤN TỪ RECRUITER (ỨNG VIÊN)

        // Danh sách lời mời phỏng vấn do Nhà tuyển dụng giao
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Assigned()
        {
            int userId = CurrentUserId;
            var assignedSessions = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Include(s => s.Job)
                .ThenInclude(j => j.Company)
                .Where(s => s.UserID == userId && s.InterviewType == 2) // 2: Bài thi do Recruiter giao
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(assignedSessions);
        }

        // Màn hình làm bài phỏng vấn tự luận do nhà tuyển dụng giao
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AssignedInterview(int sessionId)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Job)
                .ThenInclude(j => j.Company)
                .Include(s => s.Resume)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId && s.InterviewType == 2);

            if (session == null)
            {
                return NotFound("Không tìm thấy bài thi phỏng vấn được giao.");
            }

            session.Messages = session.Messages.OrderBy(m => m.CreatedAt).ToList();

            return View(session);
        }

        // Nộp bài thi tự luận do nhà tuyển dụng giao
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignedAnswers(int sessionId, Dictionary<int, string> answers)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId && s.InterviewType == 2);

            if (session == null || session.Status == 1)
            {
                return BadRequest("Bài thi phỏng vấn không hợp lệ hoặc đã nộp trước đó.");
            }

            // Lưu các câu trả lời tự luận tương ứng của ứng viên
            foreach (var qMsg in session.Messages.Where(m => m.Role == "interviewer").ToList())
            {
                string candidateAnswer = answers.ContainsKey(qMsg.MessageID) ? answers[qMsg.MessageID] : "";

                var candidateAnsMsg = new InterviewMessage
                {
                    SessionID = sessionId,
                    Role = "candidate",
                    Content = candidateAnswer.Trim(),
                    ChoicesJson = qMsg.Content, // Lưu trữ câu hỏi gốc ở đây để dễ liên kết khi hiển thị
                    CreatedAt = DateTime.Now
                };
                _context.InterviewMessages.Add(candidateAnsMsg);
            }

            session.Status = 1; // Cập nhật trạng thái: Đã hoàn thành làm bài (chờ Recruiter chấm điểm)
            session.CompletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nộp bài phỏng vấn thành công! Vui lòng chờ nhà tuyển dụng đánh giá.";
            return RedirectToAction(nameof(Assigned));
        }

        #endregion

        #region 3. CHỨC NĂNG DÀNH CHO NHÀ TUYỂN DỤNG (RECRUITER)

        // API gọi AI sinh câu hỏi phỏng vấn gợi ý (CV vs JD)
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> GetAiSuggestedQuestions(int jobId, int resumeId, int count = 3, string? customPrompt = null)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            var resume = await _context.Resumes.FindAsync(resumeId);

            if (job == null || resume == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin Job hoặc CV." });
            }

            string cvSummary = $"Vị trí ứng tuyển trong CV: {resume.JobTitle}. Giới thiệu: {resume.Summary}.";
            if (!string.IsNullOrEmpty(resume.JsonContent))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(resume.JsonContent))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("skills", out var skillsProp) && skillsProp.ValueKind == JsonValueKind.Array)
                        {
                            var skillsList = new List<string>();
                            foreach (var item in skillsProp.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("name", out var n))
                                {
                                    skillsList.Add(n.GetString() ?? "");
                                }
                                else if (item.ValueKind == JsonValueKind.String)
                                {
                                    skillsList.Add(item.GetString() ?? "");
                                }
                            }
                            cvSummary += " Kỹ năng: " + string.Join(", ", skillsList);
                        }
                    }
                }
                catch { }
            }

            string customInstruction = "";
            if (!string.IsNullOrEmpty(customPrompt))
            {
                // Ràng buộc giới hạn độ dài tối đa 300 ký tự ở phía Backend để bảo vệ API
                var sanitizedPrompt = customPrompt.Trim();
                if (sanitizedPrompt.Length > 300)
                {
                    sanitizedPrompt = sanitizedPrompt.Substring(0, 300);
                }

                // Mã hóa các ký tự HTML nguy hiểm để chống Prompt Injection
                sanitizedPrompt = System.Net.WebUtility.HtmlEncode(sanitizedPrompt);

                customInstruction = $"\n[YÊU CẦU ĐẶC BIỆT CỦA NHÀ TUYỂN DỤNG]:\n- Hãy ưu tiên tập trung thiết lập câu hỏi xoay quanh yêu cầu sau: {sanitizedPrompt}";
            }

            string prompt = $@"Bạn là trợ lý AI chuyên nghiệp hỗ trợ nhà tuyển dụng chuẩn bị phỏng vấn. Hãy so sánh độ lệch giữa CV ứng viên và mô tả công việc (JD) dưới đây, từ đó sinh ra đúng {count} câu hỏi phỏng vấn tự luận sắc sảo kèm gợi ý đáp án/tiêu chí chấm điểm để đánh giá năng lực của ứng viên đó.{customInstruction}
Yêu cầu trả về duy nhất định dạng JSON (không chứa markdown ```json):
[
  {{
    ""question"": ""Câu hỏi phỏng vấn?"",
    ""expectedAnswer"": ""Gợi ý nội dung đáp án/tiêu chí đánh giá của câu này...""
  }}
]

Mô tả công việc (JD):
Tiêu đề: {job.Title}
Mô tả: {job.Description}
Yêu cầu: {job.Requirements}

Tóm tắt CV của Ứng viên:
{cvSummary}";

            try
            {
                string aiQuestions = await _aiService.GenerateContent(prompt, responseJson: true);
                aiQuestions = CleanJson(aiQuestions);
                await LogAiUsage("interview_recruiter_questions", prompt, aiQuestions);

                return Json(new { success = true, questions = aiQuestions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi sinh câu hỏi AI: " + ex.Message });
            }
        }

        // Tạo yêu cầu phỏng vấn gửi đến Ứng viên
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInterviewToCandidate(int jobId, int resumeId, string questionsJson)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            var resume = await _context.Resumes.FindAsync(resumeId);

            if (job == null || resume == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Job hoặc CV.";
                return RedirectToAction("Candidates", "Jobs", new { id = jobId });
            }

            // Tạo hoặc cập nhật bộ câu hỏi lưu trữ cho Recruiter
            var prep = await _context.RecruiterInterviewPreps
                .FirstOrDefaultAsync(p => p.JobID == jobId && p.ResumeID == resumeId);

            if (prep == null)
            {
                prep = new RecruiterInterviewPrep
                {
                    RecruiterID = CurrentUserId,
                    ResumeID = resumeId,
                    JobID = jobId,
                    QuestionsJson = questionsJson,
                    CreatedAt = DateTime.Now
                };
                _context.RecruiterInterviewPreps.Add(prep);
            }
            else
            {
                prep.QuestionsJson = questionsJson;
            }

            // Tạo phiên phỏng vấn mới (Interview Session) cho ứng viên làm bài
            var session = new InterviewSession
            {
                UserID = resume.UserID,
                JobID = jobId,
                ResumeID = resumeId,
                InterviewType = 2, // 2: Bài thi do Recruiter giao
                Status = 0, // Đang diễn ra (Chờ ứng viên trả lời)
                AssignedByRecruiterID = CurrentUserId,
                CreatedAt = DateTime.Now
            };
            _context.InterviewSessions.Add(session);
            await _context.SaveChangesAsync();

            // Lưu các câu hỏi vào InterviewMessages với vai trò 'interviewer'
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(questionsJson))
                {
                    int index = 1;
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        string qText = item.GetProperty("question").GetString();
                        string expAns = item.TryGetProperty("expectedAnswer", out var ea) ? ea.GetString() : "";

                        var msg = new InterviewMessage
                        {
                            SessionID = session.SessionID,
                            Role = "interviewer",
                            Content = qText,
                            Feedback = expAns, // Lưu gợi ý đáp án/tiêu chí vào cột Feedback của câu hỏi
                            CreatedAt = DateTime.Now.AddSeconds(index)
                        };
                        _context.InterviewMessages.Add(msg);
                        index++;
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể phân tách danh sách câu hỏi. Chi tiết: " + ex.Message;
                return RedirectToAction("Candidates", "Jobs", new { id = jobId });
            }

            // Cập nhật trạng thái đơn ứng tuyển của ứng viên trong bảng Applications thành 'Reviewing' hoặc trạng thái khác nếu cần
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.JobID == jobId && a.ResumeID == resumeId);
            if (application != null)
            {
                application.Status = "Reviewing";
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Đã gửi yêu cầu phỏng vấn thành công đến ứng viên!";
            return RedirectToAction("Candidates", "Jobs", new { id = jobId });
        }

        // API xem kết quả bài phỏng vấn của ứng viên để chấm điểm
        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> GetCandidateInterviewResult(int sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);

            if (session == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phiên phỏng vấn." });
            }

            // Sắp xếp tin nhắn phỏng vấn
            var messages = session.Messages.OrderBy(m => m.CreatedAt).ToList();

            var resultData = new
            {
                sessionId = session.SessionID,
                candidateName = session.Resume.FullName,
                overallScore = session.OverallScore,
                aiEvaluation = session.AiEvaluation,
                status = session.Status,
                messages = messages.Select(m => new
                {
                    messageId = m.MessageID,
                    role = m.Role,
                    content = m.Content,
                    score = m.Score,
                    feedback = m.Feedback,
                    choicesJson = m.ChoicesJson // Chứa câu hỏi liên kết đối với tin nhắn trả lời
                })
            };

            return Json(new { success = true, data = resultData });
        }

        // API chấm điểm nháp tự luận bằng AI dựa trên đáp án gợi ý
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> GetAiFeedbackForAnswer(int messageId)
        {
            var candidateMsg = await _context.InterviewMessages.FindAsync(messageId);
            if (candidateMsg == null || candidateMsg.Role != "candidate")
            {
                return Json(new { success = false, message = "Không tìm thấy câu trả lời hợp lệ." });
            }

            // Tìm câu hỏi tương ứng (câu hỏi có nội dung khớp với ChoicesJson của câu trả lời)
            var questionMsg = await _context.InterviewMessages
                .FirstOrDefaultAsync(m => m.SessionID == candidateMsg.SessionID && m.Role == "interviewer" && m.Content == candidateMsg.ChoicesJson);

            string question = questionMsg != null ? questionMsg.Content : "Câu hỏi phỏng vấn";
            string expectedCriteria = questionMsg != null ? questionMsg.Feedback : "Không có tiêu chí cụ thể";

            string prompt = $@"Bạn là một nhà tuyển dụng và chuyên gia đánh giá phỏng vấn cực kỳ nghiêm khắc và chuyên nghiệp.
Nhiệm vụ của bạn là đánh giá câu trả lời tự luận của ứng viên dựa trên câu hỏi và tiêu chí đánh giá được cung cấp.

Hãy phân tích và chấm điểm thật chi tiết theo thang điểm 100 dựa trên các khía cạnh:
1. Độ chính xác và chiều sâu kiến thức chuyên môn (Technical Depth & Accuracy).
2. Sự mạch lạc, rõ ràng và cấu trúc câu trả lời (phương pháp STAR).
3. Tính thực tiễn, kinh nghiệm thực tế và giải pháp ứng viên đưa ra.

QUY TẮC CHẤM ĐIỂM NGHIÊM NGẶT:
- KHÔNG được chấm các điểm số trung bình mang tính an toàn (như luôn cho 80, 85) một cách rập khuôn cho mọi câu hỏi.
- Phải có sự phân hóa điểm số rõ ràng giữa các câu trả lời dựa trên chất lượng thực tế:
  + Nếu câu trả lời quá ngắn gọn, chung chung, thiếu ví dụ thực tế hoặc chỉ lý thuyết suông: Điểm số phải dưới 70 điểm.
  + Nếu câu trả lời ở mức khá, đúng trọng tâm nhưng chưa thực sự xuất sắc: Điểm số từ 70 - 80 điểm.
  + Chỉ cho điểm từ 85 - 100 điểm khi câu trả lời thực sự xuất sắc, đầy đủ ý, thể hiện tư duy sâu sắc và kinh nghiệm thực tiễn rõ ràng.
- Viết nhận xét chi tiết, chỉ ra điểm mạnh và điểm cần cải thiện của ứng viên theo cấu trúc STAR.

- Viết nhận xét chi tiết, chỉ ra điểm mạnh và điểm cần cải thiện của ứng viên theo cấu trúc STAR.

Yêu cầu bắt buộc trả về định dạng JSON (không chứa bất kỳ văn bản phụ nào ngoài JSON):
{{
  ""score"": 75,
  ""feedback"": ""Nhận xét chi tiết theo cấu trúc STAR...""
}}

Câu hỏi: ""{question}""
Tiêu chí đánh giá: ""{expectedCriteria}""
Câu trả lời của Ứng viên: ""{candidateMsg.Content}""";

            try
            {
                string aiFeedback = await _aiService.GenerateContent(prompt, responseJson: true);
                aiFeedback = CleanJson(aiFeedback);
                await LogAiUsage("interview_recruiter_eval", prompt, aiFeedback);

                using (JsonDocument doc = JsonDocument.Parse(aiFeedback))
                {
                    var root = doc.RootElement;
                    
                    // Xử lý an toàn phòng trường hợp AI vẫn trả về mảng hoặc chuỗi cho score
                    int score = 0;
                    var scoreProp = root.GetProperty("score");
                    if (scoreProp.ValueKind == JsonValueKind.Number)
                    {
                        score = scoreProp.GetInt32();
                    }
                    else if (scoreProp.ValueKind == JsonValueKind.Array && scoreProp.GetArrayLength() > 0)
                    {
                        var firstEl = scoreProp[0];
                        if (firstEl.ValueKind == JsonValueKind.Number) score = firstEl.GetInt32();
                        else if (firstEl.ValueKind == JsonValueKind.String) int.TryParse(firstEl.GetString(), out score);
                    }
                    else if (scoreProp.ValueKind == JsonValueKind.String)
                    {
                        int.TryParse(scoreProp.GetString(), out score);
                    }

                    string feedback = root.GetProperty("feedback").GetString();

                    return Json(new { success = true, score = score, feedback = feedback });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi AI chấm điểm: " + ex.Message });
            }
        }

        // Nhà tuyển dụng gửi điểm chấm và nhận xét cuối cùng
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRecruiterEvaluation(
            int sessionId, 
            Dictionary<int, int> scores, 
            Dictionary<int, string> feedbacks, 
            string overallEvaluation, 
            int overallScore)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);

            if (session == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiên phỏng vấn.";
                return RedirectToAction("Manage", "Jobs");
            }

            // Cập nhật điểm và nhận xét cho từng câu trả lời của ứng viên
            foreach (var msg in session.Messages.Where(m => m.Role == "candidate"))
            {
                if (scores.ContainsKey(msg.MessageID))
                {
                    msg.Score = scores[msg.MessageID];
                }
                if (feedbacks.ContainsKey(msg.MessageID))
                {
                    msg.Feedback = feedbacks[msg.MessageID];
                }
            }

            // Cập nhật điểm chung cuộc và nhận xét của Nhà tuyển dụng
            session.OverallScore = overallScore;
            session.AiEvaluation = overallEvaluation; // Lưu trữ nhận xét chính thức từ Nhà tuyển dụng tại đây
            session.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã lưu kết quả chấm điểm phỏng vấn thành công!";
            return RedirectToAction("Candidates", "Jobs", new { id = session.JobID });
        }

        private string CleanJson(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            input = input.Trim();
            if (input.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(7);
            }
            else if (input.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(3);
            }
            if (input.EndsWith("```"))
            {
                input = input.Substring(0, input.Length - 3);
            }
            return input.Trim();
        }

        private async Task LogAiUsage(string requestType, string inputText, string outputText)
        {
            try
            {
                int userId = CurrentUserId;
                int tokens = (inputText?.Length ?? 0) / 4 + (outputText?.Length ?? 0) / 4 + 120;

                var config = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == 1);
                string provider = "Gemini";
                if (config != null && !string.IsNullOrEmpty(config.ModelName) && config.ModelName.Contains("Groq", StringComparison.OrdinalIgnoreCase))
                {
                    provider = "Groq";
                }

                var log = new AILog
                {
                    UserID = userId > 0 ? userId : (int?)null,
                    RequestType = requestType,
                    InputText = inputText,
                    OutputText = outputText,
                    UsedTokens = tokens,
                    ApiProvider = provider,
                    CreatedAt = DateTime.Now
                };

                _context.AILogs.Add(log);
                if (config != null)
                {
                    config.TotalTokensUsed += tokens;
                }
                await _context.SaveChangesAsync();
            }
            catch { }
        }

        private static string CleanAiQuestionContent(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string text = raw.Trim();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"(?i)\b(SITUATION|SITTING|TASK|ACTION|RESULT)\s*:\s*", "");
            if (text.Contains("💡 Gợi ý trả lời"))
            {
                int hintIdx = text.IndexOf("💡 Gợi ý trả lời");
                text = text.Substring(0, hintIdx).Trim();
            }
            return text.Trim();
        }

        #endregion

        #region CHỨC NĂNG TRẮC NGHIỆM CHUYÊN MÔN (QUIZ ENGINE & FAILOVER)

        /// <summary>
        /// Chuẩn hóa tên kỹ năng để so sánh chính xác theo nhóm công nghệ tương đương
        /// Ngăn chặn lỗi so khớp chuỗi con naive (ví dụ "Java" vs "JavaScript", "C" vs "CSS", "Go" vs "MongoDB")
        /// </summary>
        private static string NormalizeSkillName(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill)) return string.Empty;
            string s = skill.Trim().ToLowerInvariant();

            if (s == "c#" || s == "csharp" || s == "c-sharp") return "csharp";
            if (s == ".net" || s == "dotnet" || s == "asp.net" || s == "asp.net core" || s == "aspnet" || s == "aspnetcore") return "dotnet";
            if (s == "c++" || s == "cpp") return "cpp";
            if (s == "react" || s == "reactjs" || s == "react.js") return "react";
            if (s == "vue" || s == "vuejs" || s == "vue.js") return "vue";
            if (s == "angular" || s == "angularjs" || s == "angular.js") return "angular";
            if (s == "node" || s == "nodejs" || s == "node.js") return "nodejs";
            if (s == "js" || s == "javascript") return "javascript";
            if (s == "ts" || s == "typescript") return "typescript";
            if (s == "golang" || s == "go") return "golang";
            if (s == "postgres" || s == "postgresql") return "postgresql";
            if (s == "sql server" || s == "mssql") return "sqlserver";
            if (s == "mongo" || s == "mongodb") return "mongodb";
            if (s == "docker" || s == "container") return "docker";
            if (s == "k8s" || s == "kubernetes") return "kubernetes";
            if (s == "ci/cd" || s == "cicd") return "cicd";
            if (s == "clean architecture" || s == "clean-architecture") return "cleanarchitecture";
            if (s == "solid" || s == "solid principles") return "solid";
            if (s == "unit test" || s == "unittest" || s == "unit testing") return "unittest";

            return s;
        }

        /// <summary>
        /// So khớp an toàn giữa 2 kỹ năng: Không dùng Substring IndexOf thô sơ gây match nhầm
        /// </summary>
        private static bool IsSkillMatch(string s1, string s2)
        {
            if (string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2)) return false;
            if (string.Equals(s1.Trim(), s2.Trim(), StringComparison.OrdinalIgnoreCase)) return true;

            string n1 = NormalizeSkillName(s1);
            string n2 = NormalizeSkillName(s2);
            return n1.Length > 0 && n2.Length > 0 && string.Equals(n1, n2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// GIAI ĐOẠN 2: Chẩn đoán và phân loại kỹ năng giữa hồ sơ ứng viên (CV) và tin tuyển dụng (JD)
        /// Phân loại: Matched Skills và Gap Skills (kỹ năng JD yêu cầu nhưng chưa thấy đề cập trong CV)
        /// Không fake kỹ năng khi JD không tìm thấy từ khóa; loại bỏ so khớp chuỗi con gây nhầm lẫn
        /// </summary>
        private static SkillDiagnosticDto DiagnoseSkillGaps(List<string> cvSkills, string? jobInfo)
        {
            var diag = new SkillDiagnosticDto();
            var cleanCvSkills = cvSkills
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (string.IsNullOrWhiteSpace(jobInfo))
            {
                diag.TargetMode = "cv";
                diag.MatchedSkills = cleanCvSkills;
                diag.GapSkills = new List<string>();
                diag.Summary = $"Đề thi bám sát hồ sơ ứng viên với {cleanCvSkills.Count} kỹ năng chuyên môn cốt lõi.";
                return diag;
            }

            diag.TargetMode = "jd";

            // Danh mục từ khóa công nghệ mở rộng để phát hiện kỹ năng yêu cầu trong JD
            var techCatalog = new[]
            {
                "C#", ".NET", "ASP.NET", "ASP.NET Core", "Entity Framework", "SQL", "SQL Server", "MySQL", "PostgreSQL", "MongoDB",
                "React", "ReactJS", "Vue", "VueJS", "Angular", "JavaScript", "TypeScript", "NodeJS", "HTML", "CSS", "Bootstrap", "Tailwind",
                "Python", "Django", "FastAPI", "Java", "Spring Boot", "PHP", "Laravel", "Go", "Golang", "C++", "Rust",
                "Docker", "Kubernetes", "CI/CD", "Git", "GitHub", "GitLab", "RESTful API", "GraphQL", "Microservices",
                "Redis", "Kafka", "RabbitMQ", "AWS", "Azure", "GCP", "Linux", "OOP", "SOLID", "Clean Architecture", "Unit Test"
            };

            var jdSkills = new List<string>();
            foreach (var tech in techCatalog)
            {
                string pattern;
                if (tech == "C#") pattern = @"(?i)\b(C#|CSharp)\b";
                else if (tech == ".NET") pattern = @"(?i)\b(\.NET|DotNet)\b";
                else if (tech == "C++") pattern = @"(?i)\b(C\+\+|Cpp)\b";
                else if (tech == "Go") pattern = @"(?i)\b(Go|Golang)\b";
                else pattern = $@"(?i)\b{System.Text.RegularExpressions.Regex.Escape(tech)}\b";

                if (System.Text.RegularExpressions.Regex.IsMatch(jobInfo, pattern))
                {
                    jdSkills.Add(tech);
                }
            }

            // Tuyệt đối không lấy kỹ năng từ CV gán thành JD skills khi JD không có từ khóa trong catalog
            // Tránh việc banner báo cáo sai lệch rằng CV khớp với tin tuyển dụng
            if (jdSkills.Count == 0)
            {
                diag.MatchedSkills = new List<string>();
                diag.GapSkills = new List<string>();
                diag.Summary = "Tin tuyển dụng không chỉ định yêu cầu công nghệ cụ thể trong danh mục hệ thống; bài kiểm tra sẽ tập trung đánh giá năng lực nền tảng kỹ thuật và các kỹ năng cốt lõi theo hồ sơ ứng viên.";
                return diag;
            }

            var matched = new List<string>();
            var gaps = new List<string>();

            foreach (var jdSkill in jdSkills)
            {
                bool isFoundInCv = cleanCvSkills.Any(cs => IsSkillMatch(cs, jdSkill));

                if (isFoundInCv)
                {
                    matched.Add(jdSkill);
                }
                else
                {
                    gaps.Add(jdSkill);
                }
            }

            diag.MatchedSkills = matched.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            diag.GapSkills = gaps.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (diag.GapSkills.Count > 0)
            {
                diag.Summary = $"Phát hiện {diag.MatchedSkills.Count} kỹ năng khớp với CV và {diag.GapSkills.Count} kỹ năng yêu cầu trong JD chưa thấy đề cập trong hồ sơ (cần kiểm chứng năng lực).";
            }
            else
            {
                diag.Summary = $"Hồ sơ ứng viên có độ phủ tốt, khớp {diag.MatchedSkills.Count} kỹ năng yêu cầu từ vị trí tuyển dụng.";
            }

            return diag;
        }

        /// <summary>
        /// Sinh bộ câu hỏi trắc nghiệm kỹ thuật với kiến trúc Failover 3 tầng chuẩn Offline-first:
        /// Tầng 1: Local AI (Ollama JSON Mode) - Tự động ghi nhận đúng model thực tế
        /// Tầng 2: Cloud AI (Gemini Fallback) - Chỉ gọi khi enableCloudFallback = true và đã ẩn danh hóa PII
        /// Tầng 3: Ngân hàng đề kỹ thuật Offline chuẩn (25+ câu đa ngành, xáo trộn ngẫu nhiên Random.Shared)
        /// </summary>
        private async Task<(List<QuizQuestionDto> questions, string provider)> GenerateQuizQuestionsWithFailoverAsync(
            string cvSummary, int count, string? jobInfo, bool enableCloudFallback = false, SkillDiagnosticDto? diagnostic = null)
        {
            // TẦNG 1: Thử nghiệm Local AI (Ollama JSON Mode)
            try
            {
                if (await _ollamaService.IsAvailableAsync())
                {
                    var (ollamaQuestions, modelName) = await TryGenerateQuizWithOllamaAsync(cvSummary, count, jobInfo, diagnostic);
                    if (ollamaQuestions != null && ollamaQuestions.Count >= count)
                    {
                        string providerName = string.IsNullOrWhiteSpace(modelName) ? "Local AI (Ollama)" : $"Local AI (Ollama: {modelName})";
                        return (ollamaQuestions.Take(count).ToList(), providerName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QuizEngine] Local AI Ollama không thành công: {ex.Message}. Tuyến tiếp theo được kích hoạt.");
            }

            // TẦNG 2: Cloud AI (Gemini) - Chỉ gọi khi có cấu hình bật rõ ràng (Cloud Fallback Policy)
            if (enableCloudFallback)
            {
                try
                {
                    // Giảm thiểu dữ liệu: Loại bỏ toàn bộ PII (tên, email, số điện thoại, công ty...) trước khi gửi ra Cloud
                    string minimizedCv = MinimizeCvAndJobContext(cvSummary);
                    string? minimizedJob = !string.IsNullOrEmpty(jobInfo) ? MinimizeCvAndJobContext(jobInfo) : null;

                    var geminiQuestions = await TryGenerateQuizWithGeminiAsync(minimizedCv, count, minimizedJob, diagnostic);
                    if (geminiQuestions != null && geminiQuestions.Count >= count)
                    {
                        return (geminiQuestions.Take(count).ToList(), "Cloud AI (Gemini)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[QuizEngine] Cloud Gemini không thành công: {ex.Message}. Kích hoạt Offline Template.");
                }
            }

            // TẦNG 3: Ngân hàng câu hỏi Offline Template kỹ thuật (Bảo đảm 100% offline, xáo trộn đa ngành)
            var offlineQuestions = GetOfflineTechnicalQuizTemplates(count, cvSummary, jobInfo, diagnostic);
            return (offlineQuestions, "Offline Technical Template");
        }

        /// <summary>
        /// Sinh đề trắc nghiệm bằng Local AI Ollama JSON Mode
        /// </summary>
        private async Task<(List<QuizQuestionDto>? questions, string modelName)> TryGenerateQuizWithOllamaAsync(
            string cvSummary, int count, string? jobInfo, SkillDiagnosticDto? diagnostic = null)
        {
            // Lấy đúng model thực tế từ service, không đoán hay gán cứng qwen2.5:3b
            string? activeModel = await _ollamaService.GetActiveModelAsync();
            string prompt = BuildQuizPrompt(cvSummary, count, jobInfo, diagnostic);

            string systemPrompt = "You are a Senior Tech Lead and Technical Interviewer. You must generate high-quality technical multiple-choice questions with clear explanations. Output ONLY valid JSON array without any markdown formatting or commentary.";

            // Tăng timeout lên 60s để Ollama có đủ thời gian suy luận toàn bộ 5-10 câu trọn vẹn
            string jsonResponse = await _ollamaService.GenerateJsonAsync(prompt, systemPrompt, temperature: 0.2, timeoutSeconds: 60);
            var parsed = ParseAndValidateQuizJson(jsonResponse, count);
            return (parsed, activeModel ?? "");
        }

        /// <summary>
        /// Sinh đề trắc nghiệm bằng Cloud AI Gemini (Đã qua lọc dữ liệu định danh)
        /// </summary>
        private async Task<List<QuizQuestionDto>?> TryGenerateQuizWithGeminiAsync(
            string cvSummary, int count, string? jobInfo, SkillDiagnosticDto? diagnostic = null)
        {
            string prompt = BuildQuizPrompt(cvSummary, count, jobInfo, diagnostic);
            string aiResponse = await _aiService.GenerateContent(prompt, responseJson: true);
            aiResponse = CleanJson(aiResponse);
            await LogAiUsage("interview_quiz", prompt, aiResponse);

            return ParseAndValidateQuizJson(aiResponse, count);
        }

        /// <summary>
        /// Lớp phòng vệ làm sạch dữ liệu tăng cường (Enhanced Data Sanitization / Defense-in-depth):
        /// Khử triệt để email, số điện thoại, URL, tiền tố tên, tên công ty/tập đoàn, tên trường/học viện, họ tên người, danh xưng nhân sự, địa chỉ và mức lương.
        /// Bảo mật cốt lõi kết hợp chặt chẽ với cơ chế Privacy by Design ở tầng trích xuất ngữ cảnh.
        /// </summary>
        private static string MinimizeCvAndJobContext(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string sanitized = text;

            // 1. Loại bỏ địa chỉ Email
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", "[EMAIL_REDACTED]");

            // 2. Loại bỏ Số điện thoại (Việt Nam & Quốc tế)
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?:\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}", "[PHONE_REDACTED]");

            // 3. Loại bỏ URLs / Links
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"https?:\/\/[^\s]+|(?:www\.)?[a-zA-Z0-9.-]+\.(?:com|org|vn|edu|io|net|github\.io)(?:/[^\s]*)?", "[URL_REDACTED]");

            // 4. Loại bỏ Tiền tố họ tên & Họ tên ứng viên
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)(?:Họ và tên|Họ tên|Ứng viên|Candidate Name|Full Name)\s*:\s*[^\.\,\;\n]+", "");

            // 5. Loại bỏ Tên công ty / Doanh nghiệp / Tập đoàn (Cả tiền tố và hậu tố pháp nhân, hoặc mẫu tại/at [Công ty])
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)\b(?:Công ty(?:\s+(?:TNHH|Cổ phần|CP|MTV|Liên doanh))?|Tập đoàn|Doanh nghiệp|Company|Corporation|Corp|JSC|Ltd|Inc)\s+([A-ZÀ-Ỹ0-9][\w&.\-']*(?:\s+[A-ZÀ-Ỹ0-9][\w&.\-']*)*)", "[COMPANY_REDACTED]");
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\b[A-ZÀ-Ỹ][\w&.\-']*(?:\s+[A-ZÀ-Ỹ][\w&.\-']*)*\s+(?:JSC|Corp|Corporation|Inc|Co\.,?\s*Ltd|TNHH|CP)\b", "[COMPANY_REDACTED]");
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)\b(?:tại|at)\s+(?:công ty\s+)?([A-ZÀ-Ỹ][\w&.\-']*(?:\s+[A-ZÀ-Ỹ][\w&.\-']*)*)", "[COMPANY_REDACTED]");

            // 6. Loại bỏ Tên trường đại học / Học viện / Cơ sở giáo dục
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)\b(?:Trường đại học|Đại học|Học viện|Cao đẳng|University|College|Academy)\s+([A-ZÀ-Ỹ0-9][\w&.\-']*(?:\s+[A-ZÀ-Ỹ0-9][\w&.\-']*)*)", "[ORG_REDACTED]");

            // 7. Loại bỏ Danh xưng & Tên người liên hệ / Nhân sự (HR, Recruiter, Mr/Ms...)
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)\b(?:Mr\.|Ms\.|Mrs\.|Anh|Chị|Ông|Bà|HR|Recruiter|Interviewer|Liên hệ|Contact(?:\s+person)?)\s*[:\s]+([A-ZÀ-Ỹ][a-zà-ỹ]+(?:\s+[A-ZÀ-Ỹ][a-zà-ỹ]+)*)", "[NAME_REDACTED]");

            // 8. Loại bỏ Họ tên người Việt phổ biến (2-4 từ viết hoa bắt đầu bằng các họ lớn)
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\b(?:Nguyễn|Trần|Lê|Phạm|Hoàng|Huỳnh|Phan|Vũ|Võ|Đặng|Bùi|Đỗ|Hồ|Ngô|Dương|Lý|Đinh|Đoàn|Mai|Trịnh|Đào|Cao)\s+[A-ZÀ-Ỹ][a-zà-ỹ]+(?:\s+[A-ZÀ-Ỹ][a-zà-ỹ]+){1,2}\b", "[NAME_REDACTED]");

            // 9. Loại bỏ Địa chỉ, Địa điểm, Tòa nhà làm việc
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)\b(?:Địa chỉ|Address|Địa điểm|Location|Văn phòng|Office|Tòa nhà|Building|Tầng|Lầu)\s*:\s*[^\.\,\;\n]+", "[LOCATION_REDACTED]");

            // 10. Loại bỏ Mức lương, Thu nhập
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"(?i)\b(?:Mức lương|Lương|Salary|Thu nhập|Income)\s*:\s*[^\.\,\;\n]+", "[SALARY_REDACTED]");

            return sanitized.Trim();
        }

        /// <summary>
        /// Xây dựng prompt sinh đề trắc nghiệm chuyên môn kỹ thuật (Hỗ trợ chẩn đoán trọng tâm theo JD)
        /// </summary>
        private static string BuildQuizPrompt(string cvSummary, int count, string? jobInfo, SkillDiagnosticDto? diagnostic = null)
        {
            string jobContext = string.IsNullOrWhiteSpace(jobInfo) ? "" : $"\nVỊ TRÍ VÀ YÊU CẦU CÔNG VIỆC TUYỂN DỤNG:\n{jobInfo}";

            string diagnosticGuidance = "";
            if (diagnostic != null && diagnostic.TargetMode == "jd")
            {
                if (diagnostic.GapSkills.Count > 0)
                {
                    diagnosticGuidance = $@"
ĐẶC BIỆT - PHÂN BỔ TRỌNG TÂM THEO JD (CHẨN ĐOÁN LỖ HỔNG KỸ NĂNG):
- Vị trí yêu cầu các kỹ năng sau nhưng CHƯA THẤY ĐỀ CẬP trong CV ứng viên: {string.Join(", ", diagnostic.GapSkills)}. Hãy dành ít nhất 50% đến 60% số câu hỏi để kiểm tra năng lực thực tế về các kỹ năng này!
- Các câu hỏi còn lại dành cho kỹ năng cốt lõi đã có trong hồ sơ: {string.Join(", ", diagnostic.MatchedSkills)}.
";
                }
                else if (diagnostic.MatchedSkills.Count > 0)
                {
                    diagnosticGuidance = $@"
ĐẶC BIỆT - PHÂN BỔ TRỌNG TÂM THEO JD:
- Ứng viên có kỹ năng khớp tốt với JD: {string.Join(", ", diagnostic.MatchedSkills)}. Hãy tập trung câu hỏi đào sâu về các kỹ năng này!
";
                }
            }

            return $@"Bạn là Tech Lead và Chuyên gia phỏng vấn kỹ thuật cấp cao. Dựa trên hồ sơ ứng viên và yêu cầu công việc dưới đây, hãy sinh ra đúng {count} câu hỏi trắc nghiệm chuyên môn kỹ thuật thực tế để kiểm tra năng lực lập trình, giải thuật, tối ưu hóa và kiến trúc phần mềm.
{diagnosticGuidance}
QUY TẮC CỐT LÕI:
1. Câu hỏi là bài kiểm tra kiến thức chuyên môn kỹ thuật thực tế (Ví dụ: vòng đời component trong React/Vue, tối ưu hóa truy vấn SQL, Dependency Injection trong .NET, xử lý bất đồng bộ async/await, nguyên lý SOLID, RESTful API, bảo mật JWT/CORS,... bám sát kỹ năng của ứng viên/công việc).
2. Tuyệt đối KHÔNG hỏi về thông tin tiểu sử cá nhân (KHÔNG hỏi 'Ứng viên từng học trường nào?', 'Ứng viên có bao nhiêu năm kinh nghiệm?', 'Ứng viên tên gì?').
3. Mỗi câu hỏi BẮT BUỘC có 5 thuộc tính:
   - ""question"": Câu hỏi kỹ thuật rõ ràng, đi thẳng vào bản chất vấn đề.
   - ""options"": Mảng ĐÚNG 4 chuỗi đáp án có tiền tố 'A. ', 'B. ', 'C. ', 'D. '.
   - ""correctAnswer"": Đúng 1 chữ cái in hoa ('A', 'B', 'C' hoặc 'D').
   - ""explanation"": Giải thích chi tiết và chính xác tại sao đáp án đó đúng, chỉ ra lỗi sai của các lựa chọn còn lại.
   - ""topic"": Tên chủ đề/công nghệ trọng tâm (Ví dụ: 'C# Concurrency', 'React State', 'SQL Optimization', 'OOP Design').

YÊU CẦU ĐẶC BIỆT: BẮT BUỘC sinh đúng và đủ {count} câu hỏi kỹ thuật khác nhau, sắp xếp thành một MẢNG JSON gồm đúng {count} phần tử:
[
  {{
    ""question"": ""Trong C#, từ khóa async/await hoạt động như thế nào khi gặp tác vụ I/O bound chưa hoàn thành?"",
    ""options"": [
      ""A. Tạo thêm Thread mới từ hệ điều hành để chạy song song"",
      ""B. Giải phóng Thread hiện tại về ThreadPool trong khi chờ I/O hoàn tất"",
      ""C. Khóa cứng tiến trình cho đến khi nhận được dữ liệu"",
      ""D. Chuyển đổi toàn bộ phương thức sang chạy trên Background Worker độc lập""
    ],
    ""correctAnswer"": ""B"",
    ""explanation"": ""Từ khóa await cho phép giải phóng thread đang xử lý về ThreadPool để phục vụ các yêu cầu khác trong thời gian chờ I/O hoàn thành mà không gây lãng phí tài nguyên block thread."",
    ""topic"": ""C# Concurrency""
  }}
]

THÔNG TIN ỨNG VIÊN:
{cvSummary}{jobContext}";
        }

        /// <summary>
        /// Bóc tách và kiểm tra tính hợp lệ nghiêm ngặt của JSON trả về từ AI:
        /// 1. Kiểm tra đủ 4 lựa chọn không trùng lặp nội dung.
        /// 2. Chuẩn hóa nhãn A, B, C, D theo đúng thứ tự.
        /// 3. correctAnswer BẮT BUỘC phải nằm trong ['A', 'B', 'C', 'D']. Nếu không hợp lệ -> REJECT câu hỏi (TUYỆT ĐỐI KHÔNG tự gán thành 'A').
        /// 4. Chỉ chấp nhận kết quả nếu đủ số lượng requiredCount.
        /// </summary>
        private static List<QuizQuestionDto>? ParseAndValidateQuizJson(string rawJson, int requiredCount)
        {
            if (string.IsNullOrWhiteSpace(rawJson)) return null;

            string cleaned = rawJson.Trim();
            if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(7);
            }
            else if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned.Substring(3);
            }
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            }
            cleaned = cleaned.Trim();

            try
            {
                using var doc = JsonDocument.Parse(cleaned);
                var root = doc.RootElement;
                var elementList = new List<JsonElement>();

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in root.EnumerateArray())
                    {
                        elementList.Add(el);
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    // Trường hợp 1: Object chứa property mảng (ví dụ: { "questions": [...] })
                    bool foundArray = false;
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in prop.Value.EnumerateArray())
                            {
                                elementList.Add(el);
                            }
                            foundArray = true;
                            break;
                        }
                    }

                    // Trường hợp 2: Chính root là 1 câu hỏi (chứa property "question")
                    if (!foundArray)
                    {
                        bool isDirectQuestion = false;
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Name.Equals("question", StringComparison.OrdinalIgnoreCase))
                            {
                                isDirectQuestion = true;
                                break;
                            }
                        }

                        if (isDirectQuestion)
                        {
                            elementList.Add(root);
                        }
                        else
                        {
                            // Trường hợp 3: Object chứa các sub-object (ví dụ: { "q1": {...}, "q2": {...} })
                            foreach (var prop in root.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var sub in prop.Value.EnumerateObject())
                                    {
                                        if (sub.Name.Equals("question", StringComparison.OrdinalIgnoreCase))
                                        {
                                            elementList.Add(prop.Value);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!elementList.Any()) return null;

                var result = new List<QuizQuestionDto>();
                var validAnswerLabels = new HashSet<string> { "A", "B", "C", "D" };
                string[] prefixLetters = { "A.", "B.", "C.", "D." };

                foreach (var el in elementList)
                {
                    if (el.ValueKind != JsonValueKind.Object) continue;

                    string question = "";
                    var rawOptionsList = new List<string>();
                    string correctAnswer = "";
                    string explanation = "";
                    string topic = "Kiến thức chuyên môn";

                    foreach (var prop in el.EnumerateObject())
                    {
                        string pName = prop.Name.ToLower();
                        if (pName == "question")
                        {
                            question = prop.Value.GetString() ?? "";
                        }
                        else if (pName == "options" && prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            rawOptionsList = prop.Value.EnumerateArray()
                                .Select(o => o.GetString() ?? "")
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToList();
                        }
                        else if (pName == "correctanswer" || pName == "correct_answer" || pName == "answer")
                        {
                            correctAnswer = (prop.Value.GetString() ?? "").Trim().ToUpper();
                        }
                        else if (pName == "explanation" || pName == "rationale" || pName == "detail")
                        {
                            explanation = prop.Value.GetString() ?? "";
                        }
                        else if (pName == "topic" || pName == "skill" || pName == "category")
                        {
                            topic = prop.Value.GetString() ?? topic;
                        }
                    }

                    // 1. Kiểm tra tính hợp lệ của câu hỏi
                    if (string.IsNullOrWhiteSpace(question) || question.Trim().Length < 10)
                    {
                        continue; // Bỏ qua câu hỏi rỗng hoặc quá ngắn
                    }

                    // 2. Kiểm tra số lượng lựa chọn (phải có ít nhất 4 lựa chọn)
                    if (rawOptionsList.Count < 4)
                    {
                        continue; // Bỏ qua câu hỏi không đủ 4 lựa chọn
                    }

                    // 3. Chuẩn hóa nhãn và nội dung các lựa chọn (Đảm bảo nhãn và correctAnswer khớp 100%)
                    var labeledOptions = new Dictionary<string, string>(); // Key: "A", "B", "C", "D" -> Value: nội dung thuần
                    var unlabelledOptions = new List<string>();

                    for (int i = 0; i < 4; i++)
                    {
                        string opt = rawOptionsList[i].Trim();
                        // Kiểm tra xem chuỗi có chứa nhãn tiền tố A, B, C hoặc D hay không
                        var matchPrefix = System.Text.RegularExpressions.Regex.Match(
                            opt, @"^(?:Option\s*)?([A-D])\s*[\.\:\-\)]\s*(.*)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        if (matchPrefix.Success)
                        {
                            string label = matchPrefix.Groups[1].Value.ToUpper();
                            string content = matchPrefix.Groups[2].Value.Trim();
                            if (string.IsNullOrWhiteSpace(content)) content = opt;

                            if (!labeledOptions.ContainsKey(label))
                            {
                                labeledOptions[label] = content;
                            }
                            else
                            {
                                // Trùng lặp nhãn (ví dụ có 2 đáp án cùng mang nhãn A) -> REJECT câu hỏi vì dữ liệu AI bị lỗi
                                labeledOptions = null;
                                break;
                            }
                        }
                        else
                        {
                            unlabelledOptions.Add(opt);
                        }
                    }

                    if (labeledOptions == null) continue; // Bỏ qua câu hỏi có nhãn trùng lặp

                    var normalizedOptions = new List<string>();
                    var cleanedOptionsContent = new List<string>();

                    if (labeledOptions.Count == 4)
                    {
                        // Trường hợp AI đã gắn đủ 4 nhãn A, B, C, D (dù thứ tự trong mảng có thể bị lộn xộn):
                        // Ta sắp xếp lại chuẩn theo thứ tự A, B, C, D để khớp chính xác 100% với correctAnswer
                        foreach (var lbl in new[] { "A", "B", "C", "D" })
                        {
                            string cText = labeledOptions[lbl];
                            cleanedOptionsContent.Add(cText);
                            normalizedOptions.Add($"{lbl}. {cText}");
                        }
                    }
                    else if (unlabelledOptions.Count == 4)
                    {
                        // Trường hợp 4 lựa chọn đều là văn bản thuần không có tiền tố: gán A, B, C, D theo thứ tự
                        for (int i = 0; i < 4; i++)
                        {
                            string cText = unlabelledOptions[i];
                            cleanedOptionsContent.Add(cText);
                            normalizedOptions.Add($"{prefixLetters[i]} {cText}");
                        }
                    }
                    else
                    {
                        // Trường hợp lai tạp (vừa có nhãn vừa không, hoặc chỉ có 1-2 nhãn) -> Dữ liệu không tin cậy, REJECT!
                        continue;
                    }

                    // Kiểm tra xem 4 phương án có bị trùng lặp nội dung hay không
                    if (cleanedOptionsContent.Distinct(StringComparer.OrdinalIgnoreCase).Count() < 4)
                    {
                        continue; // BỎ QUA câu hỏi nếu các lựa chọn trùng nội dung
                    }

                    // 4. Validate correctAnswer: Phải là 1 ký tự thuộc A, B, C, D
                    string parsedCorrectAnswer = "";
                    var matchAnswer = System.Text.RegularExpressions.Regex.Match(correctAnswer, @"^[A-D]\b|[A-D]");
                    if (matchAnswer.Success && validAnswerLabels.Contains(matchAnswer.Value))
                    {
                        parsedCorrectAnswer = matchAnswer.Value;
                    }

                    // NGUYÊN TẮC BẮT BUỘC: Nếu đáp án không thuộc A–D, TUYỆT ĐỐI KHÔNG gán thành "A", BỎ QUA câu hỏi này!
                    if (string.IsNullOrEmpty(parsedCorrectAnswer) || !validAnswerLabels.Contains(parsedCorrectAnswer))
                    {
                        continue;
                    }

                    // 5. Kiểm tra tính nhất quán nhãn chéo giữa correctAnswer và explanation (Cross-Consistency Check)
                    // Nếu trong lời giải thích (explanation) AI trực tiếp tuyên bố nhãn đáp án đúng nhưng lại mâu thuẫn với correctAnswer -> REJECT câu hỏi!
                    if (!string.IsNullOrWhiteSpace(explanation))
                    {
                        var expMatch = System.Text.RegularExpressions.Regex.Match(
                            explanation, @"(?i)(?:Đáp án|Phương án|Lựa chọn|Option|Correct answer|Answer)\s*(?:đúng|chính xác|là|is)?\s*[:\s]*([A-D])\b");

                        if (expMatch.Success)
                        {
                            string expAnswerLabel = expMatch.Groups[1].Value.ToUpper();
                            if (expAnswerLabel != parsedCorrectAnswer)
                            {
                                // Phát hiện mâu thuẫn trực tiếp giữa lời giải và đáp án -> REJECT câu hỏi
                                continue;
                            }
                        }
                    }

                    result.Add(new QuizQuestionDto
                    {
                        Question = question.Trim(),
                        Options = normalizedOptions,
                        CorrectAnswer = parsedCorrectAnswer,
                        Explanation = !string.IsNullOrWhiteSpace(explanation) ? explanation.Trim() : "Đáp án đúng theo chuẩn kỹ thuật chuyên ngành.",
                        Topic = !string.IsNullOrWhiteSpace(topic) ? topic.Trim() : "Kiến thức chuyên môn"
                    });
                }

                // Chỉ trả về kết quả nếu danh sách câu hỏi hợp lệ đạt đủ số lượng yêu cầu
                return result.Count >= requiredCount ? result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QuizEngine] Lỗi phân tích cú pháp JSON câu hỏi: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ngân hàng câu hỏi kỹ thuật Offline chuẩn (Offline Technical Quiz Bank)
        /// Gồm 25 câu hỏi chất lượng cao đa dạng các chủ đề: .NET, React, Vue, SQL, SOLID, Web Security, API, Docker.
        /// Áp dụng Random.Shared để xáo trộn ngẫu nhiên, bảo đảm đề 5 hoặc 10 câu không bị lặp nhóm đầu tiên.
        /// </summary>
        private static List<QuizQuestionDto> GetOfflineTechnicalQuizTemplates(int count, string cvSummary, string? jobInfo, SkillDiagnosticDto? diagnostic = null)
        {
            var bank = new List<QuizQuestionDto>
            {
                new QuizQuestionDto
                {
                    Question = "Trong ASP.NET Core, cơ chế Dependency Injection (DI) mặc định có bao nhiêu loại Service Lifetimes?",
                    Options = new List<string> {
                        "A. 2 loại: Singleton và Transient",
                        "B. 3 loại: Transient, Scoped và Singleton",
                        "C. 4 loại: Transient, Scoped, Singleton và Static",
                        "D. 1 loại: Chỉ có Scoped"
                    },
                    CorrectAnswer = "B",
                    Explanation = "ASP.NET Core hỗ trợ 3 vòng đời DI: Transient (tạo mới mỗi lần gọi), Scoped (tạo một lần cho mỗi HTTP request), và Singleton (tạo một lần duy nhất trong toàn bộ vòng đời ứng dụng).",
                    Topic = "ASP.NET Core Architecture"
                },
                new QuizQuestionDto
                {
                    Question = "Trong Middleware Pipeline của ASP.NET Core, thứ tự đăng ký nào sau đây là chuẩn xác để xử lý xác thực và phân quyền?",
                    Options = new List<string> {
                        "A. UseAuthorization() trước UseAuthentication()",
                        "B. UseRouting() -> UseAuthentication() -> UseAuthorization() -> UseEndpoints()",
                        "C. UseEndpoints() -> UseAuthentication() -> UseAuthorization()",
                        "D. UseAuthorization() -> UseRouting() -> UseAuthentication()"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Thứ tự pipeline bắt buộc: UseRouting() định tuyến URL, tiếp đến UseAuthentication() nhận diện danh tính người dùng, sau đó UseAuthorization() kiểm tra quyền truy cập, và cuối cùng UseEndpoints() thực thi endpoint.",
                    Topic = "ASP.NET Core Middleware"
                },
                new QuizQuestionDto
                {
                    Question = "Trong C#, từ khóa async/await mang lại lợi ích cốt lõi nào khi xử lý các tác vụ I/O bound?",
                    Options = new List<string> {
                        "A. Tăng tốc độ xung nhịp CPU của máy chủ",
                        "B. Giải phóng Thread hiện tại về ThreadPool trong khi chờ I/O hoàn tất để phục vụ request khác",
                        "C. Tạo thêm tiến trình (Process) độc lập mới từ hệ điều hành",
                        "D. Ép buộc mọi tác vụ phải chạy tuần tự không song song"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Từ khóa await giải phóng thread đang xử lý về ThreadPool trong thời gian chờ I/O (database, file, network) hoàn thành, giúp hệ thống tăng tối đa thông lượng xử lý đồng thời (scalability) mà không tốn tài nguyên block thread.",
                    Topic = "C# Concurrency"
                },
                new QuizQuestionDto
                {
                    Question = "Trong C#, giao diện IDisposable và khối lệnh 'using' được thiết kế nhằm mục đích gì?",
                    Options = new List<string> {
                        "A. Tăng tốc độ vòng lặp foreach",
                        "B. Giải phóng kịp thời các tài nguyên không được quản lý (Unmanaged Resources) như file handles, db connections",
                        "C. Ép Garbage Collector thu hồi bộ nhớ Managed Heap ngay lập tức",
                        "D. Chuyển đổi đối tượng sang định dạng JSON"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Khối lệnh using đảm bảo phương thức Dispose() của IDisposable luôn được gọi (kể cả khi xảy ra Exception) để giải phóng kịp thời các unmanaged resources quý giá như database connection, socket, file stream.",
                    Topic = "C# Memory Management"
                },
                new QuizQuestionDto
                {
                    Question = "Trong Entity Framework Core, phương thức nào được sử dụng để tối ưu hóa hiệu năng truy vấn chỉ đọc (Read-only)?",
                    Options = new List<string> {
                        "A. .AsNoTracking()",
                        "B. .Include()",
                        "C. .ToReadonlyList()",
                        "D. .ExecuteDelete()"
                    },
                    CorrectAnswer = "A",
                    Explanation = ".AsNoTracking() chỉ định EF Core không theo dõi thay đổi (Change Tracking) trên các thực thể trả về, giúp giảm đáng kể bộ nhớ và thời gian xử lý khi chỉ cần đọc dữ liệu hiển thị.",
                    Topic = "Entity Framework Core"
                },
                new QuizQuestionDto
                {
                    Question = "Trong Entity Framework Core, phương thức nào được sử dụng để tải dữ liệu liên quan (Eager Loading)?",
                    Options = new List<string> {
                        "A. .Join()",
                        "B. .Include()",
                        "C. .Attach()",
                        "D. .Where()"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Phương thức .Include() kết hợp với .ThenInclude() trong EF Core cho phép nạp sớm (Eager Loading) các bảng quan hệ liên quan trong cùng một câu lệnh SQL query, tránh vấn đề N+1 query.",
                    Topic = "Entity Framework Core"
                },
                new QuizQuestionDto
                {
                    Question = "Trong React, Hook nào được sử dụng để tối ưu hóa hiệu năng bằng cách ghi nhớ (memoize) giá trị tính toán tốn kém?",
                    Options = new List<string> {
                        "A. useCallback",
                        "B. useMemo",
                        "C. useRef",
                        "D. useEffect"
                    },
                    CorrectAnswer = "B",
                    Explanation = "useMemo ghi nhớ giá trị trả về của một hàm tính toán và chỉ tính lại khi dependencies thay đổi. Trong khi đó useCallback ghi nhớ định nghĩa hàm (function instance).",
                    Topic = "React Hooks & Performance"
                },
                new QuizQuestionDto
                {
                    Question = "Trong React, Virtual DOM hoạt động theo cơ chế nào để cập nhật giao diện người dùng?",
                    Options = new List<string> {
                        "A. Xóa toàn bộ Real DOM và vẽ lại từ đầu mỗi khi state thay đổi",
                        "B. So sánh cây Virtual DOM mới với cây cũ (Diffing Algorithm) và chỉ áp dụng những thay đổi tối thiểu lên Real DOM (Reconciliation)",
                        "C. Gửi yêu cầu cập nhật trực tiếp lên GPU máy khách",
                        "D. Thay thế hoàn toàn HTML bằng ảnh Canvas"
                    },
                    CorrectAnswer = "B",
                    Explanation = "React tạo cây Virtual DOM trong bộ nhớ. Khi state thay đổi, thuật toán Diffing so sánh sự khác biệt và chỉ cập nhật những phần tử thực sự thay đổi vào Real DOM (Reconciliation), giúp tối ưu hiệu năng render.",
                    Topic = "React Core Architecture"
                },
                new QuizQuestionDto
                {
                    Question = "Trong Vue.js 3 Composition API, sự khác biệt cốt lõi giữa 'ref()' và 'reactive()' là gì?",
                    Options = new List<string> {
                        "A. 'ref()' chỉ dùng cho Object, 'reactive()' chỉ dùng cho số",
                        "B. 'ref()' có thể nhận cả kiểu nguyên thủy lẫn Object và truy cập qua '.value', trong khi 'reactive()' chỉ nhận Object/Array",
                        "C. 'ref()' không hỗ trợ tính phản ứng (reactivity)",
                        "D. 'reactive()' không thể sử dụng trong hàm setup()"
                    },
                    CorrectAnswer = "B",
                    Explanation = "ref() có thể bọc bất kỳ kiểu dữ liệu nào (primitive hoặc object) và yêu cầu truy cập thông qua thuộc tính .value trong JavaScript. reactive() chỉ dùng cho đối tượng (Object, Array, Map, Set) và không cần .value.",
                    Topic = "Vue.js Reactivity"
                },
                new QuizQuestionDto
                {
                    Question = "Trong Vue.js, sự khác biệt giữa chỉ thị 'v-if' và 'v-show' là gì?",
                    Options = new List<string> {
                        "A. 'v-if' ẩn bằng CSS display:none, 'v-show' xóa phần tử khỏi DOM",
                        "B. 'v-if' thực sự tạo hoặc hủy phần tử trong DOM, 'v-show' luôn render phần tử và chỉ chuyển đổi CSS display:none",
                        "C. 'v-show' không hỗ trợ trên thẻ div",
                        "D. Cả hai hoạt động giống hệt nhau không có khác biệt"
                    },
                    CorrectAnswer = "B",
                    Explanation = "'v-if' có chi phí chuyển đổi (toggle cost) cao hơn vì trực tiếp gỡ/thêm node trong DOM. 'v-show' có chi phí khởi tạo ban đầu cao hơn nhưng chuyển đổi rất nhanh nhờ thuộc tính CSS display.",
                    Topic = "Vue.js Directives"
                },
                new QuizQuestionDto
                {
                    Question = "Chỉ mục (Index) kiểu Clustered Index trong SQL Server có đặc điểm nào sau đây?",
                    Options = new List<string> {
                        "A. Một bảng có thể có nhiều Clustered Index",
                        "B. Dữ liệu vật lý của bảng được sắp xếp trực tiếp theo thứ tự của Clustered Index",
                        "C. Clustered Index luôn chiếm nhiều dung lượng hơn Non-Clustered Index",
                        "D. Clustered Index không hỗ trợ tìm kiếm trên trường kiểu số nguyên"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Clustered Index quyết định thứ tự lưu trữ vật lý của các dòng dữ liệu trong bảng. Do đó, mỗi bảng chỉ có thể có duy nhất 1 Clustered Index.",
                    Topic = "SQL Server Optimization"
                },
                new QuizQuestionDto
                {
                    Question = "Trong cơ sở dữ liệu quan hệ, tính chất 'I' (Isolation) trong chuẩn ACID đảm bảo điều gì?",
                    Options = new List<string> {
                        "A. Dữ liệu sau khi commit sẽ được lưu vĩnh viễn",
                        "B. Toàn bộ transaction hoặc thành công hết hoặc thất bại hết",
                        "C. Các giao dịch thực thi đồng thời không can thiệp hoặc nhìn thấy trạng thái trung gian chưa hoàn thành của nhau",
                        "D. Dữ liệu luôn tuân thủ các ràng buộc toàn vẹn"
                    },
                    CorrectAnswer = "C",
                    Explanation = "Isolation (Tính cô lập) đảm bảo rằng việc thực thi đồng thời nhiều giao dịch sẽ đem lại kết quả giống như khi các giao dịch đó được thực thi tuần tự lần lượt, ngăn chặn hiện tượng Dirty Read hay Phantom Read.",
                    Topic = "Database Systems"
                },
                new QuizQuestionDto
                {
                    Question = "Trong giao thức HTTP, mã trạng thái (Status Code) nào thể hiện yêu cầu thành công nhưng không có nội dung trả về trong body?",
                    Options = new List<string> {
                        "A. 200 OK",
                        "B. 201 Created",
                        "C. 204 No Content",
                        "D. 304 Not Modified"
                    },
                    CorrectAnswer = "C",
                    Explanation = "Mã 204 No Content chỉ định rằng máy chủ đã hoàn tất thành công yêu cầu nhưng không cần trả lại bất kỳ nội dung nào trong phần thân (body) của response (thường dùng cho DELETE hoặc PUT).",
                    Topic = "RESTful API Standards"
                },
                new QuizQuestionDto
                {
                    Question = "Trong chuẩn RESTful API, các phương thức HTTP nào sau đây có tính chất Idempotent (thực thi nhiều lần cùng một request cho kết quả giống như một lần)?",
                    Options = new List<string> {
                        "A. POST và PATCH",
                        "B. GET, PUT và DELETE",
                        "C. Chỉ duy nhất GET",
                        "D. POST, GET và DELETE"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Một phương thức được coi là Idempotent nếu gọi nó N lần liên tiếp cũng tạo ra cùng một trạng thái hệ thống như gọi 1 lần. GET, PUT, DELETE là Idempotent; POST thì không.",
                    Topic = "RESTful API Standards"
                },
                new QuizQuestionDto
                {
                    Question = "Trong JavaScript, sự khác biệt cốt lõi giữa từ khóa 'let' và 'var' là gì?",
                    Options = new List<string> {
                        "A. 'var' có Block Scope, 'let' có Function Scope",
                        "B. 'let' có Block Scope, 'var' có Function Scope",
                        "C. 'let' có thể khai báo lại nhiều lần trong cùng một phạm vi",
                        "D. 'var' không bị hoisting lên đầu phạm vi"
                    },
                    CorrectAnswer = "B",
                    Explanation = "'let' và 'const' được giới thiệu trong ES6 có phạm vi theo khối lệnh (Block Scope: giữa 2 dấu ngoặc nhọn), trong khi 'var' có phạm vi theo hàm (Function Scope) và bị hoisting.",
                    Topic = "JavaScript Fundamentals"
                },
                new QuizQuestionDto
                {
                    Question = "Cơ chế Event Loop trong JavaScript hoạt động theo thứ tự ưu tiên nào giữa Microtask và Macrotask?",
                    Options = new List<string> {
                        "A. Macrotask luôn được thực thi trước tất cả Microtask",
                        "B. Microtask Queue (Promise.then, queueMicrotask) được ưu tiên xử lý hết trước khi chuyển sang Macrotask tiếp theo (setTimeout, setInterval)",
                        "C. Microtask và Macrotask được luân phiên chạy ngẫu nhiên 50/50",
                        "D. Event Loop chỉ thực thi Microtask khi bộ nhớ heap đầy"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Sau mỗi macrotask hoàn thành (hoặc sau khi Call Stack rỗng), JavaScript Engine sẽ xả sạch toàn bộ Microtask Queue trước khi lấy macrotask tiếp theo từ Callback Queue.",
                    Topic = "JavaScript Asynchronous"
                },
                new QuizQuestionDto
                {
                    Question = "Để bảo vệ ứng dụng Web khỏi tấn công Cross-Site Scripting (XSS), biện pháp nào dưới đây là quan trọng nhất?",
                    Options = new List<string> {
                        "A. Sử dụng HTTPS cho toàn bộ trang web",
                        "B. Mã hóa (Encode/Sanitize) toàn bộ dữ liệu người dùng nhập trước khi hiển thị ra trình duyệt",
                        "C. Giới hạn độ dài mật khẩu người dùng",
                        "D. Bật cờ Keep-Alive cho kết nối TCP"
                    },
                    CorrectAnswer = "B",
                    Explanation = "XSS xảy ra khi mã script độc hại được chèn và thực thi trên trình duyệt nạn nhân. Biện pháp phòng thủ cốt lõi là mã hóa đầu ra (Output Encoding/HTML Escaping) và làm sạch dữ liệu (Sanitization).",
                    Topic = "Web Security"
                },
                new QuizQuestionDto
                {
                    Question = "Cơ chế nào dưới đây giúp bảo vệ ứng dụng Web khỏi tấn công Cross-Site Request Forgery (CSRF)?",
                    Options = new List<string> {
                        "A. Sử dụng thẻ Anti-Forgery Token (Synchronizer Token Pattern) và cấu hình thuộc tính SameSite=Lax/Strict cho Cookie",
                        "B. Tăng kích thước bộ nhớ RAM máy chủ",
                        "C. Nén mã nguồn bằng Gzip",
                        "D. Cho phép mọi domain truy cập qua CORS Access-Control-Allow-Origin: *"
                    },
                    CorrectAnswer = "A",
                    Explanation = "CSRF lợi dụng cookie phiên đăng nhập tự động gửi theo request từ trang web độc hại. Anti-Forgery Token và thuộc tính Cookie SameSite đảm bảo request chỉ được chấp thuận khi xuất phát từ chính ứng dụng hợp pháp.",
                    Topic = "Web Security"
                },
                new QuizQuestionDto
                {
                    Question = "Cấu trúc của một chuỗi JSON Web Token (JWT) chuẩn bao gồm 3 phần nào, ngăn cách bởi dấu chấm ('.')?",
                    Options = new List<string> {
                        "A. Username.Password.Hash",
                        "B. Header.Payload.Signature",
                        "C. SecretKey.Claims.Timestamp",
                        "D. Algorithm.UserRole.ExpiredTime"
                    },
                    CorrectAnswer = "B",
                    Explanation = "JWT gồm 3 phần: Header (thuật toán mã hóa & loại token), Payload (thông tin claims của người dùng), và Signature (chữ ký số tạo từ Header + Payload + Secret Key để chống giả mạo).",
                    Topic = "Web Security & Authentication"
                },
                new QuizQuestionDto
                {
                    Question = "Trong nguyên lý thiết kế SOLID, chữ 'O' đại diện cho nguyên lý nào?",
                    Options = new List<string> {
                        "A. One Responsibility Principle (Nguyên lý một trách nhiệm)",
                        "B. Open/Closed Principle (Mở rộng tính năng nhưng đóng với sửa đổi mã nguồn cũ)",
                        "C. Object-Oriented Principle (Nguyên lý hướng đối tượng)",
                        "D. Output Validation Principle (Nguyên lý xác thực dữ liệu đầu ra)"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Open/Closed Principle (OCP) quy định các thành phần phần mềm (class, module, function) nên mở để mở rộng (thêm chức năng mới) nhưng đóng đối với việc chỉnh sửa mã nguồn hiện hữu.",
                    Topic = "SOLID Principles"
                },
                new QuizQuestionDto
                {
                    Question = "Trong nguyên lý SOLID, nguyên lý Dependency Inversion (DIP - chữ D) quy định điều gì?",
                    Options = new List<string> {
                        "A. Các module cấp cao không nên phụ thuộc vào các module cấp thấp; cả hai nên phụ thuộc vào Abstraction (Interface)",
                        "B. Chỉ được phép kế thừa từ một class duy nhất",
                        "C. Không được sử dụng async/await trong Controller",
                        "D. Các biến toàn cục phải được khởi tạo trước khi gọi hàm main"
                    },
                    CorrectAnswer = "A",
                    Explanation = "DIP khẳng định: Module cấp cao không phụ thuộc module cấp thấp, cả hai phụ thuộc vào Interface/Abstraction. Abstraction không phụ thuộc vào chi tiết, chi tiết phụ thuộc vào Abstraction.",
                    Topic = "SOLID Principles"
                },
                new QuizQuestionDto
                {
                    Question = "Sự khác biệt căn bản giữa Docker Image và Docker Container là gì?",
                    Options = new List<string> {
                        "A. Image là gói template chỉ đọc (Read-only Blueprint); Container là một thể hiện (Instance) đang chạy của Image có thêm lớp ghi (Read-Write Layer)",
                        "B. Image chỉ chạy trên Linux, Container chỉ chạy trên Windows",
                        "C. Image chứa database, Container chứa code nguồn",
                        "D. Container có thể chạy mà không cần bất kỳ Image nào"
                    },
                    CorrectAnswer = "A",
                    Explanation = "Docker Image là một bản thiết kế bất biến (immutable blueprint) đóng gói hệ điều hành, runtime và ứng dụng. Docker Container là instance thực thi của Image, có trạng thái động và vòng đời độc lập.",
                    Topic = "DevOps & Containers"
                },
                new QuizQuestionDto
                {
                    Question = "Trong Git, lệnh nào được sử dụng để tích hợp các commit từ nhánh tính năng vào nhánh chính bằng cách viết lại lịch sử commit thành một đường thẳng liên tục?",
                    Options = new List<string> {
                        "A. git cherry-pick",
                        "B. git rebase",
                        "C. git clone",
                        "D. git reset --hard"
                    },
                    CorrectAnswer = "B",
                    Explanation = "git rebase chuyển gốc các commit của nhánh hiện tại sang điểm cuối của nhánh đích, tạo ra lịch sử commit tuyến tính, sạch sẽ mà không sinh ra merge commit thừa.",
                    Topic = "Git & Version Control"
                },
                new QuizQuestionDto
                {
                    Question = "Độ phức tạp thời gian trung bình (Average Time Complexity) của thao tác tìm kiếm giá trị theo khóa trong bảng băm (Hash Table) là bao nhiêu?",
                    Options = new List<string> {
                        "A. O(N)",
                        "B. O(1)",
                        "C. O(log N)",
                        "D. O(N^2)"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Nhờ hàm băm (Hash Function) ánh xạ trực tiếp khóa thành chỉ số mảng, thao tác tra cứu, thêm và xóa trong bảng băm đạt độ phức tạp trung bình lý tưởng là O(1).",
                    Topic = "Data Structures & Algorithms"
                },
                new QuizQuestionDto
                {
                    Question = "Trong mô hình kiến trúc Clean Architecture, nguyên tắc 'Dependency Rule' quy định chiều phụ thuộc giữa các tầng như thế nào?",
                    Options = new List<string> {
                        "A. Tầng Core (Domain/Entities) phải phụ thuộc vào tầng Database",
                        "B. Chiều phụ thuộc của mã nguồn chỉ được phép hướng vào trong: Tầng bên ngoài (UI, DB, Frameworks) phụ thuộc tầng bên trong (UseCases, Domain)",
                        "C. Mọi tầng đều có thể gọi trực tiếp lẫn nhau không giới hạn",
                        "D. Controller được phép viết trực tiếp câu lệnh SQL đến bảng vật lý"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Quy tắc phụ thuộc trong Clean Architecture khẳng định: Mã nguồn ở tầng ngoài (Frameworks, Web, DB) phụ thuộc vào tầng nghiệp vụ bên trong (UseCases, Entities). Tầng nghiệp vụ cốt lõi không được biết gì về thế giới bên ngoài.",
                    Topic = "Software Architecture"
                }
            };

            // Phân tích Chuyên môn Trọng tâm (Domain Scoring) từ hồ sơ và mô tả công việc:
            string combined = (cvSummary + " " + (jobInfo ?? "")).ToLower();

            int frontendScore = 0;
            string[] feKeywords = { "react", "vue", "frontend", "front-end", "javascript", "typescript", "angular", "html", "css", "ui", "ux", "web developer", "spa", "nextjs", "nuxtjs", "tailwind" };
            foreach (var kw in feKeywords) if (combined.Contains(kw)) frontendScore += (kw == "react" || kw == "vue" || kw == "frontend" ? 3 : 1);

            int backendScore = 0;
            string[] beKeywords = { "backend", "back-end", ".net", "c#", "asp.net", "java", "spring", "python", "django", "fastapi", "node", "express", "php", "laravel", "c++", "golang", "api", "restful", "microservice" };
            foreach (var kw in beKeywords) if (combined.Contains(kw)) backendScore += (kw == ".net" || kw == "c#" || kw == "backend" || kw == "java" ? 3 : 1);

            int dbScore = 0;
            string[] dbKeywords = { "database", "sql", "mysql", "postgresql", "sql server", "oracle", "mongodb", "nosql", "dba", "truy vấn", "query", "index", "acid", "redis" };
            foreach (var kw in dbKeywords) if (combined.Contains(kw)) dbScore += (kw == "database" || kw == "dba" || kw == "sql server" ? 3 : 1);

            int devopsScore = 0;
            string[] devopsKeywords = { "devops", "docker", "kubernetes", "ci/cd", "cloud", "aws", "azure", "security", "bảo mật", "linux", "git", "jwt", "xss", "csrf" };
            foreach (var kw in devopsKeywords) if (combined.Contains(kw)) devopsScore += (kw == "devops" || kw == "docker" || kw == "security" ? 3 : 1);

            // GIAI ĐOẠN 2: Chấm điểm ưu tiên TRỰC TIẾP cho từng câu hỏi theo Gap Skill cụ thể
            // Khắc phục triệt để lỗi chỉ cộng điểm nhóm ngành lớn dẫn đến đề thi thiếu câu hỏi công nghệ cụ thể (ví dụ JD thiếu React nhưng đề chỉ ra câu Vue/JS)
            var scoredQuestions = bank.Select(q =>
            {
                int qScore = 0;
                string qText = $"{q.Topic} {q.Question} {q.Explanation}".ToLowerInvariant();

                // 1. Ưu tiên hàng đầu (Rank 1 - +100 điểm): Khớp trực tiếp kỹ năng còn thiếu (Gap Skills) từ JD
                if (diagnostic?.GapSkills != null && diagnostic.GapSkills.Count > 0)
                {
                    foreach (var gap in diagnostic.GapSkills)
                    {
                        string normGap = NormalizeSkillName(gap);
                        if (IsSkillMentionedInQuestion(normGap, q.Topic, q.Question, q.Explanation))
                        {
                            qScore += 100;
                        }
                    }
                }

                // 2. Ưu tiên hàng hai (Rank 2 - +35 điểm): Khớp trực tiếp kỹ năng ứng viên đã có (Matched Skills)
                if (diagnostic?.MatchedSkills != null && diagnostic.MatchedSkills.Count > 0)
                {
                    foreach (var matchSkill in diagnostic.MatchedSkills)
                    {
                        string normMatch = NormalizeSkillName(matchSkill);
                        if (IsSkillMentionedInQuestion(normMatch, q.Topic, q.Question, q.Explanation))
                        {
                            qScore += 35;
                        }
                    }
                }

                // 3. Ưu tiên hàng ba (+10 điểm): Khớp định hướng chuyên môn tổng quát
                if (frontendScore > 0 && (qText.Contains("react") || qText.Contains("vue") || qText.Contains("javascript") || qText.Contains("css"))) qScore += 10;
                if (backendScore > 0 && (qText.Contains(".net") || qText.Contains("c#") || qText.Contains("asp.net") || qText.Contains("entity framework"))) qScore += 10;
                if (dbScore > 0 && (qText.Contains("sql") || qText.Contains("database") || qText.Contains("acid"))) qScore += 10;
                if (devopsScore > 0 && (qText.Contains("docker") || qText.Contains("git") || qText.Contains("security"))) qScore += 10;

                // 4. Nền tảng kỹ sư chung (+5 điểm): SOLID, Clean Architecture, Git, Cấu trúc dữ liệu
                if (q.Topic.Contains("SOLID") || q.Topic.Contains("Architecture") || q.Topic.Contains("Data Structures") || q.Topic.Contains("Git"))
                {
                    qScore += 5;
                }

                return new { Question = q, Score = qScore };
            }).ToList();

            // Sắp xếp câu hỏi theo điểm ưu tiên giảm dần; trong cùng mức điểm thì xáo trộn ngẫu nhiên bằng Random.Shared
            var finalQuestions = scoredQuestions
                .OrderByDescending(x => x.Score)
                .ThenBy(_ => Random.Shared.Next())
                .Select(x => x.Question)
                .Take(count)
                .ToList();

            return finalQuestions;
        }

        /// <summary>
        /// Kiểm tra câu hỏi trắc nghiệm có đề cập trực tiếp đến kỹ năng chuyên môn cần đánh giá hay không
        /// </summary>
        private static bool IsSkillMentionedInQuestion(string normSkill, string topic, string question, string explanation)
        {
            if (string.IsNullOrWhiteSpace(normSkill)) return false;
            string text = $"{topic} {question} {explanation}".ToLowerInvariant();

            if (normSkill == "react") return text.Contains("react");
            if (normSkill == "vue") return text.Contains("vue");
            if (normSkill == "csharp") return text.Contains("c#") || text.Contains("csharp");
            if (normSkill == "dotnet") return text.Contains(".net") || text.Contains("dotnet") || text.Contains("asp.net");
            if (normSkill == "sqlserver" || normSkill == "sql") return text.Contains("sql") || text.Contains("database");
            if (normSkill == "docker") return text.Contains("docker") || text.Contains("container");
            if (normSkill == "javascript") return text.Contains("javascript") || text.Contains(" js ");
            if (normSkill == "typescript") return text.Contains("typescript") || text.Contains(" ts ");
            if (normSkill == "solid") return text.Contains("solid");
            if (normSkill == "cleanarchitecture") return text.Contains("clean architecture") || text.Contains("architecture");
            if (normSkill == "git") return text.Contains("git");

            return text.Contains(normSkill);
        }

        /// <summary>
        /// GIAI ĐOẠN 4: Sinh các khuyến nghị ôn tập và lộ trình khắc phục chuyên môn theo nhóm lỗi (Weaknesses)
        /// Sử dụng phân tích rule-based chuyên gia offline, bảo đảm 0ms latency và không phụ thuộc mạng
        /// </summary>
        private static List<string> GenerateQuizRecommendations(List<string> weaknesses)
        {
            var recs = new List<string>();
            if (weaknesses == null || weaknesses.Count == 0)
            {
                recs.Add("Năng lực chuyên môn của bạn rất đồng đều và toàn diện! Hãy tiếp tục duy trì và thử sức với các bài thi trắc nghiệm nâng cao theo từng vị trí tuyển dụng cụ thể.");
                return recs;
            }

            foreach (var w in weaknesses)
            {
                string topic = w.ToLowerInvariant();
                if (topic.Contains("react"))
                {
                    recs.Add("React: Ôn lại cơ chế Virtual DOM, Memoization (useMemo, useCallback) và các quy tắc tối ưu vòng đời re-render của Component.");
                }
                else if (topic.Contains("vue"))
                {
                    recs.Add("Vue.js: Nắm vững hệ thống phản ứng (Reactivity) với ref()/reactive() và phân biệt chi phí chuyển đổi giữa v-if và v-show.");
                }
                else if (topic.Contains("asp.net") || topic.Contains("middleware"))
                {
                    recs.Add("ASP.NET Core: Củng cố 3 vòng đời Dependency Injection (Transient, Scoped, Singleton) và thứ tự pipeline middleware chuẩn xác.");
                }
                else if (topic.Contains("c#") || topic.Contains("concurrency") || topic.Contains("memory"))
                {
                    recs.Add("C# & .NET: Ôn lại nguyên lý giải phóng Thread của async/await và mẫu thiết kế IDisposable để thu hồi unmanaged resources.");
                }
                else if (topic.Contains("sql") || topic.Contains("database"))
                {
                    recs.Add("Database & SQL: Tìm hiểu sâu về Clustered vs Non-Clustered Index, phân tích Execution Plan và 4 tính chất ACID trong giao dịch.");
                }
                else if (topic.Contains("entity framework"))
                {
                    recs.Add("Entity Framework Core: Áp dụng .AsNoTracking() cho truy vấn chỉ đọc và tối ưu hóa Eager Loading (.Include) để tránh lỗi N+1.");
                }
                else if (topic.Contains("solid") || topic.Contains("architecture"))
                {
                    recs.Add("Kiến trúc phần mềm: Nắm vững nguyên tắc Đảo ngược phụ thuộc (DIP) trong SOLID và quy tắc Dependency Rule trong Clean Architecture.");
                }
                else if (topic.Contains("devops") || topic.Contains("docker") || topic.Contains("git"))
                {
                    recs.Add("DevOps & Git: Thực hành quản lý commit an toàn (rebase vs merge) và các lệnh quản lý Container/Volume trong Docker.");
                }
                else
                {
                    recs.Add($"Chuyên đề {w}: Dành thời gian xem lại lý thuyết cốt lõi và các câu hỏi phỏng vấn thực tế thuộc chủ đề này.");
                }
            }

            return recs.Distinct().ToList();
        }

        #endregion
    }
}

