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

// Cấu hình Rate Limiting (Chống Spam API/Form)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ContactLimiter", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 3; // Tối đa 3 tin nhắn
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    
    // Tùy chỉnh thông báo lỗi khi vượt giới hạn (429 Too Many Requests)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
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

        context.Database.Migrate();

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

app.Run();