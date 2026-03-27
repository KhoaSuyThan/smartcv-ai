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
            var realJobsData = await _jobApiService.GetRealTimeJobsAsync("IT Developer Vietnam");
            
            var viewModel = new HomeViewModel
            {
                PopularTemplates = await _context.Templates
                    .Where(t => t.IsActive == true)
                    .Take(4)
                    .ToListAsync(),

                // Trang chủ chỉ hiện 6 cái đầu tiên, không cần phân trang 1 2 3
                RealJobs = realJobsData.ToPagedList(1, 6) 
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

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var job = await _jobApiService.GetJobByIdAsync(id);
            
            if (job == null) return NotFound();

            return View(job); // Trả về Model là 1 đối tượng JobDto
        }
    }
}