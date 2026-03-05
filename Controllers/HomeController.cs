using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DoAnLTWeb.Models;

namespace DoAnLTWeb.Controllers;

public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("User") == null)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
        }
    }

