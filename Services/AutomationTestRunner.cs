using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DoAnCS.Data;

namespace DoAnCS.Services
{
    // Cấu trúc kết quả của từng ca kiểm thử nhỏ
    public class TestCaseResult
    {
        public string Name { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public long ResponseTimeMs { get; set; }
        public string Status { get; set; } // "Success" hoặc "Failed"
        public string ErrorMessage { get; set; }
    }

    // Cấu trúc kết quả của cả bộ kiểm thử
    public class TestSuiteResult
    {
        public string SuiteName { get; set; }
        public List<TestCaseResult> TestCases { get; set; } = new List<TestCaseResult>();
    }

    // Giao diện điều phối kiểm thử tự động
    public interface IAutomationTestRunner
    {
        Task<List<TestSuiteResult>> RunAllTestsAsync(string localBaseUrl);
    }

    public class AutomationTestRunner : IAutomationTestRunner
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAIService _aiService;
        private readonly IConfiguration _config;

        public AutomationTestRunner(
            AppDbContext context, 
            IHttpClientFactory httpClientFactory, 
            IAIService aiService, 
            IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _aiService = aiService;
            _config = config;
        }

        // Chạy toàn bộ các suite kiểm thử
        public async Task<List<TestSuiteResult>> RunAllTestsAsync(string localBaseUrl)
        {
            var results = new List<TestSuiteResult>();

            // 1. Chạy Suite kiểm tra sức khỏe hệ thống
            results.Add(await RunSystemHealthSuiteAsync());

            // 2. Chạy Suite xác thực tích hợp API
            results.Add(await RunApiVerificationSuiteAsync(localBaseUrl));

            // 3. Chạy Suite kiểm thử hiệu năng / tải đồng thời
            results.Add(await RunPerformanceSuiteAsync(localBaseUrl));

            return results;
        }

        // Kiểm tra kết nối SQL Server, FastAPI Python AI và Google Gemini API
        private async Task<TestSuiteResult> RunSystemHealthSuiteAsync()
        {
            var suite = new TestSuiteResult { SuiteName = "System Health Check" };
            var client = _httpClientFactory.CreateClient();

            // Ca kiểm thử Database SQL Server
            var dbTest = new TestCaseResult 
            { 
                Name = "Cơ sở dữ liệu SQL Server", 
                Method = "EF Core Connect", 
                Url = "SQL Server LocalInstance",
                Expected = "Kết nối thành công đến database và kiểm tra trạng thái hoạt động."
            };
            var sw = Stopwatch.StartNew();
            try
            {
                if (await _context.Database.CanConnectAsync())
                {
                    dbTest.Status = "Success";
                    dbTest.Actual = "Kết nối SQL Server thành công và sẵn sàng xử lý truy vấn.";
                }
                else
                {
                    dbTest.Status = "Failed";
                    dbTest.Actual = "Không thể thiết lập kết nối đến SQL Server.";
                }
            }
            catch (Exception ex)
            {
                dbTest.Status = "Failed";
                dbTest.Actual = "Lỗi kết nối database: " + ex.Message;
                dbTest.ErrorMessage = ex.ToString();
            }
            dbTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(dbTest);

            // Ca kiểm thử FastAPI Python AI
            var pythonBaseUrl = _config["PythonAI:BaseUrl"] ?? "http://localhost:8000";
            var pythonTest = new TestCaseResult 
            { 
                Name = "Dịch vụ Python FastAPI AI", 
                Method = "GET", 
                Url = pythonBaseUrl + "/docs",
                Expected = "HTTP 200 OK từ tài liệu API docs của server FastAPI."
            };
            sw.Restart();
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var response = await client.GetAsync(pythonTest.Url, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    pythonTest.Status = "Success";
                    pythonTest.Actual = $"Dịch vụ FastAPI phản hồi thành công. HTTP {(int)response.StatusCode}";
                }
                else
                {
                    pythonTest.Status = "Failed";
                    pythonTest.Actual = $"Dịch vụ FastAPI trả về mã lỗi: HTTP {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                pythonTest.Status = "Failed";
                pythonTest.Actual = "Không thể kết nối FastAPI. Vui lòng kiểm tra cổng 8000. Chi tiết: " + ex.Message;
                pythonTest.ErrorMessage = ex.ToString();
            }
            pythonTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(pythonTest);

            // Ca kiểm thử API Gemini
            var geminiTest = new TestCaseResult 
            { 
                Name = "Google Gemini AI API", 
                Method = "IAIService Generate", 
                Url = "Google Gemini Endpoints",
                Expected = "Hệ thống gọi API của Gemini thành công và nhận phản hồi mong muốn."
            };
            sw.Restart();
            try
            {
                // Thử sinh một phản hồi siêu ngắn để kiểm tra khóa API và cấu hình hoạt động
                var response = await _aiService.GenerateContent("hello, reply 'ok' only.");
                if (!string.IsNullOrEmpty(response) && !response.StartsWith("Lỗi"))
                {
                    geminiTest.Status = "Success";
                    geminiTest.Actual = $"Kết nối API Gemini thành công. Nội dung phản hồi: '{response.Trim()}'";
                }
                else
                {
                    geminiTest.Status = "Failed";
                    geminiTest.Actual = $"Không nhận được phản hồi hợp lệ từ Gemini API. Nội dung: {response}";
                }
            }
            catch (Exception ex)
            {
                geminiTest.Status = "Failed";
                geminiTest.Actual = "Lỗi xác thực API Key hoặc kết nối mạng đến Google API: " + ex.Message;
                geminiTest.ErrorMessage = ex.ToString();
            }
            geminiTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(geminiTest);

            return suite;
        }

        // Kiểm tra các API đầu vào quan trọng của trang web
        private async Task<TestSuiteResult> RunApiVerificationSuiteAsync(string localBaseUrl)
        {
            var suite = new TestSuiteResult { SuiteName = "API Integration Verification" };
            var client = _httpClientFactory.CreateClient();

            // 1. Kiểm tra Trang chủ /
            var homeTest = new TestCaseResult 
            { 
                Name = "Tải giao diện Trang chủ", 
                Method = "GET", 
                Url = localBaseUrl,
                Expected = "Mã phản hồi HTTP 200 OK khi người dùng truy cập trang chủ."
            };
            var sw = Stopwatch.StartNew();
            try
            {
                var response = await client.GetAsync(localBaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    homeTest.Status = "Success";
                    homeTest.Actual = $"Trang chủ tải thành công. HTTP {(int)response.StatusCode}";
                }
                else
                {
                    homeTest.Status = "Failed";
                    homeTest.Actual = $"Trang chủ trả về lỗi: HTTP {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                homeTest.Status = "Failed";
                homeTest.Actual = "Lỗi kết nối mạng: " + ex.Message;
                homeTest.ErrorMessage = ex.ToString();
            }
            homeTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(homeTest);

            // 2. Kiểm tra API CV Templates /Resume/VueTemplates
            var templatesUrl = localBaseUrl.TrimEnd('/') + "/Resume/VueTemplates";
            var templatesTest = new TestCaseResult 
            { 
                Name = "API Danh sách mẫu CV", 
                Method = "GET", 
                Url = templatesUrl,
                Expected = "HTTP 200 OK và trả về danh sách template dạng JSON chứa dữ liệu mẫu CV."
            };
            sw.Restart();
            try
            {
                var response = await client.GetAsync(templatesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    templatesTest.Status = "Success";
                    templatesTest.Actual = $"API hoạt động ổn định. Nhận JSON độ dài: {content.Length} kí tự.";
                }
                else
                {
                    templatesTest.Status = "Failed";
                    templatesTest.Actual = $"API trả về lỗi: HTTP {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                templatesTest.Status = "Failed";
                templatesTest.Actual = "Lỗi kết nối API: " + ex.Message;
                templatesTest.ErrorMessage = ex.ToString();
            }
            templatesTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(templatesTest);

            // 3. Kiểm tra Job Board /Home/Jobs
            var jobsUrl = localBaseUrl.TrimEnd('/') + "/Home/Jobs";
            var jobsTest = new TestCaseResult 
            { 
                Name = "Tải trang Tìm việc làm (Job Board)", 
                Method = "GET", 
                Url = jobsUrl,
                Expected = "HTTP 200 OK từ giao diện danh sách việc làm."
            };
            sw.Restart();
            try
            {
                var response = await client.GetAsync(jobsUrl);
                if (response.IsSuccessStatusCode)
                {
                    jobsTest.Status = "Success";
                    jobsTest.Actual = $"Tải trang danh sách việc làm thành công. HTTP {(int)response.StatusCode}";
                }
                else
                {
                    jobsTest.Status = "Failed";
                    jobsTest.Actual = $"Trang việc làm trả về lỗi: HTTP {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                jobsTest.Status = "Failed";
                jobsTest.Actual = "Lỗi kết nối trang việc làm: " + ex.Message;
                jobsTest.ErrorMessage = ex.ToString();
            }
            jobsTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(jobsTest);

            return suite;
        }

        // Kiểm thử hiệu năng và độ trễ
        private async Task<TestSuiteResult> RunPerformanceSuiteAsync(string localBaseUrl)
        {
            var suite = new TestSuiteResult { SuiteName = "Performance & Stress Testing" };
            var client = _httpClientFactory.CreateClient();

            // 1. Stress Test gửi 15 requests đồng thời
            var stressTest = new TestCaseResult 
            { 
                Name = "Stress Test: 15 Requests Đồng Thời", 
                Method = "GET Concurrent", 
                Url = localBaseUrl,
                Expected = "Gửi 15 request đồng thời và tất cả đều phản hồi thành công (100% success rate)."
            };
            var sw = Stopwatch.StartNew();
            try
            {
                int concurrentCount = 15;
                var tasks = new List<Task<HttpResponseMessage>>();
                for (int i = 0; i < concurrentCount; i++)
                {
                    tasks.Add(client.GetAsync(localBaseUrl));
                }

                var responses = await Task.WhenAll(tasks);
                int successCount = 0;
                foreach (var resp in responses)
                {
                    if (resp.IsSuccessStatusCode) successCount++;
                }

                stressTest.Status = (successCount == concurrentCount) ? "Success" : "Failed";
                stressTest.Actual = $"Xử lý thành công {successCount}/{concurrentCount} requests. Tổng thời gian: {sw.ElapsedMilliseconds} ms.";
            }
            catch (Exception ex)
            {
                stressTest.Status = "Failed";
                stressTest.Actual = "Gặp lỗi khi stress test: " + ex.Message;
                stressTest.ErrorMessage = ex.ToString();
            }
            stressTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(stressTest);

            // 2. Độ trễ truy vấn Database EF Core
            var dbLatencyTest = new TestCaseResult 
            { 
                Name = "Độ trễ truy vấn Database EF Core", 
                Method = "EF Count Query", 
                Url = "SQL Server LocalInstance",
                Expected = "Thời gian truy vấn cơ sở dữ liệu để lấy số lượng bản ghi phải dưới 100ms."
            };
            sw.Restart();
            try
            {
                var userCount = await _context.Users.CountAsync();
                var templateCount = await _context.Templates.CountAsync();
                dbLatencyTest.Status = sw.ElapsedMilliseconds < 100 ? "Success" : "Failed";
                dbLatencyTest.Actual = $"Truy vấn đếm (Users: {userCount}, Templates: {templateCount}) thành công trong {sw.ElapsedMilliseconds} ms.";
            }
            catch (Exception ex)
            {
                dbLatencyTest.Status = "Failed";
                dbLatencyTest.Actual = "Truy vấn database lỗi: " + ex.Message;
                dbLatencyTest.ErrorMessage = ex.ToString();
            }
            dbLatencyTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(dbLatencyTest);

            return suite;
        }
    }
}
