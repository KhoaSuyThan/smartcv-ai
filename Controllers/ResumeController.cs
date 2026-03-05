using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class ResumeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}