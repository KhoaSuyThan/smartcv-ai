using System.Text.Json.Serialization;

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

        [JsonPropertyName("themeColor")]
        public string? ThemeColor { get; set; }

        [JsonPropertyName("fontFamily")]
        public string? FontFamily { get; set; }

        [JsonPropertyName("bgColor")]
        public string? BgColor { get; set; }

        [JsonPropertyName("textAlign")]
        public string? TextAlign { get; set; }

        [JsonPropertyName("visibleSections")]
        public Dictionary<string, bool>? VisibleSections { get; set; }

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
        [JsonPropertyName("company")]
        public string Company { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("time")]
        public string Duration { get; set; }
        [JsonPropertyName("desc")]
        public string Description { get; set; }
    }

    public class EducationItem {
        [JsonPropertyName("school")]
        public string School { get; set; }
        [JsonPropertyName("major")]
        public string Major { get; set; }
        [JsonPropertyName("year")]
        public string Year { get; set; }
        [JsonPropertyName("gradType")]
        public string GraduationType { get; set; }
    }

    public class SkillItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("level")]
        public string Level { get; set; }
    }

    public class LanguageItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("level")]
        public string Level { get; set; }
    }

    public class OtherSkillItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("level")]
        public string Level { get; set; }
    }

    public class AwardItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ReferenceItem {
        [JsonPropertyName("info")]
        public string Info { get; set; }
    }

    public class CertificationItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("year")]
        public string Year { get; set; }
    }

    public class ActivityItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("time")]
        public string Time { get; set; }
        [JsonPropertyName("desc")]
        public string Description { get; set; }
    }

    public class HobbyItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ProjectItem {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("time")]
        public string Time { get; set; }
        [JsonPropertyName("desc")]
        public string Description { get; set; }
    }
}