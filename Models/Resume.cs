using System;
using System.Collections.Generic;

namespace DoAnCS.Models {
    public class Resume {
        public int ResumeID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public int TemplateID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ThemeColor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<ResumeSection> ResumeSections { get; set; }
        public ICollection<ResumeSkill> ResumeSkills { get; set; }
    }
}