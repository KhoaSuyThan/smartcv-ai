using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class ResumeController : BaseController
    {
        public IActionResult Index()// Trả về trang Resume
        {
            return View();
        }
    }
}