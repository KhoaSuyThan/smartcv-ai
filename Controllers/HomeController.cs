using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DoAnLTWeb.Models;

namespace DoAnLTWeb.Controllers;

public class HomeController : Controller
    {
        public IActionResult Index() // Trả về trang Home
        {
            return View();
        }

        public IActionResult Dashboard() // Trả về trang Dashboard
        {
            if (HttpContext.Session.GetString("User") == null) 
        {
            return RedirectToAction("Login", "Account");// Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }
        return View();
        }
    }

