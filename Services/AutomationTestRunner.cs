using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using DoAnCS.Data;
using DoAnCS.Models;

namespace DoAnCS.Services
{
    // Cấu trúc kết quả của từng ca kiểm thử nhỏ
    public class TestCaseResult
    {
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Expected { get; set; } = string.Empty;
        public string Actual { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public string Status { get; set; } = "Failed"; // "Success" hoặc "Failed"
        public string? ErrorMessage { get; set; }
    }

    // Cấu trúc kết quả của cả bộ kiểm thử
    public class TestSuiteResult
    {
        public string SuiteName { get; set; } = string.Empty;
        public List<TestCaseResult> TestCases { get; set; } = new List<TestCaseResult>();
    }

    // Giao diện điều phối kiểm thử tự động
    public interface IAutomationTestRunner
    {
        Task<List<TestSuiteResult>> RunAllTestsAsync(string localBaseUrl);
        Task<TestSuiteResult> RunSystemHealthSuiteAsync();
        Task<TestSuiteResult> RunApiVerificationSuiteAsync(string localBaseUrl);
        Task<TestSuiteResult> RunPerformanceSuiteAsync(string localBaseUrl);
        Task<TestSuiteResult> RunE2EFlowSuiteAsync(string localBaseUrl);
        Task SaveTestRunToDbAsync(TestSuiteResult suiteResult);
        Task<List<TestRun>> GetTestHistoryAsync();
        Task<bool> DeleteTestRunAsync(int testRunId);
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

        // Chạy toàn bộ các suite kiểm thử (System Health, API, Performance) và tự động lưu DB
        public async Task<List<TestSuiteResult>> RunAllTestsAsync(string localBaseUrl)
        {
            var results = new List<TestSuiteResult>();

            // 1. Chạy Suite kiểm tra sức khỏe hệ thống
            var healthSuite = await RunSystemHealthSuiteAsync();
            results.Add(healthSuite);
            await SaveTestRunToDbAsync(healthSuite);

            // 2. Chạy Suite xác thực tích hợp API
            var apiSuite = await RunApiVerificationSuiteAsync(localBaseUrl);
            results.Add(apiSuite);
            await SaveTestRunToDbAsync(apiSuite);

            // 3. Chạy Suite kiểm thử hiệu năng
            var perfSuite = await RunPerformanceSuiteAsync(localBaseUrl);
            results.Add(perfSuite);
            await SaveTestRunToDbAsync(perfSuite);

            return results;
        }

        // Kiểm tra kết nối SQL Server, FastAPI Python AI và Google Gemini API
        public async Task<TestSuiteResult> RunSystemHealthSuiteAsync()
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
        public async Task<TestSuiteResult> RunApiVerificationSuiteAsync(string localBaseUrl)
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

            // 2. API CV Templates
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

            // 3. Job Board /Home/Jobs
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
        public async Task<TestSuiteResult> RunPerformanceSuiteAsync(string localBaseUrl)
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

        // Hàm phụ trợ thực thi động danh sách các bước kịch bản kiểm thử
        private async Task ExecuteDynamicStepsAsync(IPage page, List<TestStep> steps, string localBaseUrl)
        {
            foreach (var step in steps)
            {
                // In log ra debug console
                Console.WriteLine($"[Playwright E2E] Running Step {step.StepOrder}: {step.Description} (Action: {step.ActionType}, Selector: {step.TargetSelector}, Value: {step.Value})");
                
                switch (step.ActionType)
                {
                    case "Navigate":
                        await page.GotoAsync(localBaseUrl + step.Value);
                        break;

                    case "Click":
                        if (step.Value == "ACCEPT_DIALOG")
                        {
                            page.Dialog += (_, dialog) => dialog.AcceptAsync();
                        }
                        
                        // Chờ selector hiển thị trước khi click
                        await page.WaitForSelectorAsync(step.TargetSelector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
                        await page.ClickAsync(step.TargetSelector);
                        
                        if (int.TryParse(step.Value, out int timeoutClickMs))
                        {
                            await page.WaitForTimeoutAsync(timeoutClickMs);
                        }
                        break;

                    case "Fill":
                        if (step.TargetSelector == ".otp-input")
                        {
                            var otpInputs = await page.QuerySelectorAllAsync(".otp-input");
                            if (otpInputs.Count == 6)
                            {
                                string otp = step.Value ?? "123456";
                                for (int i = 0; i < 6; i++)
                                {
                                    await otpInputs[i].FillAsync(otp[i].ToString());
                                }
                            }
                            else
                            {
                                throw new Exception("Không tìm thấy đủ 6 ô nhập mã OTP trên màn hình xác thực.");
                            }
                        }
                        else
                        {
                            string fillValue = step.Value ?? "";
                            if (fillValue == "TOMORROW")
                            {
                                fillValue = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                            }
                            await page.WaitForSelectorAsync(step.TargetSelector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
                            await page.FillAsync(step.TargetSelector, fillValue);
                        }
                        break;

                    case "Select":
                        await page.WaitForSelectorAsync(step.TargetSelector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
                        await page.SelectOptionAsync(step.TargetSelector, new[] { step.Value });
                        if (step.TargetSelector == "#roleSelect")
                        {
                            await page.WaitForTimeoutAsync(500);
                        }
                        break;

                    case "AssertUrl":
                        string expectedUrl = step.Value ?? "";
                        if (expectedUrl.Contains("**/"))
                        {
                            await page.WaitForURLAsync(expectedUrl);
                        }
                        else
                        {
                            string fullExpected = expectedUrl.StartsWith("/") ? (localBaseUrl + expectedUrl) : expectedUrl;
                            if (fullExpected.EndsWith("*"))
                            {
                                string prefix = fullExpected.TrimEnd('*');
                                await page.WaitForURLAsync(url => url.StartsWith(prefix));
                            }
                            else
                            {
                                await page.WaitForURLAsync(fullExpected);
                            }
                        }
                        break;

                    case "AssertText":
                        await page.WaitForSelectorAsync(step.TargetSelector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
                        var element = page.Locator(step.TargetSelector);
                        var text = await element.InnerTextAsync();
                        if (!text.Contains(step.Value ?? ""))
                        {
                            throw new Exception($"Kiểm tra nội dung thất bại ở selector '{step.TargetSelector}'. Mong đợi chứa: '{step.Value}', Thực tế: '{text}'");
                        }
                        break;

                    case "AssertTextNot":
                        await page.WaitForSelectorAsync(step.TargetSelector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
                        var textContent = await page.Locator(step.TargetSelector).InnerTextAsync();
                        if (textContent.Contains(step.Value ?? ""))
                        {
                            throw new Exception($"Kiểm tra phủ định thất bại ở selector '{step.TargetSelector}'. Mong đợi KHÔNG chứa: '{step.Value}', nhưng nội dung thực tế vẫn chứa.");
                        }
                        break;

                    default:
                        throw new Exception($"Hành động kiểm thử '{step.ActionType}' không được hỗ trợ.");
                }
            }
        }

        // Chạy kiểm thử luồng người dùng E2E sử dụng Playwright và các bước cấu hình động từ database
        public async Task<TestSuiteResult> RunE2EFlowSuiteAsync(string localBaseUrl)
        {
            var suite = new TestSuiteResult { SuiteName = "E2E User Flow Testing" };

            // Khởi tạo các bản ghi dọn dẹp dữ liệu kiểm thử trùng lặp trước khi bắt đầu
            await CleanE2ETestDataAsync();

            // 1. CHẠY TEST LUỒNG ĐĂNG KÝ/ĐĂNG NHẬP (AUTH E2E)
            var authSteps = await _context.TestSteps
                .Where(s => s.ScenarioName == "Auth E2E")
                .OrderBy(s => s.StepOrder)
                .ToListAsync();

            var authTest = new TestCaseResult
            {
                Name = "Test Luồng Đăng nhập/Đăng ký (Auth E2E)",
                Method = "Playwright E2E",
                Url = localBaseUrl + "/Account/Login",
                Expected = "Người dùng đăng ký mới với OTP 123456 -> Đăng nhập thành công -> Điều hướng về Trang chủ -> Đăng xuất (thực thi động từ DB)."
            };
            var sw = Stopwatch.StartNew();
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
                var page = await context.NewPageAsync();

                if (!authSteps.Any())
                {
                    throw new Exception("Không tìm thấy các bước cấu hình kiểm thử Auth E2E trong database.");
                }

                await ExecuteDynamicStepsAsync(page, authSteps, localBaseUrl);

                authTest.Status = "Success";
                authTest.Actual = $"Hoàn tất thành công {authSteps.Count} bước kiểm thử động.";
            }
            catch (Exception ex)
            {
                authTest.Status = "Failed";
                authTest.Actual = "Gặp lỗi trong quá trình thực thi E2E Auth động.";
                authTest.ErrorMessage = ex.Message + "\n" + ex.StackTrace;
            }
            authTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(authTest);

            // 2. CHẠY TEST LUỒNG QUẢN LÝ TIN TUYỂN DỤNG (JOBS E2E)
            var jobsSteps = await _context.TestSteps
                .Where(s => s.ScenarioName == "Jobs E2E")
                .OrderBy(s => s.StepOrder)
                .ToListAsync();

            var jobsTest = new TestCaseResult
            {
                Name = "Test Luồng Tin tuyển dụng (Jobs E2E)",
                Method = "Playwright E2E",
                Url = localBaseUrl + "/Jobs/Create",
                Expected = "Đăng ký Recruiter -> Đăng nhập -> Tạo tin tuyển dụng -> Chỉnh sửa bài -> Xóa bài đăng (thực thi động từ DB)."
            };
            sw.Restart();
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
                var page = await context.NewPageAsync();

                if (!jobsSteps.Any())
                {
                    throw new Exception("Không tìm thấy các bước cấu hình kiểm thử Jobs E2E trong database.");
                }

                await ExecuteDynamicStepsAsync(page, jobsSteps, localBaseUrl);

                jobsTest.Status = "Success";
                jobsTest.Actual = $"Hoàn tất thành công {jobsSteps.Count} bước kiểm thử động.";
            }
            catch (Exception ex)
            {
                jobsTest.Status = "Failed";
                jobsTest.Actual = "Gặp lỗi trong quá trình thực thi E2E Jobs động.";
                jobsTest.ErrorMessage = ex.Message + "\n" + ex.StackTrace;
            }
            jobsTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(jobsTest);

            // 3. CHẠY TEST LUỒNG AUTH VALIDATION E2E
            var authValidationSteps = await _context.TestSteps
                .Where(s => s.ScenarioName == "Auth Validation E2E")
                .OrderBy(s => s.StepOrder)
                .ToListAsync();

            var authValidationTest = new TestCaseResult
            {
                Name = "Kiểm thử nhập liệu & Validation Auth (Auth Validation E2E)",
                Method = "Playwright E2E",
                Url = localBaseUrl + "/Account/Login",
                Expected = "Kiểm tra chặn form trống, lỗi email/mật khẩu yếu, và thông báo lỗi nhập liệu đăng ký/đăng nhập."
            };
            sw.Restart();
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
                var page = await context.NewPageAsync();

                if (!authValidationSteps.Any())
                {
                    throw new Exception("Không tìm thấy các bước cấu hình kiểm thử Auth Validation E2E trong database.");
                }

                await ExecuteDynamicStepsAsync(page, authValidationSteps, localBaseUrl);

                authValidationTest.Status = "Success";
                authValidationTest.Actual = $"Hoàn tất thành công {authValidationSteps.Count} bước kiểm thử validation.";
            }
            catch (Exception ex)
            {
                authValidationTest.Status = "Failed";
                authValidationTest.Actual = "Gặp lỗi trong quá trình thực thi E2E Auth Validation động.";
                authValidationTest.ErrorMessage = ex.Message + "\n" + ex.StackTrace;
            }
            authValidationTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(authValidationTest);

            // 4. CHẠY TEST LUỒNG JOBS VALIDATION E2E
            var jobsValidationSteps = await _context.TestSteps
                .Where(s => s.ScenarioName == "Jobs Validation E2E")
                .OrderBy(s => s.StepOrder)
                .ToListAsync();

            var jobsValidationTest = new TestCaseResult
            {
                Name = "Kiểm thử nghiệp vụ & Validation Đăng tin (Jobs Validation E2E)",
                Method = "Playwright E2E",
                Url = localBaseUrl + "/Jobs/Create",
                Expected = "Kiểm tra chặn form trống và thông báo lỗi khi nhập hạn nộp hồ sơ trong quá khứ."
            };
            sw.Restart();
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                var context = await browser.NewContextAsync(new BrowserNewContextOptions { IgnoreHTTPSErrors = true });
                var page = await context.NewPageAsync();

                if (!jobsValidationSteps.Any())
                {
                    throw new Exception("Không tìm thấy các bước cấu hình kiểm thử Jobs Validation E2E trong database.");
                }

                await ExecuteDynamicStepsAsync(page, jobsValidationSteps, localBaseUrl);

                jobsValidationTest.Status = "Success";
                jobsValidationTest.Actual = $"Hoàn tất thành công {jobsValidationSteps.Count} bước kiểm thử validation.";
            }
            catch (Exception ex)
            {
                jobsValidationTest.Status = "Failed";
                jobsValidationTest.Actual = "Gặp lỗi trong quá trình thực thi E2E Jobs Validation động.";
                jobsValidationTest.ErrorMessage = ex.Message + "\n" + ex.StackTrace;
            }
            jobsValidationTest.ResponseTimeMs = sw.ElapsedMilliseconds;
            suite.TestCases.Add(jobsValidationTest);

            // Dọn dẹp dữ liệu kiểm thử sau khi hoàn thành
            await CleanE2ETestDataAsync();

            // Lưu kết quả vào database
            await SaveTestRunToDbAsync(suite);

            return suite;
        }

        // Dọn dẹp dữ liệu của E2E kiểm thử khỏi database
        private async Task CleanE2ETestDataAsync()
        {
            try
            {
                // Xóa Users test_e2e_
                var testUsers = await _context.Users.Where(u => u.Email.StartsWith("test_e2e_")).ToListAsync();
                if (testUsers.Any())
                {
                    _context.Users.RemoveRange(testUsers);
                }

                // Xóa Companies E2E Test Company
                var testCompanies = await _context.Companies.Where(c => c.Name.StartsWith("E2E Test Company")).ToListAsync();
                if (testCompanies.Any())
                {
                    _context.Companies.RemoveRange(testCompanies);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi dọn dẹp dữ liệu E2E: " + ex.Message);
            }
        }

        // Lưu kết quả kiểm thử vào Database
        public async Task SaveTestRunToDbAsync(TestSuiteResult suiteResult)
        {
            if (suiteResult == null || suiteResult.TestCases.Count == 0) return;

            int total = suiteResult.TestCases.Count;
            int passed = suiteResult.TestCases.Count(tc => tc.Status == "Success");
            int failed = total - passed;
            long avgResponseTime = total > 0 ? (long)suiteResult.TestCases.Average(tc => tc.ResponseTimeMs) : 0;

            var testRun = new TestRun
            {
                ExecutionTime = DateTime.Now,
                SuiteName = suiteResult.SuiteName,
                TotalCases = total,
                PassedCases = passed,
                FailedCases = failed,
                AvgResponseTimeMs = avgResponseTime,
                Details = suiteResult.TestCases.Select(tc => new TestCaseDetail
                {
                    Name = tc.Name,
                    Method = tc.Method,
                    Url = tc.Url,
                    Status = tc.Status,
                    ResponseTimeMs = tc.ResponseTimeMs,
                    ExpectedResult = tc.Expected,
                    ActualResult = tc.Actual,
                    ErrorMessage = tc.ErrorMessage
                }).ToList()
            };

            _context.TestRuns.Add(testRun);
            await _context.SaveChangesAsync();
        }

        // Lấy lịch sử kiểm thử
        public async Task<List<TestRun>> GetTestHistoryAsync()
        {
            return await _context.TestRuns
                .Include(tr => tr.Details)
                .OrderByDescending(tr => tr.ExecutionTime)
                .ToListAsync();
        }

        // Xóa một lịch sử kiểm thử
        public async Task<bool> DeleteTestRunAsync(int testRunId)
        {
            var run = await _context.TestRuns.FindAsync(testRunId);
            if (run == null) return false;

            _context.TestRuns.Remove(run);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
