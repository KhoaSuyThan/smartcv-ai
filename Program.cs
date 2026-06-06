using DoAnCS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Services;
using Microsoft.AspNetCore.HttpOverrides;
using PayOS;
var builder = WebApplication.CreateBuilder(args);

var onlineConnectionString = builder.Configuration.GetConnectionString("OnlineConnection");
var localConnectionString = builder.Configuration.GetConnectionString("LocalConnection");

string activeConnectionString = !string.IsNullOrEmpty(onlineConnectionString) ? onlineConnectionString : localConnectionString;

Console.WriteLine($"Using Connection String: {activeConnectionString}");
// --- 1. ĐĂNG KÝ SERVICES ---
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddScoped<JobApiService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIService, GeminiService>(); 
builder.Services.AddScoped<IEmailService, EmailService>(); 

// Cấu hình PayOS
var clientId = builder.Configuration["PayOS:ClientId"] ?? throw new Exception("Không tìm thấy PayOS:ClientId");
var apiKey = builder.Configuration["PayOS:ApiKey"] ?? throw new Exception("Không tìm thấy PayOS:ApiKey");
var checksumKey = builder.Configuration["PayOS:ChecksumKey"] ?? throw new Exception("Không tìm thấy PayOS:ChecksumKey");
builder.Services.AddSingleton(new PayOSClient(clientId, apiKey, checksumKey));

// Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(activeConnectionString) 
);

// --- 2. CẤU HÌNH AUTHENTICATION (CHỈ GỘP VÀO 1 CHỖ NÀY) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "ExternalCookies";
})

.AddCookie(options => 
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.Cookie.Name = "CVBuilder_Auth"; // Đặt tên riêng cho Cookie
    
    // BẢO MẬT: Bắt buộc xác thực một thiết bị mỗi lần Request (Server-side)
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            var userIdClaim = context.Principal.FindFirst("UserID")?.Value;
            var loginTimeClaim = context.Principal.FindFirst("LoginTime")?.Value;
            
            if (int.TryParse(userIdClaim, out int userId) && long.TryParse(loginTimeClaim, out long loginTime))
            {
                if (!DoAnCS.Services.SessionTracker.IsValidSession(userId, loginTime))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        }
    };
})

.AddCookie("ExternalCookies", options => 
{
    options.Cookie.Name = "CVBuilder_ExternalAuth";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // Chỉ cần tồn tại trong thời gian ngắn
})

.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.SignInScheme = "ExternalCookies";
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "";
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";
    options.SignInScheme = "ExternalCookies";
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
    options.SignInScheme = "ExternalCookies";
    options.Scope.Add("user:email");
})
.AddLinkedIn(options =>
{
    options.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"] ?? "";
    options.SignInScheme = "ExternalCookies";
});

// --- 3. CẤU HÌNH AUTHORIZATION (PHÂN QUYỀN) ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RecruiterOnly", policy => policy.RequireRole("Recruiter"));
    options.AddPolicy("CandidateOnly", policy => policy.RequireRole("User"));
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// --- 4. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Đoạn lệnh tạo bảng bằng raw SQL đã được loại bỏ để sử dụng chuẩn Entity Framework Migrations

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- BẮT ĐẦU: CẤU HÌNH THƯ MỤC LƯU TRỮ NGOÀI CHO FILE UPLOAD ---
var uploadsFolder = builder.Configuration["StorageSettings:UploadsFolder"] 
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");

if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

// Map /avt
var avtFolder = Path.Combine(uploadsFolder, "avt");
if (!Directory.Exists(avtFolder)) Directory.CreateDirectory(avtFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(avtFolder),
    RequestPath = "/avt"
});

// Cấu hình thư mục uploads chứa file mật (sẽ được kiểm soát qua FileController)
var generalUploadsFolder = Path.Combine(uploadsFolder, "uploads");
if (!Directory.Exists(generalUploadsFolder)) Directory.CreateDirectory(generalUploadsFolder);
// ĐÃ XÓA USE STATIC FILES CHO /uploads ĐỂ BẢO MẬT DỮ LIỆU CÁ NHÂN

// Map /images/templates
var imagesTemplatesFolder = Path.Combine(uploadsFolder, "images", "templates");
if (!Directory.Exists(imagesTemplatesFolder)) Directory.CreateDirectory(imagesTemplatesFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesTemplatesFolder),
    RequestPath = "/images/templates"
});
// --- KẾT THÚC: CẤU HÌNH THƯ MỤC LƯU TRỮ NGOÀI ---

app.UseRouting();

app.UseSession(); // Session phải nằm trước Authentication

app.UseAuthentication();
app.UseAuthorization();

// Đăng ký API Controllers (attribute routing - dùng cho [ApiController] + [Route(...)])
app.MapControllers();

app.MapHub<DoAnCS.Hubs.UserSessionHub>("/userSessionHub");

// Đăng ký MVC Controllers (conventional routing - dùng cho Views)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();