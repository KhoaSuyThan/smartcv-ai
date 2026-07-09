using System.Text;
using DoAnCS.Services;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCS.Services 
{
    public class GeminiService : IAIService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IEncryptionService _encryptionService; // Inject Encryption Service

        public GeminiService(AppDbContext context, IHttpClientFactory httpClientFactory, IEncryptionService encryptionService)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _encryptionService = encryptionService;
        }

        public async Task<string> GenerateContent(string prompt, bool isPro = false, string? systemInstruction = null, double? temperature = null, bool responseJson = false)
        {
            int maxRetries = 5;
            int delayMs = 5000;

            for (int i = 0; i < maxRetries; i++)
            {
                try {
                    // 1. Lấy cấu hình - Thêm .AsNoTracking() để tăng tốc độ đọc dữ liệu
                    var config = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
                    
                    if (config == null || string.IsNullOrEmpty(config.ApiKey)) {
                        Console.WriteLine("CRITICAL ERROR: API Key is NULL in Database!");
                        return "Lỗi: Hệ thống chưa lấy được mã API!";
                    }

                    // Giải mã API key bảo mật trước khi gọi các dịch vụ LLM bên ngoài
                    string decryptedApiKey = _encryptionService.Decrypt(config.ApiKey);
                    string decryptedGroqApiKey = _encryptionService.Decrypt(config.GroqApiKey);

                    // Chọn Model dựa trên trạng thái Pro
                    string selectedModel = isPro ? (config.ProModelName ?? "gemini-2.5-pro") : config.ModelName;
                    double selectedTemp = temperature ?? (isPro ? config.ProTemperature : config.Temperature);
                    int selectedMaxTokens = isPro ? config.ProMaxOutputTokens : config.MaxOutputTokens;
                    
                    bool isGroq = selectedModel.Contains("llama") || selectedModel.Contains("mixtral");
                    
                    if (!isGroq && selectedMaxTokens < 8192) {
                        selectedMaxTokens = 8192; // Tăng lên 8192 vì tiếng Việt tốn rất nhiều token (Chỉ áp dụng cho Gemini)
                    } else if (isGroq) {
                        // Groq Free Tier TPM is often 6000. 
                        // If we request too many max_tokens, it instantly hits TPM limit.
                        if (selectedMaxTokens > 4000) selectedMaxTokens = 4000;
                    }

                    if (isGroq && string.IsNullOrEmpty(decryptedGroqApiKey)) {
                        Console.WriteLine("CRITICAL ERROR: Groq API Key is NULL in Database!");
                        return "Lỗi: Hệ thống chưa có mã Groq API Key!";
                    }

                    // 2. Build URL với Key đã giải mã
                    string url = isGroq ? "https://api.groq.com/openai/v1/chat/completions" 
                                        : $"https://generativelanguage.googleapis.com/v1beta/models/{selectedModel}:generateContent?key={decryptedApiKey}";

                    // 2. Đảm bảo Prompt không rỗng
                    if (string.IsNullOrWhiteSpace(prompt)) return "Nội dung yêu cầu trống.";

                    string finalSystemInstruction = systemInstruction ?? config.SystemInstruction ?? "Bạn là trợ lý ảo hỗ trợ tạo CV chuyên nghiệp.";

                    object requestBody;
                    if (isGroq)
                    {
                        requestBody = responseJson ? (object)new
                        {
                            model = selectedModel,
                            messages = new[]
                            {
                                new { role = "system", content = finalSystemInstruction },
                                new { role = "user", content = prompt }
                            },
                            temperature = selectedTemp,
                            max_tokens = selectedMaxTokens,
                            response_format = new { type = "json_object" }
                        } : new
                        {
                            model = selectedModel,
                            messages = new[]
                            {
                                new { role = "system", content = finalSystemInstruction },
                                new { role = "user", content = prompt }
                            },
                            temperature = selectedTemp,
                            max_tokens = selectedMaxTokens
                        };
                    }
                    else
                    {
                        requestBody = new { 
                            system_instruction = new {
                                parts = new { text = finalSystemInstruction }
                            },
                            contents = new[] { 
                                new { parts = new[] { new { text = prompt } } } 
                            },
                            generationConfig = responseJson ? (object)new {
                                temperature = selectedTemp,
                                maxOutputTokens = selectedMaxTokens,
                                topP = 0.95,
                                topK = 64,
                                responseMimeType = "application/json"
                            } : new {
                                temperature = selectedTemp,
                                maxOutputTokens = selectedMaxTokens,
                                topP = 0.95,
                                topK = 64
                            }
                        };
                    }

                    var json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var requestMsg = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                    if (isGroq) {
                        requestMsg.Headers.Add("Authorization", $"Bearer {decryptedGroqApiKey}");
                    }

                    var response = await _httpClient.SendAsync(requestMsg);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        
                        if (isGroq) {
                            if (result?.choices != null && result.choices.Count > 0) {
                                return result.choices[0].message.content;
                            }
                        } else {
                            if (result?.candidates != null && result.candidates.Count > 0) {
                                return result.candidates[0].content.parts[0].text;
                            }
                        }
                        return "AI không thể tạo nội dung cho yêu cầu này.";
                    }
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || 
                        response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        // Try to extract exact retry delay from Google's error message (Groq also sometimes provides retry-after)
                        int exactDelayMs = delayMs;
                        try {
                            if (response.Headers.TryGetValues("Retry-After", out var values))
                            {
                                if (int.TryParse(values.FirstOrDefault(), out int retryAfterSecs)) {
                                    exactDelayMs = (retryAfterSecs * 1000) + 1000;
                                }
                            }
                            else 
                            {
                                dynamic errorJson = JsonConvert.DeserializeObject(responseString);
                                if (errorJson?.error?.details != null) {
                                    foreach (var detail in errorJson.error.details) {
                                        if (detail["@type"]?.ToString() == "type.googleapis.com/google.rpc.RetryInfo" && detail.retryDelay != null) {
                                            string delayStr = detail.retryDelay.ToString().Replace("s", "");
                                            if (double.TryParse(delayStr, System.Globalization.CultureInfo.InvariantCulture, out double secs)) {
                                                exactDelayMs = (int)(secs * 1000) + 2000; // Add 2s buffer
                                            }
                                        }
                                    }
                                }
                            }
                        } catch {}

                        Console.WriteLine($"[API Error] Lỗi {response.StatusCode}. Thử lại lần {i + 1}/{maxRetries} sau {exactDelayMs}ms...");
                        if (i < maxRetries - 1)
                        {
                            await Task.Delay(exactDelayMs);
                            delayMs = exactDelayMs > 0 ? exactDelayMs : delayMs * 2;
                            continue;
                        }
                    }

                    // In lỗi chi tiết ra Console để bạn copy gửi tôi nếu vẫn lỗi
                    Console.WriteLine("AI API Error: " + responseString);
                    return $"Lỗi API: {response.StatusCode}";
                }
                catch (Exception ex) {
                    Console.WriteLine("Exception: " + ex.Message);
                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(delayMs);
                        delayMs *= 2;
                        continue;
                    }
                    return "Lỗi kết nối hệ thống AI!";
                }
            }
            return "Lỗi kết nối hệ thống AI sau nhiều lần thử!";
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            try
            {
                var config = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
                if (config == null || string.IsNullOrEmpty(config.ApiKey))
                {
                    Console.WriteLine("CRITICAL ERROR: API Key is NULL in Database!");
                    return Array.Empty<float>();
                }

                if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

                // Giải mã API key bảo mật
                string decryptedApiKey = _encryptionService.Decrypt(config.ApiKey);

                // Sử dụng chính xác model gemini-embedding-001 từ danh sách API của bạn
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={decryptedApiKey}";

                // Payload chuẩn của Google Gemini API embedContent
                var requestBody = new
                {
                    content = new
                    {
                        parts = new[] { new { text = text } }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    if (result?.embedding?.values != null)
                    {
                        var values = new List<float>();
                        foreach (var val in result.embedding.values)
                        {
                            values.Add((float)val);
                        }
                        return values.ToArray();
                    }
                }

                Console.WriteLine($"Embedding API Error: {responseString}");
                return Array.Empty<float>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Embedding Exception: {ex.Message}");
                return Array.Empty<float>();
            }
        }
        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || texts.Count == 0) return new List<float[]>();

            try
            {
                var config = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
                if (config == null || string.IsNullOrEmpty(config.ApiKey))
                {
                    Console.WriteLine("CRITICAL ERROR: API Key is NULL in Database!");
                    return new List<float[]>();
                }

                // Giải mã API key bảo mật
                string decryptedApiKey = _encryptionService.Decrypt(config.ApiKey);

                // URL cho batch embedding với Key đã giải mã
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:batchEmbedContents?key={decryptedApiKey}";

                // Payload chuẩn cho batchEmbedContents
                var requestBody = new
                {
                    requests = texts.Select(t => new
                    {
                        model = "models/gemini-embedding-001",
                        content = new { parts = new[] { new { text = t } } }
                    }).ToArray()
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    if (result?.embeddings != null)
                    {
                        var allEmbeddings = new List<float[]>();
                        foreach (var emb in result.embeddings)
                        {
                            var values = new List<float>();
                            foreach (var val in emb.values)
                            {
                                values.Add((float)val);
                            }
                            allEmbeddings.Add(values.ToArray());
                        }
                        return allEmbeddings;
                    }
                }

                Console.WriteLine($"Batch Embedding API Error: {responseString}");
                return new List<float[]>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Batch Embedding Exception: {ex.Message}");
                return new List<float[]>();
            }
        }
    }
}