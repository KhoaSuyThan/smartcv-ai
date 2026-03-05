using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class AIController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}