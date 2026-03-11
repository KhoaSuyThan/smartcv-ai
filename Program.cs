using DoAnCS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = @"Server=LAPTOP-V23SMM4O;Database=DoAnWebCS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
// Add services to the container.
builder.Services.AddControllersWithViews();


// Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString) 
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Trang khi không có quyền
        options.ExpireTimeSpan = TimeSpan.FromHours(24); // Ghi nhớ đăng nhập trong 24h
    });

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IAIService, GeminiService>(); // Hoặc OpenAIService

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// Session middleware
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


// Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();