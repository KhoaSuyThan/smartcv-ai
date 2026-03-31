using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.AspNetCore.Authorization;
using DoAnCS.Models.ViewModels;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace DoAnCS.Controllers
{
    // Chỉ những người có Role là Admin mới được vào Controller này
    [Authorize(Roles = "Admin")] 
    public class AdminController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHost;
        public AdminController(AppDbContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        // 1. Trang Dashboard của Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1)
        {
            // --- 1. THIẾT LẬP PHÂN TRANG CHO TEMPLATES ---
            int pageSize = 4; // Số lượng mẫu CV hiện trên 1 trang
            var totalTemplatesCount = await _context.Templates.CountAsync();

            // --- 2. LẤY TẤT CẢ THỐNG KÊ TỪ DATABASE (BỎ JSON) ---
            // Sử dụng await đồng thời giúp code gọn và dữ liệu chuẩn 100%
            var totalUsers = await _context.Users.CountAsync();
            var totalResumes = await _context.Resumes.CountAsync();
            var totalJobs = await _context.Jobs.CountAsync();
            var totalCompanies = await _context.Companies.CountAsync();

            // --- 3. LẤY DANH SÁCH TEMPLATES THEO TRANG ---
            var templates = await _context.Templates
                .OrderByDescending(t => t.TemplateID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- 4. LẤY DANH SÁCH 10 VIỆC LÀM MỚI NHẤT ĐỂ HIỂN THỊ ---
            // Nhớ .Include(j => j.Company) để không bị lỗi Null khi gọi tên công ty ở View
            var recentJobs = await _context.Jobs
                .Include(j => j.Company)
                .OrderByDescending(j => j.CreatedAt)
                .Take(10) 
                .ToListAsync();

            // --- 5. ĐỔ DỮ LIỆU VÀO VIEWMODEL ---
            var stats = new AdminDashboardVM 
            {
                TotalUsers = totalUsers,
                TotalResumes = totalResumes,
                TotalJobs = totalJobs,
                TotalCompanies = totalCompanies,
                Templates = templates,
                Jobs = recentJobs, // Danh sách 10 tin mới nhất
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalTemplatesCount / pageSize)
            };

            return View(stats);
        }

        // 2. Danh sách tất cả người dùng
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Include(u => u.Company).ToListAsync();
            return View(users);
        }

        // 3. Chỉnh sửa thông tin người dùng (Đoạn mã bạn hỏi)
        // [GET] Hiển thị form chỉnh sửa
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return NotFound();

            // Lấy danh sách công ty để Admin có thể gán User vào công ty nếu cần
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(user);
        }

        // [POST] Lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, string FullName, string Role, int? CompanyID)
        {
            var userInDb = await _context.Users.FindAsync(id);
            if (userInDb == null) return NotFound();

            // Chỉ cập nhật các trường cần thiết, tuyệt đối không đụng vào PasswordHash
            userInDb.FullName = FullName;
            userInDb.Role = Role;
            userInDb.CompanyID = (Role == "Recruiter") ? CompanyID : null; // Nếu không phải Recruiter thì bỏ CompanyID

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật người dùng thành công!";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu.");
                ViewBag.Companies = await _context.Companies.ToListAsync();
                return View(userInDb);
            }
        }

        public class JobJsonModel
        {
            public int job_id { get; set; }
            public string company_name { get; set; }
        }

        // [GET] Hiển thị form tạo mới
        [HttpGet]
        public IActionResult CreateTemplate()
        {
            // Tạo sẵn code mẫu cơ bản để Admin không phải gõ trắng trơn
            var model = new Template
            {
                HtmlContent = "<div class=\"cv-container\">\n  <h1>{{FullName}}</h1>\n</div>",
                CssContent = ".cv-container { font-family: sans-serif; }",
                IsActive = true
            };
            return View(model);
        }

        // [POST] Lưu mẫu CV mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTemplate(Template template, IFormFile uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    // 1. Định nghĩa thư mục lưu trữ
                    string folder = Path.Combine(_webHost.WebRootPath, "images", "templates");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    // 2. Đặt lại tên file: template_ + chuỗi duy nhất + đuôi file
                    string fileName = "template_" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(uploadImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    // 3. Lưu file vào thư mục vật lý
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadImage.CopyToAsync(stream);
                    }

                    // 4. Lưu đường dẫn vào Database (để hiển thị)
                    template.PreviewImageUrl = "/images/templates/" + fileName;
                }

                _context.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }
        
        // [GET] Load dữ liệu mẫu CV lên form
        [HttpGet]
        public async Task<IActionResult> EditCV(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) return NotFound();
            
            return View(template);
        }

        // [POST] Xử lý lưu dữ liệu sau khi sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCV(int id, Template template, IFormFile? uploadImage)
        {
            if (id != template.TemplateID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý nếu Admin chọn upload file mới
                    if (uploadImage != null && uploadImage.Length > 0)
                    {
                        string folder = Path.Combine(_webHost.WebRootPath, "images", "templates");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        // Tạo tên file mới để tránh trùng
                        string fileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + uploadImage.FileName;
                        string filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadImage.CopyToAsync(stream);
                        }

                        // Cập nhật lại đường dẫn mới cho Model
                        template.PreviewImageUrl = "/images/templates/" + fileName;
                    }
                    // Nếu không upload ảnh mới, thuộc tính PreviewImageUrl sẽ nhận giá trị từ ô Input văn bản

                    _context.Update(template);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Cập nhật mẫu thiết kế thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            return View(template);
        }

        // [GET] Danh sách việc làm
        public async Task<IActionResult> ManageJobs()
        {
            var jobs = await _context.Jobs.Include(j => j.Company).OrderByDescending(j => j.CreatedAt).ToListAsync();
            return View(jobs);
        }

        // [GET] Trang thêm việc làm mới
        [HttpGet]
        public async Task<IActionResult> CreateJob()
        {
            ViewBag.Companies = await _context.Companies.ToListAsync(); 
            
            return View();
        }

        // [POST] Xử lý thêm việc làm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đăng tin tuyển dụng mới thành công!";
                return RedirectToAction(nameof(ManageJobs));
            }
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(job);
        }

        // [POST] Xóa việc làm
        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
            }
            return Ok(); // Trả về Ok để xử lý AJAX cho mượt
        }
        
        // 1. Trang quản lý Jobs
        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.Jobs.Include(j => j.Company).ToListAsync();
            return View(jobs);
        }

        // 2. Trang quản lý Mẫu CV (Templates)
        public async Task<IActionResult> Templates(int page = 1)
        {
            // 1. Cấu hình phân trang (4 mẫu mỗi trang theo ý Khoa)
            int pageSize = 4;
            var query = _context.Templates.AsQueryable();

            // 2. Tính toán tổng số trang
            int totalTemplates = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalTemplates / pageSize);

            // 3. Lấy dữ liệu của trang hiện tại
            var templates = await query
                .OrderByDescending(t => t.TemplateID) // Mới nhất hiện lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. ĐÓNG GÓI VÀO VM (Đây là bước fix lỗi InvalidOperationException)
            var vm = new AdminDashboardVM
            {
                Templates = templates,
                CurrentPage = page,
                TotalPages = totalPages,
                
                // Nạp thêm các chỉ số để Layout hoặc Sidebar không bị trống (nếu cần)
                TotalUsers = await _context.Users.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalResumes = await _context.Resumes.CountAsync()
            };

            // 5. Trả về đúng cái VM này
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTemplateStatus(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) return NotFound();

            // Đảo trạng thái (True -> False, False -> True)
            template.IsActive = !template.IsActive;
            
            await _context.SaveChangesAsync();
            return Ok();
        }
        // 1. Danh sách công ty
        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(companies);
        }

        // 2. Thêm công ty (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany(Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm công ty thành công!";
            }
            return RedirectToAction(nameof(Companies));
        }

        // 3. Sửa công ty (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCompany(Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Update(company);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction(nameof(Companies));
        }

        // 4. Xóa công ty và tất cả Jobs liên quan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                // Tìm và xóa tất cả Jobs thuộc về công ty này
                var relatedJobs = _context.Jobs.Where(j => j.CompanyID == id);
                _context.Jobs.RemoveRange(relatedJobs);

                // Sau đó xóa công ty
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa công ty và các tin tuyển dụng liên quan!";
            }
            return RedirectToAction(nameof(Companies));
        }
    }
}