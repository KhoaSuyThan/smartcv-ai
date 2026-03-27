using DoAnCS.Models;
using System.Text.Json;

namespace DoAnCS.Services
{
    public class JobApiService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Dùng IWebHostEnvironment để tìm đường dẫn thư mục wwwroot
        public JobApiService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        
        
        public async Task<List<JobDto>> GetRealTimeJobsAsync(string query = "")
        {
            var jobs = new List<JobDto>();
            try 
            {
                // Tìm đường dẫn đến file jobs.json trong wwwroot
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "jobs.json");
                
                // Đọc toàn bộ nội dung file
                string content = await File.ReadAllTextAsync(filePath);
                
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("jobs_results", out var jobsElement))
                {
                    foreach (var item in jobsElement.EnumerateArray())
                    {
                        string displayedSalary = "Thỏa thuận";
                        if (item.TryGetProperty("salary", out var sal) && sal.ValueKind != JsonValueKind.Null)
                            {
                                displayedSalary = sal.GetString();
                            }
                            // Cách 2: Nếu không thấy, tìm trong detected_extensions (Google Jobs rất hay để ở đây)
                            else if (item.TryGetProperty("detected_extensions", out var extensions) && 
                                    extensions.TryGetProperty("salary", out var extSal))
                            {
                                displayedSalary = extSal.GetString();
                            }
                        jobs.Add(new JobDto {
                            job_id = item.TryGetProperty("job_id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                            job_title = item.GetProperty("title").GetString(),
                            employer_name = item.GetProperty("company_name").GetString(),
                            job_salary = displayedSalary,
                            employer_logo = item.TryGetProperty("thumbnail", out var logo) ? logo.GetString() : "https://via.placeholder.com/50",
                            job_city = item.TryGetProperty("location", out var loc) ? loc.GetString() : "Việt Nam",
                            job_description = item.TryGetProperty("description", out var desc) ? desc.GetString() : "",
                            job_apply_link = item.TryGetProperty("related_links", out var links) && links.GetArrayLength() > 0 
                                             ? links[0].GetProperty("link").GetString() : "#"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi đọc file: " + ex.Message);
            }
            return jobs;
        }

        public async Task<JobDto> GetJobByIdAsync(string id)
        {
            var allJobs = await GetRealTimeJobsAsync(); // Lấy tất cả từ file json
            return allJobs.FirstOrDefault(j => j.job_id == id); // Tìm cái trùng ID
        }
    }
}