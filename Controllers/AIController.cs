using Microsoft.AspNetCore.Mvc;

namespace DoAnLTWeb.Controllers
{
    public class AIController : BaseController
    {
        public IActionResult Index() //Gọi view Index.cshtml trong Views/AI
        {
            return View();
        }
    }
}