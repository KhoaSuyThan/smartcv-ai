using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class TemplatesController : BaseController
    {
        public IActionResult Index()// Trả về trang Templates
        {
            return View();
        }
    }
}