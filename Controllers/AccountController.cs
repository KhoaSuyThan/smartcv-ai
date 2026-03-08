using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.EntityFrameworkCore;

public class AccountController : Controller {
    private readonly AppDbContext _context;
    public AccountController(AppDbContext context) => _context = context;

    [HttpGet] public IActionResult Register() => View();

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (ModelState.IsValid)
        {
            // 1. Kiểm tra xem Email đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                return View(model);
            }

            // 2. Tạo đối tượng User mới từ dữ liệu người dùng nhập
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                // Băm mật khẩu bằng BCrypt để lưu vào cột PasswordHash
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "User", // Mặc định là User bình thường
                CreatedAt = DateTime.Now
            };

            // 3. Lưu vào SQL Server
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Đăng ký xong thì chuyển sang trang Đăng nhập
            return RedirectToAction("Login");
        }
        return View(model);
    }

    [HttpGet] public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model) {
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        
        if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash)) {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserID", user.UserID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Email hoặc mật khẩu không chính xác";
        return View(model);
    }

    public async Task<IActionResult> Logout() {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}