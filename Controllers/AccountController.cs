using Microsoft.AspNetCore.Mvc;
using DoAnLTWeb.Models;

namespace DoAnLTWeb.Controllers
{
    public class AccountController : Controller
    {
        // Trang Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Trả về trang Login
        }

        // Xử lý khi bấm Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {

            // Lưu trạng thái đăng nhập
            HttpContext.Session.SetString("User", email);
            // TEST đăng nhập (sau này sẽ check database)

            if (email == "admin@gmail.com" && password == "123456")
            {
                // Đăng nhập thành công → vào Dashboard
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.Error = "Sai email hoặc mật khẩu";
            return View();
        }


        // Trang Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Xử lý khi bấm nút Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model) // Nhận dữ liệu từ form đăng ký
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.Message = "Đăng ký thành công (Test)";
            return View();
        }

        public IActionResult Logout() // Xử lý khi bấm Logout
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}