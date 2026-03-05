using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class ProfileController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}