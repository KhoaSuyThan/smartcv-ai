using Microsoft.AspNetCore.Authentication;
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
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật chống giả mạo yêu cầu
        public async Task<IActionResult> Register(DoAnCS.Models.ViewModels.RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Email bất đồng bộ để tránh treo hệ thống
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                    return View(model);
                }

                // 2. Tạo User và băm mật khẩu
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "User",
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ==========================================
        // ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpGet] 
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model) 
        {
            if (ModelState.IsValid)
            {
                // SỬA LỖI CS0411: Sử dụng FirstOrDefaultAsync<User> với await
                var user = await _context.Users.FirstOrDefaultAsync<User>(u => u.Email == model.Email);
                
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash)) 
                {
                    // Thiết lập các quyền (Claims) cho người dùng
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserID", user.UserID.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true }; // Ghi nhớ đăng nhập

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
    }
}