using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    public class GeminiConfig
    {
        [Key]
        public int Id { get; set; } = 1; // Chỉ dùng 1 bản ghi duy nhất

        // 1. Core Settings (Free)
        public string? ApiKey { get; set; }
        public string? GroqApiKey { get; set; }
        public string? ChatbotApiKey { get; set; }  // Key riêng cho Chatbox AI (tách quota)
        public string? ModelName { get; set; } = "gemini-2.5-flash";
        public double Temperature { get; set; } = 0.7;
        public int MaxOutputTokens { get; set; } = 2048;

        // 1.1 Core Settings (Pro)
        public string? ProModelName { get; set; } = "gemini-2.5-pro";
        public double ProTemperature { get; set; } = 0.9;
        public int ProMaxOutputTokens { get; set; } = 4096;

        // 2. Prompt Management
        public string? SystemInstruction { get; set; }
        public string? ChatbotSystemInstruction { get; set; }
        public string? SkillTemplate { get; set; }
        public string? SummaryTemplate { get; set; }
        public string? GrammarTemplate { get; set; }

        // 3. Quota & Limits
        public int UserRateLimit { get; set; } = 10;
        public int ProUserRateLimit { get; set; } = 50; 
        public long TotalTokensUsed { get; set; } = 0; // Cộng dồn để tính tiền

        // 4. Smart Match Settings
        /// <summary>Số ứng viên tối đa đưa vào phân tích LLM sau bước lọc Vector (mặc định: 6)</summary>
        public int TopCandidatesCount { get; set; } = 6;
    }
}