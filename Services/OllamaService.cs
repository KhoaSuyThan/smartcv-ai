using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DoAnCS.Services
{
    /// <summary>
    /// Dịch vụ kết nối và suy luận trực tiếp với Ollama Local AI Engine
    /// Hỗ trợ nạp mô hình Qwen 2.5 cục bộ không tốn chi phí Token
    /// </summary>
    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService> _logger;
        private static string? _cachedModel = null;
        private static DateTime _lastModelCheck = DateTime.MinValue;
        private static readonly string[] PriorityModels = new[]
        {
            "qwen2.5:3b",
            "qwen2.5:1.5b",
            "qwen2.5-coder:7b",
            "qwen2.5:7b",
            "llama3.1:8b",
            "deepseek-r1:7b"
        };

        public OllamaService(IHttpClientFactory httpClientFactory, ILogger<OllamaService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:11434/");
            _logger = logger;
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var res = await _httpClient.GetAsync("api/tags", cts.Token);
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetActiveModelAsync()
        {
            // Cache model name trong 2 phút để giảm tải truy vấn lặp lại
            if (_cachedModel != null && (DateTime.Now - _lastModelCheck).TotalMinutes < 2)
            {
                return _cachedModel;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
                var res = await _httpClient.GetAsync("api/tags", cts.Token);
                if (!res.IsSuccessStatusCode) return null;

                string jsonStr = await res.Content.ReadAsStringAsync();
                var data = JObject.Parse(jsonStr);
                var modelsArr = data["models"] as JArray;
                if (modelsArr == null || !modelsArr.Any()) return null;

                var installedModels = modelsArr
                    .Select(m => m["name"]?.ToString())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Select(n => n!.ToLower().Trim())
                    .ToList();

                // Lựa chọn theo thứ tự ưu tiên cấu hình
                foreach (var pref in PriorityModels)
                {
                    var found = installedModels.FirstOrDefault(m => m == pref || m.StartsWith(pref + ":") || m.StartsWith(pref));
                    if (found != null)
                    {
                        _cachedModel = found;
                        _lastModelCheck = DateTime.Now;
                        _logger.LogInformation("[OllamaService] Đã chọn mô hình suy luận: {Model}", _cachedModel);
                        return _cachedModel;
                    }
                }

                // Nếu không có model ưu tiên, lấy model đầu tiên có sẵn
                _cachedModel = installedModels.First();
                _lastModelCheck = DateTime.Now;
                return _cachedModel;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[OllamaService] Không thể kết nối lấy danh sách model: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<string> GenerateJsonAsync(string prompt, string? systemInstruction = null, double temperature = 0.15, int timeoutSeconds = 45)
        {
            string? model = await GetActiveModelAsync();
            if (string.IsNullOrEmpty(model))
            {
                throw new InvalidOperationException("Ollama không khả dụng hoặc chưa tải mô hình nào.");
            }

            var requestBody = new
            {
                model = model,
                system = systemInstruction ?? "You are an expert technical interviewer. Output valid JSON only.",
                prompt = prompt,
                format = "json",
                stream = false,
                options = new
                {
                    temperature = temperature,
                    top_p = 0.9,
                    num_ctx = 4096
                }
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var res = await _httpClient.PostAsync("api/generate", content, cts.Token);
            res.EnsureSuccessStatusCode();

            string raw = await res.Content.ReadAsStringAsync();
            var parsed = JObject.Parse(raw);
            string responseText = parsed["response"]?.ToString() ?? string.Empty;

            return responseText.Trim();
        }

        public async Task<string> GenerateTextAsync(string prompt, string? systemInstruction = null, double temperature = 0.7, int timeoutSeconds = 45)
        {
            string? model = await GetActiveModelAsync();
            if (string.IsNullOrEmpty(model))
            {
                throw new InvalidOperationException("Ollama không khả dụng hoặc chưa tải mô hình nào.");
            }

            var requestBody = new
            {
                model = model,
                system = systemInstruction ?? "You are a professional hiring manager and interviewer.",
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = temperature,
                    top_p = 0.9,
                    num_ctx = 4096
                }
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var res = await _httpClient.PostAsync("api/generate", content, cts.Token);
            res.EnsureSuccessStatusCode();

            string raw = await res.Content.ReadAsStringAsync();
            var parsed = JObject.Parse(raw);
            return (parsed["response"]?.ToString() ?? string.Empty).Trim();
        }
    }
}
