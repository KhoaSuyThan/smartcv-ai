using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;

namespace DoAnCS.Controllers
{
    [Authorize(Roles = "Recruiter,Admin")]
    public class TalentController : BaseController
    {
        private readonly AppDbContext _context;

        public TalentController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, string jobTitle)
        {
            var query = _context.Resumes
                .Include(r => r.User)
                .Where(r => r.IsPublic && !r.IsDraft)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r => 
                    (r.FullName != null && r.FullName.ToLower().Contains(searchTerm)) ||
                    (r.Summary != null && r.Summary.ToLower().Contains(searchTerm)) ||
                    (r.JobTitle != null && r.JobTitle.ToLower().Contains(searchTerm)) ||
                    (r.User != null && r.User.Summary != null && r.User.Summary.ToLower().Contains(searchTerm)) ||
                    (r.User != null && r.User.Skills != null && r.User.Skills.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(jobTitle))
            {
                jobTitle = jobTitle.ToLower();
                query = query.Where(r => r.JobTitle != null && r.JobTitle.ToLower().Contains(jobTitle));
            }

            var resumes = await query
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            // Lấy danh sách ứng viên đã lưu để hiển thị bên cánh phải
            int recruiterId = CurrentUserId;
            var savedCandidates = await _context.SavedCandidates
                .Include(s => s.Resume)
                .ThenInclude(r => r.User)
                .Where(s => s.RecruiterId == recruiterId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            ViewBag.SavedCandidates = savedCandidates;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.JobTitleFilter = jobTitle;

            return View(resumes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Resumes)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return NotFound();

            // Chỉ lấy những CV công khai và không phải nháp
            var publicResumes = user.Resumes
                .Where(r => r.IsPublic && !r.IsDraft)
                .OrderByDescending(r => r.UpdatedAt)
                .ToList();

            ViewBag.PublicResumes = publicResumes;

            // Lấy danh sách ID các CV mà nhà tuyển dụng này đã lưu
            int recruiterId = CurrentUserId;
            ViewBag.SavedResumeIds = await _context.SavedCandidates
                .Where(s => s.RecruiterId == recruiterId)
                .Select(s => s.ResumeId)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", user);
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSave(int resumeId)
        {
            int recruiterId = CurrentUserId;
            if (recruiterId == 0) return Json(new { success = false, message = "Bạn cần đăng nhập." });

            var existing = await _context.SavedCandidates
                .FirstOrDefaultAsync(s => s.RecruiterId == recruiterId && s.ResumeId == resumeId);

            if (existing != null)
            {
                _context.SavedCandidates.Remove(existing);
                await _context.SaveChangesAsync();
                return Json(new { success = true, saved = false, message = "Đã bỏ lưu ứng viên." });
            }
            else
            {
                var saved = new SavedCandidate
                {
                    RecruiterId = recruiterId,
                    ResumeId = resumeId
                };
                _context.SavedCandidates.Add(saved);
                await _context.SaveChangesAsync();
                return Json(new { success = true, saved = true, message = "Đã lưu ứng viên vào danh sách tiềm năng." });
            }
        }
    }
}
