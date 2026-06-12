using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DoAnCS.Data;
using Microsoft.AspNetCore.SignalR;

namespace DoAnCS.Services
{
    public class ProExpirationService : BackgroundService
    {
        private readonly ILogger<ProExpirationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ProExpirationService(ILogger<ProExpirationService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProExpirationService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("ProExpirationService is checking for expired Pro packages.");
                    
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // Đồng bộ hóa các tài khoản Pro CŨ (đã mua trước khi có tính năng tự động hết hạn)
                        var legacyProUsers = context.Users
                            .Where(u => u.IsPro == true && !u.ProExpirationDate.HasValue)
                            .ToList();

                        if (legacyProUsers.Any())
                        {
                            foreach (var user in legacyProUsers)
                            {
                                // Tìm lịch sử mua gói Pro gần nhất của user này
                                var lastUpgrade = context.UpgradeRequests
                                    .Where(r => r.UserID == user.UserID && r.Status == 1 && r.DecisionDate.HasValue)
                                    .OrderByDescending(r => r.DecisionDate)
                                    .FirstOrDefault();

                                if (lastUpgrade != null)
                                {
                                    user.ProExpirationDate = lastUpgrade.DecisionDate.Value.AddMonths(1);
                                    _logger.LogInformation($"Backfilled ProExpirationDate for legacy User {user.UserID} to {user.ProExpirationDate}");
                                }
                            }
                            await context.SaveChangesAsync(stoppingToken);
                        }

                        // Tìm các User có trạng thái IsPro = true, có ngày hết hạn và ngày đó < ngày hiện tại
                        var expiredUsers = context.Users
                            .Where(u => u.IsPro == true && u.ProExpirationDate.HasValue && u.ProExpirationDate.Value < DateTime.Now)
                            .ToList();

                        if (expiredUsers.Any())
                        {
                            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                            var hubContext = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<DoAnCS.Hubs.UserSessionHub>>();

                            foreach (var user in expiredUsers)
                            {
                                user.IsPro = false;
                                _logger.LogInformation($"Auto canceled Pro package for User {user.UserID} - {user.Email} (Expired at: {user.ProExpirationDate})");

                                // 1. Gửi Email thông báo
                                string emailSubject = "Thông báo: Gói Pro của bạn đã hết hạn";
                                string emailBody = $@"
                                    <h3>Xin chào {user.FullName},</h3>
                                    <p>Gói CVBuilder Pro của bạn đã chính thức hết hạn vào ngày {user.ProExpirationDate?.ToString("dd/MM/yyyy")}.</p>
                                    <p>Cảm ơn bạn đã đồng hành và sử dụng các tính năng cao cấp của chúng tôi trong suốt thời gian qua.</p>
                                    <p>Để tiếp tục sử dụng các mẫu CV độc quyền và tính năng Smart Match (AI), vui lòng gia hạn gói Pro bằng cách đăng nhập vào hệ thống và chọn ""Nâng cấp Pro"".</p>
                                    <br/>
                                    <p>Trân trọng,<br/>Đội ngũ CVBuilder Pro</p>
                                ";
                                _ = emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                                // 2. Gửi SignalR Event cho trình duyệt reload ngay lập tức
                                var connectionIds = DoAnCS.Hubs.UserSessionHub.GetConnectionIds(user.UserID);
                                foreach (var conn in connectionIds)
                                {
                                    await hubContext.Clients.Client(conn).SendAsync("ProExpiredNotification");
                                }
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing ProExpirationService.");
                }

                // Chờ 24 giờ (1 ngày) trước khi quét lại
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
