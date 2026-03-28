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
            // --- 1. THIẾT LẬP PHÂN TRANG ---
            int pageSize = 4; // Số lượng mẫu CV hiện trên 1 trang
            var totalTemplatesCount = await _context.Templates.CountAsync();

            // --- 2. ĐẾM DỮ LIỆU TỪ DATABASE ---
            var totalUsers = await _context.Users.CountAsync();
            var totalResumes = await _context.Resumes.CountAsync();

            // --- 3. ĐẾM DỮ LIỆU TỪ FILE JSON (Jobs & Companies) ---
            int totalJobs = await _context.Jobs.CountAsync();
            int totalCompanies = await _context.Companies.CountAsync();

            try 
            {
                string filePath = Path.Combine(_webHost.WebRootPath, "jobs.json");
                if (System.IO.File.Exists(filePath))
                {
                    string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);

                    // Dùng Regex đếm số lượng dựa trên Key của JSON
                    totalJobs = Regex.Matches(jsonContent, "\"job_id\"").Count;
                    totalCompanies = Regex.Matches(jsonContent, "\"company_name\"").Count;
                }
            } 
            catch (Exception ex) 
            {
                // Ghi log lỗi nếu cần: System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // --- 4. LẤY DANH SÁCH TEMPLATES THEO TRANG ---
            var templates = await _context.Templates
                .OrderByDescending(t => t.TemplateID) // Hoặc .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy thêm danh sách 5-10 việc làm mới nhất để hiện ở Dashboard
            var jobs = await _context.Jobs.Include(j => j.Company)
                .OrderByDescending(j => j.CreatedAt)
                .Take(10) // Lấy 10 tin mới nhất
                .ToListAsync();

            // --- 5. ĐỔ DỮ LIỆU VÀO VIEWMODEL ---
            var stats = new AdminDashboardVM 
            {
                TotalUsers = totalUsers,
                TotalResumes = totalResumes,
                TotalJobs = totalJobs,
                TotalCompanies = totalCompanies,
                Templates = templates,
                Jobs = jobs,
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
        // Danh sách mẫu CV
        public async Task<IActionResult> Templates()
        {
            var templates = await _context.Templates.ToListAsync();
            return View(templates);
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
        
    }
}