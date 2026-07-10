using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    // Đại diện cho bộ câu hỏi và đáp án chuẩn bị phỏng vấn mà nhà tuyển dụng tạo ra cho từng ứng viên
    public class RecruiterInterviewPrep
    {
        [Key]
        public int PrepID { get; set; }

        // ID của nhà tuyển dụng tạo bộ câu hỏi
        [Required]
        public int RecruiterID { get; set; }

        [ForeignKey("RecruiterID")]
        public User Recruiter { get; set; }

        // ID của CV ứng viên được nhắm mục tiêu phỏng vấn
        [Required]
        public int ResumeID { get; set; }

        [ForeignKey("ResumeID")]
        public Resume Resume { get; set; }

        // ID của công việc tương ứng
        [Required]
        public int JobID { get; set; }

        [ForeignKey("JobID")]
        public Job Job { get; set; }

        // Dữ liệu câu hỏi, đáp án gợi ý và tiêu chí đánh giá được lưu dưới dạng JSON
        [Required]
        public string QuestionsJson { get; set; }

        // Thời gian tạo
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
