using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    // Đại diện cho một tin nhắn, câu hỏi hoặc câu trả lời trong phiên phỏng vấn
    public class InterviewMessage
    {
        [Key]
        public int MessageID { get; set; }

        // ID của phiên phỏng vấn chứa tin nhắn này
        [Required]
        public int SessionID { get; set; }

        [ForeignKey("SessionID")]
        public InterviewSession Session { get; set; }

        // Vai trò của người gửi: 'interviewer' hoặc 'candidate'
        [Required]
        public string Role { get; set; }

        // Nội dung tin nhắn hoặc câu hỏi/câu trả lời
        [Required]
        public string Content { get; set; }

        // Các tùy chọn trắc nghiệm A, B, C, D dưới dạng JSON (nếu là câu trắc nghiệm)
        public string? ChoicesJson { get; set; }

        // Đáp án mà ứng viên lựa chọn (nếu là câu trắc nghiệm)
        public string? SelectedAnswer { get; set; }

        // Điểm đánh giá riêng cho câu trả lời tự luận này (0-100)
        public int? Score { get; set; }

        // Phản hồi nhận xét chi tiết (mô hình STAR) cho câu trả lời này
        public string? Feedback { get; set; }

        // Thời gian gửi tin nhắn
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
