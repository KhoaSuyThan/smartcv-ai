using System.Collections.Generic;

namespace DoAnCS.Models
{
    public class ResumeViewModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public int TemplateID { get; set; }
        public string ThemeColor { get; set; }
        public List<ExperienceItem> Experiences { get; set; }
        public List<EducationItem> Educations { get; set; }
    }

    public class ExperienceItem
    {
        public string Company { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
    }

    public class EducationItem
    {
        public string School { get; set; }
        public string Degree { get; set; }
        public string Year { get; set; }
    }
}