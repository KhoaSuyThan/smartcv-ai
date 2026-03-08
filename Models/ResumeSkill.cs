namespace DoAnCS.Models {
    public class ResumeSkill {
        public int ResumeID { get; set; }
        public Resume Resume { get; set; }
        public int SkillID { get; set; }
        public Skill Skill { get; set; }
        public string Proficiency { get; set; }
    }
}