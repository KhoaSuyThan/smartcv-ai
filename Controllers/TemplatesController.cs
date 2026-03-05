using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class TemplatesController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}