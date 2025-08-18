(function () {
    async function start(opts) {
        const { videoElId, startBtnId, stopBtnId, resolutionSelId, whipEndpoint, bearerToken } = opts;
        const videoEl = document.getElementById(videoElId);
        const btnStart = document.getElementById(startBtnId);
        const btnStop = document.getElementById(stopBtnId);
        const resSel = document.getElementById(resolutionSelId);
        const statusEl = document.getElementById('status');

        let pc, localStream, resourceUrl;

        function log(msg, isErr) { statusEl.textContent = msg; statusEl.style.color = isErr ? '#a00' : '#0a0'; }

        async function getConstraints() {
            const [w, h] = (resSel.value || '1280x720').split('x').map(Number);
            return {
                audio: { echoCancellation: true, noiseSuppression: true },
                video: { width: { ideal: w }, height: { ideal: h }, frameRate: { ideal: 30 } }
            };
        }

        async function openMedia() {
            if (localStream) { localStream.getTracks().forEach(t => t.stop()); }
            localStream = await navigator.mediaDevices.getUserMedia(await getConstraints());
            videoEl.srcObject = localStream;
            videoEl.muted = true; // preview without echo
            await videoEl.play();
        }

        async function createPeer() {
            pc = new RTCPeerConnection();
            localStream.getTracks().forEach(t => pc.addTrack(t, localStream));
            return pc;
        }

        async function publish() {
            await openMedia();
            await createPeer();

            const offer = await pc.createOffer({ offerToReceiveAudio: false, offerToReceiveVideo: false });
            await pc.setLocalDescription(offer);

            const headers = { 'Content-Type': 'application/sdp' };
            if (bearerToken) headers['Authorization'] = `Bearer ${bearerToken}`;

            // Create WHIP resource
            const res = await fetch(whipEndpoint, { method: 'POST', headers, body: offer.sdp });
            if (!res.ok) { throw new Error('WHIP POST failed: ' + res.status); }

            const answerSdp = await res.text();
            await pc.setRemoteDescription({ type: 'answer', sdp: answerSdp });

            resourceUrl = res.headers.get('Location'); // WHIP resource URL for teardown/ICE

            // Trickle ICE: send candidates via PATCH if supported
            pc.onicecandidate = async (evt) => {
                if (!evt.candidate || !resourceUrl) return;
                try {
                    const hdr = { 'Content-Type': 'application/trickle-ice-sdpfrag' };
                    if (bearerToken) hdr['Authorization'] = `Bearer ${bearerToken}`;
                    const frag = `a=ice-ufrag:${pc.localDescription.sdp.match(/ice-ufrag:([^\r\n]+)/)[1]}\r\n` +
                        `a=ice-pwd:${pc.localDescription.sdp.match(/ice-pwd:([^\r\n]+)/)[1]}\r\n` +
                        `a=candidate:${evt.candidate.candidate}`;
                    await fetch(resourceUrl, { method: 'PATCH', headers: hdr, body: frag });
                } catch (e) { console.warn('ICE PATCH error', e); }
            }

            btnStart.disabled = true; btnStop.disabled = false; log('Đang phát livestream…');
        }

        async function stop() {
            try {
                if (resourceUrl) {
                    const headers = {};
                    if (bearerToken) headers['Authorization'] = `Bearer ${bearerToken}`;
                    await fetch(resourceUrl, { method: 'DELETE', headers });
                }
            } catch (e) { console.warn('WHIP DELETE error', e); }
            try { pc && pc.close(); } catch { }
            try { localStream && localStream.getTracks().forEach(t => t.stop()); } catch { }
            btnStart.disabled = false; btnStop.disabled = true; log('Đã dừng.');
        }

        btnStart.onclick = publish;
        btnStop.onclick = stop;

        // open camera preview immediately with default constraints
        openMedia().catch(err => log('Không mở được camera: ' + err.message, true));
    }

    window.initWhipPublisher = (opts) => start(opts);
})();