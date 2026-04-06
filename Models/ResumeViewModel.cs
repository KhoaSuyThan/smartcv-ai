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
        public string? Website { get; set; }

        // Các danh sách động (Lưu vào ResumeSections dạng JSON)
        public List<ExperienceItem>? Experiences { get; set; }
        public List<EducationItem>? Educations { get; set; }
        public List<SkillItem>? Skills { get; set; }      // Bổ sung cho mục "Tin học" & "Kỹ năng khác"
        public List<LanguageItem>? Languages { get; set; } // Bổ sung cho mục "Ngoại ngữ"
        public List<OtherSkillItem>? OtherSkills { get; set; }
        public List<AwardItem>? Awards { get; set; }
        public List<ReferenceItem>? References { get; set; }
        public List<CertificationItem>? Certifications { get; set; }
        public List<ActivityItem>? Activities { get; set; }
        public List<HobbyItem>? Hobbies { get; set; }
        public List<ProjectItem>? Projects { get; set; }
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
        public string GraduationType { get; set; }
    }

    public class SkillItem {
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class LanguageItem {
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class OtherSkillItem {
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class AwardItem {
        public string Name { get; set; }
    }

    public class ReferenceItem {
        public string Info { get; set; }
    }

    public class CertificationItem {
        public string Name { get; set; }
        public string Year { get; set; }
    }

    public class ActivityItem {
        public string Name { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
    }

    public class HobbyItem {
        public string Name { get; set; }
    }

    public class ProjectItem {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
    }
}