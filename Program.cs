using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Services;
using Microsoft.AspNetCore.HttpOverrides;
using PayOS;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Bỏ bớt log hệ thống thừa
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var onlineConnectionString = builder.Configuration.GetConnectionString("OnlineConnection");
var localConnectionString = builder.Configuration.GetConnectionString("LocalConnection");

string activeConnectionString = !string.IsNullOrEmpty(onlineConnectionString) ? onlineConnectionString : localConnectionString;

Console.WriteLine($"Using Connection String: {activeConnectionString}");
// --- 1. ĐĂNG KÝ SERVICES ---
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddScoped<JobApiService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIService, GeminiService>(); 
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddScoped<IAutomationTestRunner, AutomationTestRunner>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>(); // Đăng ký dịch vụ mã hóa API Key
builder.Services.AddHostedService<ProExpirationService>();

// Đăng ký dịch vụ Health Checks kiểm tra DB và FastAPI Python AI
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("Database")
    .AddCheck<PythonAiHealthCheck>("Python_AI");

// Cấu hình PayOS
var clientId = builder.Configuration["PayOS:ClientId"] ?? throw new Exception("Không tìm thấy PayOS:ClientId");
var apiKey = builder.Configuration["PayOS:ApiKey"] ?? throw new Exception("Không tìm thấy PayOS:ApiKey");
var checksumKey = builder.Configuration["PayOS:ChecksumKey"] ?? throw new Exception("Không tìm thấy PayOS:ChecksumKey");
builder.Services.AddSingleton(new PayOSClient(clientId, apiKey, checksumKey));

// Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(activeConnectionString)
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
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
                // === BƯỚC 1: Kiểm tra cache trước (không tốn DB query) ===
                var cacheResult = DoAnCS.Services.SessionTracker.IsValidSessionFromCache(userId, loginTime);

                if (cacheResult.HasValue)
                {
                    // Cache còn hợp lệ → dùng kết quả từ cache
                    if (!cacheResult.Value)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        return;
                    }
                }
                else
                {
                    // === BƯỚC 2: Cache hết hạn → query DB lấy LastLoginTime ===
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                    var dbLoginTime = await dbContext.Users
                        .Where(u => u.UserID == userId)
                        .Select(u => u.LastLoginTime)
                        .FirstOrDefaultAsync();

                    if (dbLoginTime.HasValue)
                    {
                        // Refresh cache với giá trị mới từ DB
                        DoAnCS.Services.SessionTracker.RefreshCache(userId, dbLoginTime.Value);

                        if (loginTime < dbLoginTime.Value)
                        {
                            // Cookie cũ hơn DB → phiên không hợp lệ (đăng nhập thiết bị khác sau)
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            return;
                        }
                    }
                    else
                    {
                        // LastLoginTime chưa có trong DB (user cũ chưa đăng nhập lại lần nào)
                        // → Chấp nhận và ghi lại để bảo vệ từ lần sau
                        DoAnCS.Services.SessionTracker.RefreshCache(userId, loginTime);
                    }
                }

                // === BƯỚC 3: Tự động làm mới quyền Pro ngầm ===
                var isProClaim = context.Principal.HasClaim("IsPro", "True");
                var dbCtx = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var userInDb = await dbCtx.Users.FindAsync(userId);

                if (userInDb != null && userInDb.IsPro != isProClaim)
                {
                    var identity = context.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                    if (identity != null)
                    {
                        var oldClaim = identity.FindFirst("IsPro");
                        if (oldClaim != null) identity.RemoveClaim(oldClaim);

                        if (userInDb.IsPro == true)
                            identity.AddClaim(new System.Security.Claims.Claim("IsPro", "True"));

                        context.ReplacePrincipal(context.Principal);
                        context.ShouldRenew = true;
                    }
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

// Cấu hình Rate Limiting (Chống Spam API/Form và AI API Endpoints)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ContactLimiter", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 3; // Tối đa 3 tin nhắn
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    
    // Thêm Policy cho các API Trí Tuệ Nhân Tạo (AI Endpoints) để giới hạn theo địa chỉ IP
    options.AddPolicy("AiApiPolicy", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 15, // Tối đa 15 request/phút trên mỗi địa chỉ IP
            QueueLimit = 0
        });
    });
    
    // Tùy chỉnh thông báo lỗi khi vượt giới hạn (429 Too Many Requests)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        var path = context.HttpContext.Request.Path.Value ?? "";
        
        // Nếu là yêu cầu API/AI, trả về định dạng JSON thay vì trang HTML
        if (path.Contains("/api/", StringComparison.OrdinalIgnoreCase) || 
            path.Contains("/AI/", StringComparison.OrdinalIgnoreCase) || 
            path.Contains("/SmartMatch/", StringComparison.OrdinalIgnoreCase))
        {
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            await context.HttpContext.Response.WriteAsync("{\"success\": false, \"reply\": \"Hệ thống phát hiện tần suất yêu cầu quá cao từ IP của bạn. Vui lòng thử lại sau 1 phút!\", \"data\": \"Hệ thống phát hiện tần suất yêu cầu quá cao từ IP của bạn. Vui lòng thử lại sau 1 phút!\"}", cancellationToken: token);
        }
        else
        {
            context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
            await context.HttpContext.Response.WriteAsync(@"
                <html>
                <head><title>Quá nhiều yêu cầu</title></head>
                <body style='text-align:center; padding: 50px; font-family: sans-serif;'>
                    <h2 style='color:#dc3545;'>Bạn đã gửi quá nhiều yêu cầu!</h2>
                    <p>Vui lòng đợi một khoảng thời gian trước khi gửi thêm tin nhắn mới.</p>
                    <button onclick='window.history.back()' style='padding:10px 20px; border:none; background:#0d6efd; color:white; border-radius:5px; cursor:pointer;'>Quay lại</button>
                </body>
                </html>
            ", cancellationToken: token);
        }
    };
});

// Cấu hình dịch vụ nén phản hồi (Response Compression) dùng Brotli & Gzip
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

var app = builder.Build();

// --- TỰ ĐỘNG CHẠY MIGRATION KHI STARTUP ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Kiểm tra nếu cột ProExpirationDate đã tồn tại trong DB thực tế (do tạo thủ công hoặc chạy script SQL)
        // nhưng lại chưa được ghi nhận trong bảng lịch sử Migrations của EF Core, ta sẽ thêm thủ công vào lịch sử để tránh lỗi.
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END
            ");

            context.Database.ExecuteSqlRaw(@"
                IF COL_LENGTH('Users', 'ProExpirationDate') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260612072825_AddProExpirationDate')
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20260612072825_AddProExpirationDate', '8.0.8');
                    END
                END
            ");
        }
        catch (Exception dbEx)
        {
            Console.WriteLine("Lưu ý: Không thể kiểm tra cột ProExpirationDate bằng raw SQL, tiến hành chạy migration mặc định. Chi tiết: " + dbEx.Message);
        }

        try
        {
            context.Database.Migrate();
            Console.WriteLine("EF Core Database Migration: OK");
        }
        catch (Exception migrateEx)
        {
            Console.WriteLine("Lưu ý: EF Core Migration gặp lỗi nhưng sẽ tiếp tục các bước migration thủ công. Chi tiết: " + migrateEx.Message);
        }

        // Migration thủ công: Thêm cột TopCandidatesCount vào GeminiConfigs nếu chưa có
        // (cột này được thêm sau khi DB đã khởi tạo, cần ALTER TABLE để tự động cập nhật)
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF COL_LENGTH('GeminiConfigs', 'TopCandidatesCount') IS NULL
                BEGIN
                    ALTER TABLE [GeminiConfigs]
                    ADD [TopCandidatesCount] INT NOT NULL DEFAULT 6;
                END
            ");
            Console.WriteLine("Migration GeminiConfigs.TopCandidatesCount: OK");
        }
        catch (Exception colEx)
        {
            Console.WriteLine("Lưu ý: Không thể thêm cột TopCandidatesCount. Chi tiết: " + colEx.Message);
        }

        // Migration thủ công: Thêm cột LastLoginTime vào Users nếu chưa có
        // (dùng để kiểm tra phiên đăng nhập đa thiết bị, bền vững qua server restart)
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF COL_LENGTH('Users', 'LastLoginTime') IS NULL
                BEGIN
                    ALTER TABLE [Users]
                    ADD [LastLoginTime] BIGINT NULL;
                END
            ");
            Console.WriteLine("Migration Users.LastLoginTime: OK");
        }
        catch (Exception colEx)
        {
            Console.WriteLine("Lưu ý: Không thể thêm cột LastLoginTime. Chi tiết: " + colEx.Message);
        }

        // Migration thủ công: Tạo Database Indexes để tăng tốc độ query (nếu chưa có)
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Resumes_IsPublic' AND object_id = OBJECT_ID('Resumes'))
                    CREATE INDEX IX_Resumes_IsPublic ON Resumes(IsPublic);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Resumes_UserID' AND object_id = OBJECT_ID('Resumes'))
                    CREATE INDEX IX_Resumes_UserID ON Resumes(UserID);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Jobs_Status' AND object_id = OBJECT_ID('Jobs'))
                    CREATE INDEX IX_Jobs_Status ON Jobs(Status);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AILogs_UserID_CreatedAt' AND object_id = OBJECT_ID('AILogs'))
                    CREATE INDEX IX_AILogs_UserID_CreatedAt ON AILogs(UserID, CreatedAt);

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CVEmbeddings_ResumeID' AND object_id = OBJECT_ID('CVEmbeddings'))
                    CREATE INDEX IX_CVEmbeddings_ResumeID ON CVEmbeddings(ResumeID);
            ");
            Console.WriteLine("Migration Database Indexes: OK");
        }
        catch (Exception indexEx)
        {
            Console.WriteLine("Lưu ý: Không thể tạo Database Indexes. Chi tiết: " + indexEx.Message);
        }

        // Migration thủ công: Tạo bảng TestRuns và TestCaseDetails nếu chưa có phục vụ lưu lịch sử kiểm thử
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID('TestRuns', 'U') IS NULL
                BEGIN
                    CREATE TABLE TestRuns (
                        TestRunID INT PRIMARY KEY IDENTITY(1,1),
                        ExecutionTime DATETIME DEFAULT GETDATE(),
                        SuiteName NVARCHAR(100) NOT NULL,
                        TotalCases INT NOT NULL,
                        PassedCases INT NOT NULL,
                        FailedCases INT NOT NULL,
                        AvgResponseTimeMs BIGINT NOT NULL
                    );
                END

                IF OBJECT_ID('TestCaseDetails', 'U') IS NULL
                BEGIN
                    CREATE TABLE TestCaseDetails (
                        TestCaseID INT PRIMARY KEY IDENTITY(1,1),
                        TestRunID INT NOT NULL,
                        Name NVARCHAR(255) NOT NULL,
                        Method NVARCHAR(50) NOT NULL,
                        Url NVARCHAR(500) NULL,
                        Status NVARCHAR(50) NOT NULL,
                        ResponseTimeMs BIGINT NOT NULL,
                        ExpectedResult NVARCHAR(MAX) NULL,
                        ActualResult NVARCHAR(MAX) NULL,
                        ErrorMessage NVARCHAR(MAX) NULL,
                        CONSTRAINT FK_TestCaseDetails_TestRuns FOREIGN KEY (TestRunID) REFERENCES TestRuns(TestRunID) ON DELETE CASCADE
                    );
                END
            ");
            Console.WriteLine("Migration Test History Tables: OK");
        }
        catch (Exception dbEx)
        {
            Console.WriteLine("Lưu ý: Không thể tự động tạo bảng lịch sử kiểm thử. Chi tiết: " + dbEx.Message);
        }

        // Migration thủ công: Tạo các bảng phục vụ tính năng Phỏng vấn thông minh AI
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID('InterviewSessions', 'U') IS NULL
                BEGIN
                    CREATE TABLE InterviewSessions (
                        SessionID INT PRIMARY KEY IDENTITY(1,1),
                        UserID INT NOT NULL,
                        JobID INT NULL,
                        ResumeID INT NOT NULL,
                        Status INT NOT NULL DEFAULT 0,
                        OverallScore INT NULL,
                        AiEvaluation NVARCHAR(MAX) NULL,
                        AssignedByRecruiterID INT NULL,
                        InterviewType INT NOT NULL DEFAULT 0,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        CompletedAt DATETIME NULL,
                        CONSTRAINT FK_InterviewSessions_Users FOREIGN KEY (UserID) REFERENCES Users(UserID),
                        CONSTRAINT FK_InterviewSessions_Jobs FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
                        CONSTRAINT FK_InterviewSessions_Resumes FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID),
                        CONSTRAINT FK_InterviewSessions_Recruiters FOREIGN KEY (AssignedByRecruiterID) REFERENCES Users(UserID)
                    );
                END

                IF OBJECT_ID('InterviewMessages', 'U') IS NULL
                BEGIN
                    CREATE TABLE InterviewMessages (
                        MessageID INT PRIMARY KEY IDENTITY(1,1),
                        SessionID INT NOT NULL,
                        Role NVARCHAR(50) NOT NULL,
                        Content NVARCHAR(MAX) NOT NULL,
                        ChoicesJson NVARCHAR(MAX) NULL,
                        SelectedAnswer NVARCHAR(MAX) NULL,
                        Score INT NULL,
                        Feedback NVARCHAR(MAX) NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT FK_InterviewMessages_Sessions FOREIGN KEY (SessionID) REFERENCES InterviewSessions(SessionID) ON DELETE CASCADE
                    );
                END

                IF OBJECT_ID('RecruiterInterviewPreps', 'U') IS NULL
                BEGIN
                    CREATE TABLE RecruiterInterviewPreps (
                        PrepID INT PRIMARY KEY IDENTITY(1,1),
                        RecruiterID INT NOT NULL,
                        ResumeID INT NOT NULL,
                        JobID INT NOT NULL,
                        QuestionsJson NVARCHAR(MAX) NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT FK_RecruiterInterviewPreps_Recruiters FOREIGN KEY (RecruiterID) REFERENCES Users(UserID),
                        CONSTRAINT FK_RecruiterInterviewPreps_Resumes FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID),
                        CONSTRAINT FK_RecruiterInterviewPreps_Jobs FOREIGN KEY (JobID) REFERENCES Jobs(JobID)
                    );
                END
            ");
            Console.WriteLine("Migration Interview Tables: OK");
        }
        catch (Exception interviewDbEx)
        {
            Console.WriteLine("Lưu ý: Không thể tự động tạo các bảng Phỏng vấn. Chi tiết: " + interviewDbEx.Message);
        }

        // Migration thủ công: Tạo bảng Notifications (Hệ thống thông báo) nếu chưa có
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID('Notifications', 'U') IS NULL
                BEGIN
                    CREATE TABLE Notifications (
                        NotificationID INT PRIMARY KEY IDENTITY(1,1),
                        UserID INT NOT NULL,
                        Type NVARCHAR(50) NOT NULL DEFAULT 'System',
                        Title NVARCHAR(200) NOT NULL,
                        Message NVARCHAR(MAX) NOT NULL,
                        Link NVARCHAR(500) NULL,
                        IsRead BIT NOT NULL DEFAULT 0,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
                    );

                    CREATE INDEX IX_Notifications_UserID_IsRead ON Notifications(UserID, IsRead);
                END
            ");
            Console.WriteLine("Migration Notifications Table: OK");
        }
        catch (Exception notifEx)
        {
            Console.WriteLine("Lưu ý: Không thể tạo bảng Notifications. Chi tiết: " + notifEx.Message);
        }

        // Tự động dọn dẹp thông báo cũ quá 30 ngày để giữ DB gọn nhẹ
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var oldNotifications = context.Notifications.Where(n => n.CreatedAt < cutoffDate).ToList();
            if (oldNotifications.Any())
            {
                context.Notifications.RemoveRange(oldNotifications);
                context.SaveChanges();
                Console.WriteLine($"Đã dọn dẹp {oldNotifications.Count} thông báo cũ quá 30 ngày.");
            }
        }
        catch (Exception cleanEx)
        {
            Console.WriteLine("Lỗi dọn dẹp thông báo cũ: " + cleanEx.Message);
        }

        // Tự động seed tài khoản PRO test để phục vụ kiểm thử dịch thuật CV
        try
        {
            var testUser = context.Users.FirstOrDefault(u => u.Email == "test_e2e_translation@smartcv.vn");
            if (testUser == null)
            {
                testUser = new User
                {
                    FullName = "Test CV Translation",
                    Email = "test_e2e_translation@smartcv.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = "User",
                    IsPro = true,
                    ProExpirationDate = DateTime.Now.AddYears(5),
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(testUser);
                context.SaveChanges();
                Console.WriteLine("Seed PRO Test User for translation: OK");
            }
            else if (!testUser.IsPro)
            {
                testUser.IsPro = true;
                testUser.ProExpirationDate = DateTime.Now.AddYears(5);
                context.SaveChanges();
                Console.WriteLine("Update PRO status for test user: OK");
            }
        }
        catch (Exception userEx)
        {
            Console.WriteLine("Lỗi seed PRO user: " + userEx.Message);
        }

        // Migration thủ công: Tạo bảng TestSteps và seed dữ liệu kịch bản mặc định nếu chưa có
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID('TestSteps', 'U') IS NULL
                BEGIN
                    CREATE TABLE TestSteps (
                        StepID INT IDENTITY(1,1) PRIMARY KEY,
                        ScenarioName NVARCHAR(50) NOT NULL,
                        StepOrder INT NOT NULL,
                        ActionType NVARCHAR(20) NOT NULL,
                        TargetSelector NVARCHAR(250) NULL,
                        Value NVARCHAR(MAX) NULL,
                        Description NVARCHAR(500) NULL
                    );
                END
            ");
            Console.WriteLine("Migration TestSteps Table: OK");

            // Dọn dẹp các bước cũ của kịch bản E2E chính để luôn đồng bộ mới nhất
            try
            {
                var oldMainSteps = context.TestSteps.Where(s => s.ScenarioName == "Auth E2E" || s.ScenarioName == "Jobs E2E").ToList();
                if (oldMainSteps.Any())
                {
                    context.TestSteps.RemoveRange(oldMainSteps);
                    context.SaveChanges();
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi dọn dẹp E2E chính: " + ex.Message); }

            // Seed dữ liệu mặc định cho các bước E2E
            if (!context.TestSteps.Any(s => s.ScenarioName == "Auth E2E" || s.ScenarioName == "Jobs E2E"))
            {
                // 1. Auth E2E steps
                context.TestSteps.AddRange(new List<TestStep>
                {
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 1, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Login", Description = "Vào trang Login" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 2, ActionType = "Click", TargetSelector = ".register-btn", Value = "1000", Description = "Chuyển sang Tab Register (chờ 1s)" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 3, ActionType = "Fill", TargetSelector = "input[name='Register.FullName']", Value = "E2E Candidate", Description = "Nhập họ tên ứng viên" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 4, ActionType = "Fill", TargetSelector = "input[name='Register.Email']", Value = "test_e2e_candidate@smartcv.vn", Description = "Nhập email ứng viên" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 5, ActionType = "Fill", TargetSelector = "input[name='Register.Password']", Value = "Password123!", Description = "Nhập mật khẩu" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 6, ActionType = "Fill", TargetSelector = "input[name='Register.ConfirmPassword']", Value = "Password123!", Description = "Nhập xác nhận mật khẩu" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 7, ActionType = "Click", TargetSelector = "form[action='/Account/Register'] button[type='submit']", Value = "", Description = "Nhấp Đăng ký" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 8, ActionType = "AssertUrl", TargetSelector = "", Value = "**/Account/VerifyRegisterOTP", Description = "Kiểm tra chuyển sang trang OTP" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 9, ActionType = "Fill", TargetSelector = ".otp-input", Value = "123456", Description = "Điền mã OTP bypass 123456" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 10, ActionType = "Click", TargetSelector = "form button[type='submit']", Value = "", Description = "Xác thực OTP" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 11, ActionType = "AssertUrl", TargetSelector = "", Value = "**/Account/Login", Description = "Kiểm tra chuyển về trang Đăng nhập" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 12, ActionType = "Fill", TargetSelector = "input[name='Login.Email']", Value = "test_e2e_candidate@smartcv.vn", Description = "Nhập email đăng nhập" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 13, ActionType = "Fill", TargetSelector = "input[name='Login.Password']", Value = "Password123!", Description = "Nhập mật khẩu đăng nhập" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 14, ActionType = "Click", TargetSelector = "form[action='/Account/Login'] button[type='submit']", Value = "", Description = "Nhấp nút Đăng nhập" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 15, ActionType = "AssertUrl", TargetSelector = "", Value = "/", Description = "Xác nhận đăng nhập thành công và về Trang chủ" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 16, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Logout", Description = "Đăng xuất tài khoản" },
                    new TestStep { ScenarioName = "Auth E2E", StepOrder = 17, ActionType = "AssertUrl", TargetSelector = "", Value = "/", Description = "Kiểm tra đăng xuất hoàn tất" }
                });

                // 2. Jobs E2E steps
                context.TestSteps.AddRange(new List<TestStep>
                {
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 1, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Login", Description = "Vào trang Login" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 2, ActionType = "Click", TargetSelector = ".register-btn", Value = "1000", Description = "Chuyển sang Tab Register (chờ 1s)" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 3, ActionType = "Fill", TargetSelector = "input[name='Register.FullName']", Value = "E2E Recruiter User", Description = "Nhập họ tên nhà tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 4, ActionType = "Fill", TargetSelector = "input[name='Register.Email']", Value = "test_e2e_recruiter@smartcv.vn", Description = "Nhập email nhà tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 5, ActionType = "Fill", TargetSelector = "input[name='Register.Password']", Value = "Password123!", Description = "Nhập mật khẩu" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 6, ActionType = "Fill", TargetSelector = "input[name='Register.ConfirmPassword']", Value = "Password123!", Description = "Nhập xác nhận mật khẩu" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 7, ActionType = "Select", TargetSelector = "#roleSelect", Value = "Recruiter", Description = "Chọn vai trò Recruiter" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 8, ActionType = "Fill", TargetSelector = "input[name='Register.CompanyName']", Value = "E2E Test Company", Description = "Nhập tên công ty" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 9, ActionType = "Fill", TargetSelector = "input[name='Register.TaxCode']", Value = "123456789", Description = "Nhập mã số thuế" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 10, ActionType = "Click", TargetSelector = "form[action='/Account/Register'] button[type='submit']", Value = "", Description = "Nhấp nút Đăng ký" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 11, ActionType = "AssertUrl", TargetSelector = "", Value = "**/Account/VerifyRegisterOTP", Description = "Kiểm tra chuyển sang trang OTP" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 12, ActionType = "Fill", TargetSelector = ".otp-input", Value = "123456", Description = "Điền mã OTP bypass 123456" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 13, ActionType = "Click", TargetSelector = "form button[type='submit']", Value = "", Description = "Xác thực OTP" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 14, ActionType = "AssertUrl", TargetSelector = "", Value = "**/Account/Login", Description = "Kiểm tra chuyển về trang Đăng nhập" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 15, ActionType = "Fill", TargetSelector = "input[name='Login.Email']", Value = "test_e2e_recruiter@smartcv.vn", Description = "Nhập email đăng nhập" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 16, ActionType = "Fill", TargetSelector = "input[name='Login.Password']", Value = "Password123!", Description = "Nhập mật khẩu đăng nhập" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 17, ActionType = "Click", TargetSelector = "form[action='/Account/Login'] button[type='submit']", Value = "", Description = "Nhấp nút Đăng nhập" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 18, ActionType = "AssertUrl", TargetSelector = "", Value = "/", Description = "Xác nhận đăng nhập thành công và về Trang chủ" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 19, ActionType = "Navigate", TargetSelector = "", Value = "/Jobs/Create", Description = "Vào trang Tạo tin tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 20, ActionType = "Fill", TargetSelector = "input[name='Title']", Value = "Vị trí tuyển dụng E2E Test", Description = "Nhập tiêu đề tin tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 21, ActionType = "Select", TargetSelector = "#salaryType", Value = "Nhập liệu", Description = "Chọn loại lương Nhập liệu" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 22, ActionType = "Fill", TargetSelector = "#customSalary", Value = "15-20", Description = "Nhập mức lương tự chọn" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 23, ActionType = "Fill", TargetSelector = "input[name='Deadline']", Value = "TOMORROW", Description = "Nhập hạn nộp hồ sơ (ngày mai)" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 24, ActionType = "Fill", TargetSelector = "textarea[name='Description']", Value = "Mô tả công việc kiểm thử tự động.", Description = "Nhập mô tả công việc" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 25, ActionType = "Fill", TargetSelector = "textarea[name='Requirements']", Value = "Yêu cầu ứng viên thành thạo Playwright.", Description = "Nhập yêu cầu công việc" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 26, ActionType = "Click", TargetSelector = "form button[type='submit']", Value = "", Description = "Đăng tin tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 27, ActionType = "AssertUrl", TargetSelector = "", Value = "**/Jobs/Manage", Description = "Kiểm tra chuyển hướng về trang Quản lý tin" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 28, ActionType = "AssertText", TargetSelector = "body", Value = "Vị trí tuyển dụng E2E Test", Description = "Xác nhận tin tuyển dụng hiển thị trên danh sách" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 29, ActionType = "Click", TargetSelector = "a[title='Chỉnh sửa']", Value = "", Description = "Nhấn nút Chỉnh sửa tin" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 30, ActionType = "AssertUrl", TargetSelector = "**/Jobs/Edit/*", Value = "", Description = "Kiểm tra chuyển hướng về trang Sửa tin" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 31, ActionType = "Fill", TargetSelector = "input[name='Title']", Value = "Vị trí tuyển dụng E2E Test - Updated", Description = "Nhập tiêu đề mới đã chỉnh sửa" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 32, ActionType = "Click", TargetSelector = "form button[type='submit']", Value = "", Description = "Cập nhật tin tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 33, ActionType = "AssertUrl", TargetSelector = "", Value = "**/Jobs/Manage", Description = "Kiểm tra chuyển hướng về trang Quản lý tin sau khi cập nhật" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 34, ActionType = "AssertText", TargetSelector = "body", Value = "Vị trí tuyển dụng E2E Test - Updated", Description = "Xác nhận tin đã được cập nhật tiêu đề mới" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 35, ActionType = "Click", TargetSelector = "button[title='Xóa']", Value = "ACCEPT_DIALOG", Description = "Nhấn nút Xóa tin (chấp nhận dialog)" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 36, ActionType = "AssertTextNot", TargetSelector = "body", Value = "Vị trí tuyển dụng E2E Test - Updated", Description = "Xác nhận tin tuyển dụng đã bị biến mất" },
                    new TestStep { ScenarioName = "Jobs E2E", StepOrder = 37, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Logout", Description = "Đăng xuất tài khoản Recruiter" }
                });
                context.SaveChanges();
                Console.WriteLine("Seed default E2E TestSteps successfully!");
            }

            // Dọn dẹp các bước cũ của kịch bản Validation để luôn đồng bộ mới nhất
            try
            {
                var oldAuthValidationSteps = context.TestSteps.Where(s => s.ScenarioName == "Auth Validation E2E").ToList();
                if (oldAuthValidationSteps.Any())
                {
                    context.TestSteps.RemoveRange(oldAuthValidationSteps);
                    context.SaveChanges();
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi dọn dẹp Auth Validation E2E: " + ex.Message); }

            // Seed kịch bản Validation mới
            if (!context.TestSteps.Any(s => s.ScenarioName == "Auth Validation E2E"))
            {
                context.TestSteps.AddRange(new List<TestStep>
                {
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 1, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Login", Description = "Vào trang Login" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 2, ActionType = "Click", TargetSelector = ".register-btn", Value = "500", Description = "Chuyển sang Tab Register (chờ 0.5s)" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 3, ActionType = "Fill", TargetSelector = "input[name='Register.FullName']", Value = "E2E Validation User", Description = "Nhập họ tên hợp lệ" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 4, ActionType = "Fill", TargetSelector = "input[name='Register.Email']", Value = "test_e2e_candidate@smartcv.vn", Description = "Nhập email đã tồn tại để trigger lỗi server" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 5, ActionType = "Fill", TargetSelector = "input[name='Register.Password']", Value = "Password123!", Description = "Nhập mật khẩu hợp lệ" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 6, ActionType = "Fill", TargetSelector = "input[name='Register.ConfirmPassword']", Value = "Password123!", Description = "Nhập xác nhận mật khẩu khớp" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 7, ActionType = "Click", TargetSelector = "form[action='/Account/Register'] button[type='submit']", Value = "", Description = "Nhấp Đăng ký để trigger trùng email" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 8, ActionType = "AssertText", TargetSelector = ".form-box.register form div[style*='color: #d9534f']", Value = "Email này đã được đăng ký", Description = "Kiểm tra thông báo lỗi trùng email từ máy chủ" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 9, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Login", Description = "Làm mới quay về trang Đăng nhập" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 10, ActionType = "Fill", TargetSelector = "input[name='Login.Email']", Value = "test_e2e_candidate@smartcv.vn", Description = "Nhập email đúng" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 11, ActionType = "Fill", TargetSelector = "input[name='Login.Password']", Value = "WrongPass123!", Description = "Nhập sai mật khẩu" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 12, ActionType = "Click", TargetSelector = "form[action='/Account/Login'] button[type='submit']", Value = "", Description = "Nhấp nút Đăng nhập" },
                    new TestStep { ScenarioName = "Auth Validation E2E", StepOrder = 13, ActionType = "AssertText", TargetSelector = ".form-box.login form div[style*='color: #d9534f']", Value = "không chính xác", Description = "Kiểm tra thông báo đăng nhập sai mật khẩu" }
                });
                context.SaveChanges();
                Console.WriteLine("Seed Auth Validation E2E TestSteps successfully!");
            }

            // Dọn dẹp các bước cũ của kịch bản Jobs Validation để luôn đồng bộ mới nhất
            try
            {
                var oldJobsValidationSteps = context.TestSteps.Where(s => s.ScenarioName == "Jobs Validation E2E").ToList();
                if (oldJobsValidationSteps.Any())
                {
                    context.TestSteps.RemoveRange(oldJobsValidationSteps);
                    context.SaveChanges();
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi dọn dẹp Jobs Validation E2E: " + ex.Message); }

            if (!context.TestSteps.Any(s => s.ScenarioName == "Jobs Validation E2E"))
            {
                context.TestSteps.AddRange(new List<TestStep>
                {
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 1, ActionType = "Navigate", TargetSelector = "", Value = "/Account/Login", Description = "Vào trang Login" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 2, ActionType = "Fill", TargetSelector = "input[name='Login.Email']", Value = "test_e2e_recruiter@smartcv.vn", Description = "Nhập email nhà tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 3, ActionType = "Fill", TargetSelector = "input[name='Login.Password']", Value = "Password123!", Description = "Nhập mật khẩu" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 4, ActionType = "Click", TargetSelector = "form[action='/Account/Login'] button[type='submit']", Value = "", Description = "Nhấp Đăng nhập" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 5, ActionType = "AssertUrl", TargetSelector = "", Value = "/", Description = "Đợi chuyển hướng thành công về trang chủ" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 6, ActionType = "Navigate", TargetSelector = "", Value = "/Jobs/Create", Description = "Đến trang Tạo tin tuyển dụng" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 7, ActionType = "Click", TargetSelector = "form button[type='submit']", Value = "", Description = "Nhấp đăng tin khi form trống" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 8, ActionType = "AssertText", TargetSelector = "span[data-valmsg-for='Title']", Value = "Tiêu đề là bắt buộc", Description = "Kiểm tra thông báo lỗi trống Tiêu đề" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 9, ActionType = "Fill", TargetSelector = "input[name='Title']", Value = "Validation Job Test", Description = "Nhập tiêu đề hợp lệ" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 10, ActionType = "Fill", TargetSelector = "textarea[name='Description']", Value = "Mô tả công việc validation.", Description = "Nhập mô tả công việc" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 11, ActionType = "Fill", TargetSelector = "textarea[name='Requirements']", Value = "Yêu cầu ứng viên validation.", Description = "Nhập yêu cầu công việc" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 12, ActionType = "Fill", TargetSelector = "input[name='Deadline']", Value = "2000-01-01", Description = "Nhập hạn nộp hồ sơ trong quá khứ" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 13, ActionType = "Click", TargetSelector = "form button[type='submit']", Value = "", Description = "Nhấp Đăng tin lỗi hạn nộp" },
                    new TestStep { ScenarioName = "Jobs Validation E2E", StepOrder = 14, ActionType = "AssertText", TargetSelector = "span[data-valmsg-for='Deadline']", Value = "phải lớn hơn ngày hiện tại", Description = "Kiểm tra thông báo ngày hết hạn quá khứ" }
                });
                context.SaveChanges();
                Console.WriteLine("Seed Jobs Validation E2E TestSteps successfully!");
            }
        }
        catch (Exception tsEx)
        {
            Console.WriteLine("Lưu ý: Không thể khởi tạo bảng hoặc seed TestSteps: " + tsEx.Message);
        }

        // Khởi tạo dữ liệu tự động cho VueTemplates nếu chưa tồn tại ClassicHarvard hoặc NguyenMinhTrang
        try
        {
            if (!context.VueTemplates.Any(t => t.ComponentName == "ClassicHarvard"))
            {
                context.VueTemplates.Add(new VueTemplate
                {
                    TemplateName = "Classic Harvard",
                    ComponentName = "ClassicHarvard",
                    ThumbnailUrl = "/images/templates/ClassicHarvard.png",
                    IsPremium = false,
                    IsActive = true,
                    Category = "IT, Chuyên nghiệp, Tối giản, 1 cột",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
                Console.WriteLine("Seed VueTemplate ClassicHarvard thành công!");
            }

            if (!context.VueTemplates.Any(t => t.ComponentName == "NguyenMinhTrang"))
            {
                context.VueTemplates.Add(new VueTemplate
                {
                    TemplateName = "Nguyễn Minh Trang",
                    ComponentName = "NguyenMinhTrang",
                    ThumbnailUrl = "/images/templates/NguyenMinhTrang.png",
                    IsPremium = false,
                    IsActive = true,
                    Category = "Chuyên nghiệp, Banner pastel, 1 cột",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
                Console.WriteLine("Seed VueTemplate NguyenMinhTrang thành công!");
            }

            if (!context.VueTemplates.Any(t => t.ComponentName == "DoQuynhAnNhien"))
            {
                context.VueTemplates.Add(new VueTemplate
                {
                    TemplateName = "Đỗ Quỳnh An Nhiên",
                    ComponentName = "DoQuynhAnNhien",
                    ThumbnailUrl = "/images/templates/DoQuynhAnNhien.png",
                    IsPremium = false,
                    IsActive = true,
                    Category = "Cổ điển, Tối giản, Serif, 1 cột",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
                Console.WriteLine("Seed VueTemplate DoQuynhAnNhien thành công!");
            }
        }
        catch (Exception seedEx)
        {
            Console.WriteLine("Lỗi khi seed dữ liệu VueTemplates: " + seedEx.Message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi khi chạy Migration tự động: " + ex.Message);
    }
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);
// --- 4. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Đoạn lệnh tạo bảng bằng raw SQL đã được loại bỏ để sử dụng chuẩn Entity Framework Migrations

app.UseResponseCompression();
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

app.UseRateLimiter(); // Phải nằm giữa UseRouting và UseAuthentication

app.UseSession(); // Session phải nằm trước Authentication

app.UseAuthentication();
app.UseAuthorization();

// Đăng ký API Controllers (attribute routing - dùng cho [ApiController] + [Route(...)])
app.MapControllers();

// Đăng ký Health Check Endpoint trả về thông tin chi tiết dạng JSON
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var responseObj = new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(responseObj);
    }
});

app.MapHub<DoAnCS.Hubs.UserSessionHub>("/userSessionHub");

// Đăng ký MVC Controllers (conventional routing - dùng cho Views)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tải trước trình duyệt Playwright Chromium trong nền để tránh timeout khi chạy test E2E lần đầu
_ = Task.Run(() =>
{
    try
    {
        Console.WriteLine("[Playwright Startup] Đang tải/kiểm tra trình duyệt Chromium trong nền...");
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        Console.WriteLine($"[Playwright Startup] Hoàn tất kiểm tra Chromium với mã thoát: {exitCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Playwright Startup] Lỗi tải Chromium trong nền: {ex.Message}");
    }
});

app.Run();