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

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options => 
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "";
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
    options.Scope.Add("user:email");
})
.AddLinkedIn(options =>
{
    options.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"] ?? "";
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