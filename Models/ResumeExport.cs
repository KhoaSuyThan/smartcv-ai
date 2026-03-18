using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models // Đổi lại đúng Namespace dự án của bạn
{
    public class ResumeExport
    {
        [Key]
        public int ExportID { get; set; }

        public int ResumeID { get; set; }

        public DateTime ExportDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string FileUrl { get; set; }

        // Liên kết với bảng Resumes
        [ForeignKey("ResumeID")]
        public virtual Resume Resume { get; set; }
    }
}