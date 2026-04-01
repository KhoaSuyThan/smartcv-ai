using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    public class GeminiConfig
    {
        [Key]
        public int Id { get; set; } = 1; // Chỉ dùng 1 bản ghi duy nhất

        // 1. Core Settings
        public string? ApiKey { get; set; }
        public string ModelName { get; set; } = "gemini-2.5-flash";
        public double Temperature { get; set; } = 0.7;
        public int MaxOutputTokens { get; set; } = 2048;

        // 2. Prompt Management
        public string? SystemInstruction { get; set; }
        public string? SkillTemplate { get; set; }
        public string? SummaryTemplate { get; set; }
        public string? GrammarTemplate { get; set; }

        // 3. Quota & Limits
        public int UserRateLimit { get; set; } = 10;
        public long TotalTokensUsed { get; set; } = 0; // Cộng dồn để tính tiền
    }
}