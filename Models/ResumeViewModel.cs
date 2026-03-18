namespace DoAnCS.Models.ViewModels
{
    public class ResumeViewModel
    {
        public int TemplateID { get; set; }
        public string Title { get; set; }
        public int ResumeID { get; set; }
        // Thông tin cá nhân (Đầy đủ theo mẫu)
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; } // Nên để string để nhập format 27/01/1998 dễ hơn
        public string Summary { get; set; }
        public string? AvatarUrl { get; set; } // Cần có để hiện ảnh thay vì icon vỡ

        // Các danh sách động (Lưu vào ResumeSections dạng JSON)
        public List<ExperienceItem>? Experiences { get; set; }
        public List<EducationItem>? Educations { get; set; }
        public List<SkillItem>? Skills { get; set; }      // Bổ sung cho mục "Tin học" & "Kỹ năng khác"
        public List<LanguageItem>? Languages { get; set; } // Bổ sung cho mục "Ngoại ngữ"
    }

    public class ExperienceItem {
        public string Company { get; set; }
        public string Role { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
    }

    public class EducationItem {
        public string School { get; set; }
        public string Major { get; set; }
        public string Year { get; set; }
    }

    // Bổ sung Class để lưu Kỹ năng (có đánh giá sao)
    public class SkillItem {
        public string Name { get; set; }
        public double Rating { get; set; } // Ví dụ: 4 hoặc 4.5 sao
    }

    // Bổ sung Class để lưu Ngoại ngữ
    public class LanguageItem {
        public string Name { get; set; }
        public string Level { get; set; } // Ví dụ: Trung cấp, Sơ cấp
    }
}