using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DoAnCS.Models;
using DoAnCS.Data; 
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DoAnCS.Controllers
{
    // [Authorize] // Tạm thời comment lại để bạn test giao diện không cần đăng nhập
    public class ResumeController : Controller
    {
        private readonly AppDbContext _context;

        public ResumeController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. PHƯƠNG THỨC GET: Hiển thị trang tạo CV (Sửa lỗi 404)
        // ============================================================
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // 2. PHƯƠNG THỨC POST: Lưu dữ liệu từ AJAX
        // ============================================================
        [HttpPost]
        [AllowAnonymous] // Cho phép gửi dữ liệu lên khi chưa có hệ thống Login
        public async Task<IActionResult> SaveResume([FromBody] ResumeViewModel model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu gửi lên trống!" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            try
            {
                // 1. Lấy UserID (Mặc định lấy UserID = 1 từ Database mẫu nếu chưa Login)
                int userId = 1; 
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null) 
                {
                    userId = int.Parse(userIdClaim.Value);
                }

                // 2. Tạo đối tượng Resume chính
                var resume = new Resume
                {
                    UserID = userId,
                    TemplateID = model.TemplateID > 0 ? model.TemplateID : 1, // Mặc định template 1
                    Title = string.IsNullOrEmpty(model.Title) ? "CV Mới" : model.Title,
                    Summary = model.Summary,
                    ThemeColor = model.ThemeColor ?? "#3498db",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync(); // Lưu để sinh ra ResumeID

                // 3. Tạo danh sách các Section (Kinh nghiệm, Học vấn) để lưu vào DB
                var sections = new List<ResumeSection>();

                // Xử lý lưu Kinh nghiệm (Experience)
                if (model.Experiences != null && model.Experiences.Any())
                {
                    sections.Add(new ResumeSection {
                        ResumeID = resume.ResumeID,
                        SectionType = "Experience",
                        ContentJSON = JsonSerializer.Serialize(model.Experiences),
                        SortOrder = 1
                    });
                }

                // Xử lý lưu Học vấn (Education)
                if (model.Educations != null && model.Educations.Any())
                {
                    sections.Add(new ResumeSection {
                        ResumeID = resume.ResumeID,
                        SectionType = "Education",
                        ContentJSON = JsonSerializer.Serialize(model.Educations),
                        SortOrder = 2
                    });
                }

                // Nếu có section thì mới lưu
                if (sections.Any())
                {
                    _context.ResumeSections.AddRange(sections);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, resumeId = resume.ResumeID, message = "Lưu CV thành công!" });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra console để debug dễ hơn
                Console.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }
}