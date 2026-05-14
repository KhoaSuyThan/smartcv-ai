using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    /// <summary>
    /// Lưu trữ Vector nhúng (Text Embeddings) của CV ứng viên phục vụ cho tìm kiếm RAG.
    /// </summary>
    public class CVEmbedding
    {
        [Key]
        public int Id { get; set; }

        public int ResumeID { get; set; }

        [ForeignKey("ResumeID")]
        public virtual Resume Resume { get; set; }

        /// <summary>Chuỗi JSON chứa mảng float[] biểu diễn vector (ví dụ: [0.12, -0.04, ...])</summary>
        public string VectorJson { get; set; } = "[]";

        /// <summary>Thời điểm nhúng dữ liệu</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
