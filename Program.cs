using DoAnCS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- 1. ĐĂNG KÝ SERVICES ---
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JobApiService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIService, GeminiService>(); 
builder.Services.AddScoped<IEmailService, EmailService>(); 

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

// AUTO-CREATE GeminiConfigs table to bypass Migration history corruption
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='GeminiConfigs' and xtype='U')
        BEGIN
            CREATE TABLE [GeminiConfigs] (
                [Id] int NOT NULL,
                [ApiKey] nvarchar(max) NULL,
                [ChatbotApiKey] nvarchar(max) NULL,
                [ModelName] nvarchar(max) NOT NULL DEFAULT 'gemini-2.5-flash',
                [Temperature] float NOT NULL DEFAULT 0.7,
                [MaxOutputTokens] int NOT NULL DEFAULT 2048,
                [SystemInstruction] nvarchar(max) NULL,
                [SkillTemplate] nvarchar(max) NULL,
                [SummaryTemplate] nvarchar(max) NULL,
                [GrammarTemplate] nvarchar(max) NULL,
                [UserRateLimit] int NOT NULL DEFAULT 10,
                [TotalTokensUsed] bigint NOT NULL DEFAULT 0,
                CONSTRAINT [PK_GeminiConfigs] PRIMARY KEY ([Id])
            );
            INSERT INTO [GeminiConfigs] ([Id], [ApiKey], [ModelName], [Temperature], [MaxOutputTokens], [SystemInstruction], [UserRateLimit], [TotalTokensUsed])
            VALUES (1, N'', N'gemini-2.5-flash', 0.7, 2048, N'Bạn là trợ lý ảo hỗ trợ đánh giá CV.', 10, 0);
        END
        ");

        // Thêm cột ChatbotApiKey nếu chưa có (cho DB cũ)
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('GeminiConfigs') AND name = 'ChatbotApiKey')
        BEGIN
            ALTER TABLE [GeminiConfigs] ADD [ChatbotApiKey] nvarchar(max) NULL;
        END
        ");
    } 
    catch(Exception ex) 
    { 
        Console.WriteLine("SQL Create Table Error: " + ex.Message); 
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session phải nằm trước Authentication

app.UseAuthentication();
app.UseAuthorization();

// Đăng ký API Controllers (attribute routing - dùng cho [ApiController] + [Route(...)])
app.MapControllers();

// Đăng ký MVC Controllers (conventional routing - dùng cho Views)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();