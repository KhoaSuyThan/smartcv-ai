using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    /// <summary>
    /// Lưu kết quả phân tích AI so khớp giữa CV ứng viên và Job Description.
    /// Mỗi bản ghi đại diện cho một cặp (Job, Resume) đã được AI đánh giá.
    /// </summary>
    public class CVMatchResult
    {
        [Key]
        public int Id { get; set; }

        public int JobID { get; set; }

        [ForeignKey("JobID")]
        public virtual Job Job { get; set; }

        public int ResumeID { get; set; }

        [ForeignKey("ResumeID")]
        public virtual Resume Resume { get; set; }

        /// <summary>Điểm phù hợp tổng thể (0-100)</summary>
        public int MatchScore { get; set; }

        /// <summary>JSON array các kỹ năng đã khớp, ví dụ: ["C#", ".NET"]</summary>
        public string? MatchedSkills { get; set; }

        /// <summary>JSON array các kỹ năng còn thiếu, ví dụ: ["Docker", "AWS"]</summary>
        public string? MissingSkills { get; set; }

        /// <summary>JSON array các gợi ý cải thiện</summary>
        public string? Suggestions { get; set; }

        /// <summary>3 điểm mạnh nổi bật</summary>
        public string? Strengths { get; set; }

        /// <summary>Nhận xét tổng quan bằng tiếng Việt</summary>
        public string? Summary { get; set; }

        /// <summary>Mức phân loại: strong_match, good_match, partial_match, weak_match</summary>
        public string? Recommendation { get; set; }

        /// <summary>Thời điểm AI phân tích</summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.Now;
    }
}
