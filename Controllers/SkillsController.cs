using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class SkillsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}