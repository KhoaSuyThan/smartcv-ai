using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

        public InterviewController(AppDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        #region 1. CHỨC NĂNG LUYỆN TẬP TỰ DO (ỨNG VIÊN)

        // Trang chủ Luyện phỏng vấn tự do: Cho phép chọn CV và chế độ luyện tập
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Practice()
        {
            int userId = CurrentUserId;
            // Lấy danh sách CV của người dùng này để chọn làm nguồn phỏng vấn
            var resumes = await _context.Resumes
                .Where(r => r.UserID == userId && r.IsDraft == false)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            // Lấy lịch sử các phiên phỏng vấn luyện tập
            var history = await _context.InterviewSessions
                .Include(s => s.Resume)
                .Where(s => s.UserID == userId && s.InterviewType != 2)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            ViewBag.Resumes = resumes;
            ViewBag.History = history;
            return View();
        }

        // Bắt đầu một phiên phỏng vấn luyện tập mới
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartPracticeSession(int resumeId, int interviewType)
        {
            int userId = CurrentUserId;

            // Kiểm tra CV có tồn tại và thuộc về user không
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == resumeId && r.UserID == userId);
            if (resume == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV hợp lệ.";
                return RedirectToAction(nameof(Practice));
            }

            // Tạo mới một phiên phỏng vấn
            var session = new InterviewSession
            {
                UserID = userId,
                ResumeID = resumeId,
                InterviewType = interviewType, // 0: Tự luận chat 1-1, 1: Trắc nghiệm
                Status = 0, // Đang diễn ra
                CreatedAt = DateTime.Now
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
                // CHẾ ĐỘ TRẮC NGHIỆM: Sinh 5 câu hỏi trắc nghiệm dựa trên CV
                string prompt = $@"Hãy đóng vai là một nhà tuyển dụng kỹ thuật chuyên nghiệp. Dựa trên thông tin CV của ứng viên sau đây, hãy sinh ra đúng 5 câu hỏi trắc nghiệm để kiểm tra kiến thức chuyên môn kỹ thuật liên quan đến CV.
Mỗi câu hỏi phải bao gồm nội dung câu hỏi, 4 đáp án lựa chọn A, B, C, D (ghi rõ nhãn 'A.', 'B.', 'C.', 'D.') và chỉ rõ chữ cái đáp án đúng (A, B, C hoặc D).
Yêu cầu trả về duy nhất một chuỗi JSON hợp lệ theo định dạng mảng đối tượng như mẫu sau (không chứa ký tự markdown hay văn bản thừa bên ngoài):
[
  {{
    ""question"": ""Nội dung câu hỏi 1?"",
    ""options"": [""A. Tùy chọn A"", ""B. Tùy chọn B"", ""C. Tùy chọn C"", ""D. Tùy chọn D""],
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
                        int index = 1;
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            string qText = item.GetProperty("question").GetString();
                            var optList = item.GetProperty("options").EnumerateArray().Select(o => o.GetString()).ToList();
                            string correctAns = item.GetProperty("correctAnswer").GetString();

                            var msg = new InterviewMessage
                            {
                                SessionID = session.SessionID,
                                Role = "interviewer",
                                Content = qText,
                                ChoicesJson = JsonSerializer.Serialize(optList),
                                SelectedAnswer = correctAns, // Lưu đáp án đúng vào cột SelectedAnswer của interviewer để đối chiếu
                                CreatedAt = DateTime.Now.AddSeconds(index)
                            };
                            _context.InterviewMessages.Add(msg);
                            index++;
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
                // CHẾ ĐỘ CHAT TỰ LUẬN 1-1: Sinh câu hỏi đầu tiên
                string prompt = $@"Bạn là nhà phỏng vấn tuyển dụng AI chuyên nghiệp. Dựa trên thông tin CV của ứng viên dưới đây, hãy đặt câu hỏi phỏng vấn tự luận đầu tiên để kiểm tra kinh nghiệm hoặc kỹ năng nổi bật của ứng viên.
Câu hỏi cần ngắn gọn, trực diện và tạo cơ hội để ứng viên kể về kinh nghiệm thực tế. Chỉ trả về duy nhất nội dung câu hỏi phỏng vấn.

Thông tin CV ứng viên:
{cvSummary}";

                try
                {
                    string firstQuestion = await _aiService.GenerateContent(prompt);
                    await LogAiUsage("interview_chat_start", prompt, firstQuestion);
                    var msg = new InterviewMessage
                    {
                        SessionID = session.SessionID,
                        Role = "interviewer",
                        Content = firstQuestion.Trim(),
                        CreatedAt = DateTime.Now
                    };
                    _context.InterviewMessages.Add(msg);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(PracticeChat), new { sessionId = session.SessionID });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "AI không thể đặt câu hỏi phỏng vấn lúc này. Chi tiết: " + ex.Message;
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

            return View(session);
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

            string lastQuestion = lastQuestionMsg != null ? lastQuestionMsg.Content : "Hãy tự giới thiệu bản thân.";

            // 2. Chấm điểm câu trả lời vừa gửi bằng AI theo mô hình STAR
            string evaluationPrompt = $@"Bạn là nhà phỏng vấn AI chuyên nghiệp. Hãy đánh giá câu trả lời của ứng viên cho câu hỏi phỏng vấn dưới đây theo phương pháp STAR (S - Situation, T - Task, A - Action, R - Result).
Cho điểm câu trả lời trên thang điểm từ 0 đến 100.
Yêu cầu trả về duy nhất một chuỗi JSON hợp lệ theo định dạng dưới đây (không chứa văn bản phụ):
{{
  ""score"": 85,
  ""feedback"": ""Nhận xét chi tiết theo cấu trúc STAR: Tình huống, Nhiệm vụ, Hành động, Kết quả. Nêu ưu điểm và điểm cần cải thiện.""
}}

Câu hỏi của bạn: ""{lastQuestion}""
Câu trả lời của ứng viên: ""{answer}""";

            int itemScore = 70; // Điểm mặc định nếu AI lỗi
            string itemFeedback = "Đã ghi nhận câu trả lời.";
            try
            {
                string evalResponse = await _aiService.GenerateContent(evaluationPrompt, responseJson: true);
                evalResponse = CleanJson(evalResponse);
                await LogAiUsage("interview_chat_eval", evaluationPrompt, evalResponse);

                using (JsonDocument doc = JsonDocument.Parse(evalResponse))
                {
                    var root = doc.RootElement;
                    itemScore = root.GetProperty("score").GetInt32();
                    itemFeedback = root.GetProperty("feedback").GetString();
                }
            }
            catch { }

            candidateMsg.Score = itemScore;
            candidateMsg.Feedback = itemFeedback;
            await _context.SaveChangesAsync();

            // Tính số lượng câu trả lời đã gửi trong phiên này
            int answerCount = session.Messages.Count(m => m.Role == "candidate");

            if (answerCount >= 3) // Kết thúc phỏng vấn sau 3 lượt trả lời
            {
                session.Status = 1; // Đã hoàn thành
                session.CompletedAt = DateTime.Now;

                // Tính điểm trung bình cộng các câu trả lời
                var candidateMsgs = await _context.InterviewMessages
                    .Where(m => m.SessionID == sessionId && m.Role == "candidate")
                    .ToListAsync();
                
                double avgScore = candidateMsgs.Any() ? candidateMsgs.Average(m => m.Score ?? 0) : 0;
                session.OverallScore = (int)Math.Round(avgScore);

                // Tạo nhận xét tổng quan từ AI
                string summaryPrompt = $@"Hãy tổng hợp buổi phỏng vấn dưới đây của ứng viên. Đưa ra nhận xét tổng quan về thế mạnh chuyên môn, kỹ năng giao tiếp xử lý tình huống và chấm điểm chung cuộc.
Câu hỏi & Trả lời phỏng vấn:
" + string.Join("\n", candidateMsgs.Select((m, i) => $"Câu {i+1} (Điểm: {m.Score}): {m.Content}"));

                string finalEvaluation = "Hoàn thành buổi phỏng vấn.";
                try
                {
                    finalEvaluation = await _aiService.GenerateContent(summaryPrompt);
                    await LogAiUsage("interview_chat_summary", summaryPrompt, finalEvaluation);
                }
                catch { }

                session.AiEvaluation = finalEvaluation;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bạn đã hoàn thành buổi phỏng vấn luyện tập!";
                return RedirectToAction(nameof(PracticeChat), new { sessionId = sessionId });
            }
            else
            {
                // Sinh câu hỏi tiếp theo
                string cvSummary = $"Họ tên: {session.Resume.FullName}, Vị trí: {session.Resume.JobTitle}.";
                string nextQuestionPrompt = $@"Dựa trên thông tin CV ứng viên và lịch sử hội thoại phỏng vấn dưới đây, hãy đặt câu hỏi phỏng vấn tự luận tiếp theo (câu hỏi số {answerCount + 1}/3).
Câu hỏi nên nối tiếp tự nhiên từ ý của câu trả lời trước đó hoặc hỏi sang một mảng kinh nghiệm khác trong CV. Chỉ trả về duy nhất nội dung câu hỏi mới.

CV ứng viên:
{cvSummary}

Lịch sử phỏng vấn:
" + string.Join("\n", session.Messages.OrderBy(m => m.CreatedAt).Select(m => $"{(m.Role == "interviewer" ? "Người phỏng vấn" : "Ứng viên")}: {m.Content}")) + "\nĐặt câu hỏi tiếp theo:";

                try
                {
                    string nextQuestion = await _aiService.GenerateContent(nextQuestionPrompt);
                    var nextMsg = new InterviewMessage
                    {
                        SessionID = sessionId,
                        Role = "interviewer",
                        Content = nextQuestion.Trim(),
                        CreatedAt = DateTime.Now
                    };
                    _context.InterviewMessages.Add(nextMsg);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Không thể sinh câu hỏi tiếp theo: " + ex.Message;
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
            foreach (var qMsg in session.Messages.Where(m => m.Role == "interviewer"))
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
        public async Task<IActionResult> GetAiSuggestedQuestions(int jobId, int resumeId)
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

            string prompt = $@"Bạn là trợ lý AI chuyên nghiệp hỗ trợ nhà tuyển dụng chuẩn bị phỏng vấn. Hãy so sánh độ lệch giữa CV ứng viên và mô tả công việc (JD) dưới đây, từ đó sinh ra đúng 3 câu hỏi phỏng vấn tự luận sắc sảo kèm gợi ý đáp án/tiêu chí chấm điểm để đánh giá năng lực của ứng viên đó.
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

            string prompt = $@"Bạn là nhà phỏng vấn chuyên nghiệp chấm điểm câu trả lời của ứng viên.
Dựa trên câu hỏi phỏng vấn, tiêu chí chấm điểm và câu trả lời thực tế của ứng viên dưới đây, hãy chấm điểm (từ 0 đến 100) và viết nhận xét chi tiết theo phương pháp STAR.
Yêu cầu trả về định dạng JSON (không chứa văn bản phụ):
{{
  ""score"": 80,
  ""feedback"": ""Nhận xét chi tiết...""
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
                    int score = root.GetProperty("score").GetInt32();
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

        #endregion
    }
}
