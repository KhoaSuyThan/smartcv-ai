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

        public GeminiService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GenerateContent(string prompt)
        {
            try {
                // 1. Lấy cấu hình - Thêm .AsNoTracking() để tăng tốc độ đọc dữ liệu
                var config = await _context.GeminiConfigs.AsNoTracking().FirstOrDefaultAsync();
                
                if (config == null || string.IsNullOrEmpty(config.ApiKey)) {
                    Console.WriteLine("CRITICAL ERROR: API Key is NULL in Database!");
                    return "Lỗi: Hệ thống chưa lấy được mã API!";
                }

                // 2. Build URL (Sử dụng v1beta để dùng được tính năng System Instruction)
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{config.ModelName}:generateContent?key={config.ApiKey}";

                // 2. Đảm bảo Prompt không rỗng
                if (string.IsNullOrWhiteSpace(prompt)) return "Nội dung yêu cầu trống.";

                var requestBody = new { 
                    // Ngăn riêng cho "Cái tôi" của AI - Giúp AI bám sát vai trò chuyên gia CV
                    system_instruction = new {
                        parts = new { text = config.SystemInstruction ?? "Bạn là trợ lý ảo hỗ trợ tạo CV chuyên nghiệp." }
                    },
                    contents = new[] { 
                        new { parts = new[] { new { text = prompt } } } 
                    },
                    generationConfig = new {
                        temperature = config.Temperature,
                        maxOutputTokens = config.MaxOutputTokens,
                        topP = 0.95,
                        topK = 64
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