using Microsoft.AspNetCore.Mvc;

namespace DoAnCS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Jobs()
        {
            return View(); // Trang giới thiệu việc làm
        }
    }
}