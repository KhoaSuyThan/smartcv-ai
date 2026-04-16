using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCS.Models
{
    [Table("VueTemplates")]
    public class VueTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ComponentName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        public bool IsPremium { get; set; } = false;

        public bool IsActive { get; set; } = true;
        
        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
