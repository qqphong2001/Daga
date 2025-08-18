namespace webdaga.Helper
{
    public sealed class StreamOptions
    {
        public string? HlsPublicUrl { get; set; } // e.g. https://yourdomain.com/hls/stream.m3u8
        public string? HlsLocalProbeUrl { get; set; } // e.g. http://127.0.0.1/hls/stream.m3u8 for HEAD checks
        public string? VodPhysicalPath { get; set; } // e.g. C:\\livestream\\vod
        public string? VodPublicBaseUrl { get; set; } // e.g. /vod
        public string? WhipEndpoint { get; set; } // e.g. https://yourdomain.com/whip/yourapp/stream (media server WHIP)
        public string? WhipBearerToken { get; set; } // if your WHIP needs auth (optional)
    }
}
