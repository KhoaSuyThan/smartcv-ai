using System.Collections.Concurrent;

namespace DoAnCS.Services
{
    /// <summary>
    /// Quản lý phiên đăng nhập để ngăn chặn đa thiết bị (SSO enforcement).
    /// Chiến lược Hybrid: memory cache 30 giây để giảm DB query, DB làm nguồn sự thật sau restart.
    /// </summary>
    public static class SessionTracker
    {
        // Cache ngắn hạn trong RAM: UserID → (LastLoginTime, CacheExpiry)
        // Tránh query DB mỗi HTTP request — tự động expire sau 30 giây
        private static readonly ConcurrentDictionary<int, (long LoginTime, DateTime Expiry)> _memCache = new();

        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Cập nhật session mới nhất — ghi vào cache, DB được ghi riêng trong AccountController.
        /// </summary>
        public static void UpdateSession(int userId, long loginTime)
        {
            // Luôn ghi vào cache với thời gian mới nhất
            _memCache[userId] = (loginTime, DateTime.UtcNow.Add(CacheDuration));
        }

        /// <summary>
        /// Kiểm tra session còn hợp lệ không từ cache.
        /// Trả về null nếu cache đã hết hạn → cần query DB.
        /// </summary>
        public static bool? IsValidSessionFromCache(int userId, long loginTime)
        {
            if (_memCache.TryGetValue(userId, out var cached))
            {
                if (DateTime.UtcNow <= cached.Expiry)
                {
                    // Cache còn sống: so sánh trực tiếp
                    return loginTime >= cached.LoginTime;
                }
                // Cache hết hạn: xóa để force query DB
                _memCache.TryRemove(userId, out _);
            }
            return null; // Không có trong cache → cần DB
        }

        /// <summary>
        /// Cập nhật cache sau khi đọc được giá trị mới từ DB.
        /// </summary>
        public static void RefreshCache(int userId, long loginTime)
        {
            _memCache[userId] = (loginTime, DateTime.UtcNow.Add(CacheDuration));
        }
    }
}
