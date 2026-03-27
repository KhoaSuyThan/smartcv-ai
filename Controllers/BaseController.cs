using Microsoft.AspNetCore.Mvc;

namespace DoAnCS.Controllers;

public class BaseController : Controller
{
    // Lấy UserID từ Claims
    protected int CurrentUserId => int.Parse(User.FindFirst("UserID")?.Value ?? "0");

    // Lấy Role từ Claims
    protected string CurrentRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

    // Lấy CompanyID từ Claims (dành cho Nhà tuyển dụng)
    protected int? CurrentCompanyId => User.FindFirst("CompanyID")?.Value != null 
        ? int.Parse(User.FindFirst("CompanyID").Value) 
        : null;
}
