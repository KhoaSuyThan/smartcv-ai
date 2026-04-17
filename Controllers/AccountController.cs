using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Models.ViewModels;

namespace DoAnCS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AccountController(IWebHostEnvironment webHostEnvironment, AppDbContext context) {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        // ==========================================
        // ĐĂNG KÝ (REGISTER)
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Companies = _context.Companies.ToList();
            return View("Login", new AuthVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
        public async Task<IActionResult> Register(AuthVM model)
        {
            ModelState.Remove("Login.Email");
            ModelState.Remove("Login.Password");
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra xem Email đã tồn tại chưa
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Register.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Register.Email", "Email này đã được đăng ký.");
                    return View("Login", model);
                }

                // 2. Tạo đối tượng User mới từ dữ liệu người dùng nhập
                var user = new User
                {
                    FullName = model.Register.FullName,
                    Email = model.Register.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Register.Password),
                    
                    // 1. LẤY ROLE TỪ GIAO DIỆN (Bạn cần thêm 1 dropdown chọn Role ở View)
                    // Nếu không chọn, mặc định là 'User' (Candidate)
                    Role = !string.IsNullOrEmpty(model.Register.Role) ? model.Register.Role : "User",                    
                    CreatedAt = DateTime.Now
                };

                // XỬ LÝ TỰ ĐỘNG TẠO CÔNG TY
                if (user.Role == "Recruiter" && !string.IsNullOrEmpty(model.Register.CompanyName))
                {
                    // Tìm xem tên công ty đã có trong database chưa
                    var company = await _context.Companies
                        .FirstOrDefaultAsync(c => c.Name == model.Register.CompanyName);

                    if (company == null)
                    {
                        // Nếu chưa có thì tạo mới công ty
                        company = new Company { 
                            Name = model.Register.CompanyName,
                            CreatedAt = DateTime.Now 
                        };
                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync(); // Lưu để lấy ID tự tăng
                    }
                    
                    // Gán ID công ty cho User
                    user.CompanyID = company.CompanyID;
                }

                // 3. Lưu vào SQL Server
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Đăng ký xong thì chuyển sang trang Đăng nhập
                return RedirectToAction("Login");
            }
            return View("Login", model);
        }

        // ==========================================
        // ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Companies = _context.Companies.ToList();
            return View(new AuthVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthVM model)
        {
            ModelState.Remove("Register.FullName");
            ModelState.Remove("Register.Email");
            ModelState.Remove("Register.Password");
            ModelState.Remove("Register.ConfirmPassword");

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Login.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Login.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim("CompanyID", user.CompanyID.ToString()?? "")
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Email hoặc mật khẩu không chính xác";
            }
            return View(model);
        }

        // ==========================================
        // ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // ĐĂNG NHẬP MẠNG XÃ HỘI (EXTERNAL LOGIN)
        // ==========================================

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Yêu cầu chuyển hướng đến trang đăng nhập của bên thứ 3 (Google, Facebook...)
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // LƯU Ý: Khi dùng External Login, ta dùng Scheme "External" (hoặc Identity.External)
            // để lấy thông tin tạm thời từ Google/GitHub trước khi chuyển sang Cookie chính thức
            var result = await HttpContext.AuthenticateAsync("ExternalCookies"); 
            
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Tự động tạo tài khoản nếu chưa có
                user = new User
                {
                    FullName = name ?? "Social User",
                    Email = email,
                    PasswordHash = "EXTERNAL_LOGIN_" + Guid.NewGuid().ToString(),
                    Role = "User", // Mặc định tài khoản MXH là Candidate
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // THIẾT LẬP COOKIE CHÍNH THỨC CỦA HỆ THỐNG
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng: Gán quyền từ DB
                new Claim("UserID", user.UserID.ToString()),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim("CompanyID", user.CompanyID?.ToString() ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Xóa cookie tạm thời sau khi đã đăng nhập thành công
            await HttpContext.SignOutAsync("ExternalCookies");

            return LocalRedirect(returnUrl);
        }

        // ==========================================
        // HỒ SƠ CÁ NHÂN (PROFILE)
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        // BỔ SUNG: Nhận thêm tham số IFormFile từ View gửi lên
        public async Task<IActionResult> Profile([Bind("FullName,Phone")] User model, IFormFile? avatarFile, bool isDeleteAvatar = false, string? companyName = null, string? companyAddress = null)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                // --- XỬ LÝ UPLOAD ẢNH ĐẠI DIỆN ---
                // TRƯỜNG HỢP 1: NGƯỜI DÙNG NHẤN XÓA ẢNH
                if (isDeleteAvatar)
                {
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        // Xóa file vật lý trong wwwroot/avt
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        
                        user.AvatarUrl = null; // Reset về null
                    }
                }
                // TRƯỜNG HỢP 2: NGƯỜI DÙNG TẢI ẢNH MỚI
                else if (avatarFile != null && avatarFile.Length > 0)
                {
                    string folder = "avt/";
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                    if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                    // Xóa ảnh cũ trước khi thay ảnh mới
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string filePath = Path.Combine(serverFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(fileStream);
                    }
                    user.AvatarUrl = "/" + folder + fileName;
                }

                // Cập nhật các thông tin khác
                user.FullName = model.FullName;
                user.Phone = model.Phone;

                // --- XỬ LÝ THÔNG TIN CÔNG TY CHO NHÀ TUYỂN DỤNG ---
                if (user.Role == "Recruiter" && (!string.IsNullOrEmpty(companyName) || !string.IsNullOrEmpty(companyAddress)))
                {
                    if (user.Company != null)
                    {
                        // Cập nhật công ty đã có
                        if (!string.IsNullOrEmpty(companyName)) user.Company.Name = companyName;
                        if (companyAddress != null) user.Company.Address = companyAddress;
                    }
                    else
                    {
                        // Tạo mới công ty nếu chưa có
                        var newCompany = new Company
                        {
                            Name = companyName ?? "Chưa cập nhật",
                            Address = companyAddress,
                            CreatedAt = DateTime.Now
                        };
                        _context.Companies.Add(newCompany);
                        await _context.SaveChangesAsync();
                        user.CompanyID = newCompany.CompanyID;
                    }
                }

                await _context.SaveChangesAsync();

                // Cập nhật lại Claims để Header/Sidebar hiện tên mới ngay lập tức
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserID", user.UserID.ToString()),
                    new Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png"), // Thêm cả Claim ảnh cho xịn
                    new Claim("IsPro", user.IsPro.ToString()),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("CompanyID", user.CompanyID?.ToString() ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true });

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // ==========================================
        // ĐỔI MẬT KHẨU (CHANGE PASSWORD)
        // ==========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) 
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["PasswordErrorMessage"] = "Vui lòng điền đầy đủ các trường.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            if (newPassword != confirmPassword)
            {
                TempData["PasswordErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            // Ignore external login users or users without password set like this
            if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash.StartsWith("EXTERNAL_LOGIN_"))
            {
                TempData["PasswordErrorMessage"] = "Tài khoản đăng nhập bằng mạng xã hội không thể đổi mật khẩu.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                TempData["PasswordErrorMessage"] = "Mật khẩu hiện tại không chính xác.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["PasswordSuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile", new { t = "password" });
        }

        // ==========================================
        // NÂNG CẤP TÀI KHOẢN (UPGRADE)
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Upgrade()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            // Nếu đã là Pro thì chuyển về Profile kèm thông báo
            if (user.IsPro)
            {
                TempData["InfoMessage"] = "Bạn hiện đang là thành viên Pro!";
                return RedirectToAction("Profile");
            }

            // Kiểm tra xem đã có yêu cầu nào đang chờ duyệt không
            var existingRequest = await _context.UpgradeRequests
                .OrderByDescending(r => r.RequestDate)
                .FirstOrDefaultAsync(r => r.UserID == userId && r.Status == 0);
            
            ViewBag.PendingRequest = existingRequest;

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeConfirmed(IFormFile? evidenceFile, string? notes)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            // Kiểm tra yêu cầu trùng lặp đang chờ duyệt
            var existingRequest = await _context.UpgradeRequests
                .FirstOrDefaultAsync(r => r.UserID == userId && r.Status == 0);
            
            if (existingRequest != null)
            {
                TempData["InfoMessage"] = "Bạn đã gửi yêu cầu nâng cấp rồi. Vui lòng đợi Admin phê duyệt!";
                return RedirectToAction("Upgrade");
            }

            // Xử lý upload ảnh minh chứng
            string? imageUrl = null;
            if (evidenceFile != null && evidenceFile.Length > 0)
            {
                string folder = "uploads/evidence/";
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(evidenceFile.FileName);
                string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                string filePath = Path.Combine(serverFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await evidenceFile.CopyToAsync(fileStream);
                }
                imageUrl = "/" + folder + fileName;
            }

            // Tạo bản ghi yêu cầu mới
            var request = new UpgradeRequest
            {
                UserID = userId,
                RequestDate = DateTime.Now,
                Status = 0, // Chờ duyệt
                EvidenceImageUrl = imageUrl,
                Notes = notes
            };

            _context.UpgradeRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gửi yêu cầu nâng cấp thành công! Vui lòng đợi Admin xét duyệt.";
            return RedirectToAction("Profile");
        }

        // ==========================================
        // TRANG BÁO LỖI QUYỀN TRUY CẬP (ACCESS DENIED)
        // ==========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
