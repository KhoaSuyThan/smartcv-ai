using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DoAnCS.Models;
using DoAnCS.Data; 
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Models.ViewModels;

namespace DoAnCS.Controllers
{
    [Authorize(Roles = "User,Admin")] // Chỉ cho phép Ứng viên (User) và Admin truy cập. Đã khóa với Nhà tuyển dụng (Recruiter).
    public class ResumeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public ResumeController(AppDbContext context, IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;
        }

        // 1. Hiển thị danh sách mẫu CV
        public async Task<IActionResult> Templates()
        {
            if (!_env.IsDevelopment()) return RedirectToAction("VueTemplates");

            var templates = await _context.Templates
                                .Where(t => t.IsActive == true)
                                .ToListAsync();
            return View(templates);
        }

        // 2. Hiển thị trình biên tập CV (Trang Create)
        [HttpGet]
        public async Task<IActionResult> Create(int id) // id là TemplateID (ví dụ: 39)
        {
            if (!_env.IsDevelopment()) return RedirectToAction("VueTemplates");

            // 1. Sử dụng hàm hỗ trợ để lấy ID người dùng
            int userId = GetCurrentUserId();

            // Nếu không lấy được ID (chưa đăng nhập hoặc session hết hạn)
            if (userId == 0) 
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Kiểm tra xem Mẫu CV (TemplateID) có tồn tại và quyền truy cập
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.TemplateID == id);
            if (template == null)
            {
                return RedirectToAction("Templates");
            }

            // Kiểm tra quyền Pro
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            bool isPro = user?.IsPro ?? false;

            if (template.IsProOnly && !isPro)
            {
                TempData["ErrorMessage"] = "Mẫu CV này chỉ dành cho thành viên Pro. Hãy nâng cấp tài khoản để sử dụng!";
                return RedirectToAction("Templates");
            }

            // 2. TÌM BẢN NHÁP CŨ: Chỉ lấy những bản ghi truyền thống (Không phải CV Vue)
            var resume = await _context.Resumes
                .Include(r => r.Template)
                .Include(r => r.ResumeSections)
                .Where(r => r.UserID == userId && r.TemplateID == id && !r.Title.StartsWith("CV Vue: "))
                .OrderByDescending(r => r.UpdatedAt)
                .FirstOrDefaultAsync();

            // 3. NẾU CHƯA CÓ THÌ MỚI TẠO MỚI
            if (resume == null)
            {
                resume = new Resume
                {
                    UserID = userId,
                    TemplateID = id,
                    Title = "Bản nháp CV",
                    IsDraft = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync();

                // Nạp lại để đảm bảo đối tượng Template đi kèm không bị null
                resume = await _context.Resumes
                    .Include(r => r.Template)
                    .Include(r => r.ResumeSections) // Bổ sung Include ResumeSections
                    .FirstOrDefaultAsync(r => r.ResumeID == resume.ResumeID);
            }

            // 4. Gán ID vào ViewBag để dùng cho các script AutoSave
            ViewBag.ResumeId = resume.ResumeID; 

            // Trả về View cùng với Model là bản Resume (đầy đủ nội dung cũ nếu có)
            return View("Create", resume); 
        }

        // ==========================================
        // UPLOAD CV PDF MỚI NHẤT
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPdf(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return RedirectToAction("Profile", "Account", new { t = "cv" });

            if (pdfFile.ContentType != "application/pdf")
            {
                TempData["ErrorMessage"] = "Chỉ chấp nhận file định dạng PDF.";
                return RedirectToAction("Profile", "Account", new { t = "cv" });
            }

            if (pdfFile.Length > 5 * 1024 * 1024) // Giới hạn 5MB
            {
                TempData["ErrorMessage"] = "Kích thước file PDF không được vượt quá 5MB.";
                return RedirectToAction("Profile", "Account", new { t = "cv" });
            }

            var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
            var uploadsFolder = Path.Combine(baseUploadsFolder, "uploads", "cvs");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(pdfFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(fileStream);
            }

            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            // Tạo bản record Resume đánh dấu là file upload
            var resume = new Resume
            {
                UserID = userId,
                Title = Path.GetFileNameWithoutExtension(pdfFile.FileName),
                IsDraft = false, // Là file hoàn chỉnh
                FileUploadUrl = "/uploads/cvs/" + uniqueFileName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                FullName = user?.FullName,
                Email = user?.Email,
                Phone = user?.Phone
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tải lên CV PDF thành công!";
            return RedirectToAction("Profile", "Account", new { t = "cv" });
        }

        // ==========================================
        // XÓA CV
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCVProfile(int id)
        {
            int userId = GetCurrentUserId();
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.ResumeID == id && r.UserID == userId);

            if (resume == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy CV hoặc bạn không có quyền xóa.";
                return RedirectToAction("Profile", "Account", new { t = "cv" });
            }

            // 1. Xóa file vật lý nếu có
            if (!string.IsNullOrEmpty(resume.FileUploadUrl))
            {
                var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                var filePath = Path.Combine(baseUploadsFolder, resume.FileUploadUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // 2. Xóa các đơn ứng tuyển liên quan (Tránh lỗi khóa ngoại)
            var applications = await _context.Applications.Where(a => a.ResumeID == id).ToListAsync();
            if (applications.Any())
            {
                _context.Applications.RemoveRange(applications);
            }

            // 3. EF Core sẽ tự động cascade xóa ResumeSections nếu cấu hình chuẩn,
            // nhưng để an toàn ta xóa thủ công luôn nếu có (chỉ cho CV builder)
            var sections = await _context.ResumeSections.Where(s => s.ResumeID == id).ToListAsync();
            if (sections.Any())
            {
                _context.ResumeSections.RemoveRange(sections);
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa CV thành công.";
            return RedirectToAction("Profile", "Account", new { t = "cv" });
        }

        // 3. PHƯƠNG THỨC POST: Lưu toàn bộ thông tin CV
        [HttpPost]
        public async Task<IActionResult> SaveResume([FromBody] ResumeViewModel model)
        {
            if (model == null) return Json(new { success = false, message = "Dữ color liệu không hợp lệ." });

            try {
                // Lấy UserID từ Claims người dùng đang đăng nhập
                var userIdClaim = User.FindFirst("UserID")?.Value;
                int userId = string.IsNullOrEmpty(userIdClaim) ? 1 : int.Parse(userIdClaim);

                // A. Lưu vào bảng Resumes (Thông tin cá nhân chính)
                var resume = new Resume {
                    UserID = userId,
                    TemplateID = model.TemplateID,
                    Title = model.Title ?? "CV Mới",
                    FullName = model.FullName,
                    JobTitle = model.JobTitle,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    BirthDate = DateTime.TryParse(model.BirthDate, out var dt) ? dt : (DateTime?)null,
                    Summary = model.Summary,
                    AvatarUrl = model.AvatarUrl,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync(); // Lưu để lấy ResumeID

                // B. Lưu các Section chi tiết vào ResumeSections (Dạng JSON)
                await SaveSectionJson(resume.ResumeID, "Experience", model.Experiences ?? new List<ExperienceItem>());
                await SaveSectionJson(resume.ResumeID, "Education", model.Educations ?? new List<EducationItem>());
                await SaveSectionJson(resume.ResumeID, "Skills", model.Skills ?? new List<SkillItem>());
                await SaveSectionJson(resume.ResumeID, "Languages", model.Languages ?? new List<LanguageItem>());
                await SaveSectionJson(resume.ResumeID, "OtherSkills", model.OtherSkills ?? new List<OtherSkillItem>());
                await SaveSectionJson(resume.ResumeID, "Awards", model.Awards ?? new List<AwardItem>());
                await SaveSectionJson(resume.ResumeID, "References", model.References ?? new List<ReferenceItem>());
                await SaveSectionJson(resume.ResumeID, "Certifications", model.Certifications ?? new List<CertificationItem>());
                await SaveSectionJson(resume.ResumeID, "Activities", model.Activities ?? new List<ActivityItem>());
                await SaveSectionJson(resume.ResumeID, "Hobbies", model.Hobbies ?? new List<HobbyItem>());
                await SaveSectionJson(resume.ResumeID, "Projects", model.Projects ?? new List<ProjectItem>());
                await SaveSectionJson(resume.ResumeID, "Website", model.Website ?? "");

                await _context.SaveChangesAsync();

                return Json(new { success = true, resumeId = resume.ResumeID, message = "Lưu CV thành công!" });
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Lỗi SaveResume: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AutoSave([FromBody] ResumeViewModel model)
        {
            try
            {
                // 1. Lấy UserID từ hàm helper (đảm bảo lấy từ Session/Cookie sạch)
                int userId = GetCurrentUserId();
                if (userId == 0) 
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
                }

                // 2. Tìm CV: Phải khớp ResumeID VÀ phải thuộc về UserID đang đăng nhập
                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.ResumeID == model.ResumeID && r.UserID == userId);

                // 3. Chốt chặn bảo mật: Nếu không tìm thấy (do sai ID hoặc sai chủ sở hữu)
                if (resume == null) 
                {
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa bản ghi này!" });
                }

                // 4. Cập nhật các thông tin cơ bản
                resume.FullName = model.FullName;
                resume.JobTitle = model.JobTitle;
                resume.Email = model.Email;
                resume.Phone = model.Phone;
                resume.Address = model.Address;
                resume.BirthDate = DateTime.TryParse(model.BirthDate, out var dt) ? dt : (DateTime?)null;
                resume.Summary = model.Summary;
                resume.AvatarUrl = model.AvatarUrl; // Lưu Base64 ảnh đại diện
                resume.UpdatedAt = DateTime.Now;
                resume.IsDraft = true; 

                // 5. Lưu các phần nội dung động (JSON) qua hàm bổ trợ
                // Lưu ý: Đảm bảo model.Experiences, model.Educations... không bị null để tránh lỗi Serialize
                await SaveSectionJson(resume.ResumeID, "Experience", model.Experiences ?? new List<ExperienceItem>());
                await SaveSectionJson(resume.ResumeID, "Education", model.Educations ?? new List<EducationItem>());
                await SaveSectionJson(resume.ResumeID, "Skills", model.Skills ?? new List<SkillItem>());
                await SaveSectionJson(resume.ResumeID, "Languages", model.Languages ?? new List<LanguageItem>());
                await SaveSectionJson(resume.ResumeID, "OtherSkills", model.OtherSkills ?? new List<OtherSkillItem>());
                await SaveSectionJson(resume.ResumeID, "Awards", model.Awards ?? new List<AwardItem>());
                await SaveSectionJson(resume.ResumeID, "References", model.References ?? new List<ReferenceItem>());
                await SaveSectionJson(resume.ResumeID, "Certifications", model.Certifications ?? new List<CertificationItem>());
                await SaveSectionJson(resume.ResumeID, "Activities", model.Activities ?? new List<ActivityItem>());
                await SaveSectionJson(resume.ResumeID, "Hobbies", model.Hobbies ?? new List<HobbyItem>());
                await SaveSectionJson(resume.ResumeID, "Projects", model.Projects ?? new List<ProjectItem>());
                await SaveSectionJson(resume.ResumeID, "Website", model.Website ?? "");

                // 6. Thực thi lưu vào Database
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    lastSaved = DateTime.Now.ToString("HH:mm:ss"),
                    message = "Đã tự động lưu nháp." 
                });
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Lỗi AutoSave: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        // Hàm bổ trợ xử lý JSON cho gọn code
        private async Task SaveSectionJson(int resumeId, string type, object data)
        {
            var section = await _context.ResumeSections
                .FirstOrDefaultAsync(s => s.ResumeID == resumeId && s.SectionType == type);

            string json = System.Text.Json.JsonSerializer.Serialize(data);

            if (section == null) {
                _context.ResumeSections.Add(new ResumeSection { 
                    ResumeID = resumeId, SectionType = type, ContentJSON = json 
                });
            } else {
                section.ContentJSON = json;
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogExport(int resumeId)
        {
            try
            {
                int userId = GetCurrentUserId();
                // Ghi lại lịch sử xuất file
                var exportLog = new ResumeExport
                {
                    ResumeID = resumeId,
                    ExportDate = DateTime.Now,
                    FileUrl = "Local Download" // Bạn có thể lưu tên file hoặc link nếu có
                };

                _context.ResumeExports.Add(exportLog);
                // 2. CẬP NHẬT TRẠNG THÁI CV: Đã xuất PDF -> Không còn là bản nháp
                var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == resumeId && r.UserID == userId);
                if (resume != null)
                {
                    resume.IsDraft = false; 
                }
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        private int GetCurrentUserId()
        {
            // Lấy ID từ Claims lúc người dùng đăng nhập
            var userIdClaim = User.FindFirst("UserID")?.Value 
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        // 5. Hiển thị trang "CV của tôi"
        [HttpGet]
        public async Task<IActionResult> MyResumes()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) 
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách CV của User, Include thêm Template để sau này lấy ảnh Thumbnail mẫu
            var myResumes = await _context.Resumes
                .Include(r => r.Template) 
                .Where(r => r.UserID == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            // Lấy thêm danh sách VueTemplates để map thumbnail cho CV Vue
            ViewBag.VueTemplates = await _context.VueTemplates.Where(t => t.IsActive).ToListAsync();

            return View(myResumes);
        }

        // 6. Đổi tên CV
        [HttpPost]
        public async Task<IActionResult> UpdateName(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return Json(new { success = false, message = "Tên CV không được để trống." });
            }

            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản CV." });
            }

            // Cập nhật thông tin
            resume.Title = newName.Trim();
            resume.UpdatedAt = DateTime.Now;

            try
            {
                _context.Resumes.Update(resume);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi UpdateName: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        // Cập nhật Vị trí (Job Title) của CV
        [HttpPost]
        public async Task<IActionResult> UpdateJobTitle(int id, string jobTitle)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản CV." });
            }

            resume.JobTitle = string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle.Trim();
            resume.UpdatedAt = DateTime.Now;

            try
            {
                _context.Resumes.Update(resume);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi UpdateJobTitle: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        // 2. Chức năng xóa bản nháp CV
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null)
            {
                return Json(new { success = false, message = "Bản CV không tồn tại hoặc đã bị xóa." });
            }

            try
            {
                // Khi xóa Resume, nhờ vào CASCADE trong SQL, các bảng con như ResumeSections, ResumeSkills cũng sẽ mất theo
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Delete CV: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        // Chức năng xóa tất cả CV của người dùng hiện tại
        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn." });
            }

            try
            {
                var myResumes = await _context.Resumes
                    .Where(r => r.UserID == userId)
                    .ToListAsync();

                if (myResumes.Any())
                {
                    // Xóa file vật lý của các CV tải lên nếu có
                    foreach (var resume in myResumes)
                    {
                        if (!string.IsNullOrEmpty(resume.FileUploadUrl))
                        {
                            var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                            var filePath = Path.Combine(baseUploadsFolder, resume.FileUploadUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }

                    _context.Resumes.RemoveRange(myResumes);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Đã xóa tất cả CV thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi DeleteAll CV: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }
        // 7. Hiển thị trang Builder (Vue SPA)
        [HttpGet]
        public IActionResult Builder(int id)
        {
            // Trả ID của CV để Vue có thể móc vào API GET /api/cvbuilder/data/{id}
            ViewBag.ResumeId = id;
            return View();
        }

        // ==========================================
        // TEST VUE CV FUNCTIONALITY
        // ==========================================

        // 8. Hiển thị danh sách mẫu CV Vue (Dùng cho Testing)
        public async Task<IActionResult> VueTemplates()
        {
            var vueTemplates = await _context.VueTemplates
                                .Where(t => t.IsActive == true)
                                .ToListAsync();
            return View(vueTemplates);
        }

        // 9. Tạo CV từ mẫu Vue
        [HttpGet]
        public async Task<IActionResult> CreateVue(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var vueTemplate = await _context.VueTemplates.FindAsync(id);
            if (vueTemplate == null) return RedirectToAction("VueTemplates");

            // --- CẢI TIẾN: Tránh lưu trùng lặp bằng cách tìm theo UserID và TemplateID thực tế ---
            // Chỉ tìm các bản ghi có Title bắt đầu bằng "CV Vue: " để tránh bốc nhầm dữ liệu của bảng cũ
            var existingResume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.UserID == userId 
                                       && r.TemplateID == id 
                                       && r.Title.StartsWith("CV Vue: "));

            if (existingResume != null)
            {
                // Nếu đã có, chuyển hướng thẳng vào Builder với ID cũ
                return RedirectToAction("Builder", new { id = existingResume.ResumeID });
            }

            // Nếu chưa có, tiến hành tạo mới với TemplateID thực tế
            var resume = new Resume
            {
                UserID = userId,
                TemplateID = id, // Lấy Id thực tế từ bảng VueTemplates
                Title = "CV Vue: " + vueTemplate.TemplateName,
                IsDraft = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                // Lưu ComponentName vào JsonContent để Vue Builder nhận diện mẫu cần dùng
                JsonContent = JsonSerializer.Serialize(new { 
                    overrideTemplate = vueTemplate.ComponentName,
                    theme = new { primaryColor = "#2b5c8f" },
                    general = new { fullName = "", jobTitle = "" }
                })
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            return RedirectToAction("Builder", new { id = resume.ResumeID });
        }

        // ==========================================
        // TÍNH NĂNG CV CÔNG KHAI
        // ==========================================
        [HttpGet("/cv/p/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> Public(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return Content("Lỗi: URL không hợp lệ.", "text/plain; charset=utf-8");

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Slug == slug && r.IsPublic == true);

            if (resume == null) return Content("Lỗi: CV không tồn tại hoặc chủ sở hữu đã tắt tính năng chia sẻ cộng đồng.", "text/plain; charset=utf-8");

            // Tăng lượt xem
            resume.ViewCount += 1;
            await _context.SaveChangesAsync();

            // Kiểm tra trạng thái Pro của chủ sở hữu CV (để hiển thị watermark cho Free)
            var owner = await _context.Users.FirstOrDefaultAsync(u => u.UserID == resume.UserID);
            ViewBag.IsOwnerPro = owner?.IsPro ?? false;

            ViewBag.ResumeId = resume.ResumeID;
            return View("PublicViewerCVVue", resume);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePublic([FromBody] TogglePublicRequest model)
        {
            int userId = GetCurrentUserId();
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == model.ResumeId && r.UserID == userId);
            if (resume == null) return Json(new { success = false, message = "Không tìm thấy CV." });

            resume.IsPublic = model.IsPublic;

            // Generate slug if making public and doesn't exist
            if (resume.IsPublic && string.IsNullOrEmpty(resume.Slug))
            {
                string baseStr = !string.IsNullOrEmpty(resume.FullName) ? resume.FullName : (!string.IsNullOrEmpty(resume.Title) ? resume.Title : "cv");
                string slug = GenerateSlug(baseStr) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                resume.Slug = slug;
            }

            // --- BỔ SUNG: Giới hạn tối đa 4 CV công khai ---
            if (model.IsPublic)
            {
                int publicCount = await _context.Resumes.CountAsync(r => r.UserID == userId && r.IsPublic);
                if (publicCount >= 4)
                {
                    return Json(new { success = false, message = "Bạn chỉ có thể công khai tối đa 4 CV trên hồ sơ cá nhân." });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, slug = resume.Slug });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi TogglePublic: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return "cv";
            // Xóa dấu tiếng Việt
            string str = text.ToLower().Normalize(System.Text.NormalizationForm.FormD);
            str = new string(str.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
            // Chỉ giữ lại chữ cái, số và khoảng trắng
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-").Trim();
            return str;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Preview(int id)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null || (!resume.IsPublic && !User.IsInRole("Admin"))) 
                return NotFound("Hồ sơ không tồn tại hoặc đã bị tắt chia sẻ.");

            // Tăng lượt xem nếu không phải chủ sở hữu xem
            int currentUserId = GetCurrentUserId();
            if (resume.UserID != currentUserId)
            {
                resume.ViewCount += 1;
                await _context.SaveChangesAsync();
            }

            var owner = await _context.Users.FirstOrDefaultAsync(u => u.UserID == resume.UserID);
            ViewBag.IsOwnerPro = owner?.IsPro ?? false;
            ViewBag.ResumeId = resume.ResumeID;

            return View("PublicViewerCVVue", resume);
        }
    }

    public class TogglePublicRequest {
        public int ResumeId { get; set; }
        public bool IsPublic { get; set; }
    }
}