using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;

namespace DoAnCS.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly AppDbContext _context;

        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var companies = await _context.Companies
                .Include(c => c.Jobs)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(companies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(m => m.CompanyID == id);

            if (company == null)
            {
                return NotFound();
            }

            // Lấy danh sách công việc của công ty này (thông qua các User thuộc công ty hoặc trực tiếp nếu Job có CompanyID)
            // Theo Model Job.cs, có CompanyID trực tiếp.
            var jobs = await _context.Jobs
                .Where(j => j.CompanyID == id && j.Status == 1)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.Jobs = jobs;

            return View(company);
        }
    }
}
