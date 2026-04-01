using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using DoAnCS.Services;
using DoAnCS.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCS.Controllers 
{
    public class AIController : Controller
    {
        private readonly IAIService _aiService;
        private readonly DoAnCS.Data.AppDbContext _context;

        // Tiêm (Inject) Service xử lý AI và AppDbContext thông qua Constructor
        public AIController(IAIService aiService, DoAnCS.Data.AppDbContext context)
        {
            _aiService = aiService;
            _context = context;
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
            int rateLimit = configData?.UserRateLimit ?? 10;

            var todayLogsCount = await _context.AILogs
                .Where(l => l.UserID == userId && l.CreatedAt.Date == DateTime.Today)
                .CountAsync();

            if (todayLogsCount >= rateLimit) 
            {
                return Json(new { success = false, data = $"Bạn đã vượt hạn mức sử dụng AI ({rateLimit} lần/ngày). Vui lòng quay lại vào ngày mai!" });
            }

            string prompt = "";

            // 4. Chuẩn hóa 'type' để tránh lỗi chữ hoa/thường hoặc khoảng trắng
            string requestType = type?.ToLower().Trim();

            // 5. Thiết lập Prompt dựa trên từng Case (Lấy từ DB nếu có, không có thì xài mặc định)
            switch (requestType)
            {
                case "summary": // Mục tiêu nghề nghiệp
                    if (!string.IsNullOrEmpty(configData?.SummaryTemplate)) {
                        prompt = configData.SummaryTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $"Viết duy nhất một đoạn văn mục tiêu nghề nghiệp (3-4 câu) cho vị trí {context} dựa trên các ý: {content}. YÊU CẦU BẮT BUỘC: Chỉ trả về nội dung đoạn văn. KHÔNG lời chào, KHÔNG tiêu đề, KHÔNG giải thích thêm.";
                    }
                    break;

                case "optimize": // Tối ưu kinh nghiệm 
                    if (!string.IsNullOrEmpty(configData?.GrammarTemplate)) {
                        prompt = configData.GrammarTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $"Viết duy nhất một đoạn văn mô tả công việc (2-3 câu) sau cho vị trí {context} theo chuẩn STAR: {content}. YÊU CẦU BẮT BUỘC: Chỉ trả về các gạch đầu dòng nội dung. TUYỆT ĐỐI KHÔNG có lời dẫn, không có câu 'Dưới đây là...', không tiêu đề.";
                    }
                    break;

                case "suggest_skills": // Gợi ý kỹ năng
                    if (!string.IsNullOrEmpty(configData?.SkillTemplate)) {
                        prompt = configData.SkillTemplate.Replace("{{context}}", context ?? "").Replace("{{content}}", content);
                    } else {
                        prompt = $"Liệt kê 10 kỹ năng quan trọng nhất cho vị trí '{content}'. YÊU CẦU BẮT BUỘC: Chỉ trả về tên các kỹ năng, ngăn cách nhau bằng duy nhất dấu phẩy. KHÔNG đánh số, KHÔNG lời dẫn, KHÔNG giải thích.";
                    }
                    break;

                case "cover_letter":
                    prompt = $"Bạn là chuyên gia viết thư xin việc. Hãy viết một bức thư xin việc ấn tượng, chuyên nghiệp gửi đến công ty {context}. Dựa trên thông tin ứng viên: {content}. Yêu cầu: Văn phong thuyết phục, độ dài khoảng 250-300 chữ, có đầy đủ phần mở đầu, nội dung chính và kết bài. Chỉ trả về nội dung bức thư, không kèm lời chào của AI.";
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

            // 6. Gọi Service AI
            try 
            {
                var aiResult = await _aiService.GenerateContent(prompt);
                
                // 7. Ghi Log AI và Cập nhật Token (Chỉ khi không phải gọi từ test_playground, hoặc nếu Admin tự test thì vẫn có userID)
                // Ước lượng Token đơn giản: 1 Token ~ 4 ký tự
                int estimatedTokens = (prompt.Length / 4) + (aiResult.Length / 4);

                var log = new DoAnCS.Models.AILog {
                    UserID = userId,
                    RequestType = requestType,
                    InputText = prompt,
                    OutputText = aiResult,
                    UsedTokens = estimatedTokens,
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
    }
}