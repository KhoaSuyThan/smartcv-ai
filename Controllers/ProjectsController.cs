using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class ProjectsController : BaseController
    {
        public IActionResult Index() // Trả về trang Projects
        {
            return View();
        }
    }
}