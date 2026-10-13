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
        public async Task<IActionResult> StartPracticeSession(int resumeId, int interviewType, int questionCount, int? jobId = null)
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

            // 3. Tự động liên kết JobID nếu chưa truyền trực tiếp (tìm trong đơn ứng tuyển hoặc cache SmartMatch)
            if (!jobId.HasValue || jobId.Value <= 0)
            {
                jobId = await _context.Applications
                    .Where(a => a.ResumeID == resumeId)
                    .OrderByDescending(a => a.AppliedAt)
                    .Select(a => (int?)a.JobID)
                    .FirstOrDefaultAsync();

                if (!jobId.HasValue)
                {
                    try
                    {
                        string cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "JobMatchResults");
                        if (Directory.Exists(cacheDir))
                        {
                            var files = Directory.GetFiles(cacheDir, "*.json");
                            foreach (var file in files)
                            {
                                string text = await System.IO.File.ReadAllTextAsync(file);
                                if (text.Contains($"\"resumeId\": {resumeId}") || text.Contains($"\"resumeId\":{resumeId}"))
                                {
                                    string fName = Path.GetFileNameWithoutExtension(file);
                                    if (int.TryParse(fName, out int parsedJobId))
                                    {
                                        jobId = parsedJobId;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
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

            // Tạo mới một phiên phỏng vấn
            // Dùng cột AiEvaluation làm nơi lưu cấu hình số câu hỏi STAR tạm thời: "MaxQuestions:X"
            var session = new InterviewSession
            {
                UserID = userId,
                JobID = jobId,
                ResumeID = resumeId,
                InterviewType = interviewType, // 0: Tự luận chat 1-1, 1: Trắc nghiệm
                Status = 0, // Đang diễn ra
                CreatedAt = DateTime.Now,
                AiEvaluation = interviewType == 0 ? ("MaxQuestions:" + questionCount) : null
            };

            _context.InterviewSessions.Add(session);
            await _context.SaveChangesAsync();

            // Rút gọn thông tin CV để truyền vào LLM nhằm tiết kiệm token và tăng tốc độ xử lý
            string cvSummary = $"Họ tên: {resume.FullName}, Vị trí ứng tuyển: {resume.JobTitle}, Giới thiệu: {resume.Summary}.";
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
                                string company = item.TryGetProperty("name", out var cName) ? cName.GetString() : "";
                                string position = item.TryGetProperty("position", out var pos) ? pos.GetString() : "";
                                string desc = item.TryGetProperty("summary", out var sName) ? sName.GetString() : "";
                                cvSummary += $"- Tại {company} làm {position}: {desc}. ";
                            }
                        }
                        if (root.TryGetProperty("skills", out var skillsProp) && skillsProp.ValueKind == JsonValueKind.Array)
                        {
                            cvSummary += " Kỹ năng: ";
                            var skillsList = new List<string>();
                            foreach (var item in skillsProp.EnumerateArray())
                            {
                                if (item.TryGetProperty("name", out var sName))
                                {
                                    skillsList.Add(sName.GetString());
                                }
                            }
                            cvSummary += string.Join(", ", skillsList) + ".";
                        }
                    }
                }
                catch { }
            }

            if (interviewType == 1)
            {
                // CHẾ ĐỘ TRẮC NGHIỆM: Sinh câu hỏi trắc nghiệm chuyên môn kỹ thuật trực tiếp cho ứng viên dựa trên công nghệ trong CV
                string prompt = $@"Bạn là một nhà tuyển dụng kỹ thuật chuyên nghiệp. Dựa trên thông tin CV của ứng viên dưới đây, hãy sinh ra đúng {questionCount} câu hỏi trắc nghiệm kiểm tra kiến thức chuyên môn kỹ thuật thực tế.

Yêu cầu QUAN TRỌNG VỀ NỘI DUNG CÂU HỎI:
1. Đây là bài kiểm tra dành cho ứng viên tự làm. Câu hỏi phải là câu hỏi kỹ thuật, kiến thức lý thuyết chuyên môn hoặc kinh nghiệm thực tế (Ví dụ: hỏi về cách giải quyết bài toán cụ thể trong React/SQL, khái niệm OOP, cách tối ưu hóa truy vấn, HTTP protocol,... liên quan đến các kỹ năng công nghệ được liệt kê trong CV).
2. Tuyệt đối KHÔNG đặt các câu hỏi hỏi về thông tin có sẵn trên CV của ứng viên (Ví dụ KHÔNG hỏi: 'Ứng viên đã học công nghệ nào?', 'Ứng viên làm việc ở đâu?', 'Ứng viên có bao nhiêu năm kinh nghiệm?', 'Ứng viên có kỹ năng mềm gì?'). Người trả lời là chính ứng viên, do đó họ cần được thử thách bằng câu hỏi kiến thức kỹ thuật thực tế.

Mỗi câu hỏi phải bao gồm nội dung câu hỏi, 4 đáp án lựa chọn A, B, C, D (ghi rõ nhãn 'A.', 'B.', 'C.', 'D.') và chỉ rõ chữ cái đáp án đúng (A, B, C hoặc D).
Yêu cầu trả về duy nhất một chuỗi JSON hợp lệ theo định dạng mảng đối tượng như mẫu sau (không chứa ký tự markdown hay văn bản thừa bên ngoài):
[
  {{
    ""question"": ""Nội dung câu hỏi kỹ thuật trực tiếp cần hỏi ứng viên?"",
    ""options"": [""A. Đáp án A"", ""B. Đáp án B"", ""C. Đáp án C"", ""D. Đáp án D""],
    ""correctAnswer"": ""A""
  }}
]

Thông tin CV ứng viên:
{cvSummary}";

                try
                {
                    string aiResponse = await _aiService.GenerateContent(prompt, responseJson: true);
                    aiResponse = CleanJson(aiResponse);
                    await LogAiUsage("interview_quiz", prompt, aiResponse);

                    using (JsonDocument doc = JsonDocument.Parse(aiResponse))
                    {
                        var root = doc.RootElement;
                        var questionElements = new List<JsonElement>();

                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in root.EnumerateArray())
                            {
                                questionElements.Add(el);
                            }
                        }
                        else if (root.ValueKind == JsonValueKind.Object)
                        {
                            bool foundArray = false;
                            foreach (var prop in root.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var el in prop.Value.EnumerateArray())
                                    {
                                        questionElements.Add(el);
                                    }
                                    foundArray = true;
                                    break;
                                }
                            }
                            if (!foundArray)
                            {
                                foreach (var prop in root.EnumerateObject())
                                {
                                    // Chấp nhận Object có thuộc tính tương tự question (không phân biệt hoa thường)
                                    if (prop.Value.ValueKind == JsonValueKind.Object)
                                    {
                                        bool isQuestionObj = false;
                                        foreach (var subProp in prop.Value.EnumerateObject())
                                        {
                                            if (subProp.Name.Equals("question", StringComparison.OrdinalIgnoreCase))
                                            {
                                                isQuestionObj = true;
                                                break;
                                            }
                                        }
                                        if (isQuestionObj)
                                        {
                                            questionElements.Add(prop.Value);
                                        }
                                    }
                                }
                            }
                        }

                        if (!questionElements.Any())
                        {
                            throw new Exception("AI phản hồi sai định dạng JSON. Vui lòng thử lại.");
                        }

                        int index = 1;
                        int addedCount = 0;
                        foreach (var item in questionElements)
                        {
                            if (item.ValueKind == JsonValueKind.Object)
                            {
                                string qText = null;
                                List<string> optList = new List<string>();
                                string correctAns = null;

                                foreach (var prop in item.EnumerateObject())
                                {
                                    string name = prop.Name.ToLower();
                                    if (name == "question")
                                    {
                                        qText = prop.Value.GetString();
                                    }
                                    else if (name == "options")
                                    {
                                        if (prop.Value.ValueKind == JsonValueKind.Array)
                                        {
                                            optList = prop.Value.EnumerateArray().Select(o => o.GetString()).ToList();
                                        }
                                    }
                                    else if (name == "correctanswer" || name == "correct_answer" || name == "answer")
                                    {
                                        correctAns = prop.Value.GetString();
                                    }
                                }

                                if (!string.IsNullOrEmpty(qText) && optList.Any() && !string.IsNullOrEmpty(correctAns))
                                {
                                    var msg = new InterviewMessage
                                    {
                                        SessionID = session.SessionID,
                                        Role = "interviewer",
                                        Content = qText,
                                        ChoicesJson = JsonSerializer.Serialize(optList),
                                        SelectedAnswer = correctAns,
                                        CreatedAt = DateTime.Now.AddSeconds(index)
                                    };
                                    _context.InterviewMessages.Add(msg);
                                    index++;
                                    addedCount++;
                                }
                            }
                        }

                        if (addedCount == 0)
                        {
                            throw new Exception("AI sinh ra câu hỏi nhưng không đúng định dạng thuộc tính yêu cầu (question, options, correctAnswer).");
                        }

                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(PracticeQuiz), new { sessionId = session.SessionID });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "AI không thể khởi tạo câu hỏi trắc nghiệm lúc này. Chi tiết: " + ex.Message;
                    _context.InterviewSessions.Remove(session);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Practice));
                }
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

                try
                {
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

                    return RedirectToAction(nameof(PracticeChat), new { sessionId = session.SessionID });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "AI không thể lưu câu hỏi phỏng vấn lúc này. Chi tiết: " + ex.Message;
                    _context.InterviewSessions.Remove(session);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Practice));
                }
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

        // Màn hình làm bài thi trắc nghiệm luyện tập
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PracticeQuiz(int sessionId)
        {
            int userId = CurrentUserId;
            var session = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId && s.UserID == userId);

            if (session == null || session.InterviewType != 1)
            {
                return NotFound("Không tìm thấy phiên trắc nghiệm hợp lệ.");
            }

            session.Messages = session.Messages.OrderBy(m => m.CreatedAt).ToList();

            return View(session);
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

            if (session == null || session.Status == 1)
            {
                return BadRequest("Phiên làm bài không hợp lệ hoặc đã kết thúc.");
            }

            int correctCount = 0;
            int totalQuestions = session.Messages.Count(m => m.Role == "interviewer");

            // Duyệt danh sách các câu hỏi trắc nghiệm của phiên để đối chiếu kết quả
            foreach (var qMsg in session.Messages.Where(m => m.Role == "interviewer").ToList())
            {
                string chosenAnswer = answers.ContainsKey(qMsg.MessageID) ? answers[qMsg.MessageID] : "";
                string correctAnswer = qMsg.SelectedAnswer; // Đã lưu đáp án đúng vào SelectedAnswer của interviewer lúc khởi tạo

                // Lưu câu trả lời của ứng viên
                var candidateAnsMsg = new InterviewMessage
                {
                    SessionID = sessionId,
                    Role = "candidate",
                    Content = chosenAnswer,
                    SelectedAnswer = chosenAnswer,
                    Feedback = chosenAnswer == correctAnswer ? "Đúng" : $"Sai (Đáp án đúng: {correctAnswer})",
                    Score = chosenAnswer == correctAnswer ? 100 : 0,
                    CreatedAt = DateTime.Now
                };
                _context.InterviewMessages.Add(candidateAnsMsg);

                if (chosenAnswer == correctAnswer)
                {
                    correctCount++;
                }
            }

            session.Status = 1; // Hoàn thành
            session.CompletedAt = DateTime.Now;
            session.OverallScore = (int)Math.Round((double)correctCount / totalQuestions * 100);
            session.AiEvaluation = $"Kết quả làm bài trắc nghiệm: Trả lời đúng {correctCount}/{totalQuestions} câu hỏi. Đạt tỉ lệ {(double)correctCount / totalQuestions * 100}%.";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã nộp bài trắc nghiệm thành công!";
            return RedirectToAction(nameof(PracticeQuiz), new { sessionId = sessionId });
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
    }
}
