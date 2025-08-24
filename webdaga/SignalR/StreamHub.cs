using Microsoft.AspNetCore.SignalR;

namespace webdaga.SignalR
{
    namespace LiveStreamApp.Hubs
    {
        public class StreamHub : Hub
        {
            public async Task SendFrame(byte[] frame)
            {
                // Broadcast video frame to all connected clients
                await Clients.All.SendAsync("ReceiveFrame", frame);
            }

            public async Task NotifyStreamStarted()
            {
                await Clients.All.SendAsync("StreamStarted");
            }

            public async Task NotifyStreamStopped(string videoPath)
            {
                await Clients.All.SendAsync("StreamStopped", videoPath);
            }
        }
    }
}
