using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using DoAnCS.Services;
using X.PagedList;
using X.PagedList.Extensions; // <--- Thêm dòng này

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
        public async Task<IActionResult> Jobs(int? page, string searchQuery, List<string> specialties)
        {
            // 1. Lấy tất cả việc làm từ file JSON
            var allJobs = await _jobApiService.GetRealTimeJobsAsync("IT Jobs Vietnam");

            // 2. Thực hiện LỌC nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                allJobs = allJobs.Where(j => 
                    j.job_title.ToLower().Contains(searchQuery) || 
                    j.employer_name.ToLower().Contains(searchQuery) ||
                    j.job_description.ToLower().Contains(searchQuery)
                ).ToList();
            }
            // 2. Lọc theo CHUYÊN MÔN (nếu có tích checkbox)
            if (specialties != null && specialties.Any())
            {
                // Chỉ lấy những việc làm mà tiêu đề chứa bất kỳ chuyên môn nào được chọn
                allJobs = allJobs.Where(j => 
                    specialties.Any(s => j.job_title.Contains(s, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            // 3. Cấu hình phân trang
            int pageSize = 10; // 10 việc làm mỗi trang
            int pageNumber = page ?? 1; // Nếu page null thì mặc định là trang 1
            

            // 4. Tạo ViewModel và thực hiện phân trang
            var viewModel = new HomeViewModel
            {
                // Chuyển danh sách thường thành danh sách có phân trang
                RealJobs = allJobs.ToPagedList(pageNumber, pageSize),
                SearchQuery = searchQuery,
                SelectedSpecialties = specialties
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