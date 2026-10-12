using System.Threading.Tasks;

namespace DoAnCS.Services
{
    /// <summary>
    /// Giao thức giao tiếp với Local AI Inference Engine (Ollama REST API)
    /// </summary>
    public interface IOllamaService
    {
        /// <summary>Kiểm tra xem dịch vụ Ollama cục bộ có đang lắng nghe tại localhost:11434 không</summary>
        Task<bool> IsAvailableAsync();

        /// <summary>Lấy tên mô hình khả dụng tốt nhất theo thứ tự ưu tiên (qwen2.5:3b > qwen2.5:1.5b > qwen2.5-coder:7b)</summary>
        Task<string?> GetActiveModelAsync();

        /// <summary>Sinh phản hồi ép chuẩn JSON từ Local LLM</summary>
        Task<string> GenerateJsonAsync(string prompt, string? systemInstruction = null, double temperature = 0.15, int timeoutSeconds = 45);

        /// <summary>Sinh văn bản tự do thông thường</summary>
        Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null, double temperature = 0.7, int timeoutSeconds = 45);
    }
}
