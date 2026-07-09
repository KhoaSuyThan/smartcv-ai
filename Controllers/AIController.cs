using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoAnCS.Services;
using DoAnCS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TiktokenSharp;
using Microsoft.AspNetCore.RateLimiting; // Sử dụng middleware Rate Limiting

namespace DoAnCS.Controllers 
{
    [EnableRateLimiting("AiApiPolicy")] // Áp dụng Rate Limit theo IP cho toàn bộ Controller xử lý AI
    public class AIController : Controller
    {
        private readonly IAIService _aiService;
        private readonly DoAnCS.Data.AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEncryptionService _encryptionService; // Thêm dịch vụ giải mã API Key

        // Tiêm (Inject) Service xử lý AI và AppDbContext thông qua Constructor
        public AIController(
            IAIService aiService, 
            DoAnCS.Data.AppDbContext context, 
            IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            IEncryptionService encryptionService) // Inject Encryption Service
        {
            _aiService = aiService;
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _encryptionService = encryptionService;
        }

        // ============================================================
        // CHATBOX AI - Endpoint công khai, dùng API key riêng
        // ============================================================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ChatboxAsk([FromBody] ChatboxRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return Json(new { success = false, reply = "Vui lòng nhập câu hỏi!" });

            // 1. Lấy cấu hình từ DB
            var dbConfig = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();

            // 2. Sử dụng ChatbotApiKey (dành riêng cho Groq Chatbot)
            var chatbotApiKey = dbConfig?.ChatbotApiKey;
            if (string.IsNullOrEmpty(chatbotApiKey))
                return Json(new { success = false, reply = "Tính năng Chatbot đang được bảo trì. Vui lòng quay lại sau!" });

            // Giải mã Chatbot API key được mã hóa bảo mật từ DB
            chatbotApiKey = _encryptionService.Decrypt(chatbotApiKey).Trim();

            // 3. Chuẩn bị Request Body cho Groq
            var systemPrompt = dbConfig?.ChatbotSystemInstruction;
            if (string.IsNullOrWhiteSpace(systemPrompt)) 
            {
                systemPrompt = @"Bạn là trợ lý ảo của website CVBuilder Pro - nền tảng tạo CV và tìm việc làm IT tại Việt Nam.
Nhiệm vụ: Giải đáp thắc mắc của người dùng về dịch vụ, hướng dẫn sử dụng, tư vấn CV/nghề nghiệp.
Quy tắc:
- Trả lời ngắn gọn, thân thiện, bằng tiếng Việt.
- Chỉ trả lời các câu hỏi liên quan đến việc làm, CV, tài khoản, dịch vụ của CVBuilder Pro.
- Nếu câu hỏi ngoài phạm vi, hướng dẫn liên hệ hỗ trợ.
- KHÔNG tiết lộ thông tin kỹ thuật nội bộ.
- Giới hạn câu trả lời trong 150 từ.";
            }

            var requestBody = new
            {
                model = "llama-3.1-8b-instant", // Sử dụng model Llama 3.1 8B
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = request.Message }
                },
                temperature = 0.7,
                max_tokens = 512
            };

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var url = "https://api.groq.com/openai/v1/chat/completions";
                
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var requestMsg = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                requestMsg.Headers.Add("Authorization", $"Bearer {chatbotApiKey}");

                var response = await httpClient.SendAsync(requestMsg);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[Chatbox] HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    if (result?.choices != null && result.choices.Count > 0)
                    {
                        string reply = result.choices[0].message.content;
                        
                        // --- GHI LOG SỬ DỤNG VÀO DATABASE ---
                        try {
                            int userId = 0;
                            var userIdClaim = User.FindFirst("UserID");
                            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int pid)) userId = pid;

                            int tokens = (request.Message.Length / 4) + (reply.Length / 4) + 100; // Ước lượng + system prompt
                            var log = new AILog {
                                UserID = userId > 0 ? userId : (int?)null,
                                RequestType = "chatbot",
                                InputText = request.Message,
                                OutputText = reply,
                                UsedTokens = tokens,
                                ApiProvider = "Groq",
                                CreatedAt = DateTime.Now
                            };
                            _context.AILogs.Add(log);

                            // Cập nhật tích lũy vào config (Id=1)
                            var trackedConfig = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == 1);
                            if (trackedConfig != null) trackedConfig.TotalTokensUsed += tokens;

                            await _context.SaveChangesAsync();
                        } catch { /* Bỏ qua nếu lỗi log để ko chặn người dùng */ }

                        return Json(new { success = true, reply });
                    }
                    return Json(new { success = false, reply = "AI không tạo được câu trả lời. Vui lòng thử câu hỏi khác!" });
                }

                // Lỗi API - trả về status code cụ thể
                string errorMsg = (int)response.StatusCode switch
                {
                    429 => "Chatbot đang bị quá tải, vui lòng thử lại sau vài phút!",
                    401 or 403 => "Lỗi hệ thống, hãy liên hệ admin để giải quyết",
                    400 => "Yêu cầu không hợp lệ.",
                    _ => $"Lỗi kết nối AI ({(int)response.StatusCode}). Vui lòng liên hệ Admin!"
                };
                return Json(new { success = false, reply = errorMsg });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chatbox Error: " + ex.Message);
                return Json(new { success = false, reply = "Hệ thống đang bận. Vui lòng thử lại sau!" });
            }
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> TranslateCV([FromBody] TranslateCVRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.JsonContent) || string.IsNullOrWhiteSpace(request?.TargetLanguage))
            {
                Console.WriteLine($"[TranslateCV] Validation FAILED! TargetLanguage: '{request?.TargetLanguage}', JsonContent: '{request?.JsonContent}'");
                return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ!" });
            }

            int userId = 0;
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId)) 
            {
                userId = parsedId;
            }

            if (userId == 0 && (User.Identity == null || !User.Identity.IsAuthenticated))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng tính năng dịch thuật CV!" });
            }

            string languageText = request.TargetLanguage.ToLower() switch
            {
                "en" => "Tiếng Anh (English)",
                "ja" => "Tiếng Nhật (Japanese)",
                "ko" => "Tiếng Hàn (Korean)",
                "vi" => "Tiếng Việt (Vietnamese)",
                _ => request.TargetLanguage
            };

            string originalAvatarUrl = null;
            string payloadJson = request.JsonContent;

            try
            {
                var cvObj = Newtonsoft.Json.Linq.JObject.Parse(request.JsonContent);
                if (cvObj["general"] != null && cvObj["general"]["avatarUrl"] != null)
                {
                    originalAvatarUrl = cvObj["general"]["avatarUrl"]?.ToString();
                    cvObj["general"]["avatarUrl"] = ""; // Tạm xóa để tiết kiệm token và tránh bị tràn/cắt cụt dữ liệu
                    payloadJson = cvObj.ToString(Newtonsoft.Json.Formatting.None);
                }
            }
            catch (Exception pEx)
            {
                Console.WriteLine($"[TranslateCV] Cảnh báo parse JSON đầu vào: {pEx.Message}");
            }

            string prompt = $@"Bạn là chuyên gia dịch thuật CV và tối ưu hóa hồ sơ chuyên nghiệp. Hãy dịch toàn bộ nội dung của cấu trúc JSON CV sau đây sang ngôn ngữ: {languageText}.
YÊU CẦU BẮT BUỘC:
1. Dịch tất cả các giá trị chuỗi văn bản (ví dụ: kinh nghiệm, dự án, kỹ năng, mục tiêu nghề nghiệp, tóm tắt...) và tên các đề mục lớn/nhỏ (ví dụ: 'title', 'name'...).
2. Giữ nguyên cấu trúc khóa (keys) của JSON, tuyệt đối KHÔNG được thay đổi bất kỳ thuộc tính kỹ thuật nào (ví dụ: 'id', 'overrideTemplate', 'general', 'fullName', 'jobTitle', 'sections', 'items'...).
3. Chỉ trả về chuỗi JSON kết quả hợp lệ duy nhất. KHÔNG kèm theo bất kỳ lời giải thích nào, KHÔNG bọc trong block code ```json ... ```. Đảm bảo cấu trúc JSON hoàn chỉnh, không bị cắt ngang hoặc thiếu ngoặc đóng.
4. Nếu một số phần hoặc toàn bộ nội dung ban đầu đã là ngôn ngữ đích, hãy giữ nguyên phần đó và trả về JSON chuẩn, không thêm bớt thông tin ngoài lề.
5. TUYỆT ĐỐI KHÔNG được sử dụng escape unicode sequence kiểu '\uXXXX' (như '\u90d0\u7d22'). Hãy xuất các ký tự Unicode/tiếng Nhật/tiếng Hàn/tiếng Việt trực tiếp dưới dạng ký tự UTF-8 bình thường (ví dụ: '日本語', '한국어', 'Nguyễn Văn A').

Dưới đây là dữ liệu JSON CV cần dịch:
{payloadJson}";

            try
            {
                string systemInstruction = $"Bạn là chuyên gia dịch thuật CV chuyên nghiệp. Nhiệm vụ của bạn là dịch các giá trị văn bản trong chuỗi JSON sang ngôn ngữ {languageText}. Hãy giữ nguyên cấu trúc JSON gốc và các key kỹ thuật (như 'id', 'overrideTemplate', 'theme', 'general', 'sections', 'items', 'title'...). Chỉ trả về chuỗi JSON thô hợp lệ duy nhất, tuyệt đối không kèm giải thích hay bọc trong markdown block code. QUY TẮC QUAN TRỌNG: Hãy viết chữ bản địa trực tiếp dưới dạng UTF-8 (ví dụ: '日本語', '한국어'), TUYỆT ĐỐI KHÔNG sử dụng ký tự escape dạng '\\uXXXX'.";
                string translatedJson = await _aiService.GenerateContent(prompt, true, systemInstruction, 0.2, true);
                
                // Dọn dẹp nếu AI tự động bọc Markdown
                translatedJson = CleanMarkdownCodeBlocks(translatedJson);

                // Thử parse để xác thực JSON
                try
                {
                    var parsed = Newtonsoft.Json.Linq.JToken.Parse(translatedJson);
                    
                    // Khôi phục lại ảnh đại diện ban đầu nếu có
                    if (parsed is Newtonsoft.Json.Linq.JObject parsedObj && originalAvatarUrl != null && parsedObj["general"] != null)
                    {
                        parsedObj["general"]["avatarUrl"] = originalAvatarUrl;
                    }

                    // Ghi log sử dụng AI vào DB
                    try
                    {
                        int tokens = (payloadJson.Length / 4) + (translatedJson.Length / 4) + 150;
                        var log = new AILog {
                            UserID = userId > 0 ? userId : (int?)null,
                            RequestType = "translate",
                            InputText = $"Translate to {request.TargetLanguage}",
                            OutputText = "Successfully translated CV JSON.",
                            UsedTokens = tokens,
                            ApiProvider = "Gemini",
                            CreatedAt = DateTime.Now
                        };
                        _context.AILogs.Add(log);
                        await _context.SaveChangesAsync();
                    }
                    catch { /* Không chặn luồng chính khi lỗi ghi log */ }

                    return Json(new { success = true, translatedJson = parsed.ToString(Newtonsoft.Json.Formatting.None) });
                }
                catch (Exception jsonEx)
                {
                    Console.WriteLine($"[TranslateCV] Lỗi parse JSON kết quả dịch: {jsonEx.Message}");
                    Console.WriteLine($"[TranslateCV] RAW AI OUTPUT: {translatedJson}");
                    return Json(new { success = false, message = $"AI trả về cấu trúc JSON không hợp lệ. Lỗi: {jsonEx.Message}. Kết quả thô: {(translatedJson.Length > 300 ? translatedJson.Substring(0, 300) + "..." : translatedJson)}" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TranslateCV] Lỗi dịch thuật: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình dịch thuật CV!" });
            }
        }

        private static string CleanMarkdownCodeBlocks(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var cleaned = input.Trim();
            if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(7);
            }
            else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(3);
            }

            if (cleaned.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            }

            return cleaned.Trim();
        }

        public class TranslateCVRequest
        {
            public string? JsonContent { get; set; }
            public string? TargetLanguage { get; set; }
        }

        public class ChatboxRequest
        {
            public string? Message { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessText(string type, string content, string context)
        {
            // 1. Kiểm tra đầu vào rỗng
            if (string.IsNullOrWhiteSpace(content)) 
            {
                return Json(new { success = false, data = "Vui lòng nhập nội dung để AI xử lý!" });
            }

            // 2. Xác thực người dùng
            int userId = 0;
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId)) 
            {
                userId = parsedId;
            }

            if (userId == 0) 
            {
                // Thử cách khác xem có claims dạng số nguyên không (để không cản trở Tester)
                if (User.Identity != null && User.Identity.IsAuthenticated) {
                    // Nếu đã đăng nhập mà ko bắt đc ID, default = 1 để pass rate limit
                    userId = 1; 
                } else {
                    return Json(new { success = false, data = "Vui lòng đăng nhập để sử dụng tính năng AI." });
                }
            }

            // 3. Rate Limit - Lấy cấu hình và kiểm tra hạn mức
            var configData = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
            
            // Lấy thông tin người dùng để kiểm tra IsPro
            var userInfo = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == userId);
            bool isPro = userInfo?.IsPro ?? false;

            int rateLimit = isPro ? (configData?.ProUserRateLimit ?? 50) : (configData?.UserRateLimit ?? 10);

            // Chỉ đếm những lần gọi AI thành công (có kết quả trả về) ← lần lỗi không tốn quota
            var todayLogsCount = await _context.AILogs
                .Where(l => l.UserID == userId
                         && l.CreatedAt.Date == DateTime.Today
                         && !string.IsNullOrEmpty(l.OutputText))
                .CountAsync();

            if (todayLogsCount >= rateLimit) 
            {
                string accountType = isPro ? "Pro" : "thường";
                return Json(new { success = false, data = $"Tài khoản {accountType} của bạn đã vượt hạn mức sử dụng AI ({rateLimit} lần/ngày). Vui lòng quay lại vào ngày mai!" });
            }

            string prompt = "";

            // 4. Chuẩn hóa 'type' để tránh lỗi chữ hoa/thường hoặc khoảng trắng
            string? requestType = type?.ToLower().Trim();

            // 5. Thiết lập Prompt dựa trên từng Case (Lấy từ DB nếu có, không có thì xài mặc định)
            switch (requestType)
            {
                case "summary": // Mục tiêu nghề nghiệp
                    if (!string.IsNullOrEmpty(configData?.SummaryTemplate)) {
                        prompt = configData.SummaryTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $@"Viết duy nhất một đoạn văn mục tiêu nghề nghiệp ngắn gọn (2-4 câu, tối đa 90 từ) cho vị trí {context} dựa trên các ý: {content}.
YÊU CẦU BẮT BUỘC:
- Chỉ trả về duy nhất đoạn văn mục tiêu nghề nghiệp để đưa vào CV. Tuyệt đối không dùng gạch đầu dòng hay danh sách liệt kê.
- TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép.
- TUYỆT ĐỐI KHÔNG có lời chào, lời dẫn (ví dụ: KHÔNG viết ""Là một chuyên gia viết CV..."", ""Dưới đây là mục tiêu..."", ""Chào bạn...""), KHÔNG tiêu đề, KHÔNG giải thích.";
                    }
                    break;

                case "optimize": // Tối ưu kinh nghiệm 
                    if (!string.IsNullOrEmpty(configData?.GrammarTemplate)) {
                        prompt = configData.GrammarTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $@"Hãy viết chính xác từ 3 đến 5 gạch đầu dòng ngắn gọn (bắt buộc tối thiểu phải có 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng dưới 15 từ và bắt đầu bằng động từ hành động) mô tả công việc cho vị trí {context} dựa trên thông tin: {content}.
YÊU CẦU BẮT BUỘC:
- Nếu thông tin cung cấp quá ngắn hoặc thiếu ý, bạn phải tự động suy luận thêm các nhiệm vụ và công việc đặc trưng của vị trí đó để đảm bảo có đủ ít nhất 3 gạch đầu dòng.
- Chỉ trả về duy nhất các gạch đầu dòng nội dung, không đánh số.
- TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép.
- TUYỆT ĐỐI KHÔNG có bất kỳ câu dẫn dắt, lời chào hay giải thích nào ở đầu và cuối kết quả (Ví dụ: KHÔNG viết ""Dưới đây là..."", ""Đây là các công việc..."", ""Tôi đã tối ưu hóa..."").";
                    }
                    break;

                case "project": // Tối ưu dự án
                    prompt = $@"Hãy viết chính xác từ 3 đến 5 gạch đầu dòng ngắn gọn (bắt buộc tối thiểu phải có 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng dưới 15 từ) mô tả dự án '{context}' dựa trên các ý: {content}.
YÊU CẦU BẮT BUỘC:
- Nếu thông tin quá ngắn, bạn phải tự động suy luận và bổ sung thêm các tính năng, công nghệ hoặc kết quả đặc trưng của loại dự án đó để đảm bảo có đủ ít nhất 3 gạch đầu dòng.
- Chỉ trả về duy nhất các gạch đầu dòng mô tả kết quả và công nghệ, không đánh số.
- TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép.
- TUYỆT ĐỐI KHÔNG có bất kỳ câu dẫn dắt, lời chào hay giải thích nào ở đầu và cuối kết quả (Ví dụ: KHÔNG viết ""Dưới đây là..."", ""Đây là mô tả dự án..."", ""Tôi đã viết lại..."").";
                    break;

                case "activity": // Tối ưu hoạt động
                    prompt = $@"Hãy viết chính xác từ 3 đến 5 gạch đầu dòng ngắn gọn (bắt buộc tối thiểu phải có 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng dưới 15 từ) mô tả hoạt động '{context}' dựa trên các ý: {content}.
YÊU CẦU BẮT BUỘC:
- Nếu thông tin quá ngắn, bạn phải tự động suy luận và bổ sung thêm các nhiệm vụ, vai trò hoặc kỹ năng đạt được đặc trưng của hoạt động đó để đảm bảo có đủ ít nhất 3 gạch đầu dòng.
- Chỉ trả về duy nhất các gạch đầu dòng mô tả đóng góp và kỹ năng đạt được, không đánh số.
- TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép.
- TUYỆT ĐỐI KHÔNG có bất kỳ câu dẫn dắt, lời chào hay giải thích nào ở đầu và cuối kết quả (Ví dụ: KHÔNG viết ""Dưới đây là..."", ""Tôi đã tối ưu..."").";
                    break;

                case "suggest_skills": // Gợi ý kỹ năng
                    if (!string.IsNullOrEmpty(configData?.SkillTemplate)) {
                        prompt = configData.SkillTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $"Liệt kê đúng 5 kỹ năng quan trọng nhất cho vị trí '{content}'. YÊU CẦU BẮT BUỘC: Chỉ trả về tên các kỹ năng, ngăn cách nhau bằng duy nhất dấu phẩy. KHÔNG đánh số, KHÔNG lời dẫn, KHÔNG giải thích.";
                    }
                    break;

                case "cover_letter":
                    prompt = $"Bạn là chuyên gia viết thư xin việc. Hãy viết một bức thư xin việc ấn tượng, chuyên nghiệp gửi đến công ty {context}. Dựa trên thông tin ứng viên: {content}. Yêu cầu: Văn phong thuyết phục, độ dài khoảng 250-300 chữ, có đầy đủ phần mở đầu, nội dung chính và kết bài. Chỉ trả về nội dung bức thư, không kèm lời chào của AI.";
                    break;

                case "job_match": // So khớp CV với Job Description
                    prompt = $@"Bạn là chuyên gia tuyển dụng IT hàng đầu Việt Nam với 15 năm kinh nghiệm. Hãy phân tích mức độ phù hợp giữa CV ứng viên và mô tả công việc (JD) dưới đây.

=== CV CỦA ỨNG VIÊN ===
{content}

=== MÔ TẢ CÔNG VIỆC (JD) ===
{context}

YÊU CẦU PHÂN TÍCH:
1. Tính điểm phù hợp tổng thể (matchScore) từ 0-100 dựa trên: kỹ năng kỹ thuật (40%), kinh nghiệm liên quan (30%), trình độ học vấn (15%), kỹ năng mềm (15%).
2. Liệt kê các kỹ năng/từ khóa trong CV đã khớp với JD (matchedSkills).
3. Liệt kê các kỹ năng/yêu cầu trong JD mà CV chưa có (missingSkills).
4. Đưa ra 3-5 gợi ý CỤ THỂ để cải thiện CV nhằm tăng tỷ lệ trúng tuyển (suggestions).
5. Viết nhận xét tổng quan 2-3 câu bằng tiếng Việt (summary).

TUYỆT ĐỐI chỉ trả về JSON thuần (KHÔNG có markdown, KHÔNG có ```json, KHÔNG có lời dẫn):
{{""matchScore"": 75, ""matchedSkills"": [""C#"", "".NET""], ""missingSkills"": [""Docker"", ""AWS""], ""suggestions"": [""Bổ sung kinh nghiệm Docker...""], ""summary"": ""Ứng viên có nền tảng tốt...""}}";
                    break;

                case "check_grammar": // Kiểm tra lỗi chính tả & ngữ pháp
                    prompt = $@"Bạn là một biên tập viên CV chuyên nghiệp. Hãy kiểm tra các lỗi chính tả, lỗi dùng từ nghiêm trọng trong văn bản sau:
""{content}""

YÊU CẦU BẮT BUỘC (RẤT QUAN TRỌNG):
1. CHỈ bắt lỗi chính tả thực sự (ví dụ: sai dấu, viết sai từ tiếng Việt/tiếng Anh) hoặc viết tắt không trang trọng (ví dụ: 'mún' -> 'muốn', 'ko' -> 'không', 'đc' -> 'được').
2. CHỈ bắt lỗi sai thuật ngữ chuyên ngành rõ ràng (ví dụ: 'Bachend' -> 'Backend', 'Develope' -> 'Developer').
3. TUYỆT ĐỐI KHÔNG tự ý sửa đổi, bắt bẻ hoặc báo lỗi đối với văn phong cá nhân, cách diễn đạt đúng ngữ pháp và các thông tin mẫu của khách hàng nếu chúng không sai chính tả hoặc không vi phạm lỗi dùng từ nghiêm trọng.
4. Trả về kết quả dưới dạng JSON Array: [{{""error"": ""từ/cụm từ sai"", ""fix"": ""gợi ý đúng"", ""reason"": ""lý do cụ thể""}}].
5. Nếu văn bản hoàn toàn không có lỗi chính tả hoặc lỗi dùng từ nghiêm trọng, hãy trả về mảng rỗng []. Tuyệt đối không tự bịa ra lỗi, không trả về lời dẫn hay markdown.";
                    break;

                case "custom_prompt": // Sinh nội dung theo prompt tự gõ kèm ngữ cảnh (dành cho Experience, Project, Activity)
                    prompt = $@"Bạn là một chuyên gia viết CV chuyên nghiệp hàng đầu. Hãy thực hiện yêu cầu của người dùng để cải thiện, viết lại, sửa lỗi hoặc dịch nội dung cho CV của họ.
Nội dung hiện tại của ô nhập liệu: {context}
Yêu cầu của người dùng: {content}

YÊU CẦU BẮT BUỘC (QUAN TRỌNG NHẤT):
1. Thực hiện chính xác và tập trung vào yêu cầu của người dùng.
2. Kết quả bắt buộc phải trả về dưới dạng từ 3 đến 5 gạch đầu dòng ngắn gọn (tối thiểu là 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng không quá 15 từ, không đánh số).
3. Nếu thông tin người dùng cung cấp quá ngắn, bạn phải tự động suy luận và bổ sung thêm các chi tiết nghiệp vụ liên quan để đảm bảo có đủ nhất 3 gạch đầu dòng.
4. CHỈ TRẢ VỀ DUY NHẤT các gạch đầu dòng kết quả (ví dụ:
- Công việc 1...
- Công việc 2...).
5. TUYỆT ĐỐI KHÔNG có bất kỳ lời chào, lời dẫn, giải thích hay câu mở đầu vô nghĩa nào (Ví dụ: KHÔNG viết ""Dưới đây là..."", ""Đây là bản dịch..."", ""Here is the translation:"", ""Tôi đã sửa lỗi chính tả..."", ""Là một chuyên gia viết CV..."").
6. KHÔNG sử dụng các thẻ bao bọc markdown code block (như ```html hoặc ```) và không bọc kết quả trong dấu ngoặc kép.
7. Nếu yêu cầu liên quan đến dịch thuật hoặc đổi ngôn ngữ (ví dụ: người dùng nhập ""tiếng Anh"", ""dịch sang..."", ""translate...""), bạn BẮT BUỘC phải dịch toàn bộ các từ tiếng Việt sang ngôn ngữ đích, bao gồm cả các chức danh, vị trí làm việc, tên ngành nghề (ví dụ: ""Thiết kế đồ họa"" phải dịch sang tiếng Anh là ""Graphic design"" hoặc ""Graphic Designer"", ""Lập trình viên"" phải dịch là ""Developer""). Tuyệt đối không giữ nguyên cụm từ tiếng Việt trong kết quả dịch.";
                    break;

                case "custom_prompt_summary": // Sinh nội dung theo prompt tự gõ kèm ngữ cảnh dành riêng cho Mục tiêu nghề nghiệp
                    prompt = $@"Bạn là một chuyên gia viết CV chuyên nghiệp hàng đầu. Hãy thực hiện yêu cầu của người dùng để cải thiện, dịch hoặc sửa đổi mục tiêu nghề nghiệp (summary) của họ.
Nội dung hiện tại của ô nhập liệu: {context}
Yêu cầu của người dùng: {content}

YÊU CẦU BẮT BUỘC (QUAN TRỌNG NHẤT):
1. Thực hiện chính xác và tập trung vào yêu cầu của người dùng.
2. Kết quả bắt buộc phải viết dưới dạng MỘT ĐOẠN VĂN LIỀN MẠCH, ngắn gọn (từ 2 đến 3 câu, tối đa 60 từ).
3. TUYỆT ĐỐI KHÔNG sử dụng bất kỳ gạch đầu dòng, danh sách liệt kê hay đánh số nào.
4. CHỈ TRẢ VỀ DUY NHẤT đoạn văn mục tiêu nghề nghiệp sau khi đã xử lý xong.
5. TUYỆT ĐỐI KHÔNG có bất kỳ lời chào, lời dẫn, giải thích hay câu mở đầu vô nghĩa nào (Ví dụ: KHÔNG viết ""Dưới đây là mục tiêu nghề nghiệp..."", ""Đây là bản dịch..."", ""Here is the translation:"", ""Tôi đã tối ưu hóa..."", ""Chào bạn..."").
6. KHÔNG sử dụng các thẻ bao bọc markdown code block (như ```html hoặc ```) và không bọc kết quả trong dấu ngoặc kép.
7. Nếu yêu cầu liên quan đến dịch thuật hoặc đổi ngôn ngữ (ví dụ: người dùng nhập ""tiếng Anh"", ""dịch sang..."", ""translate...""), bạn BẮT BUỘC phải dịch toàn bộ các từ tiếng Việt sang ngôn ngữ đích, bao gồm cả các chức danh, vị trí làm việc, tên ngành nghề (ví dụ: ""Thiết kế đồ họa"" phải dịch sang tiếng Anh là ""Graphic design"" hoặc ""Graphic Designer"", ""Lập trình viên"" phải dịch là ""Developer""). Tuyệt đối không giữ nguyên cụm từ tiếng Việt trong kết quả dịch.";
                    break;

                case "custom_prompt_lang": // Đổi ngôn ngữ cho Experience, Project, Activity
                    {
                        string targetLang = "tiếng Anh (English)";
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            string contentLower = content.ToLower();
                            if (contentLower.Contains("nhật") || contentLower.Contains("japan")) targetLang = "tiếng Nhật (Japanese)";
                            else if (contentLower.Contains("trung") || contentLower.Contains("hoa") || contentLower.Contains("china") || contentLower.Contains("chinese")) targetLang = "tiếng Trung (Chinese)";
                            else if (contentLower.Contains("hàn") || contentLower.Contains("korea")) targetLang = "tiếng Hàn (Korean)";
                            else if (contentLower.Contains("pháp") || contentLower.Contains("french")) targetLang = "tiếng Pháp (French)";
                            else if (contentLower.Contains("đức") || contentLower.Contains("german")) targetLang = "tiếng Đức (German)";
                            else if (contentLower.Contains("nga") || contentLower.Contains("russian")) targetLang = "tiếng Nga (Russian)";
                            else if (contentLower.Contains("tây ban nha") || contentLower.Contains("spanish")) targetLang = "tiếng Tây Ban Nha (Spanish)";
                            else if (contentLower.Contains("việt") || contentLower.Contains("vietnamese")) targetLang = "tiếng Việt (Vietnamese)";
                            else 
                            {
                                targetLang = content;
                            }
                        }

                        prompt = $@"Bạn là một chuyên gia viết CV chuyên nghiệp đa ngôn ngữ. Hãy dịch nội dung CV dưới đây sang {targetLang}.
Nội dung hiện tại cần dịch: {context}

YÊU CẦU BẮT BUỘC (QUAN TRỌNG NHẤT):
1. Dịch chính xác nội dung sang {targetLang} với văn phong chuyên nghiệp phù hợp với CV.
2. Kết quả dịch bắt buộc phải giữ nguyên cấu trúc dưới dạng từ 3 đến 5 gạch đầu dòng ngắn gọn (tối thiểu là 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng không quá 15 từ, không đánh số).
3. CHỈ TRẢ VỀ DUY NHẤT các gạch đầu dòng kết quả dịch (ví dụ:
- Work 1...
- Work 2...).
4. TUYỆT ĐỐI KHÔNG có bất kỳ lời chào, lời dẫn, giải thích hay câu mở đầu vô nghĩa nào (Ví dụ: KHÔNG viết ""Here is the translation:"", ""Dưới đây là bản dịch..."", ""Translation to..."").
5. KHÔNG sử dụng các thẻ bao bọc markdown code block (như ```html hoặc ```) và không bọc kết quả trong dấu ngoặc kép.
6. Bạn BẮT BUỘC phải dịch toàn bộ từ ngữ trong văn bản sang ngôn ngữ đích {targetLang}, kể cả các chức danh, vị trí làm việc, tên ngành nghề (ví dụ: ""Thiết kế đồ họa"" phải dịch sang tiếng Anh là ""Graphic design"" hoặc ""Graphic Designer"", ""Lập trình viên"" phải dịch là ""Developer""). Tuyệt đối không giữ nguyên cụm từ tiếng Việt trong kết quả dịch.";
                    }
                    break;

                case "custom_prompt_lang_summary": // Đổi ngôn ngữ cho Mục tiêu nghề nghiệp (Summary)
                    {
                        string targetLangSummary = "tiếng Anh (English)";
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            string contentLower = content.ToLower();
                            if (contentLower.Contains("nhật") || contentLower.Contains("japan")) targetLangSummary = "tiếng Nhật (Japanese)";
                            else if (contentLower.Contains("trung") || contentLower.Contains("hoa") || contentLower.Contains("china") || contentLower.Contains("chinese")) targetLangSummary = "tiếng Trung (Chinese)";
                            else if (contentLower.Contains("hàn") || contentLower.Contains("korea")) targetLangSummary = "tiếng Hàn (Korean)";
                            else if (contentLower.Contains("pháp") || contentLower.Contains("french")) targetLangSummary = "tiếng Pháp (French)";
                            else if (contentLower.Contains("đức") || contentLower.Contains("german")) targetLangSummary = "tiếng Đức (German)";
                            else if (contentLower.Contains("nga") || contentLower.Contains("russian")) targetLangSummary = "tiếng Nga (Russian)";
                            else if (contentLower.Contains("tây ban nha") || contentLower.Contains("spanish")) targetLangSummary = "tiếng Tây Ban Nha (Spanish)";
                            else if (contentLower.Contains("việt") || contentLower.Contains("vietnamese")) targetLangSummary = "tiếng Việt (Vietnamese)";
                            else 
                            {
                                targetLangSummary = content;
                            }
                        }

                        prompt = $@"Bạn là một chuyên gia viết CV chuyên nghiệp đa ngôn ngữ. Hãy dịch mục tiêu nghề nghiệp (summary) dưới đây sang {targetLangSummary}.
Nội dung hiện tại cần dịch: {context}

YÊU CẦU BẮT BUỘC (QUAN TRỌNG NHẤT):
1. Dịch chính xác nội dung sang {targetLangSummary} với văn phong chuyên nghiệp phù hợp với CV.
2. Kết quả dịch bắt buộc phải viết dưới dạng MỘT ĐOẠN VĂN LIỀN MẠCH, ngắn gọn (từ 2 đến 3 câu, tối đa 60 từ).
3. TUYỆT ĐỐI KHÔNG sử dụng bất kỳ gạch đầu dòng, danh sách liệt kê hay đánh số nào.
4. CHỈ TRẢ VỀ DUY NHẤT đoạn văn kết quả dịch sau khi đã dịch xong.
5. TUYỆT ĐỐI KHÔNG có bất kỳ lời chào, lời dẫn, giải thích hay câu mở đầu vô nghĩa nào (Ví dụ: KHÔNG viết ""Here is the translation:"", ""Dưới đây là bản dịch..."", ""Translation to..."").
6. KHÔNG sử dụng các thẻ bao bọc markdown code block (như ```html hoặc ```) và không bọc kết quả trong dấu ngoặc kép.
7. Bạn BẮT BUỘC phải dịch toàn bộ từ ngữ trong văn bản sang ngôn ngữ đích {targetLangSummary}, kể cả các chức danh, vị trí làm việc, tên ngành nghề (ví dụ: ""Thiết kế đồ họa"" phải dịch sang tiếng Anh là ""Graphic design"" hoặc ""Graphic Designer"", ""Lập trình viên"" phải dịch là ""Developer""). Tuyệt đối không giữ nguyên cụm từ tiếng Việt trong kết quả dịch.";
                    }
                    break;



                default:
                    // Dùng cho trường hợp Test Playground trên Admin
                    if (requestType == "test_playground") {
                        prompt = content;
                    } else {
                        return Json(new { success = false, data = "Loại yêu cầu không hợp lệ!" });
                    }
                    break;
            }

            // 6. Gọi Service AI (Truyền thêm trạng thái Pro)
            try 
            {
                var rawResult = await _aiService.GenerateContent(prompt, isPro);
                var aiResult = CleanAIResult(rawResult);
                
                // 7. Ghi Log AI và Cập nhật Token
                // Đếm token chính xác bằng TiktokenSharp (cl100k_base dùng cho Gemini/GPT)
                int estimatedTokens = CountTokens(prompt, aiResult);
                
                string selectedModel = isPro ? (configData?.ProModelName ?? "") : (configData?.ModelName ?? "");
                string apiProvider = selectedModel.Contains("llama") || selectedModel.Contains("mixtral") ? "Groq" : "Gemini";

                var log = new DoAnCS.Models.AILog {
                    UserID = userId,
                    RequestType = requestType,
                    InputText = prompt,
                    OutputText = aiResult,
                    UsedTokens = estimatedTokens,
                    ApiProvider = apiProvider,
                    CreatedAt = DateTime.Now
                };

                _context.AILogs.Add(log);

                // Cập nhật Total Tokens vào Cấu Hình
                if (configData != null) {
                    var trackedConfig = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == configData.Id);
                    if (trackedConfig != null) {
                        trackedConfig.TotalTokensUsed += estimatedTokens;
                    }
                }

                await _context.SaveChangesAsync();
                
                // Trả về kết quả thành công
                return Json(new { success = true, data = aiResult });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = "Lỗi server: " + ex.Message });
            }
        }

        // HÀM HẬU XỬ LÝ LÀM SẠCH KẾT QUẢ TỪ AI SERVICE (LOẠI BỎ DẤU NGOẶC KÉP VÀ CÂU DẪN THỪA)
        private string CleanAIResult(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // 1. Loại bỏ khoảng trắng thừa
            text = text.Trim();

            // 2. Sửa lỗi dính chữ do AI ghép từ thiếu khoảng trắng (ví dụ: "tríSenior" -> "trí Senior")
            text = System.Text.RegularExpressions.Regex.Replace(text, @"trí([A-Z])", "trí $1");

            // 3. Loại bỏ dấu ngoặc kép bao bọc ở đầu và cuối (cả dạng thẳng và dạng cong)
            if ((text.StartsWith("\"") && text.EndsWith("\"")) ||
                (text.StartsWith("'") && text.EndsWith("'")) ||
                (text.StartsWith("“") && text.EndsWith("”")) ||
                (text.StartsWith("`") && text.EndsWith("`")))
            {
                text = text.Substring(1, text.Length - 2).Trim();
            }

            // 3.5. Loại bỏ các câu dẫn dắt hoàn chỉnh đầu tiên kết thúc bằng dấu chấm
            bool hasRemovedIntro = true;
            while (hasRemovedIntro)
            {
                hasRemovedIntro = false;
                int firstDotIndex = text.IndexOf('.');
                if (firstDotIndex > 0 && firstDotIndex < 180)
                {
                    string firstSentence = text.Substring(0, firstDotIndex).ToLower();
                    string[] sentenceKeywords = new[] {
                        "tôi sẽ giúp", "tôi sẽ cải thiện", "tôi hiểu yêu cầu", "tôi đã tối ưu", "tôi đã sửa", "tôi đã dịch", 
                        "dưới đây là", "đây là bản", "sau đây là", "gửi bạn bản", "chúc bạn", "tôi xin gửi", "tôi đã cải thiện",
                        "chào bạn", "tôi xin giúp", "tôi đã tinh chỉnh"
                    };

                    foreach (var kw in sentenceKeywords)
                    {
                        if (firstSentence.Contains(kw))
                        {
                            text = text.Substring(firstDotIndex + 1).Trim();
                            hasRemovedIntro = true;
                            break;
                        }
                    }
                }
            }

            // 4. Phát hiện và loại bỏ lời dẫn giới thiệu có chứa dấu hai chấm ":" ở phần đầu (dưới 180 ký tự)
            int colonIndex = text.IndexOf(':');
            if (colonIndex > 0 && colonIndex < 180)
            {
                string introPart = text.Substring(0, colonIndex).ToLower();
                string[] introKeywords = new[] {
                    "dưới đây là", "đây là", "yêu cầu", "người dùng", "sửa đổi", "tối ưu", "dịch",
                    "bản dịch", "mục tiêu", "mô tả", "chào bạn", "tôi hiểu", "sau đây", "gửi bạn",
                    "here is", "translation", "văn bản", "kết quả", "cải thiện", "chuyên nghiệp",
                    "đoạn văn", "gạch đầu dòng", "tiếng anh", "chỉnh sửa"
                };

                bool isIntro = false;
                foreach (var kw in introKeywords)
                {
                    if (introPart.Contains(kw))
                    {
                        isIntro = true;
                        break;
                    }
                }

                if (isIntro)
                {
                    text = text.Substring(colonIndex + 1).Trim();
                }
            }

            // 5. Loại bỏ các tiền tố giới thiệu phổ biến của AI nếu nó vẫn còn ở đầu chuỗi (kể cả không có dấu hai chấm)
            string[] prefixesToRemove = new[] {
                "dưới đây là", "đây là", "mục tiêu nghề nghiệp của", "mục tiêu nghề nghiệp là",
                "mục tiêu nghề nghiệp", "mục tiêu", "mô tả công việc của", "mô tả công việc là",
                "tôi xin gửi", "là một chuyên gia", "dưới đây là mục tiêu nghề nghiệp",
                "dưới đây là mô tả công việc", "tôi hiểu yêu cầu của người dùng", "tôi hiểu yêu cầu",
                "sau đây là", "gửi bạn bản dịch", "bản dịch sang tiếng", "đây là bản sửa đổi",
                "đây là mục tiêu", "đây là mô tả"
            };

            foreach (var prefix in prefixesToRemove)
            {
                if (text.ToLower().StartsWith(prefix))
                {
                    text = text.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // 6. Nếu sau khi cắt prefix mà ký tự đầu là dấu hai chấm, dấu gạch ngang, dấu chấm... thì bỏ đi
            while (text.StartsWith(":") || text.StartsWith("-") || text.StartsWith("–") || text.StartsWith(".") || text.StartsWith(" "))
            {
                text = text.Substring(1).Trim();
            }

            // Một lần nữa loại bỏ dấu ngoặc kép nếu sau khi cắt prefix lại lòi ra dấu ngoặc kép
            if ((text.StartsWith("\"") && text.EndsWith("\"")) ||
                (text.StartsWith("'") && text.EndsWith("'")) ||
                (text.StartsWith("“") && text.EndsWith("”")) ||
                (text.StartsWith("`") && text.EndsWith("`")))
            {
                text = text.Substring(1, text.Length - 2).Trim();
            }

            return text;
        }

        /// <summary>
        /// Đếm token chính xác bằng TiktokenSharp (cl100k_base — tokenizer tương thích Gemini và GPT-4).
        /// Fallback về ước tính len/4 nếu có lỗi khởi tạo tokenizer.
        /// </summary>
        private static int CountTokens(string prompt, string result)
        {
            try
            {
                // cl100k_base là tokenizer dùng cho GPT-4 và tương thích Gemini
                var tikToken = TikToken.EncodingForModel("gpt-4");
                return tikToken.Encode(prompt).Count + tikToken.Encode(result ?? "").Count;
            }
            catch
            {
                // Fallback về ước tính nếu tokenizer lỗi (không crash app)
                return (prompt.Length / 4) + ((result?.Length ?? 0) / 4);
            }
        }
    }
}