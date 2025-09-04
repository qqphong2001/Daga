using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using webdaga.DbContext;
using webdaga.Models;

namespace webdaga.SignalR
{
    public class ChatHub : Hub
    {
        private static int _viewerCount = 0;
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            _viewerCount++;
            await Clients.All.SendAsync("UpdateViewerCount", _viewerCount);

            // Gửi 50 tin nhắn gần nhất cho người mới vào (giống code cũ nhưng được tối ưu)
            try
            {
                var messages = await _context.ChatMessages
                    .OrderByDescending(m => m.Timestamp) // Lấy tin nhắn mới nhất trước
                    .Take(50)
                    .Select(m => new { m.User, m.Message, m.Timestamp })
                    .ToListAsync();

                // Gửi theo thứ tự thời gian (cũ nhất trước)
                var orderedMessages = messages.OrderBy(m => m.Timestamp).ToList();

                foreach (var msg in orderedMessages)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", msg.User, msg.Message, msg.Timestamp);
                }

                // Thông báo đã load xong tin nhắn ban đầu
                await Clients.Caller.SendAsync("InitialMessagesLoaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading initial messages: {ex.Message}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _viewerCount = Math.Max(0, _viewerCount - 1);
            await Clients.All.SendAsync("UpdateViewerCount", _viewerCount);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
                return;

            // Giới hạn độ dài tin nhắn và tên user
            user = user.Length > 50 ? user.Substring(0, 50) : user;
            message = message.Length > 500 ? message.Substring(0, 500) : message;

            var chatMessage = new ChatMessageModel
            {
                User = user,
                Message = message,
                Timestamp = DateTime.Now
            };

            try
            {
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                // Gửi tin nhắn đến tất cả clients với timestamp
                await Clients.All.SendAsync("ReceiveMessage", user, message, chatMessage.Timestamp);

                // Dọn dẹp tin nhắn cũ định kỳ (chạy async không block)
                _ = Task.Run(async () => await CleanupOldMessagesAsync());
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                Console.WriteLine($"Error saving message: {ex.Message}");
            }
        }

        public async Task StartLive()
        {
            await Clients.All.SendAsync("LiveStarted");
        }

        // Method mới để lấy tin nhắn theo trang
        public async Task LoadMessages(int page = 1, int size = 50)
        {
            try
            {
                // Validate parameters
                if (page < 1) page = 1;
                if (size < 1 || size > 100) size = 50;

                var skip = (page - 1) * size;

                // Đếm tổng số tin nhắn
                var totalCount = await _context.ChatMessages.CountAsync();

                // Lấy tin nhắn theo trang (từ mới nhất)
                var messages = await _context.ChatMessages
                    .OrderByDescending(m => m.Timestamp)
                    .Skip(skip)
                    .Take(size)
                    .Select(m => new
                    {
                        User = m.User,
                        Message = m.Message,
                        Timestamp = m.Timestamp
                    })
                    .ToListAsync();

                var hasMore = (skip + size) < totalCount;

                // Gửi kết quả về cho client yêu cầu
                await Clients.Caller.SendAsync("MessagesLoaded", new
                {
                    Messages = messages.OrderBy(m => m.Timestamp).ToList(), // Sắp xếp lại theo thời gian tăng dần
                    HasMore = hasMore,
                    TotalCount = totalCount,
                    CurrentPage = page
                });
            }
            catch (Exception ex)
            {
                // Gửi lỗi về client
                await Clients.Caller.SendAsync("LoadMessagesError", "Không thể tải tin nhắn");
                Console.WriteLine($"Error loading messages: {ex.Message}");
            }
        }

        // Method để lấy tin nhắn gần nhất (dùng khi mới connect)
        public async Task LoadRecentMessages(int count = 50)
        {
            try
            {
                if (count > 100) count = 100;

                var messages = await _context.ChatMessages
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .Select(m => new
                    {
                        User = m.User,
                        Message = m.Message,
                        Timestamp = m.Timestamp
                    })
                    .ToListAsync();

                // Gửi tin nhắn theo thứ tự thời gian (cũ nhất trước)
                var orderedMessages = messages.OrderBy(m => m.Timestamp).ToList();

                await Clients.Caller.SendAsync("RecentMessagesLoaded", orderedMessages);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("LoadRecentMessagesError", "Không thể tải tin nhắn gần nhất");
                Console.WriteLine($"Error loading recent messages: {ex.Message}");
            }
        }

        // Method để dọn dẹp tin nhắn cũ
        private async Task CleanupOldMessagesAsync()
        {
            try
            {
                const int maxMessages = 10000; // Giữ lại tối đa 10000 tin nhắn
                var totalCount = await _context.ChatMessages.CountAsync();

                if (totalCount > maxMessages)
                {
                    var messagesToDelete = await _context.ChatMessages
                        .OrderBy(m => m.Timestamp)
                        .Take(totalCount - maxMessages)
                        .ToListAsync();

                    _context.ChatMessages.RemoveRange(messagesToDelete);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Cleaned up {messagesToDelete.Count} old messages");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up messages: {ex.Message}");
            }
        }

        // Method để lấy thống kê chat
        public async Task GetChatStats()
        {
            try
            {
                var totalMessages = await _context.ChatMessages.CountAsync();
                var todayMessages = await _context.ChatMessages
                    .Where(m => m.Timestamp.Date == DateTime.Today)
                    .CountAsync();

                var activeUsers = await _context.ChatMessages
                    .Where(m => m.Timestamp >= DateTime.Now.AddHours(-24))
                    .Select(m => m.User)
                    .Distinct()
                    .CountAsync();

                await Clients.Caller.SendAsync("ChatStatsLoaded", new
                {
                    TotalMessages = totalMessages,
                    TodayMessages = todayMessages,
                    ActiveUsers = activeUsers,
                    OnlineViewers = _viewerCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting chat stats: {ex.Message}");
            }
        }

        // Method để admin xóa tin nhắn
        public async Task DeleteMessage(int messageId, string adminKey)
        {
            // Kiểm tra quyền admin (thay thế bằng logic authentication thực tế)
            if (adminKey != "your-admin-key") // Thay đổi key này
            {
                await Clients.Caller.SendAsync("UnauthorizedAction");
                return;
            }

            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message != null)
                {
                    _context.ChatMessages.Remove(message);
                    await _context.SaveChangesAsync();

                    // Thông báo cho tất cả client về việc xóa tin nhắn
                    await Clients.All.SendAsync("MessageDeleted", messageId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting message: {ex.Message}");
            }
        }

        // Method để ban user tạm thời
        public async Task BanUser(string username, int durationMinutes, string adminKey)
        {
            90
            if (adminKey != "your-admin-key")
            {
                await Clients.Caller.SendAsync("UnauthorizedAction");
                return;
            }

            // Logic ban user - có thể lưu vào database hoặc cache
            await Clients.All.SendAsync("UserBanned", username, durationMinutes);
        }
    }
}