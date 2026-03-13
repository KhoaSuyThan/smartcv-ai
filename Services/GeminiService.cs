using System.Text;
using DoAnCS.Services;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DoAnCS.Services 
{
    public class GeminiService : IAIService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerateContent(string prompt)
        {
            try {
                // 1. Kiểm tra API Key có lấy được không
                string apiKey = _config["Gemini:ApiKey"]?.Trim();
                if (string.IsNullOrEmpty(apiKey)) {
                    Console.WriteLine("CRITICAL ERROR: API Key is NULL. Check appsettings.json!");
                    return "Lỗi: Hệ thống chưa lấy được mã API!";
                }

                string url =
$"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                // 2. Đảm bảo Prompt không rỗng
                if (string.IsNullOrWhiteSpace(prompt)) return "Nội dung yêu cầu trống.";

                var requestBody = new { 
                    contents = new[] { 
                        new { parts = new[] { new { text = prompt } } } 
                    } 
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    
                    // 3. Kiểm tra xem có kết quả trả về không (phòng trường hợp bị chặn nội dung)
                    if (result?.candidates != null && result.candidates.Count > 0) {
                        return result.candidates[0].content.parts[0].text;
                    }
                    return "AI không thể tạo nội dung cho yêu cầu này.";
                }
                
                // In lỗi chi tiết ra Console để bạn copy gửi tôi nếu vẫn lỗi
                Console.WriteLine("AI API Error: " + responseString);
                return $"Lỗi API: {response.StatusCode}";
            }
            catch (Exception ex) {
                Console.WriteLine("Exception: " + ex.Message);
                return "Lỗi kết nối hệ thống AI!";
            }
        }
    }
}