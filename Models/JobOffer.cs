using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    // Model lưu trữ thông tin thư mời nhận việc (Job Offer) của ứng viên
    public class JobOffer
    {
        [Key]
        public int OfferID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [ForeignKey("ApplicationID")]
        public Application Application { get; set; }

        [Required]
        [MaxLength(100)]
        public string Salary { get; set; } // Mức lương đề nghị (ví dụ: "15,000,000 VND" hoặc "Thỏa thuận")

        [Required]
        public DateTime StartDate { get; set; } // Ngày bắt đầu làm việc chính thức

        public string? Notes { get; set; } // Ghi chú, điều khoản thử việc hoặc lời nhắn gửi thêm

        [MaxLength(500)]
        public string? WorkLocation { get; set; } // Địa điểm làm việc đề xuất

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái phản hồi của ứng viên: "Pending", "Accepted", "Declined"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
