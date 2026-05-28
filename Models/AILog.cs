using System;
using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    public class AILog
    {
        [Key]
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public User? User { get; set; }

        public string RequestType { get; set; }
        public string InputText { get; set; }
        public string OutputText { get; set; }
        public int UsedTokens { get; set; }
        public string? ApiProvider { get; set; } // "Gemini" or "Groq"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}