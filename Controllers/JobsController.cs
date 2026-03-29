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
                .Where(j => j.Status == 1)
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
            // Xóa validation cho các object liên kết
            ModelState.Remove("Recruiter");
            ModelState.Remove("Company");
            ModelState.Remove("Applications");

            if (ModelState.IsValid)
            {
                // Lấy User từ DB để lấy CompanyID chính xác nhất
                var userInDb = await _context.Users.FindAsync(CurrentUserId);

                job.CreatedAt = DateTime.Now;
                job.RecruiterID = CurrentUserId;
                job.Status = (CurrentRole == "Admin") ? 1 : 0;

                if (CurrentRole == "Recruiter")
                {
                    // Lấy ID từ DB thay vì lấy từ CurrentCompanyId (Claims)
                    job.CompanyID = userInDb?.CompanyID;
                }

                if (job.CompanyID == null || job.CompanyID == 0)
                {
                    ModelState.AddModelError("", "Lỗi: Hệ thống không thấy ID công ty của bạn trong DB.");
                    return View(job);
                }

                _context.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }
            return View(job);
        }


        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            // Check quyền sở hữu
            if (CurrentRole == "Recruiter" && job.CompanyID != CurrentCompanyId) return Forbid();

            if (CurrentRole == "Admin") ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(job);
        }

        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job job)
        {
            if (id != job.JobID) return NotFound();

            if (ModelState.IsValid)
            {
                // Khi sửa bài, đẩy trạng thái về Chờ duyệt (0)
                // Admin sửa thì có thể giữ nguyên trạng thái Đã duyệt (1)
                job.Status = (CurrentRole == "Admin") ? 1 : 0;

                _context.Update(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }
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

        // ==========================================
        // 5. CHỨC NĂNG DÀNH RIÊNG CHO ADMIN DUYỆT TIN
        // ==========================================


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, string returnUrl = null)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                job.Status = 1;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã duyệt bài!";
            }

            // Nếu có địa chỉ quay lại (từ Admin Dashboard) thì về đó, không thì về Manage
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string returnUrl = null)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                job.Status = 2;
                await _context.SaveChangesAsync();
                TempData["Error"] = "Đã từ chối bài!";
            }

            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Manage));
        }
    }
}