using Microsoft.AspNetCore.Mvc;

namespace DoAnCS.Controllers;

public class BaseController : Controller
{
    protected int CurrentUserId => int.TryParse(User.FindFirst("UserID")?.Value, out var id) ? id : 0;

    protected string CurrentRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

    // Sửa lại dòng này cho chắc chắn
    protected int? CurrentCompanyId 
    {
        get 
        {
            var val = User.FindFirst("CompanyID")?.Value;
            if (!string.IsNullOrEmpty(val) && int.TryParse(val, out var id) && id > 0)
            {
                return id;
            }
            return null;
        }
    }
}