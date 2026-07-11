using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Hubs;
using DoAnCS.Models;

namespace DoAnCS.Controllers
{
    /// <summary>
    /// API Controller quản lý thông báo hệ thống cho người dùng.
    /// Hỗ trợ lấy danh sách, đếm chưa đọc, đánh dấu đã đọc và xóa thông báo.
    /// </summary>
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<UserSessionHub> _hubContext;

        public NotificationController(AppDbContext context, IHubContext<UserSessionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Lấy danh sách 20 thông báo gần nhất của người dùng hiện tại.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (CurrentUserId == 0) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserID == CurrentUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.NotificationID,
                    n.Type,
                    n.Title,
                    n.Message,
                    n.Link,
                    n.IsRead,
                    n.CreatedAt,
                    // Tính thời gian tương đối (VD: "5 phút trước") ở phía server
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                })
                .ToListAsync();

            return Json(notifications);
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc để hiển thị badge trên chuông.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            if (CurrentUserId == 0) return Unauthorized();

            var count = await _context.Notifications
                .CountAsync(n => n.UserID == CurrentUserId && !n.IsRead);

            return Json(new { count });
        }

        /// <summary>
        /// Đánh dấu 1 thông báo là đã đọc.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (CurrentUserId == 0) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == CurrentUserId);

            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của user hiện tại là đã đọc.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (CurrentUserId == 0) return Unauthorized();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserID == CurrentUserId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, count = unreadNotifications.Count });
        }

        /// <summary>
        /// Xóa 1 thông báo cụ thể.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == CurrentUserId);

            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ==========================================
        // HELPER: Tạo thông báo mới + đẩy real-time qua SignalR
        // ==========================================

        /// <summary>
        /// Static helper tạo thông báo mới và đẩy real-time qua SignalR.
        /// Được gọi từ các Controller nghiệp vụ khác.
        /// </summary>
        public static async Task CreateNotification(
            AppDbContext context,
            IHubContext<UserSessionHub> hubContext,
            int userId, string type, string title, string message, string? link = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserID = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    Link = link,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                // Đẩy thông báo real-time qua SignalR tới tất cả kết nối đang hoạt động của user
                var connectionIds = UserSessionHub.GetConnectionIds(userId).ToList();
                if (connectionIds.Any())
                {
                    await hubContext.Clients.Clients(connectionIds)
                        .SendAsync("ReceiveNotification", new
                        {
                            notification.NotificationID,
                            notification.Type,
                            notification.Title,
                            notification.Message,
                            notification.Link,
                            notification.IsRead,
                            notification.CreatedAt,
                            TimeAgo = "Vừa xong"
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo thông báo: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm tính thời gian tương đối (VD: "5 phút trước", "Hôm qua").
        /// </summary>
        private static string GetTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.Now - createdAt;

            if (diff.TotalSeconds < 60) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 2) return "Hôm qua";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} tuần trước";
            return createdAt.ToString("dd/MM/yyyy");
        }
    }
}
