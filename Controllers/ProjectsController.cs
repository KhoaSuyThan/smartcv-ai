using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class ProjectsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}