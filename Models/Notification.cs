using System;

namespace DoAnCS.Models
{
    /// <summary>
    /// Model lưu trữ thông báo hệ thống cho người dùng.
    /// Mỗi thông báo gắn với 1 UserID cụ thể và có loại (Type) phân biệt.
    /// </summary>
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; } // FK → Users(UserID)
        public string Type { get; set; } = "System"; // Application, Interview, Login, System, Admin
        public string Title { get; set; } = ""; // Tiêu đề ngắn gọn
        public string Message { get; set; } = ""; // Nội dung chi tiết
        public string? Link { get; set; } // URL điều hướng khi nhấn vào thông báo
        public bool IsRead { get; set; } = false; // Trạng thái đã đọc
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual User? User { get; set; }
    }
}
