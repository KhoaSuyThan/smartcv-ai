using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    public class UpgradeRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public int Status { get; set; } = 0; // 0: Chờ duyệt, 1: Đã duyệt, 2: Từ chối

        public string? EvidenceImageUrl { get; set; } // Ảnh minh chứng thanh toán

        public string? Notes { get; set; } // Ghi chú từ người dùng hoặc lý do từ chối từ Admin
        
        public DateTime? DecisionDate { get; set; } // Ngày Admin xử lý
    }
}
