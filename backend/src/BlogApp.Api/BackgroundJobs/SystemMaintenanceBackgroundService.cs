using BlogApp.Application.Features.System.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlogApp.Api.BackgroundJobs;

/// <summary>
/// Hosted Service chạy ngầm định kỳ mỗi 6 giờ thực hiện bảo trì hệ thống:
/// - Xoá phiên đánh giá và OTP hết hạn.
/// - Reset giới hạn email SMTP khi sang ngày mới.
/// - Tổng hợp thống kê truy cập tháng trước rồi dọn dẹp log quá 30 ngày.
/// </summary>
public class SystemMaintenanceBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SystemMaintenanceBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(6);

    public SystemMaintenanceBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SystemMaintenanceBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SystemMaintenanceBackgroundService đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var maintenanceService = scope.ServiceProvider.GetRequiredService<SystemMaintenanceService>();
                await maintenanceService.RunMaintenanceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình chạy chu trình bảo trì hệ thống.");
            }

            try
            {
                await Task.Delay(_period, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("SystemMaintenanceBackgroundService đã dừng.");
    }
}
