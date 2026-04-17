using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using DoAnCS.Services;
using X.PagedList;
using X.PagedList.Extensions;

namespace DoAnCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JobApiService _jobApiService;

        public HomeController(AppDbContext context, JobApiService jobApiService)
        {
            _context = context;
            _jobApiService = jobApiService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy Job từ Database
            var jobsFromDb = await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.Status == 1)
                .OrderByDescending(j => j.CreatedAt)
                .Take(6)
                .ToListAsync();

            // 2. Mapping sang JobDto
            var mappedJobs = jobsFromDb.Select(j => new JobDto
            {
                job_id = j.JobID.ToString(),
                job_title = j.Title,
                employer_name = j.Company?.Name,
                employer_logo = j.Company?.LogoUrl,
                job_city = j.Company?.Address,
                job_description = j.Description,
                job_apply_link = j.Company?.Website ?? "#"
            }).ToList();

            // 3. Nạp vào đúng thuộc tính LatestJobs
            var viewModel = new HomeViewModel
            {
                LatestJobs = mappedJobs.ToPagedList(1, 6), // Đổ vào đây nè Khoa!
                PopularTemplates = await _context.Templates
                    .Where(t => t.IsActive == true)
                    .Take(4)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // SỬA: Thêm tham số int? page
        public async Task<IActionResult> Jobs(int? page, string searchQuery, List<string> specialties, List<string> selectedCompanies, string sortBy)
        {
            // 1. Khởi tạo Query lấy từ Database
            IQueryable<Job> query = _context.Jobs.Include(j => j.Company)
            .Where(j => j.Status == 1);

            // 2. Bộ lọc tìm kiếm theo từ khóa (Tiêu đề hoặc Mô tả)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(j => j.Title.Contains(searchQuery) || j.Description.Contains(searchQuery));
            }

            // 3. Bộ lọc theo Chuyên môn (Checkboxes)
            if (specialties != null && specialties.Any())
            {
                // Lọc những Job mà Tiêu đề hoặc Mô tả có chứa các từ khóa chuyên môn
                query = query.Where(j => specialties.Any(s => j.Title.Contains(s) || j.Description.Contains(s)));
            }
            // 4. Bộ lọc theo Công ty (Checkboxes)
            if (selectedCompanies != null && selectedCompanies.Any())
            {
                query = query.Where(j => selectedCompanies.Contains(j.Company.Name));
            }

            // 4. Sắp xếp
            if (sortBy == "salary")
            {
                // Sắp xếp theo lương (chuỗi): Thử mẹo sắp xếp theo độ dài trước để số lớn hơn đứng đầu
                query = query.OrderByDescending(j => j.Salary.Length).ThenByDescending(j => j.Salary);
            }
            else
            {
                query = query.OrderByDescending(j => j.CreatedAt);
            }

            // 5. Mapping sang JobDto để View không bị lỗi
            var jobDtos = await query.Select(j => new JobDto
            {
                job_id = j.JobID.ToString(),
                job_title = j.Title,
                employer_name = j.Company != null ? j.Company.Name : "N/A",
                employer_logo = j.Company != null ? j.Company.LogoUrl : null,
                job_salary = j.Salary,
                job_city = j.Company != null ? j.Company.Address : "Toàn quốc",
                job_description = j.Description,
                job_apply_link = j.Company != null ? j.Company.Website : "#"
            }).ToListAsync();

            // 6. Cấu hình phân trang
            int pageSize = 10; // Mỗi trang hiện 5 tin
            int pageNumber = page ?? 1;

            var allSpecs = new List<string>{".NET Engineer",
                "Accounting Intern",
                "Administrative Intern",
                "Agriculture Technician / Farm Intern",
                "AI Developer",
                "AI Prompt Engineering",
                "AI Team Lead",
                "Android Developer",
                "Backend Developer",
                "BI Database Developer Intern",
                "Bridge System Engineer",
                "Business Analyst",
                "Business Development Specialist",
                "Business Intern",
                "Business Support Intern",
                "Category Manager",
                "Cloud Engineer",
                "Cobol Developer",
                "Communication / Marketing Intern",
                "Cybersecurity Engineer",
                "Data Analyst",
                "Data Engineer",
                "Database Administrator",
                "Deputy Head of Customer Applications",
                "Deputy Head of Internal Applications",
                "Developer Intern",
                "DevOps Engineer",
                "Digital Marketing Specialist",
                "ERP Consultant",
                "Flutter Developer Intern",
                "Fresher Developer",
                "Frontend Developer",
                "Fullstack Developer",
                "Graphic Designer",
                "Head of Application Development",
                "Head of Data & AI",
                "Head of Information Security",
                "Head of Infrastructure & Platform",
                "Head of IT Governance & Compliance / PMO",
                "HR Intern (Recruitment)",
                "HR Specialist / HRBP",
                "Implementation Consultant",
                "Import-Export / Logistics Intern",
                "IoT Engineer",
                "IT & Product Designer",
                "IT Governance Specialist",
                "IT Helpdesk Specialist",
                "IT Operations Specialist",
                "IT Service Quality",
                "Java Developer",
                "JS Engineer",
                "Lead Cybersecurity Engineer",
                "Lead Data Engineer",
                "Mobile Developer",
                "Network Engineer",
                "Operations Executive",
                "Platform Engineer",
                "PMO Specialist",
                "Production / Manufacturing Staff",
                "QA/QC Automation Engineer",
                "QC/Tester",
                "R&D Specialist (Product/Food/Bio)",
                "Senior IT Operations Engineer",
                "Senior IT System Engineer",
                "Senior Platform Engineer",
                "Service Desk Consultant",
                "Social Media",
                "STEM Instructor / Teacher",
                "System Operations Specialist",
                "Technical Architect",
                "UI/UX Designer"};

            var companyNames = await _context.Companies
                .Select(c => c.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                RealJobs = jobDtos.ToPagedList(pageNumber, pageSize),
                SearchQuery = searchQuery,
                SelectedSpecialties = specialties ?? new List<string>(), // Lưu lại các checkbox đã chọn
                SelectedCompanies = selectedCompanies ?? new List<string>(),
                AllSpecialties = allSpecs.OrderBy(s => s).ToList(),
                AllCompanies = await _context.Companies.Select(c => c.Name).Distinct().ToListAsync(),
                SortBy = sortBy ?? "latest"
            };

            return View(viewModel);
        }

        // Đổi tham số từ string sang int vì JobID trong DB của Khoa là kiểu int
        public async Task<IActionResult> Details(int id)
        {
            // 1. Tìm Job trong Database kèm theo thông tin Công ty (Include)
            var jobDb = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(m => m.JobID == id);

            // 2. Nếu không tìm thấy trong DB thì mới thử gọi API (hoặc báo lỗi)
            if (jobDb == null) return NotFound();

            // 3. Quan trọng nhất: Mapping dữ liệu từ Job (DB) sang JobDto (View)
            var jobDto = new JobDto
            {
                job_title = jobDb.Title,
                employer_name = jobDb.Company?.Name,
                employer_logo = jobDb.Company?.LogoUrl, // Link ảnh SerpApi lưu ở đây
                job_city = jobDb.Company?.Address,
                job_description = jobDb.Description,
                job_apply_link = jobDb.Company?.Website ?? "#" // Link ứng tuyển
            };

            // 4. Trả về View với Model là đối tượng JobDto đã được map xong
            return View(jobDto);
        }
    }
}