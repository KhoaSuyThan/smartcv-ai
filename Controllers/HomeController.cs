using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;

namespace DoAnCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                // Lấy 4 mẫu CV từ database để hiển thị lên trang chủ
                PopularTemplates = await _context.Templates
                    .Where(t => t.IsActive == true)
                    .Take(4)
                    .ToListAsync(),

                // Khởi tạo danh sách rỗng vì bạn chưa làm Database cho Job
                LatestJobs = new List<Job>() 
            };
            return View(viewModel);
        }
        public IActionResult Jobs()
        {
            // Vì chưa có DB Jobs, chúng ta chỉ trả về View mà không có dữ liệu
            return View();
        }
    }
}