namespace DoAnCS.Models
{
    public class Resume
    {
        public int ResumeID { get; set; }
        public int UserID { get; set; }
        public int? TemplateID { get; set; }
        public string Title { get; set; }
        
        // Các trường thông tin cá nhân mới
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? AvatarUrl { get; set; }
        
        public string? Summary { get; set; }
        public string? ThemeColor { get; set; }
        public bool IsDraft { get; set; } = true;
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Trường lưu trữ toàn bộ cấu trúc CV định dạng JSON từ Vue Builder
        public string? JsonContent { get; set; }

        public virtual User User { get; set; } 
        public virtual Template Template { get; set; }
        public virtual ICollection<ResumeSection> ResumeSections { get; set; } = new List<ResumeSection>();

    }
}