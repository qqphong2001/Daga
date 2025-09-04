using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webdaga.DbContext;

namespace webdaga.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/chat/messages?page=1&size=50
        [HttpGet("messages")]
        public async Task<ActionResult> GetMessages([FromQuery] int page = 1, [FromQuery] int size = 50)
        {
            try
            {
                // Validate parameters
                if (page < 1) page = 1;
                if (size < 1 || size > 100) size = 50;

                var skip = (page - 1) * size;
                var totalCount = await _context.ChatMessages.CountAsync();

                var messages = await _context.ChatMessages
                    .OrderByDescending(m => m.Timestamp)
                    .Skip(skip)
                    .Take(size)
                    .Select(m => new
                    {
                        Id = m.Id,
                        User = m.User,
                        Message = m.Message,
                        Timestamp = m.Timestamp
                    })
                    .ToListAsync();

                var hasMore = (skip + size) < totalCount;

                return Ok(new
                {
                    Messages = messages.OrderBy(m => m.Timestamp).ToList(),
                    HasMore = hasMore,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = size
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Không thể tải tin nhắn", Details = ex.Message });
            }
        }

        // GET: api/chat/recent?count=50
        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentMessages([FromQuery] int count = 50)
        {
            try
            {
                if (count > 100) count = 100;

                var messages = await _context.ChatMessages
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .Select(m => new
                    {
                        Id = m.Id,
                        User = m.User,
                        Message = m.Message,
                        Timestamp = m.Timestamp
                    })
                    .ToListAsync();

                return Ok(messages.OrderBy(m => m.Timestamp));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Không thể tải tin nhắn gần nhất", Details = ex.Message });
            }
        }

        // GET: api/chat/stats
        [HttpGet("stats")]
        public async Task<ActionResult> GetChatStats()
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

                var topUsers = await _context.ChatMessages
                    .Where(m => m.Timestamp >= DateTime.Now.AddDays(-7))
                    .GroupBy(m => m.User)
                    .Select(g => new { User = g.Key, MessageCount = g.Count() })
                    .OrderByDescending(x => x.MessageCount)
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    TotalMessages = totalMessages,
                    TodayMessages = todayMessages,
                    ActiveUsers = activeUsers,
                    TopUsers = topUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Không thể tải thống kê", Details = ex.Message });
            }
        }

        // POST: api/chat/cleanup
        [HttpPost("cleanup")]
        public async Task<ActionResult> CleanupMessages([FromQuery] int keepCount = 5000)
        {
            try
            {
                var totalCount = await _context.ChatMessages.CountAsync();

                if (totalCount > keepCount)
                {
                    var messagesToDelete = await _context.ChatMessages
                        .OrderBy(m => m.Timestamp)
                        .Take(totalCount - keepCount)
                        .ToListAsync();

                    _context.ChatMessages.RemoveRange(messagesToDelete);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Message = $"Đã xóa {messagesToDelete.Count} tin nhắn cũ",
                        DeletedCount = messagesToDelete.Count,
                        RemainingCount = keepCount
                    });
                }

                return Ok(new { Message = "Không có tin nhắn nào cần xóa" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Không thể dọn dẹp tin nhắn", Details = ex.Message });
            }
        }

        // DELETE: api/chat/messages/{id}
        [HttpDelete("messages/{id}")]
        public async Task<ActionResult> DeleteMessage(int id, [FromHeader] string adminKey)
        {
            if (adminKey != "your-admin-key") // Thay đổi key này
            {
                return Unauthorized(new { Error = "Không có quyền truy cập" });
            }

            try
            {
                var message = await _context.ChatMessages.FindAsync(id);
                if (message == null)
                {
                    return NotFound(new { Error = "Không tìm thấy tin nhắn" });
                }

                _context.ChatMessages.Remove(message);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Đã xóa tin nhắn thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Không thể xóa tin nhắn", Details = ex.Message });
            }
        }

        // GET: api/chat/search?query=keyword&page=1&size=20
        [HttpGet("search")]
        public async Task<ActionResult> SearchMessages(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { Error = "Từ khóa tìm kiếm không được để trống" });
                }

                if (page < 1) page = 1;
                if (size < 1 || size > 50) size = 20;

                var skip = (page - 1) * size;

                var searchResults = await _context.ChatMessages
                    .Where(m => m.Message.Contains(query) || m.User.Contains(query))
                    .OrderByDescending(m => m.Timestamp)
                    .Skip(skip)
                    .Take(size)
                    .Select(m => new
                    {
                        Id = m.Id,
                        User = m.User,
                        Message = m.Message,
                        Timestamp = m.Timestamp
                    })
                    .ToListAsync();

                var totalResults = await _context.ChatMessages
                    .Where(m => m.Message.Contains(query) || m.User.Contains(query))
                    .CountAsync();

                return Ok(new
                {
                    Results = searchResults,
                    TotalResults = totalResults,
                    Query = query,
                    Page = page,
                    PageSize = size,
                    HasMore = (skip + size) < totalResults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Lỗi tìm kiếm", Details = ex.Message });
            }
        }
    }
}