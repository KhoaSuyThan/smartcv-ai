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
        public AccountController(AppDbContext context) => _context = context;

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
                new Claim("CompanyID", user.CompanyID.ToString())
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
        [Authorize] // Bắt buộc đăng nhập mới được vào
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return View(user); // Truyền thẳng Model User ra View
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind("FullName,Phone")] User model)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Phone = model.Phone; 
             //Không gán user.Role hay user.PasswordHash ở đây

                await _context.SaveChangesAsync();
                var currentNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                if (currentNameClaim != user.FullName)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("UserID", user.UserID.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    // Ghi đè lại Cookie đăng nhập
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                }
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }
            return View(user);
        }
    }
}
