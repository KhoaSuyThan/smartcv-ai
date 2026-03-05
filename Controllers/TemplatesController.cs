using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class TemplatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}