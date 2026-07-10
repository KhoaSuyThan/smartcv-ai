using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    // Đại diện cho một phiên phỏng vấn thử hoặc làm bài thi phỏng vấn của ứng viên
    public class InterviewSession
    {
        [Key]
        public int SessionID { get; set; }

        // ID của ứng viên thực hiện phỏng vấn
        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        // ID của tin tuyển dụng liên quan (có thể null nếu phỏng vấn tự do)
        public int? JobID { get; set; }

        [ForeignKey("JobID")]
        public Job Job { get; set; }

        // ID của CV ứng viên sử dụng để phỏng vấn
        [Required]
        public int ResumeID { get; set; }

        [ForeignKey("ResumeID")]
        public Resume Resume { get; set; }

        // Trạng thái phiên: 0 - Đang diễn ra, 1 - Đã hoàn thành
        public int Status { get; set; } = 0;

        // Điểm đánh giá tổng hợp của phiên phỏng vấn (0-100)
        public int? OverallScore { get; set; }

        // Nhận xét, đánh giá tổng quan từ AI hoặc Nhà tuyển dụng sau khi kết thúc
        public string? AiEvaluation { get; set; }

        // ID của Nhà tuyển dụng giao bài (null nếu tự luyện tập)
        public int? AssignedByRecruiterID { get; set; }

        [ForeignKey("AssignedByRecruiterID")]
        public User Recruiter { get; set; }

        // Loại phỏng vấn: 0 - Tự luận chat 1-1, 1 - Trắc nghiệm, 2 - Bài thi do Recruiter giao
        public int InterviewType { get; set; } = 0;

        // Thời gian bắt đầu
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Thời gian kết thúc phỏng vấn
        public DateTime? CompletedAt { get; set; }

        // Danh sách tin nhắn/câu hỏi câu trả lời trong phiên phỏng vấn
        public List<InterviewMessage> Messages { get; set; } = new List<InterviewMessage>();
    }
}
