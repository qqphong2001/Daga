using Microsoft.AspNetCore.SignalR;
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

        // Gửi tin nhắn cũ cho người mới vào
        var messages = _context.ChatMessages.OrderBy(m => m.Timestamp).Take(50).ToList();
        foreach (var msg in messages)
        {
                await Clients.Caller.SendAsync("ReceiveMessage", msg.User, msg.Message, msg.Timestamp);
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
        var chatMessage = new ChatMessageModel
        {
            User = user,
            Message = message,
            Timestamp = DateTime.Now
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
}
