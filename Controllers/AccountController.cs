using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Models.ViewModels;
using DoAnCS.Services;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace DoAnCS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;
        private readonly PayOSClient _payOS;
        private readonly IConfiguration _config;

        public AccountController(IWebHostEnvironment webHostEnvironment, AppDbContext context, IEmailService emailService, PayOSClient payOS, IConfiguration config) {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _emailService = emailService;
            _payOS = payOS;
            _config = config;
        }
        // ==========================================
        // ĐĂNG KÝ (REGISTER)
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Companies = _context.Companies.ToList();
            return View("Login", new AuthVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
        public async Task<IActionResult> Register(AuthVM model)
        {
            ModelState.Remove("Login.Email");
            ModelState.Remove("Login.Password");
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra xem Email đã tồn tại chưa
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Register.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Register.Email", "Email này đã được đăng ký.");
                    return View("Login", model);
                }

                // KIỂM TRA BẢO MẬT: Chống leo thang đặc quyền (Mass Assignment). Chỉ cho phép đăng ký User hoặc Recruiter.
                string requestedRole = !string.IsNullOrEmpty(model.Register.Role) ? model.Register.Role : "User";
                if (requestedRole != "Recruiter") 
                {
                    requestedRole = "User";
                }
                model.Register.Role = requestedRole;

                // Sinh mã OTP 6 số bằng Cryptographic RNG (không thể dự đoán, an toàn hơn Random)
                string otp = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();
                
                // Lưu thông tin đăng ký tạm thời vào Session dưới dạng JSON
                HttpContext.Session.SetString("RegisterEmail", model.Register.Email);
                HttpContext.Session.SetString("RegisterOTP", otp);
                HttpContext.Session.SetString("RegisterOTPExpires", DateTime.Now.AddMinutes(3).ToString("o")); // Định dạng ISO 8601
                HttpContext.Session.SetString("PendingRegister", System.Text.Json.JsonSerializer.Serialize(model.Register));

                // Gửi Email chứa mã OTP xác nhận tài khoản chạy ngầm để tránh nghẽn luồng xử lý
                var scheme = Request.Scheme;
                var host = Request.Host;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string subject = "[CVBuilder Pro] Mã xác thực đăng ký tài khoản";
                        string body = $@"
                            <div style='font-family: &quot;Segoe UI&quot;, Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>
                                <div style='background: linear-gradient(135deg, #0d6efd, #6610f2); padding: 30px 20px; text-align: center; color: white;'>
                                    <h2 style='margin: 0; font-size: 24px; font-weight: 700;'>Xác thực đăng ký tài khoản 🎉</h2>
                                    <p style='margin: 8px 0 0 0; opacity: 0.9; font-size: 15px;'>Cảm ơn bạn đã lựa chọn CVBuilder Pro</p>
                                </div>
                                <div style='padding: 24px; color: #334155; line-height: 1.6; font-size: 15px;'>
                                    <p>Xin chào <strong>{model.Register.FullName}</strong>,</p>
                                    <p>Bạn đang thực hiện đăng ký tài khoản trên hệ thống <strong>CVBuilder Pro</strong>. Vui lòng sử dụng mã OTP dưới đây để xác thực địa chỉ email:</p>
                                    
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #0d6efd; background: #f8fafc; padding: 15px 30px; border-radius: 8px; border: 1px dashed #0d6efd; display: inline-block;'>{otp}</span>
                                    </div>
                                    
                                    <p style='color: #ef4444; font-size: 14px; font-weight: 600;'>Lưu ý: Mã OTP này chỉ có hiệu lực trong vòng 3 phút.</p>
                                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                                </div>
                                <div style='background: #f1f5f9; padding: 20px; text-align: center; color: #64748b; font-size: 12.5px; border-top: 1px solid #e2e8f0;'>
                                    <p style='margin: 0;'>Đây là email tự động từ hệ thống CVBuilder Pro. Vui lòng không trả lời email này.</p>
                                    <p style='margin: 4px 0 0 0;'>&copy; {DateTime.Now.Year} CVBuilder Pro. All rights reserved.</p>
                                </div>
                            </div>";

                        await _emailService.SendEmailAsync(model.Register.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi gửi email OTP đăng ký: " + ex.Message);
                    }
                });

                return RedirectToAction("VerifyRegisterOTP");
            }
            return View("Login", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyRegisterOTP()
        {
            var email = HttpContext.Session.GetString("RegisterEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");
            
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyRegisterOTP(string otp)
        {
            var email = HttpContext.Session.GetString("RegisterEmail");
            var sessionOtp = HttpContext.Session.GetString("RegisterOTP");
            var expiresStr = HttpContext.Session.GetString("RegisterOTPExpires");
            var pendingJson = HttpContext.Session.GetString("PendingRegister");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(expiresStr) || string.IsNullOrEmpty(pendingJson))
            {
                return RedirectToAction("Register");
            }

            ViewBag.Email = email;

            if (!DateTime.TryParse(expiresStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expires) || expires < DateTime.Now)
            {
                ViewBag.Error = "Mã OTP đã hết hạn. Vui lòng quay lại trang đăng ký để nhận mã mới.";
                return View();
            }

            if (sessionOtp != otp)
            {
                ViewBag.Error = "Mã OTP không chính xác. Vui lòng kiểm tra lại.";
                return View();
            }

            // OTP đúng, tiến hành tạo tài khoản chính thức vào Database
            try
            {
                var registerModel = System.Text.Json.JsonSerializer.Deserialize<RegisterVM>(pendingJson);
                if (registerModel == null)
                {
                    ViewBag.Error = "Lỗi xử lý dữ liệu đăng ký. Vui lòng đăng ký lại.";
                    return View();
                }

                // Check trùng email lần cuối
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerModel.Email);
                if (existingUser != null)
                {
                    ViewBag.Error = "Email này đã được đăng ký bởi người dùng khác.";
                    return View();
                }

                var user = new User
                {
                    FullName = registerModel.FullName,
                    Email = registerModel.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerModel.Password),
                    Role = registerModel.Role,
                    CreatedAt = DateTime.Now
                };

                if (user.Role == "Recruiter")
                {
                    if (string.IsNullOrEmpty(registerModel.CompanyName) || string.IsNullOrEmpty(registerModel.TaxCode))
                    {
                        ViewBag.Error = "Thông tin công ty hoặc mã số thuế không hợp lệ.";
                        return View();
                    }

                    string inputCompanyName = registerModel.CompanyName.Trim();
                    var company = await _context.Companies
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == inputCompanyName.ToLower());

                    if (company == null)
                    {
                        company = new Company
                        {
                            Name = inputCompanyName,
                            TaxCode = registerModel.TaxCode,
                            CreatedAt = DateTime.Now
                        };
                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync();
                    }
                    user.CompanyID = company.CompanyID;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Gửi thư chào mừng đăng ký thành công (chạy ngầm)
                var scheme = Request.Scheme;
                var host = Request.Host;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string roleName = user.Role == "Recruiter" ? "Nhà tuyển dụng" : "Ứng viên";
                        string subject = "[CVBuilder Pro] Chúc mừng đăng ký tài khoản thành công!";
                        string body = $@"
                            <div style='font-family: &quot;Segoe UI&quot;, Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>
                                <div style='background: linear-gradient(135deg, #0d6efd, #6610f2); padding: 30px 20px; text-align: center; color: white;'>
                                    <h2 style='margin: 0; font-size: 24px; font-weight: 700;'>Chào mừng gia nhập CVBuilder Pro! 🎉</h2>
                                    <p style='margin: 8px 0 0 0; opacity: 0.9; font-size: 15px;'>Tài khoản của bạn đã được kích hoạt thành công</p>
                                </div>
                                <div style='padding: 24px; color: #334155; line-height: 1.6; font-size: 15px;'>
                                    <p>Xin chào <strong>{user.FullName}</strong>,</p>
                                    <p>Cảm ơn bạn đã xác thực và lựa chọn hệ thống <strong>CVBuilder Pro</strong>. Tài khoản của bạn đã được đăng ký thành công với các thông tin chi tiết sau:</p>
                                    
                                    <div style='background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 16px; margin: 18px 0;'>
                                        <div style='margin-bottom: 8px;'><strong>Email đăng nhập:</strong> <span style='color: #0d6efd;'>{user.Email}</span></div>
                                        <div><strong>Vai trò tài khoản:</strong> <span style='color: #0d6efd; font-weight: bold;'>{roleName}</span></div>
                                    </div>
                                    
                                    <div style='background: #f0fdf4; border-left: 4px solid #16a34a; padding: 14px 16px; margin: 20px 0; border-radius: 4px;'>
                                        <h4 style='margin: 0 0 8px 0; color: #14532d; font-size: 15px;'>💡 Hướng dẫn bắt đầu nhanh:</h4>
                                        <span style='font-size: 14.5px; color: #166534;'>
                                            {(user.Role == "Recruiter" 
                                                ? "• Truy cập mục quản lý công ty để cập nhật hồ sơ doanh nghiệp.<br/>• Tạo và đăng tin bài tuyển dụng miễn phí để tiếp cận ứng viên.<br/>• Sử dụng công cụ Talent Search/Smart Match (AI) để tìm và khớp hồ sơ nhân tài."
                                                : "• Truy cập thư viện mẫu CV để lựa chọn hơn 30+ thiết kế miễn phí.<br/>• Sử dụng công cụ kéo thả để cá nhân hóa CV của bạn.<br/>• Nộp hồ sơ trực tuyến tới hàng trăm vị trí tuyển dụng trên hệ thống.")}
                                        </span>
                                    </div>
                                    
                                    <div style='text-align: center; margin-top: 30px; margin-bottom: 10px;'>
                                        <a href='{scheme}://{host}/Account/Login' style='background: #0d6efd; color: white; padding: 12px 30px; text-decoration: none; border-radius: 30px; font-weight: bold; display: inline-block; box-shadow: 0 4px 10px rgba(13, 110, 253, 0.25); transition: 0.2s;'>Đăng nhập hệ thống ngay</a>
                                    </div>
                                </div>
                                <div style='background: #f1f5f9; padding: 20px; text-align: center; color: #64748b; font-size: 12.5px; border-top: 1px solid #e2e8f0;'>
                                    <p style='margin: 0;'>Đây là email tự động từ hệ thống CVBuilder Pro. Vui lòng không trả lời email này.</p>
                                    <p style='margin: 4px 0 0 0;'>&copy; {DateTime.Now.Year} CVBuilder Pro. All rights reserved.</p>
                                </div>
                            </div>";

                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi gửi email chào mừng: " + ex.Message);
                    }
                });

                // Xóa sạch session đăng ký tạm thời
                HttpContext.Session.Remove("RegisterEmail");
                HttpContext.Session.Remove("RegisterOTP");
                HttpContext.Session.Remove("RegisterOTPExpires");
                HttpContext.Session.Remove("PendingRegister");

                // Thêm thông báo thành công cho màn hình đăng nhập
                TempData["SuccessMessage"] = "Đăng ký tài khoản và xác thực email thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi VerifyRegisterOTP: " + ex.Message);
                ViewBag.Error = "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.";
                return View();
            }
        }

        // ==========================================
        // ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Companies = _context.Companies.ToList();
            return View(new AuthVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthVM model)
        {
            ModelState.Remove("Register.FullName");
            ModelState.Remove("Register.Email");
            ModelState.Remove("Register.Password");
            ModelState.Remove("Register.ConfirmPassword");

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Login.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Login.Password, user.PasswordHash))
                {
                    long currentLoginTime = DateTime.UtcNow.Ticks;
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim("CompanyID", user.CompanyID.ToString()?? ""),
                new Claim("SessionId", Guid.NewGuid().ToString()),
                new Claim("LoginTime", currentLoginTime.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // ĐĂNG KÝ PHIÊN ĐĂNG NHẬP MỚI NHẤT VÀO HỆ THỐNG
                    DoAnCS.Services.SessionTracker.UpdateSession(user.UserID, currentLoginTime);

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Email hoặc mật khẩu không chính xác";
            }
            return View(model);
        }

        // ==========================================
        // ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // ĐĂNG NHẬP MẠNG XÃ HỘI (EXTERNAL LOGIN)
        // ==========================================

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Yêu cầu chuyển hướng đến trang đăng nhập của bên thứ 3 (Google, Facebook...)
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // LƯU Ý: Khi dùng External Login, ta dùng Scheme "External" (hoặc Identity.External)
            // để lấy thông tin tạm thời từ Google/GitHub trước khi chuyển sang Cookie chính thức
            var result = await HttpContext.AuthenticateAsync("ExternalCookies"); 
            
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Tự động tạo tài khoản nếu chưa có
                user = new User
                {
                    FullName = name ?? "Social User",
                    Email = email,
                    PasswordHash = "EXTERNAL_LOGIN_" + Guid.NewGuid().ToString(),
                    Role = "User", // Mặc định tài khoản MXH là Candidate
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // THIẾT LẬP COOKIE CHÍNH THỨC CỦA HỆ THỐNG
            long currentLoginTime = DateTime.UtcNow.Ticks;
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng: Gán quyền từ DB
                new Claim("UserID", user.UserID.ToString()),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim("CompanyID", user.CompanyID?.ToString() ?? ""),
                new Claim("SessionId", Guid.NewGuid().ToString()),
                new Claim("LoginTime", currentLoginTime.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Xóa cookie tạm thời sau khi đã đăng nhập thành công
            await HttpContext.SignOutAsync("ExternalCookies");

            // ĐĂNG KÝ PHIÊN ĐĂNG NHẬP MỚI NHẤT VÀO HỆ THỐNG
            DoAnCS.Services.SessionTracker.UpdateSession(user.UserID, currentLoginTime);

            return LocalRedirect(returnUrl);
        }

        // ==========================================
        // HỒ SƠ CÁ NHÂN (PROFILE)
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users
                             .Include(u => u.Company)
                             .Include(u => u.Resumes)
                             .FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            // Nếu là nhà tuyển dụng, lấy thêm số lượng tin tuyển dụng của công ty
            if (user.Role == "Recruiter" && user.CompanyID != null)
            {
                ViewBag.JobCount = await _context.Jobs.CountAsync(j => j.CompanyID == user.CompanyID);
            }
            // Nếu là admin, lấy tổng số lượng người dùng trong hệ thống
            else if (user.Role == "Admin")
            {
                ViewBag.TotalUsers = await _context.Users.CountAsync();
            }

            return View(user);
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Applications()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var applications = await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Resume)
                .Where(a => a.Resume.UserID == userId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind("FullName,Phone,Summary,ProfessionalTitle,PortfolioLinks,YearsOfExperience,Skills,Address,ExpectedLocation,ExpectedSalary")] User model, IFormFile? avatarFile, bool isDeleteAvatar = false, string? companyName = null, string? companyAddress = null, string? taxCode = null)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                // --- XỬ LÝ UPLOAD ẢNH ĐẠI DIỆN ---
                // TRƯỜNG HỢP 1: NGƯỜI DÙNG NHẤN XÓA ẢNH
                if (isDeleteAvatar)
                {
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        // Xóa file vật lý trong wwwroot/avt
                        var uploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                        string oldFilePath = Path.Combine(uploadsFolder, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        
                        user.AvatarUrl = null; // Reset về null
                    }
                }
                // TRƯỜNG HỢP 2: NGƯỜI DÙNG TẢI ẢNH MỚI
                else if (avatarFile != null && avatarFile.Length > 0)
                {
                    string folder = "avt/";
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    var uploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                    string serverFolder = Path.Combine(uploadsFolder, folder);

                    if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                    // Xóa ảnh cũ trước khi thay ảnh mới
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        var uploadsFolder2 = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                        string oldPath = Path.Combine(uploadsFolder2, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string filePath = Path.Combine(serverFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(fileStream);
                    }
                    user.AvatarUrl = "/" + folder + fileName;
                }

                // Cập nhật các thông tin khác
                user.FullName = model.FullName;
                user.Phone = model.Phone;
                if (user.Role != "Admin")
                {
                    user.Summary = model.Summary;
                    user.ProfessionalTitle = model.ProfessionalTitle;
                    user.PortfolioLinks = model.PortfolioLinks;
                    user.YearsOfExperience = model.YearsOfExperience;
                    user.Skills = model.Skills;
                    user.Address = model.Address;
                    user.ExpectedLocation = model.ExpectedLocation;
                    user.ExpectedSalary = model.ExpectedSalary;
                }

                // --- XỬ LÝ THÔNG TIN CÔNG TY CHO NHÀ TUYỂN DỤNG ---
                if (user.Role == "Recruiter" && (!string.IsNullOrEmpty(companyName) || !string.IsNullOrEmpty(companyAddress) || !string.IsNullOrEmpty(taxCode)))
                {
                    if (user.Company != null)
                    {
                        // Cập nhật công ty đã có
                        if (!string.IsNullOrEmpty(companyName)) user.Company.Name = companyName;
                        if (companyAddress != null) user.Company.Address = companyAddress;
                        if (taxCode != null) user.Company.TaxCode = taxCode;
                    }
                    else
                    {
                        string inputCompName = (companyName ?? "Chưa cập nhật").Trim();
                        var existingComp = await _context.Companies.FirstOrDefaultAsync(c => c.Name.ToLower() == inputCompName.ToLower());
                        
                        if (existingComp != null)
                        {
                            user.CompanyID = existingComp.CompanyID;
                        }
                        else
                        {
                            // Tạo mới công ty nếu chưa có
                            var newCompany = new Company
                            {
                                Name = inputCompName,
                                Address = companyAddress,
                                TaxCode = taxCode,
                                CreatedAt = DateTime.Now
                            };
                            _context.Companies.Add(newCompany);
                            await _context.SaveChangesAsync();
                            user.CompanyID = newCompany.CompanyID;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Cập nhật lại Claims để Header/Sidebar hiện tên mới ngay lập tức
                var existingSessionId = User.FindFirst("SessionId")?.Value ?? Guid.NewGuid().ToString();
                var existingLoginTime = User.FindFirst("LoginTime")?.Value ?? DateTime.UtcNow.Ticks.ToString();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserID", user.UserID.ToString()),
                    new Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png"), // Thêm cả Claim ảnh cho xịn
                    new Claim("IsPro", user.IsPro.ToString()),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("CompanyID", user.CompanyID?.ToString() ?? ""),
                    new Claim("SessionId", existingSessionId),
                    new Claim("LoginTime", existingLoginTime)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties { IsPersistent = true });

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // ==========================================
        // ĐỔI MẬT KHẨU (CHANGE PASSWORD)
        // ==========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) 
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["PasswordErrorMessage"] = "Vui lòng điền đầy đủ các trường.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$"))
            {
                TempData["PasswordErrorMessage"] = "Mật khẩu mới phải từ 8 ký tự trở lên, chứa ít nhất 1 chữ cái, 1 chữ số và 1 ký tự đặc biệt.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            if (newPassword != confirmPassword)
            {
                TempData["PasswordErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            // Ignore external login users or users without password set like this
            if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash.StartsWith("EXTERNAL_LOGIN_"))
            {
                TempData["PasswordErrorMessage"] = "Tài khoản đăng nhập bằng mạng xã hội không thể đổi mật khẩu.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                TempData["PasswordErrorMessage"] = "Mật khẩu hiện tại không chính xác.";
                return RedirectToAction("Profile", new { t = "password" });
            }

            // --- THAY ĐỔI LOGIC: Không đổi ngay mà gửi Email xác nhận ---
            
            // 1. Tạo Token và lưu thông tin tạm thời
            string token = Guid.NewGuid().ToString();
            user.PasswordChangeToken = token;
            user.PasswordChangeTokenExpires = DateTime.Now.AddMinutes(15);
            user.PendingPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            // 2. Tạo link xác nhận
            var callbackUrl = Url.Action("ConfirmPasswordChange", "Account", 
                new { token = token }, protocol: Request.Scheme);

            // 3. Gửi Email
            string subject = "Xác nhận thay đổi mật khẩu - CVBuilder";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #007bff; text-align: center;'>Xác nhận đổi mật khẩu</h2>
                    <p>Chào <strong>{user.FullName}</strong>,</p>
                    <p>Chúng tôi nhận được yêu cầu thay đổi mật khẩu cho tài khoản của bạn. Vui lòng nhấn vào nút bên dưới để xác nhận thay đổi này:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{callbackUrl}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Xác nhận đổi mật khẩu</a>
                    </div>
                    <p style='color: #ff0000; font-size: 0.9em;'>Lưu ý: Liên kết này sẽ hết hạn trong vòng 15 phút.</p>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này hoặc liên hệ với bộ phận hỗ trợ để bảo mật tài khoản.</p>
                    <hr style='border: 0; border-top: 1px solid #eeeeee;'>
                    <p style='font-size: 0.8em; color: #777;'>Đây là email tự động, vui lòng không phản hồi.</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, message);
                TempData["PasswordSuccessMessage"] = "Một liên kết xác nhận đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SendEmail Password: " + ex.Message);
                TempData["PasswordErrorMessage"] = "Lỗi hệ thống, hãy liên hệ admin để giải quyết";
            }

            return RedirectToAction("Profile", new { t = "password" });
        }

        // ==========================================
        // XÁC NHẬN ĐỔI MẬT KHẨU (CONFIRM CHANGE PASSWORD)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ConfirmPasswordChange(string token)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordChangeToken == token);

            if (user == null || user.PasswordChangeTokenExpires < DateTime.Now)
            {
                ViewBag.Error = "Liên kết xác nhận không hợp lệ hoặc đã hết hạn.";
                return View();
            }

            // Thực hiện đổi mật khẩu chính thức
            user.PasswordHash = user.PendingPasswordHash;
            
            // Xóa thông tin tạm
            user.PasswordChangeToken = null;
            user.PasswordChangeTokenExpires = null;
            user.PendingPasswordHash = null;

            await _context.SaveChangesAsync();

            // GỬI EMAIL THÔNG BÁO THÀNH CÔNG
            string subject = "Thông báo: Thay đổi mật khẩu thành công";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #28a745; text-align: center;'>Mật khẩu đã được thay đổi</h2>
                    <p>Chào <strong>{user.FullName}</strong>,</p>
                    <p>Mật khẩu tài khoản của bạn đã được thay đổi thành công vào lúc {DateTime.Now.ToString("HH:mm dd/MM/yyyy")}.</p>
                    <p>Nếu bạn không thực hiện thay đổi này, hãy liên hệ ngay với chúng tôi để bảo mật tài khoản.</p>
                    <hr style='border: 0; border-top: 1px solid #eeeeee;'>
                    <p style='font-size: 0.8em; color: #777;'>Hệ thống CVBuilder chân trọng thông báo.</p>
                </div>";
            
            await _emailService.SendEmailAsync(user.Email, subject, message);

            ViewBag.Success = "Đổi mật khẩu thành công! Bây giờ bạn có thể đăng nhập bằng mật khẩu mới.";
            return View();
        }

        // ==========================================
        // NÂNG CẤP LÊN NHÀ TUYỂN DỤNG (UPGRADE TO EMPLOYER)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeToEmployer(string companyName, string companyAddress, string taxCode)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) 
                return RedirectToAction("Login");

            if (string.IsNullOrEmpty(companyName))
            {
                TempData["EmployerErrorMessage"] = "Vui lòng nhập tên công ty.";
                return RedirectToAction("Profile", new { t = "employer" });
            }
            if (string.IsNullOrEmpty(taxCode))
            {
                TempData["EmployerErrorMessage"] = "Vui lòng nhập mã số thuế công ty.";
                return RedirectToAction("Profile", new { t = "employer" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            if (user.Role == "Recruiter" || user.Role == "Admin")
            {
                return RedirectToAction("Profile");
            }

            // Tìm xem công ty đã có chưa
            string inputCompName = companyName.Trim();
            var existingComp = await _context.Companies.FirstOrDefaultAsync(c => c.Name.ToLower() == inputCompName.ToLower());
            
            int compId;
            if (existingComp != null)
            {
                compId = existingComp.CompanyID;
            }
            else
            {
                // Tạo mới công ty
                var newCompany = new Company
                {
                    Name = inputCompName,
                    Address = string.IsNullOrEmpty(companyAddress) ? null : companyAddress,
                    TaxCode = string.IsNullOrEmpty(taxCode) ? null : taxCode,
                    CreatedAt = DateTime.Now
                };
                
                _context.Companies.Add(newCompany);
                await _context.SaveChangesAsync();
                compId = newCompany.CompanyID;
            }

            // Cập nhật User
            user.Role = "Recruiter";
            user.CompanyID = compId;
            // Hủy trạng thái Candidate Pro khi nâng cấp lên Recruiter
            user.IsPro = false;
            user.ProExpirationDate = null;
            
            await _context.SaveChangesAsync();

            // Cập nhật lại Claims để phiên đăng nhập nhận Role mới ngay lập tức
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png"),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng: đã thành Recruiter
                new Claim("CompanyID", user.CompanyID?.ToString() ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            TempData["SuccessMessage"] = "Chúc mừng! Bạn đã trở thành Nhà Tuyển Dụng. Hãy bắt đầu đăng tin tuyển dụng nhé.";
            return RedirectToAction("Profile", new { t = "info" });
        }

        // ==========================================
        // HỦY TƯ CÁCH NHÀ TUYỂN DỤNG (CANCEL EMPLOYER ROLE)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEmployerRole()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) 
                return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            if (user.Role != "Recruiter")
            {
                return RedirectToAction("Profile");
            }

            // Gỡ thông tin công ty khỏi người dùng
            user.Role = "User";
            user.CompanyID = null;
            
            await _context.SaveChangesAsync();

            // Cập nhật lại Claims để phiên đăng nhập nhận Role mới ngay lập tức
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png"),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng: đã trở về User
                new Claim("CompanyID", "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            TempData["SuccessMessage"] = "Bạn đã hủy tư cách Nhà tuyển dụng và trở về vai trò Ứng viên thành công.";
            return RedirectToAction("Profile", new { t = "info" });
        }

        // ==========================================
        // NÂNG CẤP TÀI KHOẢN (UPGRADE)
        // ==========================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Upgrade()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return NotFound();

            // Nếu đã là Pro thì chuyển về Profile kèm thông báo
            if (user.IsPro)
            {
                TempData["InfoMessage"] = "Bạn hiện đang là thành viên Pro!";
                return RedirectToAction("Profile");
            }

            // Kiểm tra xem đã có yêu cầu nào đang chờ duyệt không
            var existingRequest = await _context.UpgradeRequests
                .OrderByDescending(r => r.RequestDate)
                .FirstOrDefaultAsync(r => r.UserID == userId && r.Status == 0);
            
            ViewBag.PendingRequest = existingRequest;

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayOSPayment(string packageType = "CandidatePro")
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) return RedirectToAction("Login");

            var existingRequest = await _context.UpgradeRequests.FirstOrDefaultAsync(r => r.UserID == userId && r.Status == 0);
            if (existingRequest != null)
            {
                _context.UpgradeRequests.Remove(existingRequest); // Xóa yêu cầu cũ đang treo
            }

            // Tạo yêu cầu mới (Status = 0: Đang chờ thanh toán)
            var request = new UpgradeRequest
            {
                UserID = userId,
                RequestDate = DateTime.Now,
                Status = 0,
                Notes = $"Thanh toán PayOS - {packageType}"
            };

            _context.UpgradeRequests.Add(request);
            await _context.SaveChangesAsync();

            // Tạo OrderCode bằng số (kiểu long) < 9007199254740991
            long orderCode = long.Parse(DateTime.Now.ToString("yyMMddHHmmss") + (request.Id % 100).ToString("D2"));

            // Lưu orderCode vào Notes để khi webhook gọi về còn biết id của bảng UpgradeRequests
            request.Notes = $"Thanh toán PayOS - {packageType} - OrderCode: {orderCode}";
            await _context.SaveChangesAsync();

            try
            {
                int amount = packageType == "RecruiterPro" ? 100000 : 20000;
                string description = packageType == "RecruiterPro" ? $"Recruiter Pro {userId}" : $"CVBuilder Pro {userId}";

                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string returnUrl = $"{baseUrl}/Account/PaymentCallback";
                string cancelUrl = $"{baseUrl}/Account/PaymentCallback?cancel=true";

                var requestData = new CreatePaymentLinkRequest {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = description,
                    CancelUrl = cancelUrl,
                    ReturnUrl = returnUrl
                };

                CreatePaymentLinkResponse createPayment = await _payOS.PaymentRequests.CreateAsync(requestData);
                
                return Json(new { success = true, checkoutUrl = createPayment.CheckoutUrl, orderCode = orderCode });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi CreatePayOSPayment: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống khi tạo thanh toán, hãy thử lại." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback(string code, string status, bool cancel, long orderCode)
        {
            if (code == "00" && status == "PAID" && !cancel)
            {
                // CƠ CHẾ DỰ PHÒNG: Tự động cập nhật trạng thái Pro ngay tại Callback
                // Rất cần thiết cho môi trường Localhost vì Webhook của PayOS không thể gọi về localhost.
                var upgradeRequest = await _context.UpgradeRequests
                    .FirstOrDefaultAsync(u => u.Notes.Contains($"OrderCode: {orderCode}") && u.Status == 0);

                if (upgradeRequest != null)
                {
                    upgradeRequest.Status = 1; // Đã thanh toán
                    upgradeRequest.DecisionDate = DateTime.Now;

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == upgradeRequest.UserID);
                    if (user != null)
                    {
                        user.IsPro = true;
                        user.ProExpirationDate = DateTime.Now.AddMonths(1);

                        // Cập nhật lại Cookie ngay lập tức để Navbar ẩn nút Nâng cấp
                        if (User.Identity != null && User.Identity.IsAuthenticated && User.FindFirst("UserID")?.Value == user.UserID.ToString())
                        {
                            var identity = (System.Security.Claims.ClaimsIdentity)User.Identity;
                            var oldClaim = identity.FindFirst("IsPro");
                            if (oldClaim != null)
                            {
                                identity.RemoveClaim(oldClaim);
                            }
                            identity.AddClaim(new System.Security.Claims.Claim("IsPro", "True"));

                            await HttpContext.SignInAsync(
                                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                                new System.Security.Claims.ClaimsPrincipal(identity),
                                new Microsoft.AspNetCore.Authentication.AuthenticationProperties { IsPersistent = true }
                            );
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                ViewBag.SuccessMessage = "Thanh toán thành công! Gói Pro của bạn đã được kích hoạt ngay lập tức.";
            }
            else
            {
                ViewBag.ErrorMessage = $"Thanh toán thất bại hoặc bị hủy.";
            }
            return View("PaymentResult");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] Webhook request)
        {
            try
            {
                WebhookData webhookData = await _payOS.Webhooks.VerifyAsync(request);
                
                if (webhookData.Code == "00") // Thanh toán thành công
                {
                    // Lấy OrderCode từ webhookData
                    long orderCode = webhookData.OrderCode;

                    // Tìm UpgradeRequest tương ứng dựa vào orderCode đã lưu trong Notes
                    var upgradeRequest = await _context.UpgradeRequests
                        .FirstOrDefaultAsync(u => u.Notes.Contains($"OrderCode: {orderCode}") && u.Status == 0);

                    if (upgradeRequest != null)
                    {
                        upgradeRequest.Status = 1; // Đã thanh toán
                        upgradeRequest.DecisionDate = DateTime.Now;
                        upgradeRequest.TransactionId = webhookData.TransactionDateTime;

                        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == upgradeRequest.UserID);
                        if (user != null)
                        {
                            user.IsPro = true;
                            user.ProExpirationDate = DateTime.Now.AddMonths(1);
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi PayOS Webhook: " + ex.Message);
                return BadRequest("Invalid webhook data");
            }
        }



        // ==========================================
        // QUÊN MẬT KHẨU (FORGOT PASSWORD)
        // ==========================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập Email.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Để bảo mật, không nên cho biết email có tồn tại hay không
                // Nhưng ở đây ta báo lỗi để người dùng biết nhập sai
                ViewBag.Error = "Email này không tồn tại trong hệ thống.";
                return View();
            }

            // Sinh mã OTP 6 số
            string otp = new Random().Next(100000, 999999).ToString();
            user.PasswordResetOTP = otp;
            user.OTPExpires = DateTime.Now.AddMinutes(3); // Hiệu lực 3 phút
            user.OTPFailCount = 0; // Reset số lần sai

            await _context.SaveChangesAsync();

            // Gửi Email
            string subject = "Mã xác thực khôi phục mật khẩu - CVBuilder";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #007bff; text-align: center;'>Mã xác thực OTP</h2>
                    <p>Chào <strong>{user.FullName}</strong>,</p>
                    <p>Bạn đã yêu cầu khôi phục mật khẩu. Vui lòng sử dụng mã OTP dưới đây để xác thực:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 10px; color: #007bff; background: #f8f9fa; padding: 15px 30px; border-radius: 5px; border: 1px dashed #007bff;'>{otp}</span>
                    </div>
                    <p style='color: #ff0000; font-size: 0.9em;'>Lưu ý: Mã này sẽ hết hạn trong vòng 3 phút.</p>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bảo mật tài khoản của mình.</p>
                    <hr style='border: 0; border-top: 1px solid #eeeeee;'>
                    <p style='font-size: 0.8em; color: #777;'>Hệ thống CVBuilder chân trọng thông báo.</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, message);
                // Lưu email vào session để dùng ở bước tiếp theo
                HttpContext.Session.SetString("ResetEmail", email);
                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi ForgotPassword SendEmail: " + ex.Message);
                ViewBag.Error = "Lỗi hệ thống, hãy liên hệ admin để giải quyết";
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOTP()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
            
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(string otp)
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("ForgotPassword");

            if ((user.OTPFailCount ?? 0) >= 5)
            {
                ViewBag.Error = "Bạn đã nhập sai quá 5 lần. Vui lòng yêu cầu mã mới.";
                return View();
            }

            if (user.OTPExpires < DateTime.Now)
            {
                ViewBag.Error = "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.";
                return View();
            }

            if (user.PasswordResetOTP != otp)
            {
                user.OTPFailCount = (user.OTPFailCount ?? 0) + 1;
                await _context.SaveChangesAsync();
                ViewBag.Error = $"Mã OTP không chính xác. Bạn còn {5 - user.OTPFailCount} lần thử.";
                return View();
            }

            // Nếu đúng, đánh dấu là đã xác thực xong
            HttpContext.Session.SetString("OTPVerified", "true");
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            var verified = HttpContext.Session.GetString("OTPVerified");
            if (verified != "true") return RedirectToAction("ForgotPassword");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            var verified = HttpContext.Session.GetString("OTPVerified");
            var email = HttpContext.Session.GetString("ResetEmail");

            if (verified != "true" || string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                // Kiểm tra mật khẩu mới có trùng mật khẩu cũ không
                if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
                {
                    ViewBag.Error = "Mật khẩu mới không được trùng với mật khẩu cũ.";
                    return View();
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                // Xóa OTP
                user.PasswordResetOTP = null;
                user.OTPExpires = null;
                user.OTPFailCount = 0;

                await _context.SaveChangesAsync();

                // Xóa session
                HttpContext.Session.Remove("ResetEmail");
                HttpContext.Session.Remove("OTPVerified");

                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Hãy đăng nhập lại.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("ForgotPassword");
        }

        // ==========================================
        // TRANG BÁO LỖI QUYỀN TRUY CẬP (ACCESS DENIED)
        // ==========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
