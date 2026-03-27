using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.AspNetCore.Authorization;

namespace DoAnCS.Controllers
{
    public class JobsController : BaseController
    {
        private readonly AppDbContext _context;

        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DÀNH CHO TẤT CẢ (Ứng viên xem tin)
        // ==========================================
        [AllowAnonymous] // Cho phép khách xem danh sách việc làm
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
            return View(jobs);
        }

        // ==========================================
        // 2. DÀNH CHO NHÀ TUYỂN DỤNG & ADMIN (Quản lý tin)
        // ==========================================
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Manage()
        {
            IQueryable<Job> query = _context.Jobs.Include(j => j.Company);

            // LOGIC PHÂN QUYỀN DỮ LIỆU:
            if (CurrentRole == "Recruiter")
            {
                // Nhà tuyển dụng chỉ thấy tin thuộc về CompanyID của mình
                query = query.Where(j => j.CompanyID == CurrentCompanyId);
            }
            // Admin không bị lọc (thấy hết)

            var myJobs = await query.ToListAsync();
            return View(myJobs);
        }

        // ==========================================
        // 3. ĐĂNG TIN MỚI 
        // ==========================================
        // [GET] Hiển thị form đăng tin
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Create()
        {
            // Nếu là Admin, có thể cần chọn công ty. Nếu là Recruiter, lấy mặc định.
            if (CurrentRole == "Admin")
            {
                ViewBag.Companies = await _context.Companies.ToListAsync();
            }
            return View();
        }

        // [POST] Xử lý lưu tin tuyển dụng
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            if (ModelState.IsValid)
            {
                // 1. Tự động gán các thông tin hệ thống
                job.CreatedAt = DateTime.Now;
                job.RecruiterID = CurrentUserId; // Lấy từ BaseController

                // 2. Logic gán CompanyID
                if (CurrentRole == "Recruiter")
                {
                    job.CompanyID = CurrentCompanyId ?? 0;
                }
                // Nếu là Admin thì CompanyID sẽ lấy từ dropdown trong Form gửi lên

                if (job.CompanyID == 0)
                {
                    ModelState.AddModelError("", "Lỗi: Không xác định được công ty tuyển dụng.");
                    if (CurrentRole == "Admin") ViewBag.Companies = await _context.Companies.ToListAsync();
                    return View(job);
                }

                _context.Add(job);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Đăng tin tuyển dụng thành công!";
                return RedirectToAction("Index", "Admin"); // Quay về Dashboard
            }

            if (CurrentRole == "Admin") ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(job);
        }

        // ==========================================
        // 4. XÓA TIN (Chủ sở hữu hoặc Admin)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            // Bảo mật: Nếu là Recruiter, phải check xem tin này có phải của mình không
            if (CurrentRole == "Recruiter" && job.CompanyID != CurrentCompanyId)
            {
                return Forbid(); // Trả về lỗi 403 - Cấm truy cập
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }
    }
}