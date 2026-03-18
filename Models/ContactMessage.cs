using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; } // Khớp với [Id]

        [Required]
        public string Name { get; set; } // Khớp với [Name]

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Khớp với [Email]

        public string? Subject { get; set; } // Khớp với [Subject]

        [Required]
        public string Message { get; set; } // Khớp với [Message]

        public DateTime SentAt { get; set; } = DateTime.Now; // Khớp với [SentAt]
        public string? AttachmentUrl { get; set; }
    }
}