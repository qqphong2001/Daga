window.setupHlsPlayer = function (videoId, src) {
    const video = document.getElementById(videoId);
    if (video.canPlayType('application/vnd.apple.mpegurl')) {
        video.src = src; // Safari iOS/macOS supports native HLS
    } else if (window.Hls) {
        const hls = new Hls({
            liveDurationInfinity: true,
            lowLatencyMode: false
        });
        hls.loadSource(src);
        hls.attachMedia(video);
    } else {
        video.src = src; // fallback
    }
}