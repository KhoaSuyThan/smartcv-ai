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

                // 2. Tạo đối tượng User mới từ dữ liệu người dùng nhập
                var user = new User
                {
                    FullName = model.Register.FullName,
                    Email = model.Register.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Register.Password),
                    
                    // 1. LẤY ROLE TỪ GIAO DIỆN (Bạn cần thêm 1 dropdown chọn Role ở View)
                    // Nếu không chọn, mặc định là 'User' (Candidate)
                    Role = !string.IsNullOrEmpty(model.Register.Role) ? model.Register.Role : "User",                    
                    CreatedAt = DateTime.Now
                };

                // XỬ LÝ TỰ ĐỘNG TẠO CÔNG TY
                if (user.Role == "Recruiter")
                {
                    if (string.IsNullOrEmpty(model.Register.CompanyName))
                    {
                        ModelState.AddModelError("Register.CompanyName", "Vui lòng nhập tên công ty.");
                        return View("Login", model);
                    }
                    if (string.IsNullOrEmpty(model.Register.TaxCode))
                    {
                        ModelState.AddModelError("Register.TaxCode", "Vui lòng nhập mã số thuế.");
                        return View("Login", model);
                    }

                    // Tìm xem tên công ty đã có trong database chưa
                    string inputCompanyName = model.Register.CompanyName.Trim();
                    var company = await _context.Companies
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == inputCompanyName.ToLower());

                    if (company == null)
                    {
                        // Nếu chưa có thì tạo mới công ty
                        company = new Company { 
                            Name = inputCompanyName,
                            TaxCode = model.Register.TaxCode,
                            CreatedAt = DateTime.Now 
                        };
                        _context.Companies.Add(company);
                        await _context.SaveChangesAsync(); // Lưu để lấy ID tự tăng
                    }
                    
                    // Gán ID công ty cho User
                    user.CompanyID = company.CompanyID;
                }

                // 3. Lưu vào SQL Server
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Đăng ký xong thì chuyển sang trang Đăng nhập
                return RedirectToAction("Login");
            }
            return View("Login", model);
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
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim("CompanyID", user.CompanyID.ToString()?? ""),
                new Claim("SessionId", Guid.NewGuid().ToString()),
                new Claim("LoginTime", DateTime.UtcNow.Ticks.ToString())
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
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role), // Quan trọng: Gán quyền từ DB
                new Claim("UserID", user.UserID.ToString()),
                new Claim("IsPro", user.IsPro.ToString()),
                new Claim("CompanyID", user.CompanyID?.ToString() ?? ""),
                new Claim("SessionId", Guid.NewGuid().ToString()),
                new Claim("LoginTime", DateTime.UtcNow.Ticks.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Xóa cookie tạm thời sau khi đã đăng nhập thành công
            await HttpContext.SignOutAsync("ExternalCookies");

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
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        
                        user.AvatarUrl = null; // Reset về null
                    }
                }
                // TRƯỜNG HỢP 2: NGƯỜI DÙNG TẢI ẢNH MỚI
                else if (avatarFile != null && avatarFile.Length > 0)
                {
                    string folder = "avt/";
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                    if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                    // Xóa ảnh cũ trước khi thay ảnh mới
                    if (!string.IsNullOrEmpty(user.AvatarUrl))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarUrl.TrimStart('/'));
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
                
                return Json(new { success = true, checkoutUrl = createPayment.CheckoutUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi CreatePayOSPayment: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống khi tạo thanh toán, hãy thử lại." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PaymentCallback(string code, string status, bool cancel)
        {
            if (code == "00" && status == "PAID" && !cancel)
            {
                ViewBag.SuccessMessage = "Thanh toán thành công! Gói Pro của bạn sẽ được kích hoạt ngay lập tức.";
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
