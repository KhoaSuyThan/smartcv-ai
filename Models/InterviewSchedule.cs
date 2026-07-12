using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    // Model lưu trữ thông tin lịch phỏng vấn của ứng viên
    public class InterviewSchedule
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public Application Application { get; set; }

        [Required]
        public DateTime InterviewTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string LocationType { get; set; } // "Online" hoặc "Offline"

        [Required]
        [MaxLength(500)]
        public string Location { get; set; } // Link online meeting (Zoom/Meet) hoặc Địa chỉ thực tế văn phòng

        public string? Notes { get; set; } // Lời nhắn hoặc chỉ dẫn từ nhà tuyển dụng

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
