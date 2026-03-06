using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class EducationController : BaseController
    {
        public IActionResult Index() // Trả về trang Education
        {
            return View();
        }
    }
}