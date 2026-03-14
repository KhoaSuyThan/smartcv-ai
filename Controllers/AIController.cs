using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DoAnCS.Services;

namespace DoAnCS.Controllers 
{
    public class AIController : Controller
    {
        private readonly IAIService _aiService;

        // Tiêm (Inject) Service xử lý AI thông qua Constructor
        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessText(string type, string content, string context)
        {
            // 1. Kiểm tra đầu vào rỗng
            if (string.IsNullOrWhiteSpace(content)) 
            {
                return Json(new { success = false, data = "Vui lòng nhập nội dung để AI xử lý!" });
            }

            string prompt = "";

            // 2. Chuẩn hóa 'type' để tránh lỗi chữ hoa/thường hoặc khoảng trắng
            string requestType = type?.ToLower().Trim();

            // 3. Thiết lập Prompt dựa trên từng Case
            switch (requestType)
            {
                case "summary": // Mục tiêu nghề nghiệp
                    prompt = $"Viết duy nhất một đoạn văn mục tiêu nghề nghiệp (3-4 câu) cho vị trí {context} dựa trên các ý: {content}. " +
                            $"YÊU CẦU BẮT BUỘC: Chỉ trả về nội dung đoạn văn. KHÔNG lời chào, KHÔNG tiêu đề, KHÔNG giải thích thêm.";
                    break;

                case "optimize": // Tối ưu kinh nghiệm 
                    prompt = $"Viết duy nhất một đoạn văn mô tả công việc (3-4 câu) sau cho vị trí {context} theo chuẩn STAR: {content}. " +
                            $"YÊU CẦU BẮT BUỘC: Chỉ trả về các gạch đầu dòng nội dung. TUYỆT ĐỐI KHÔNG có lời dẫn, không có câu 'Dưới đây là...', không tiêu đề.";
                    break;

                case "suggest_skills": // Gợi ý kỹ năng
                    prompt = $"Liệt kê 10 kỹ năng quan trọng nhất cho vị trí '{content}'. " +
                            $"YÊU CẦU BẮT BUỘC: Chỉ trả về tên các kỹ năng, ngăn cách nhau bằng duy nhất dấu phẩy. " +
                            $"KHÔNG đánh số, KHÔNG lời dẫn, KHÔNG giải thích.";
                    break;

                case "cover_letter":
                // context ở đây sẽ chứa: "Tên công ty | Vị trí ứng tuyển"
                // content ở đây sẽ chứa: Tóm tắt kinh nghiệm/kỹ năng của người dùng
                prompt = $"Bạn là chuyên gia viết thư xin việc. Hãy viết một bức thư xin việc ấn tượng, chuyên nghiệp " +
                        $"gửi đến công ty {context}. " +
                        $"Dựa trên thông tin ứng viên: {content}. " +
                        $"Yêu cầu: Văn phong thuyết phục, độ dài khoảng 250-300 chữ, có đầy đủ phần mở đầu, nội dung chính và kết bài. " +
                        $"Chỉ trả về nội dung bức thư, không kèm lời chào của AI.";
                break;
                default:
                    return Json(new { success = false, data = "Loại yêu cầu không hợp lệ!" });
            }

            // 4. Gọi Service AI
            try 
            {
                var aiResult = await _aiService.GenerateContent(prompt);
                
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