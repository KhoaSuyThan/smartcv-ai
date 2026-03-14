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
                    // Băm mật khẩu bằng BCrypt để lưu vào cột PasswordHash
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Register.Password),
                    Role = "User", // Mặc định là User bình thường
                    CreatedAt = DateTime.Now
                };

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
                new Claim("UserID", user.UserID.ToString())
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

            if (remoteError != null)
            {
                ViewBag.Error = $"Lỗi từ nhà cung cấp: {remoteError}";
                return View("Login", new AuthVM());
            }

            // Lấy thông tin xác thực từ cookie tạm thời của bên thứ 3
            var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!info.Succeeded) return RedirectToAction("Login");

            // Trích xuất các Claims (Email, Name)
            var claims = info.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Không thể lấy Email từ tài khoản mạng xã hội.";
                return View("Login", new AuthVM());
            }

            // 1. Kiểm tra xem User này đã tồn tại trong DB chưa
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // 2. Nếu chưa có tài khoản -> Tự động tạo mới (Đăng ký tự động)
                user = new User
                {
                    FullName = name ?? "Social User",
                    Email = email,
                    PasswordHash = "EXTERNAL_LOGIN", // Đánh dấu không dùng mật khẩu thường
                    Role = "User",
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // 3. Thiết lập Claims cho hệ thống của bạn
            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("UserID", user.UserID.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 4. Đăng nhập người dùng vào hệ thống của bạn
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            return LocalRedirect(returnUrl);
        }
    }
}
