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
                    (r.JobTitle != null && r.JobTitle.ToLower().Contains(searchTerm))
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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsPartial", user);
            }

            return View(user);
        }
    }
}
