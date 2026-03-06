using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class SkillsController : BaseController
    {
        public IActionResult Index() // Trả về trang Skills
        {
            return View();
        }
    }
}