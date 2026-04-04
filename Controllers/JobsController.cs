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
            // 1. Khởi tạo Query lấy kèm thông tin Công ty
            IQueryable<Job> query = _context.Jobs.Include(j => j.Company);

            // 2. Kiểm tra quyền của người dùng đang đăng nhập
            if (User.IsInRole("Recruiter"))
            {
                // Lấy CompanyID từ Claim (đã lưu lúc đăng nhập)
                var companyIdClaim = User.FindFirst("CompanyID")?.Value;

                if (companyIdClaim != null)
                {
                    int currentCompanyId = int.Parse(companyIdClaim);
                    // CHỈ LẤY các tin thuộc công ty này
                    query = query.Where(j => j.CompanyID == currentCompanyId);
                }
                else
                {
                    // Nếu không tìm thấy CompanyID, trả về danh sách rỗng để bảo mật
                    return View(new List<Job>());
                }
            }
            // Nếu là Admin thì không lọc (query giữ nguyên để thấy hết)

            // 3. Sắp xếp và thực thi truy vấn
            var jobs = await query
                .OrderBy(j => j.Status == 0 ? 0 : 1) // Tin chờ duyệt lên đầu
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();

            return View(jobs);
        }

        // ==========================================
        // 3. ĐĂNG TIN MỚI 
        // ==========================================
        // [GET] Hiển thị form đăng tin
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Create()
        {
            // LUÔN LUÔN nạp danh sách, kể cả không dùng đến để tránh lỗi Null ở View
            var companies = await _context.Companies.ToListAsync();
            ViewBag.Companies = companies ?? new List<Company>(); 
            
            return View();
        }

        // [POST] Xử lý lưu tin tuyển dụng
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            // 1. Gỡ bỏ kiểm tra các trường không nhập từ Form
            ModelState.Remove("Recruiter");
            ModelState.Remove("Company");
            ModelState.Remove("Applications");

            if (ModelState.IsValid)
            {
                // 2. Thiết lập các thông tin mặc định
                job.CreatedAt = DateTime.Now;
                job.RecruiterID = CurrentUserId; // Giả định bạn đã có thuộc tính này trong BaseController
                
                // Admin đăng thì duyệt luôn (1), Recruiter đăng thì chờ duyệt (0)
                job.Status = User.IsInRole("Admin") ? 1 : 0;

                // 3. Xử lý ID Công ty dựa trên Role
                if (User.IsInRole("Recruiter"))
                {
                    // Lấy trực tiếp từ DB để đảm bảo an toàn dữ liệu
                    var userInDb = await _context.Users.FindAsync(CurrentUserId);
                    job.CompanyID = userInDb?.CompanyID ?? 0;
                }
                // Nếu là Admin thì job.CompanyID đã được lấy từ Dropdown qua Model Binding

                // 4. Kiểm tra ID công ty lần cuối trước khi lưu
                if (job.CompanyID == null || job.CompanyID == 0)
                {
                    ModelState.AddModelError("", "Lỗi: Không xác định được công ty. Vui lòng kiểm tra lại thông tin tài khoản.");
                }
                else
                {
                    _context.Add(job);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đăng tin tuyển dụng thành công!";
                    return RedirectToAction(nameof(Manage));
                }
            }

            // FIX: Nếu có lỗi (ModelState không hợp lệ), PHẢI nạp lại ViewBag trước khi trả về View
            // Nếu không nạp lại, khi View load lại sẽ bị lỗi NullReferenceException ngay
            ViewBag.Companies = await _context.Companies.ToListAsync();
            
            return View(job);
        }

        [HttpGet]
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

            // 1. Tìm bản ghi gốc TRONG DATABASE (Không tin vào dữ liệu gửi từ View hoàn toàn)
            var jobInDb = await _context.Jobs.FindAsync(id);
            if (jobInDb == null) return NotFound();

            // 2. Check quyền sở hữu (Security check lần 2)
            if (User.IsInRole("Recruiter") && jobInDb.CompanyID != CurrentCompanyId) 
                return Forbid();

            // 3. Gỡ bỏ kiểm tra các trường không cần nhập từ Form để ModelState hợp lệ
            ModelState.Remove("Recruiter");
            ModelState.Remove("Company");
            ModelState.Remove("Applications");
            // Nếu vẫn lỗi, Khoa thêm đoạn này để debug xem trường nào đang 'hành' mình:
            // var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                try
                {
                    // 4. Chỉ cập nhật những gì người dùng được phép sửa
                    jobInDb.Title = job.Title;
                    jobInDb.Description = job.Description;
                    jobInDb.Requirements = job.Requirements;
                    jobInDb.Salary = job.Salary;
                    jobInDb.Deadline = job.Deadline;

                    // Xử lý trạng thái theo Role
                    if (User.IsInRole("Admin"))
                    {
                        // Admin chọn gì lưu nấy
                        jobInDb.Status = job.Status; 
                    }
                    else
                    {
                        // Nếu là Recruiter sửa bài, ép về 0 để chờ Admin duyệt lại (nếu hệ thống yêu cầu)
                        // Hoặc nếu muốn cho họ tự đóng/mở tin thì cũng dùng: jobInDb.Status = job.Status;
                        jobInDb.Status = 0; 
                    }

                    _context.Update(jobInDb);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    
                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Jobs", "Admin");
                    }
                    return RedirectToAction(nameof(Manage));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.JobID)) return NotFound();
                    else throw;
                }
            }
            
            // Nếu lỗi, trả lại dữ liệu gốc từ DB để View không bị trắng các trường ẩn
            return View(job);
        }

        // Hàm bổ trợ kiểm tra sự tồn tại của Job
        private bool JobExists(int id)
        {
            // Kiểm tra xem trong bảng Jobs có bất kỳ dòng nào khớp với ID này không
            return _context.Jobs.Any(e => e.JobID == id);
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