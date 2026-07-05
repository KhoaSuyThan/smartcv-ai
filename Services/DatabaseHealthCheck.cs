using Microsoft.Extensions.Diagnostics.HealthChecks;
using DoAnCS.Data;

namespace DoAnCS.Services
{
    // Lớp kiểm tra sức khỏe của cơ sở dữ liệu SQL Server
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        public DatabaseHealthCheck(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Kiểm tra xem ứng dụng có thể kết nối thành công tới Database hay không
                if (await _context.Database.CanConnectAsync(cancellationToken))
                {
                    return HealthCheckResult.Healthy("Kết nối SQL Server ổn định.");
                }
                return HealthCheckResult.Unhealthy("Không thể kết nối đến SQL Server.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Lỗi kết nối cơ sở dữ liệu.", ex);
            }
        }
    }
}
