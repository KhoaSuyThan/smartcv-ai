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
        public async Task<IActionResult> Create(int id) 
        {
            if (id <= 0) return RedirectToAction("Templates");

            var template = await _context.Templates.FindAsync(id);
            if (template == null) return RedirectToAction("Templates");

            return View(template);
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
            catch (Exception ex) {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}