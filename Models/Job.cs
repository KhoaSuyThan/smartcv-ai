using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    public class Job
    {
        public int JobID { get; set; }
        public int RecruiterID { get; set; }
        public User Recruiter { get; set; } // Liên kết với bảng Users

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả công việc là bắt buộc")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu công việc là bắt buộc")]
        public string Requirements { get; set; }

        [Required(ErrorMessage = "Mức lương là bắt buộc")]
        public string Salary { get; set; }

        [Required(ErrorMessage = "Hạn nộp hồ sơ là bắt buộc")]
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CompanyID { get; set; }
        public virtual Company Company { get; set; } // Liên kết với bảng Companies
        public ICollection<Application> Applications { get; set; }

        public int Status { get; set; } = 0;
    }
}