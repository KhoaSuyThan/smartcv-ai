using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    public class SavedCandidate
    {
        [Key]
        public int Id { get; set; }

        public int RecruiterId { get; set; }
        
        [ForeignKey("RecruiterId")]
        public virtual User Recruiter { get; set; }

        public int ResumeId { get; set; }
        
        [ForeignKey("ResumeId")]
        public virtual Resume Resume { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;
    }
}
