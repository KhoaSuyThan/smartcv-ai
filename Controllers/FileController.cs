using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCS.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly IConfiguration _config;
        private readonly Data.AppDbContext _context;

        public FileController(IConfiguration config, Data.AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpGet("/uploads/cvs/{filename}")]
        [AllowAnonymous]
        public IActionResult DownloadCv(string filename)
        {
            var resume = _context.Resumes.FirstOrDefault(r => r.FileUploadUrl == "/uploads/cvs/" + filename);
            if (resume == null) return NotFound();

            if (!resume.IsPublic)
            {
                var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int currentUserId = int.TryParse(userIdClaim, out int uid) ? uid : 0;

                if (resume.UserID != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Recruiter"))
                {
                    // Nếu chưa đăng nhập, điều hướng đến trang Login
                    if (currentUserId == 0) return RedirectToAction("Login", "Account");
                    return Forbid();
                }
            }

            var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
            var filePath = Path.Combine(baseUploadsFolder, "uploads", "cvs", filename);

            if (!System.IO.File.Exists(filePath)) return NotFound();

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/pdf";
            }

            return PhysicalFile(filePath, contentType);
        }

        [HttpGet("/uploads/{filename}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DownloadSupportAttachment(string filename)
        {
            var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
            var filePath = Path.Combine(baseUploadsFolder, "uploads", filename);

            if (!System.IO.File.Exists(filePath)) return NotFound();

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType);
        }
    }
}
