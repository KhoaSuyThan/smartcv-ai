using DoAnCS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = @"Server=LAPTOP-V23SMM4O;Database=DoAnWebCS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

// --- 1. ĐĂNG KÝ SERVICES ---
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JobApiService>();
builder.Services.AddScoped<IAIService, GeminiService>(); 

// Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString) 
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

// --- 4. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session phải nằm trước Authentication

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();