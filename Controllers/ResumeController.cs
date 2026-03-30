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
    [Authorize(Roles = "User,Admin, Recruiter")] // Chỉ cho phép các vai trò này truy cập vào ResumeController
    public class ResumeController : Controller
    {
        private readonly AppDbContext _context;

        public ResumeController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị danh sách mẫu CV
        public async Task<IActionResult> Templates()
        {
            var templates = await _context.Templates
                                .Where(t => t.IsActive == true)
                                .ToListAsync();
            return View(templates);
        }

        // 2. Hiển thị trình biên tập CV (Trang Create)
        [HttpGet]
        public async Task<IActionResult> Create(int id) // id là TemplateID (ví dụ: 39)
        {
            // 1. Sử dụng hàm hỗ trợ để lấy ID người dùng
            int userId = GetCurrentUserId();

            // Nếu không lấy được ID (chưa đăng nhập hoặc session hết hạn)
            if (userId == 0) 
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem Mẫu CV (TemplateID) có tồn tại trong Database không
            // Nếu không tồn tại (ví dụ id = 0 do lỗi hoặc URL sai), điều hướng về trang danh sách mẫu
            var templateExists = await _context.Templates.AnyAsync(t => t.TemplateID == id);
            if (!templateExists)
            {
                return RedirectToAction("Templates");
            }

            // 2. TÌM BẢN NHÁP CŨ: Lấy bản sinh ra gần nhất cho mẫu ứng với TemplateID này (Bất kể đã xuất PDF hay chưa)
            var resume = await _context.Resumes
                .Include(r => r.Template)        // Load HTML/CSS của mẫu
                .Include(r => r.ResumeSections)  // Load các phần JSON đã lưu
                .OrderByDescending(r => r.UpdatedAt)
                .FirstOrDefaultAsync(r => r.UserID == userId && r.TemplateID == id);

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
                    .FirstOrDefaultAsync(r => r.ResumeID == resume.ResumeID);
            }

            // 4. Gán ID vào ViewBag để dùng cho các script AutoSave
            ViewBag.ResumeId = resume.ResumeID; 

            // Trả về View cùng với Model là bản Resume (đầy đủ nội dung cũ nếu có)
            return View("Create", resume); 
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
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync(); // Lưu để lấy ResumeID

                // B. Lưu các Section chi tiết vào ResumeSections (Dạng JSON)
                var sections = new List<ResumeSection>();

                // 1. Lưu Kinh nghiệm
                if (model.Experiences != null && model.Experiences.Any()) {
                    sections.Add(new ResumeSection {
                        ResumeID = resume.ResumeID,
                        SectionType = "Experience",
                        ContentJSON = JsonSerializer.Serialize(model.Experiences),
                        SortOrder = 1
                    });
                }

                // 2. Lưu Học vấn
                if (model.Educations != null && model.Educations.Any()) {
                    sections.Add(new ResumeSection {
                        ResumeID = resume.ResumeID,
                        SectionType = "Education",
                        ContentJSON = JsonSerializer.Serialize(model.Educations),
                        SortOrder = 2
                    });
                }

                // 3. Lưu Kỹ năng (Nếu bạn đã cập nhật ViewModel có Skills)
                // if (model.Skills != null) { ... }

                if (sections.Any()) {
                    _context.ResumeSections.AddRange(sections);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, resumeId = resume.ResumeID, message = "Lưu CV thành công!" });
            }
            catch (Exception ex) 
            {
                // Lấy thông báo lỗi chi tiết bên trong nếu có
                string innerError = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message + " | Chi tiết: " + innerError });
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
                resume.Summary = model.Summary;
                resume.AvatarUrl = model.AvatarUrl; // Lưu Base64 ảnh đại diện
                resume.UpdatedAt = DateTime.Now;
                resume.IsDraft = true; 

                // 5. Lưu các phần nội dung động (JSON) qua hàm bổ trợ
                // Lưu ý: Đảm bảo model.Experiences, model.Educations... không bị null để tránh lỗi Serialize
                await SaveSectionJson(resume.ResumeID, "Experience", model.Experiences ?? new List<ExperienceItem>());
                await SaveSectionJson(resume.ResumeID, "Education", model.Educations ?? new List<EducationItem>());
                await SaveSectionJson(resume.ResumeID, "Skills", model.Skills ?? new List<SkillItem>());

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
                // Lấy thông báo lỗi chi tiết bên trong nếu có
                string innerError = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message + " | Chi tiết: " + innerError });
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
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu: " + ex.Message });
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
                return Json(new { success = false, message = "Không thể xóa CV: " + ex.Message });
            }
        }
    }
}