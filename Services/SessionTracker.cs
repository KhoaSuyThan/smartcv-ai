using System.Collections.Concurrent;

namespace DoAnCS.Services
{
    public static class SessionTracker
    {
        // UserID -> Latest LoginTime (Ticks)
        private static readonly ConcurrentDictionary<int, long> UserLatestLoginTime = new();

        public static void UpdateSession(int userId, long loginTime)
        {
            UserLatestLoginTime.AddOrUpdate(userId, loginTime, (key, existingVal) => Math.Max(existingVal, loginTime));
        }

        public static bool IsValidSession(int userId, long loginTime)
        {
            if (UserLatestLoginTime.TryGetValue(userId, out var latestLoginTime))
            {
                // Nếu thời gian login của Cookie cũ hơn thời gian login mới nhất -> Không hợp lệ
                return loginTime >= latestLoginTime;
            }
            
            // Nếu server vừa khởi động lại (Dictionary trống), chấp nhận request đầu tiên 
            // và lưu lại LoginTime của nó để so sánh sau này.
            UserLatestLoginTime.TryAdd(userId, loginTime);
            return true;
        }
    }
}
