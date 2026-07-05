using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace DoAnCS.Services
{
    // Lớp kiểm tra sức khỏe của dịch vụ FastAPI Python AI
    public class PythonAiHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public PythonAiHealthCheck(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var pythonBaseUrl = _config["PythonAI:BaseUrl"] ?? "http://localhost:8000";
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3); // Chờ tối đa 3 giây

                // Gửi thử GET request kiểm tra Swagger docs của FastAPI
                var response = await client.GetAsync(pythonBaseUrl + "/docs", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("Dịch vụ Python AI hoạt động bình thường.");
                }
                return HealthCheckResult.Degraded($"Dịch vụ Python AI trả về mã lỗi: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Không thể kết nối tới Dịch vụ Python AI. Vui lòng khởi chạy server FastAPI trước!", ex);
            }
        }
    }
}
