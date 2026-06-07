using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.AspNetCore.Authorization;
using DoAnCS.Models.ViewModels;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using DoAnCS.Services;

namespace DoAnCS.Controllers
{
    // Chỉ những người có Role là Admin mới được vào Controller này
    [Authorize(Roles = "Admin")] 
    public class AdminController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHost;
        private readonly IAIService _aiService;
        private readonly IConfiguration _config;

        public AdminController(AppDbContext context, IWebHostEnvironment webHost, IAIService aiService, IConfiguration config)
        {
            _context = context;
            _webHost = webHost;
            _aiService = aiService;
            _config = config;
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
                .OrderBy(j => j.Status == 0 ? 0 : 1) // Ưu tiên tin chờ duyệt (0) lên đầu
                .ThenByDescending(j => j.CreatedAt)
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
            userInDb.IsPro = Request.Form["IsPro"] == "true"; // Lấy từ checkbox
            userInDb.CompanyID = (Role == "Recruiter") ? CompanyID : null;

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
        public async Task<IActionResult> CreateTemplate(Template template, IFormFile? uploadImage, string[] selectedCategories)
        {
            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                template.Category = string.Join(", ", selectedCategories);
            }
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    // 1. Định nghĩa thư mục lưu trữ
                    var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                    string folder = Path.Combine(baseUploadsFolder, "images", "templates");
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
                TempData["Success"] = "Khởi tạo mẫu CV mới thành công!";
                return RedirectToAction(nameof(Templates));
            }
            return View(template);
        }
        
        // [GET] Load dữ liệu mẫu CV lên form
        [HttpGet]
        public async Task<IActionResult> EditCV(int id, string returnUrl = null)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) return NotFound();
            
            ViewBag.ReturnUrl = returnUrl;
            return View(template);
        }

        // [POST] Xử lý lưu dữ liệu sau khi sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCV(int id, Template template, IFormFile? uploadImage, string[] selectedCategories, string returnUrl = null)
        {
            if (id != template.TemplateID) return NotFound();

            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                template.Category = string.Join(", ", selectedCategories);
            }
            else 
            {
                template.Category = ""; // Reset if none selected
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý nếu Admin chọn upload file mới
                    if (uploadImage != null && uploadImage.Length > 0)
                    {
                        var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                        string folder = Path.Combine(baseUploadsFolder, "images", "templates");
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
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction(nameof(Templates));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(template);
        }

        // [GET] Danh sách việc làm
        public async Task<IActionResult> ManageJobs()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .OrderBy(j => j.Status == 0 ? 0 : 1) // Ưu tiên tin chờ duyệt lên đầu
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();
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
        public async Task<IActionResult> Jobs(int page = 1)
        {
            int pageSize = 10;
            var query = _context.Jobs.Include(j => j.Company);

            int totalJobs = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalJobs / pageSize);

            var jobs = await query
                .OrderBy(j => j.Status == 0 ? 0 : 1) // Ưu tiên tin chờ duyệt lên đầu
                .ThenByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new AdminDashboardVM
            {
                Jobs = jobs,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = await _context.Users.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalJobs = totalJobs,
                TotalResumes = await _context.Resumes.CountAsync()
            };

            return View(vm);
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

        // [GET] Xác nhận xóa mẫu CV
        [HttpGet]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null) return NotFound();

            return View(template);
        }

        [HttpPost, ActionName("DeleteTemplate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplateConfirmed(int id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template != null)
            {
                // Xóa tất cả Resume đang sử dụng mẫu này trước để tránh lỗi khóa ngoại (Foreign Key)
                var relatedResumes = _context.Resumes.Where(r => r.TemplateID == id);
                _context.Resumes.RemoveRange(relatedResumes);
                
                // Sau đó xóa mẫu CV
                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Đã xóa vĩnh viễn mẫu CV và các dữ liệu liên quan!";
            }
            return RedirectToAction(nameof(Templates));
        }

        // --- QUẢN LÝ VUE CV ---

        public async Task<IActionResult> VueTemplates(int page = 1)
        {
            int pageSize = 4;
            var query = _context.VueTemplates.AsQueryable();

            int totalTemplates = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalTemplates / pageSize);

            var templates = await query
                .OrderByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new AdminDashboardVM
            {
                VueTemplates = templates,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = await _context.Users.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalResumes = await _context.Resumes.CountAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVueCV(VueTemplate template, IFormFile? uploadImage, string[] selectedCategories)
        {
            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                template.Category = string.Join(", ", selectedCategories);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem file .vue có tồn tại trong thư mục templates không (chỉ kiểm tra khi ở Local)
                    string componentName = template.ComponentName?.Trim() ?? "";
                    string componentFile = componentName.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ? componentName : componentName + ".vue";
                    string templateDir = Path.Combine(_webHost.ContentRootPath, "CVBuilderApp", "src", "templates");
                    string templatePath = Path.Combine(templateDir, componentFile);

                    if (System.IO.Directory.Exists(templateDir) && !System.IO.File.Exists(templatePath))
                    {
                        TempData["Error"] = $"Cảnh báo: File component '{componentFile}' không tồn tại trong thư mục templates. Vui lòng tạo file trước!";
                        return RedirectToAction(nameof(VueTemplates));
                    }

                    if (uploadImage != null && uploadImage.Length > 0)
                    {
                        var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                        string folder = Path.Combine(baseUploadsFolder, "images", "templates");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        string fileName = "vue_" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(uploadImage.FileName);
                        string filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadImage.CopyToAsync(stream);
                        }

                        template.ThumbnailUrl = "/images/templates/" + fileName;
                    }

                    template.CreatedAt = DateTime.Now;
                    _context.VueTemplates.Add(template);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Thêm mẫu Vue CV mới thành công!";
                    return RedirectToAction(nameof(VueTemplates));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                }
            }
            
            TempData["Error"] = "Dữ liệu không hợp lệ, vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(VueTemplates));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVueTemplateStatus(int id)
        {
            var template = await _context.VueTemplates.FindAsync(id);
            if (template == null) return NotFound();

            template.IsActive = !template.IsActive;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVueTemplate(int id)
        {
            var template = await _context.VueTemplates.FindAsync(id);
            if (template != null)
            {
                _context.VueTemplates.Remove(template);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa mẫu Vue CV thành công!";
            }
            return RedirectToAction(nameof(VueTemplates));
        }

        [HttpGet]
        public async Task<IActionResult> EditVueCV(int id, string returnUrl = null)
        {
            var template = await _context.VueTemplates.FindAsync(id);
            if (template == null) return NotFound();
            ViewBag.ReturnUrl = returnUrl;
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVueCV(int id, VueTemplate template, IFormFile? uploadImage, string[] selectedCategories, string returnUrl = null)
        {
            if (id != template.Id) return NotFound();

            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                template.Category = string.Join(", ", selectedCategories);
            }
            else 
            {
                template.Category = ""; // Reset if none selected
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra file vật lý (chỉ kiểm tra khi ở Local)
                    string componentName = template.ComponentName?.Trim() ?? "";
                    string componentFile = componentName.EndsWith(".vue", StringComparison.OrdinalIgnoreCase) ? componentName : componentName + ".vue";
                    string templateDir = Path.Combine(_webHost.ContentRootPath, "CVBuilderApp", "src", "templates");
                    string templatePath = Path.Combine(templateDir, componentFile);

                    if (System.IO.Directory.Exists(templateDir) && !System.IO.File.Exists(templatePath))
                    {
                        ModelState.AddModelError("ComponentName", $"File '{componentFile}' không tồn tại trong source code.");
                        return View(template);
                    }

                    if (uploadImage != null && uploadImage.Length > 0)
                    {
                        var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                        string folder = Path.Combine(baseUploadsFolder, "images", "templates");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        string fileName = "vue_" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(uploadImage.FileName);
                        string filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadImage.CopyToAsync(stream);
                        }

                        template.ThumbnailUrl = "/images/templates/" + fileName;
                    }

                    _context.Update(template);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Cập nhật mẫu Vue CV thành công!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction(nameof(VueTemplates));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(template);
        }

        // --- KẾT THÚC QUẢN LÝ VUE CV ---

        // 1. Danh sách công ty
        public async Task<IActionResult> Companies(int page = 1)
        {
            int pageSize = 8;
            var query = _context.Companies.AsQueryable();

            int totalCompanies = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCompanies / pageSize);

            var companies = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new AdminDashboardVM
            {
                Companies = companies,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = await _context.Users.CountAsync(),
                TotalCompanies = totalCompanies,
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalResumes = await _context.Resumes.CountAsync()
            };

            return View(vm);
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

        // ==========================================
        //  Quản lý Cấu hình AI Gemini
        // ==========================================
        
        [HttpGet]
        public async Task<IActionResult> GeminiConfig()
        {
            var config = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == 1);
            if (config == null) 
            {
                // Nếu chưa có, tạo mặc định (đề phòng Seed data chưa chạy)
                config = new GeminiConfig();
                _context.GeminiConfigs.Add(config);
                await _context.SaveChangesAsync();
            }

            // Lấy thêm Usage Tracker cho View
            var today = DateTime.Today;
            var tokensGemini = await _context.AILogs
                .Where(l => l.CreatedAt.Date == today && (l.ApiProvider == "Gemini" || l.ApiProvider == null))
                .SumAsync(l => (int?)l.UsedTokens) ?? 0;

            var tokensGroq = await _context.AILogs
                .Where(l => l.CreatedAt.Date == today && l.ApiProvider == "Groq")
                .SumAsync(l => (int?)l.UsedTokens) ?? 0;

            var callsToday = await _context.AILogs
                .Where(l => l.CreatedAt.Date == today)
                .CountAsync();

            ViewBag.TokensGemini = tokensGemini;
            ViewBag.TokensGroq = tokensGroq;
            ViewBag.CallsToday = callsToday;

            // Tổng token tích lũy toàn thời gian (lấy từ AILogs cho chính xác)
            var totalTokensAll = await _context.AILogs.SumAsync(l => (long?)l.UsedTokens) ?? 0L;
            ViewBag.TotalTokensAll = totalTokensAll;

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateConfig(GeminiConfig model)
        {
            var config = await _context.GeminiConfigs.FirstOrDefaultAsync(c => c.Id == 1);
            if (config != null)
            {
                // Cập nhật giá trị
                config.ApiKey = model.ApiKey;
                config.GroqApiKey = model.GroqApiKey;
                config.ChatbotApiKey = model.ChatbotApiKey; // Key riêng cho chatbox
                config.ModelName = model.ModelName;
                config.Temperature = model.Temperature;
                config.MaxOutputTokens = model.MaxOutputTokens;
                
                // Cập nhật cấu hình Pro
                config.ProModelName = model.ProModelName;
                config.ProTemperature = model.ProTemperature;
                config.ProMaxOutputTokens = model.ProMaxOutputTokens;
                config.ProUserRateLimit = model.ProUserRateLimit;

                config.SystemInstruction = model.SystemInstruction;
                config.ChatbotSystemInstruction = model.ChatbotSystemInstruction;
                config.SkillTemplate = model.SkillTemplate;
                config.SummaryTemplate = model.SummaryTemplate;
                config.GrammarTemplate = model.GrammarTemplate;
                config.UserRateLimit = model.UserRateLimit;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã lưu cài đặt AI thành công!";
            }
            return RedirectToAction(nameof(GeminiConfig));
        }

        [HttpGet]
        public async Task<IActionResult> GetAIUsageStats()
        {
            var today = DateTime.Today;
            var tokensGemini = await _context.AILogs
                .Where(l => l.CreatedAt.Date == today && (l.ApiProvider == "Gemini" || l.ApiProvider == null))
                .SumAsync(l => (int?)l.UsedTokens) ?? 0;

            var tokensGroq = await _context.AILogs
                .Where(l => l.CreatedAt.Date == today && l.ApiProvider == "Groq")
                .SumAsync(l => (int?)l.UsedTokens) ?? 0;

            var callsToday = await _context.AILogs
                .Where(l => l.CreatedAt.Date == today)
                .CountAsync();

            var totalTokensAll = await _context.AILogs.SumAsync(l => (long?)l.UsedTokens) ?? 0L;

            return Json(new { 
                success = true, 
                callsToday = callsToday, 
                tokensGemini = tokensGemini, 
                tokensGroq = tokensGroq, 
                totalTokensAll = totalTokensAll.ToString("N0") 
            });
        }

        [HttpPost]
        public async Task<IActionResult> TestGemini([FromBody] dynamic payload)
        {
            try 
            {
                string prompt = payload.GetProperty("prompt").GetString();
                if (string.IsNullOrWhiteSpace(prompt))
                    return Json(new { success = false, answer = "Vui lòng nhập nội dung." });

                // Dùng chung Pipeline AIController cho tiện hoặc gọi trực tiếp Service
                var answer = await _aiService.GenerateContent(prompt);
                
                return Json(new { success = true, answer = answer });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, answer = "Lỗi: " + ex.Message });
            }
        }

        // ==========================================
        //  Quản lý Yêu cầu Nâng cấp Pro
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> UpgradeRequests()
        {
            var requests = await _context.UpgradeRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUpgrade(int id)
        {
            var request = await _context.UpgradeRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();

            if (request.Status == 0) // Chỉ xử lý nếu đang chờ duyệt
            {
                request.Status = 1; // Đã duyệt
                request.DecisionDate = DateTime.Now;
                
                if (request.User != null)
                {
                    request.User.IsPro = true;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã phê duyệt nâng cấp Pro cho {request.User?.FullName}";
            }

            return RedirectToAction(nameof(UpgradeRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUpgrade(int id, string? reason)
        {
            var request = await _context.UpgradeRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();

            if (request.Status == 0)
            {
                request.Status = 2; // Từ chối
                request.DecisionDate = DateTime.Now;
                request.Notes = reason;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã từ chối nâng cấp Pro cho {request.User?.FullName}";
            }

            return RedirectToAction(nameof(UpgradeRequests));
        }
    }
}