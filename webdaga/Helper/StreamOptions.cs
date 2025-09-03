namespace webdaga.Helper
{
    public sealed class StreamOptions
    {


        public string? WhipEndpoint { get; set; } // e.g. https://yourdomain.com/whip/yourapp/stream (media server WHIP)
        public string? WhipBearerToken { get; set; } // if your WHIP needs auth (optional)
        public string? RecordPath { get; set; } // e.g. C:\\livestream\\recorded
        public string? User { get; set; } // Basic Auth username for publishing
        public string? UserPassword { get; set; } // Basic Auth password for publishing
        public string? AdminPassword { get; set; } // Password to access /admin 
        public string? Admin  { get; set; } // Username to access /admin
        public string? streamUrl { get; set; } // e.g. rtmp://
    }
}
