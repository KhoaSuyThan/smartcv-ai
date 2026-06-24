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

namespace DoAnCS.Controllers 
{
    public class AIController : Controller
    {
        private readonly IAIService _aiService;
        private readonly DoAnCS.Data.AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        // Tiêm (Inject) Service xử lý AI và AppDbContext thông qua Constructor
        public AIController(IAIService aiService, DoAnCS.Data.AppDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _aiService = aiService;
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

            chatbotApiKey = chatbotApiKey.Trim();

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

            var todayLogsCount = await _context.AILogs
                .Where(l => l.UserID == userId && l.CreatedAt.Date == DateTime.Today)
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
                        prompt = $"Viết duy nhất một đoạn văn mục tiêu nghề nghiệp ngắn gọn (2-4 câu, tối đa 90 từ, TUYỆT ĐỐI KHÔNG sử dụng gạch đầu dòng hay danh sách liệt kê) cho vị trí {context} dựa trên các ý: {content}. YÊU CẦU BẮT BUỘC (QUAN TRỌNG NHẤT): Chỉ trả về duy nhất đoạn văn mục tiêu nghề nghiệp cá nhân để đưa vào CV. TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép. TUYỆT ĐỐI KHÔNG có lời chào, lời dẫn (ví dụ: KHÔNG viết \"Là một chuyên gia viết CV...\", \"Dưới đây là...\", \"Chào bạn...\"), KHÔNG tiêu đề, KHÔNG giải thích.";
                    }
                    break;

                case "optimize": // Tối ưu kinh nghiệm 
                    if (!string.IsNullOrEmpty(configData?.GrammarTemplate)) {
                        prompt = configData.GrammarTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $"Hãy viết chính xác từ 3 đến 5 gạch đầu dòng ngắn gọn (bắt buộc tối thiểu phải có 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng dưới 15 từ và bắt đầu bằng động từ hành động) mô tả công việc cho vị trí {context} dựa trên thông tin: {content}. YÊU CẦU QUAN TRỌNG: Nếu thông tin cung cấp quá ngắn hoặc thiếu ý, bạn phải tự động suy luận thêm các nhiệm vụ và công việc đặc trưng của vị trí đó để đảm bảo có đủ ít nhất 3 gạch đầu dòng. Chỉ trả về các gạch đầu dòng nội dung, không đánh số. TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép. TUYỆT ĐỐI KHÔNG có lời dẫn, lời chào, không tiêu đề.";
                    }
                    break;

                case "project": // Tối ưu dự án
                    prompt = $"Hãy viết chính xác từ 3 đến 5 gạch đầu dòng ngắn gọn (bắt buộc tối thiểu phải có 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng dưới 15 từ) mô tả dự án '{context}' dựa trên các ý: {content}. YÊU CẦU QUAN TRỌNG: Nếu thông tin quá ngắn, bạn phải tự động suy luận và bổ sung thêm các tính năng, công nghệ hoặc kết quả đặc trưng của loại dự án đó để đảm bảo có đủ ít nhất 3 gạch đầu dòng. Chỉ trả về các gạch đầu dòng mô tả kết quả và công nghệ, không đánh số. TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép. TUYỆT ĐỐI KHÔNG có lời dẫn hay lời chào.";
                    break;

                case "activity": // Tối ưu hoạt động
                    prompt = $"Hãy viết chính xác từ 3 đến 5 gạch đầu dòng ngắn gọn (bắt buộc tối thiểu phải có 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng dưới 15 từ) mô tả hoạt động '{context}' dựa trên các ý: {content}. YÊU CẦU QUAN TRỌNG: Nếu thông tin quá ngắn, bạn phải tự động suy luận và bổ sung thêm các nhiệm vụ, vai trò hoặc kỹ năng đạt được đặc trưng của hoạt động đó để đảm bảo có đủ ít nhất 3 gạch đầu dòng. Chỉ trả về các gạch đầu dòng mô tả đóng góp và kỹ năng đạt được, không đánh số. TUYỆT ĐỐI KHÔNG bọc kết quả trong dấu ngoặc kép. TUYỆT ĐỐI KHÔNG có lời dẫn hay lời chào.";
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
                    prompt = $@"Bạn là một chuyên gia tuyển dụng cao cấp và biên tập viên CV chuyên nghiệp. Hãy kiểm tra lỗi chính tả, ngữ pháp và tính chuyên nghiệp của văn bản sau đây.

Văn bản cần kiểm tra:
""{content}""

YÊU CẦU CỰC KỲ KHẮT KHE:
1. Bắt lỗi chính tả tiếng Việt, tiếng Anh và cả ""teen code"", viết tắt không trang trọng (ví dụ: 'mún' -> 'muốn', 'ko' -> 'không', 'toi' -> 'tôi').
2. Bắt lỗi sai chính tả thuật ngữ chuyên ngành (ví dụ: 'Bachend' -> 'Backend', 'Develope' -> 'Developer').
3. Kiểm tra tính chuyên nghiệp: Nếu văn bản quá bình dân, không phù hợp với CV, hãy đánh dấu là lỗi.
4. Trả về kết quả dưới dạng JSON Array: [{{""error"": ""từ/cụm từ sai"", ""fix"": ""gợi ý đúng/trang trọng hơn"", ""reason"": ""lý do (Sai chính tả/Thiếu chuyên nghiệp/Sai thuật ngữ)""}}].
5. Nếu không có lỗi, trả về []. Tuyệt đối không trả về lời dẫn hay markdown.";
                    break;

                case "custom_prompt": // Sinh nội dung theo prompt tự gõ kèm ngữ cảnh
                    // content: prompt yêu cầu từ người dùng
                    // context: nội dung hiện có của trường thông tin làm ngữ cảnh
                    prompt = $@"Bạn là một chuyên gia viết CV chuyên nghiệp hàng đầu. Hãy thực hiện yêu cầu của người dùng để cải thiện hoặc viết mới nội dung cho CV của họ.
Yêu cầu của người dùng: {content}
Nội dung hiện tại của ô nhập liệu (nếu có, hãy dùng làm ngữ cảnh hoặc sửa đổi dựa trên đây): {context}

YÊU CẦU BẮT BUỘC (QUAN TRỌNG NHẤT):
- Thực hiện chính xác và tập trung vào yêu cầu của người dùng.
- Chỉ trả về duy nhất kết quả sau khi đã xử lý/viết lại để người dùng dán vào CV.
- Kết quả phải cực kỳ ngắn gọn, súc tích, bắt buộc phải trả về từ 3 đến 5 gạch đầu dòng (tối thiểu là 3 gạch đầu dòng và tối đa là 5 gạch đầu dòng, mỗi gạch đầu dòng không quá 15 từ, không đánh số).
- YÊU CẦU BẮT BUỘC: Nếu thông tin người dùng cung cấp quá ngắn, bạn phải tự động suy luận và bổ sung thêm các chi tiết nghiệp vụ liên quan để đảm bảo có đủ ít nhất 3 gạch đầu dòng.
- TUYỆT ĐỐI KHÔNG có lời chào, lời dẫn (ví dụ: KHÔNG viết ""Là một chuyên gia viết CV..."", ""Dưới đây là..."", ""Tôi sẽ giúp bạn..."", ""Chào bạn...""), KHÔNG có tiêu đề và KHÔNG giải thích thêm.
- KHÔNG sử dụng các thẻ bao bọc markdown code block (như ```html hoặc ```). Chỉ trả về văn bản sạch sẽ.";
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
                
                // 7. Ghi Log AI và Cập nhật Token (Chỉ khi không phải gọi từ test_playground, hoặc nếu Admin tự test thì vẫn có userID)
                // Ước lượng Token đơn giản: 1 Token ~ 4 ký tự
                int estimatedTokens = (prompt.Length / 4) + (aiResult.Length / 4);
                
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

            // 4. Loại bỏ các tiền tố giới thiệu phổ biến của AI
            string[] prefixesToRemove = new[] {
                "dưới đây là", "đây là", "mục tiêu nghề nghiệp của", "mục tiêu nghề nghiệp là",
                "mục tiêu nghề nghiệp", "mục tiêu", "mô tả công việc của", "mô tả công việc là",
                "tôi xin gửi", "là một chuyên gia", "dưới đây là mục tiêu nghề nghiệp",
                "dưới đây là mô tả công việc"
            };

            foreach (var prefix in prefixesToRemove)
            {
                if (text.ToLower().StartsWith(prefix))
                {
                    // Tìm vị trí dấu hai chấm tiếp theo để cắt
                    int colonIndex = text.IndexOf(':');
                    if (colonIndex != -1 && colonIndex < 120) // Chỉ cắt nếu dấu hai chấm ở gần đầu
                    {
                        text = text.Substring(colonIndex + 1).Trim();
                    }
                    else
                    {
                        // Nếu không có dấu hai chấm, cắt bỏ phần prefix
                        text = text.Substring(prefix.Length).Trim();
                    }
                    break;
                }
            }

            // 5. Nếu sau khi cắt prefix mà ký tự đầu là dấu hai chấm, dấu gạch ngang, dấu chấm... thì bỏ đi
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
    }
}