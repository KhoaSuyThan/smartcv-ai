using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class ExperienceController : BaseController
    {
        public IActionResult Index() // Trả về trang Experience
        {
            return View();
        }
    }
}