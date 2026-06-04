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

        //Thêm cột ChatbotApiKey nếu chưa có (cho DB cũ)
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('GeminiConfigs') AND name = 'ChatbotApiKey')
        BEGIN
            ALTER TABLE [GeminiConfigs] ADD [ChatbotApiKey] nvarchar(max) NULL;
        END
        ");

        // Thêm cột GroqApiKey nếu chưa có
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('GeminiConfigs') AND name = 'GroqApiKey')
        BEGIN
            ALTER TABLE [GeminiConfigs] ADD [GroqApiKey] nvarchar(max) NULL;
        END
        ");

        // Thêm cột ChatbotSystemInstruction nếu chưa có
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('GeminiConfigs') AND name = 'ChatbotSystemInstruction')
        BEGIN
            ALTER TABLE [GeminiConfigs] ADD [ChatbotSystemInstruction] nvarchar(max) NULL;
        END
        ");

        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AILogs') AND name = 'ApiProvider')
        BEGIN
            ALTER TABLE [AILogs] ADD [ApiProvider] nvarchar(50) NULL;
        END
        ");

        // Thêm các cột cho tính năng Public CV và ViewCount
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Resumes') AND name = 'IsPublic')
        BEGIN
            ALTER TABLE [Resumes] ADD [IsPublic] BIT NOT NULL DEFAULT 0,
                                      [Slug] NVARCHAR(MAX) NULL,
                                      [ViewCount] INT NOT NULL DEFAULT 0;
        END
        ");

        // Thêm cột cho tính năng PDF Upload
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Resumes') AND name = 'FileUploadUrl')
        BEGIN
            ALTER TABLE [Resumes] ADD [FileUploadUrl] NVARCHAR(MAX) NULL;
        END
        ");

        // Thêm cột Address và ExpectedLocation cho Users
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Address')
        BEGIN
            ALTER TABLE [Users] ADD [Address] NVARCHAR(MAX) NULL;
        END

        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ExpectedLocation')
        BEGIN
            ALTER TABLE [Users] ADD [ExpectedLocation] NVARCHAR(MAX) NULL;
        END

        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfessionalTitle')
        BEGIN
            ALTER TABLE [Users] ADD [ProfessionalTitle] NVARCHAR(MAX) NULL;
        END

        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PortfolioLinks')
        BEGIN
            ALTER TABLE [Users] ADD [PortfolioLinks] NVARCHAR(MAX) NULL;
        END

        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'YearsOfExperience')
        BEGIN
            ALTER TABLE [Users] ADD [YearsOfExperience] NVARCHAR(MAX) NULL;
        END
        ");

        // Thêm cột TaxCode cho Companies
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Companies') AND name = 'TaxCode')
        BEGIN
            ALTER TABLE [Companies] ADD [TaxCode] NVARCHAR(MAX) NULL;
        END
        ");

        // Thêm các cột còn thiếu cho bảng Users
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Summary')
            ALTER TABLE [Users] ADD [Summary] NVARCHAR(MAX) NULL;
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Skills')
            ALTER TABLE [Users] ADD [Skills] NVARCHAR(MAX) NULL;
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ExpectedSalary')
            ALTER TABLE [Users] ADD [ExpectedSalary] INT NULL;
        
        -- Các cột cho tính năng quên / đổi mật khẩu
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PasswordChangeToken')
            ALTER TABLE [Users] ADD [PasswordChangeToken] NVARCHAR(MAX) NULL,
                                    [PasswordChangeTokenExpires] DATETIME2 NULL,
                                    [PendingPasswordHash] NVARCHAR(MAX) NULL,
                                    [PasswordResetOTP] NVARCHAR(MAX) NULL,
                                    [OTPExpires] DATETIME2 NULL,
                                    [OTPFailCount] INT NULL;
        ");

        // Thêm các cột còn thiếu cho bảng Resumes (Tính năng Vue CV)
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Resumes') AND name = 'JsonContent')
            ALTER TABLE [Resumes] ADD [JsonContent] NVARCHAR(MAX) NULL,
                                      [FullName] NVARCHAR(MAX) NULL,
                                      [JobTitle] NVARCHAR(MAX) NULL,
                                      [Email] NVARCHAR(MAX) NULL,
                                      [Phone] NVARCHAR(MAX) NULL,
                                      [Address] NVARCHAR(MAX) NULL,
                                      [BirthDate] DATETIME2 NULL,
                                      [AvatarUrl] NVARCHAR(MAX) NULL,
                                      [Summary] NVARCHAR(MAX) NULL,
                                      [ThemeColor] NVARCHAR(MAX) NULL;
        ");

        // AUTO-CREATE bảng CVMatchResults cho tính năng Smart CV Matcher
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CVMatchResults' and xtype='U')
        BEGIN
            CREATE TABLE [CVMatchResults] (
                [Id] int IDENTITY(1,1) NOT NULL,
                [JobID] int NOT NULL,
                [ResumeID] int NOT NULL,
                [MatchScore] int NOT NULL DEFAULT 0,
                [MatchedSkills] nvarchar(max) NULL,
                [MissingSkills] nvarchar(max) NULL,
                [Suggestions] nvarchar(max) NULL,
                [Strengths] nvarchar(max) NULL,
                [Summary] nvarchar(max) NULL,
                [Recommendation] nvarchar(100) NULL,
                [AnalyzedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                CONSTRAINT [PK_CVMatchResults] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_CVMatchResults_Jobs] FOREIGN KEY ([JobID]) REFERENCES [Jobs]([JobID]) ON DELETE CASCADE,
                CONSTRAINT [FK_CVMatchResults_Resumes] FOREIGN KEY ([ResumeID]) REFERENCES [Resumes]([ResumeID]) ON DELETE CASCADE
            );
        END
        ");

        // AUTO-CREATE bảng CVEmbeddings cho tính năng RAG Vector Search
        db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CVEmbeddings' and xtype='U')
        BEGIN
            CREATE TABLE [CVEmbeddings] (
                [Id] int IDENTITY(1,1) NOT NULL,
                [ResumeID] int NOT NULL,
                [VectorJson] nvarchar(max) NOT NULL DEFAULT '[]',
                [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                CONSTRAINT [PK_CVEmbeddings] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_CVEmbeddings_Resumes] FOREIGN KEY ([ResumeID]) REFERENCES [Resumes]([ResumeID]) ON DELETE CASCADE
            );
        END
        ");
    } 
    catch(Exception ex) 
    { 
        Console.WriteLine("SQL Create/Alter Table Error: " + ex.Message); 
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

app.MapHub<DoAnCS.Hubs.UserSessionHub>("/userSessionHub");

// Đăng ký MVC Controllers (conventional routing - dùng cho Views)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();