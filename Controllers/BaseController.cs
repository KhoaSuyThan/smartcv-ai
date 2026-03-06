using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DoAnLTWeb.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = HttpContext.Session.GetString("User"); // Kiểm tra nếu người dùng chưa đăng nhập

            if (user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null); // Chuyển hướng đến trang đăng nhập nếu người dùng chưa đăng nhập
            }

            base.OnActionExecuting(context);
        }
    }
}