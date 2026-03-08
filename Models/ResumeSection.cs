using System.ComponentModel.DataAnnotations;
namespace DoAnCS.Models {
    public class ResumeSection {
        [Key]
        public int SectionID { get; set; }
        public int ResumeID { get; set; }
        public Resume Resume { get; set; }
        public string SectionType { get; set; }
        public string ContentJSON { get; set; }
        public int SortOrder { get; set; }
    }
}