using Microsoft.EntityFrameworkCore;
using webdaga.DbContext;

namespace webdaga.Services
{
    public class VideoCleanupService : BackgroundService
    {
        private readonly ILogger<VideoCleanupService> _logger;
        private readonly string _videoDirectory = "/var/www/XoGa/wwwroot/recordings/mystream";
        private readonly TimeSpan _maxVideoAge = TimeSpan.FromHours(36);
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); 
        private readonly IServiceScopeFactory _scopeFactory;
        public VideoCleanupService(ILogger<VideoCleanupService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Video Cleanup Service đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Tính toán thời gian đợi đến 24h (00:00)
                    var now = DateTime.Now;
                    var nextMidnight = now.Date.AddDays(1); // 24h hôm nay
                    var timeUntilMidnight = nextMidnight - now;

                    _logger.LogInformation($"Đợi {timeUntilMidnight.TotalHours:F1} giờ nữa để chạy cleanup lúc 24h");

                    // Đợi đến 24h
                    await Task.Delay(timeUntilMidnight, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await CleanupOldVideos();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Video Cleanup Service đã được dừng");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Video Cleanup Service");
                    // Đợi 1 giờ trước khi thử lại
                    await Task.Delay(_checkInterval, stoppingToken);
                }
            }
        }

        private async Task CleanupOldVideos()
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                _logger.LogInformation($"Bắt đầu cleanup video trong thư mục: {_videoDirectory}");

                if (!Directory.Exists(_videoDirectory))
                {
                    _logger.LogWarning($"Thư mục không tồn tại: {_videoDirectory}");
                    return;
                }

                var videoFiles = Directory.GetFiles(_videoDirectory, "*.mp4");
                var cutoffTime = DateTime.Now - _maxVideoAge;
                int deletedCount = 0;

                foreach (var filePath in videoFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(filePath);

                        if (fileInfo.CreationTime < cutoffTime)
                        {
                            _logger.LogInformation($"Xóa video cũ: {fileInfo.Name} (Tạo lúc: {fileInfo.CreationTime})");

                            File.Delete(filePath);

                            var articles = _context.Articles.ToList(); 

                            var article = articles
                                .FirstOrDefault(a => Path.GetFileName(a.VideoUrl) == fileInfo.Name);
                            if (article != null)
                            {
                                _context.Articles.Remove(article);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"Xóa bản ghi DB cho video: {fileInfo.Name}");
                            }

                            deletedCount++;
                        }
                        else
                        {
                            _logger.LogDebug($"Giữ video: {fileInfo.Name} (Tạo lúc: {fileInfo.CreationTime})");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi xóa file: {filePath}");
                    }
                }

                _logger.LogInformation($"Cleanup hoàn thành. Đã xóa {deletedCount} video cũ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình cleanup video");
            }
        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Video Cleanup Service đang dừng...");
            return base.StopAsync(cancellationToken);
        }
    }
}
