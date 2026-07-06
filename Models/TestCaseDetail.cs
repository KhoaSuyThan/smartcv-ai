using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    // Lưu thông tin kết quả chi tiết của từng ca kiểm thử
    public class TestCaseDetail
    {
        [Key]
        public int TestCaseID { get; set; }

        // Khóa ngoại liên kết với lượt chạy TestRun
        public int TestRunID { get; set; }
        public TestRun TestRun { get; set; }

        // Tên ca kiểm thử (ví dụ: 'Đăng nhập sai mật khẩu')
        public string Name { get; set; }

        // Cách thức thực hiện (ví dụ: GET, POST, Playwright)
        public string Method { get; set; }

        // Đường dẫn URL của API hoặc trang được gọi
        public string? Url { get; set; }

        // Trạng thái: 'Success' hoặc 'Failed'
        public string Status { get; set; }

        // Thời gian phản hồi thực tế (mili giây)
        public long ResponseTimeMs { get; set; }

        // Kết quả mong đợi khi chạy
        public string? ExpectedResult { get; set; }

        // Kết quả thực tế thu được
        public string? ActualResult { get; set; }

        // Chi tiết lỗi hệ thống nếu kiểm thử bị thất bại
        public string? ErrorMessage { get; set; }
    }
}
