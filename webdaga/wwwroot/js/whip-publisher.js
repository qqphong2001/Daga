(function () {
    async function start(opts) {
        const { videoElId, startBtnId, stopBtnId, resolutionSelId, whipEndpoint, bearerToken, admin, adminPassword } = opts;
        const videoEl = document.getElementById(videoElId);
        const btnStart = document.getElementById(startBtnId);
        const btnStop = document.getElementById(stopBtnId);
        const resSel = document.getElementById(resolutionSelId);
        const statusEl = document.getElementById('status');
        const overlayCanvas = document.getElementById('overlayCanvas');
        const ctx = overlayCanvas.getContext('2d');

        let pc, localStream, resourceUrl;
        let showScoreboard = true;
        let greenScore = 0;
        let redScore = 0;
        let pairScore = 0;
        let isDrawing = false;
        let rotationAngle = 0; // 0, 90, 180, 270 degrees
        let isLandscapeMode = false;

        function log(msg, isErr) {
            console.log(msg);
            statusEl.textContent = msg;
            statusEl.style.color = isErr ? '#a00' : '#0a0';
        }

        // Hàm xoay video và canvas
        function rotateStream(angle) {
            rotationAngle = angle;
            isLandscapeMode = (angle === 90 || angle === 270);

            // Apply CSS transform to video element
            videoEl.style.transform = `rotate(${angle}deg)`;
            overlayCanvas.style.transform = `rotate(${angle}deg)`;

            // Adjust container size for landscape mode
            const container = videoEl.parentElement;
            if (isLandscapeMode) {
                container.style.width = '100vh';
                container.style.height = '100vw';
                container.style.maxWidth = 'none';
                container.style.maxHeight = 'none';
            } else {
                container.style.width = '';
                container.style.height = '';
                container.style.maxWidth = '';
                container.style.maxHeight = '';
            }

            log(`Xoay màn hình: ${angle}°`);
        }


        function drawScoreboard() {
            if (!showScoreboard || !ctx) return;

            // Save current context
            ctx.save();

            // Nếu đang ở chế độ xoay, cần điều chỉnh tọa độ
            if (rotationAngle !== 0) {
                const centerX = overlayCanvas.width / 2;
                const centerY = overlayCanvas.height / 2;

                // Move to center, rotate, then move back
                ctx.translate(centerX, centerY);
                ctx.rotate(-rotationAngle * Math.PI / 180); // Rotate opposite direction
                ctx.translate(-centerX, -centerY);
            }

            // Kích thước và vị trí bảng điện tử
            let boardWidth = 400;
            let boardHeight = 80;
            let x = 10;
            let y = 10;

            // Điều chỉnh vị trí khi xoay
            if (isLandscapeMode) {
                if (rotationAngle === 90) {
                    x = overlayCanvas.width - boardHeight - 10;
                    y = 10;
                    [boardWidth, boardHeight] = [boardHeight, boardWidth]; // Swap dimensions
                } else if (rotationAngle === 270) {
                    x = 10;
                    y = overlayCanvas.height - boardWidth - 10;
                    [boardWidth, boardHeight] = [boardHeight, boardWidth]; // Swap dimensions
                }
            }

            // Nền đen với độ trong suốt
            ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
            ctx.fillRect(x, y, boardWidth, boardHeight);

            // Viền trắng
            ctx.strokeStyle = 'white';
            ctx.lineWidth = 3;
            ctx.strokeRect(x, y, boardWidth, boardHeight);

            // Chia thành 3 phần: Xanh | Cặp | Đỏ
            const sectionWidth = boardWidth / 3;

            // Vẽ đường phân chia dọc
            ctx.strokeStyle = 'white';
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.moveTo(x + sectionWidth, y);
            ctx.lineTo(x + sectionWidth, y + boardHeight);
            ctx.moveTo(x + sectionWidth * 2, y);
            ctx.lineTo(x + sectionWidth * 2, y + boardHeight);
            ctx.stroke();

            // Font size điều chỉnh theo kích thước board
            const titleFont = isLandscapeMode ? 'bold 16px Arial' : 'bold 28px Arial';
            const scoreFont = isLandscapeMode ? 'bold 20px Arial' : 'bold 36px Arial';
            const titleY = y + (isLandscapeMode ? 25 : 25);
            const scoreY = y + (isLandscapeMode ? 50 : 65);

            // Phần XANH (bên trái)
            ctx.fillStyle = '#0000FF';
            ctx.font = titleFont;
            ctx.textAlign = 'center';
            ctx.fillText('Xanh', x + sectionWidth / 2, titleY);
            ctx.fillStyle = 'white';
            ctx.font = scoreFont;
            ctx.fillText(greenScore.toString(), x + sectionWidth / 2, scoreY);

            // Phần CẶP (giữa)
            ctx.fillStyle = 'white';

            ctx.font = titleFont;
            ctx.fillText('Cặp', x + sectionWidth + sectionWidth / 2, titleY);
            ctx.fillStyle = '#f1c40f';

            ctx.font = scoreFont;
            ctx.fillText(pairScore.toString(), x + sectionWidth + sectionWidth / 2, scoreY);

            // Phần ĐỎ (bên phải)
            ctx.fillStyle = '#FF0000';
            ctx.font = titleFont;
            ctx.fillText('Đỏ', x + sectionWidth * 2 + sectionWidth / 2, titleY);
            ctx.fillStyle = 'white';
            ctx.font = scoreFont;
            ctx.fillText(redScore.toString(), x + sectionWidth * 2 + sectionWidth / 2, scoreY);

            // Restore context
            ctx.restore();
        }

        async function getConstraints() {
            const [width, height] = resSel.value.split('x').map(Number);
            return {
                video: {
                    facingMode: "environment",
                    width: { ideal: width },
                    height: { ideal: height },
                    frameRate: { ideal: 60 }
                },
                audio: {
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            };
        }

        async function openMedia() {
            if (localStream) {
                localStream.getTracks().forEach(t => t.stop());
                localStream = null;
            }

            try {
                log('Đang mở camera...');
                localStream = await navigator.mediaDevices.getUserMedia(await getConstraints());
            } catch (e) {
                log('Không mở được camera với cài đặt yêu cầu, thử cài đặt mặc định...', false);
                try {
                    localStream = await navigator.mediaDevices.getUserMedia({
                        video: { facingMode: "environment" },
                        audio: true
                    });
                } catch (e2) {
                    log('Không thể mở camera: ' + e2.message, true);
                    throw e2;
                }
            }

            videoEl.srcObject = localStream;
            videoEl.muted = true;

            // Chờ video load và play
            return new Promise((resolve, reject) => {
                videoEl.onloadedmetadata = async () => {
                    try {
                        await videoEl.play();

                        const waitForDimensions = () => {
                            if (videoEl.videoWidth > 0 && videoEl.videoHeight > 0) {
                                // Setup canvas với kích thước video thực tế
                                overlayCanvas.width = videoEl.videoWidth;
                                overlayCanvas.height = videoEl.videoHeight;

                                // Set style để hiển thị đúng tỷ lệ
                                overlayCanvas.style.width = videoEl.clientWidth + 'px';
                                overlayCanvas.style.height = videoEl.clientHeight + 'px';

                                log(`Camera ready: ${videoEl.videoWidth}x${videoEl.videoHeight}`);
                                resolve();
                            } else {
                                setTimeout(waitForDimensions, 100);
                            }
                        };
                        waitForDimensions();
                    } catch (e) {
                        reject(e);
                    }
                };

                videoEl.onerror = () => {
                    reject(new Error('Lỗi khi load video'));
                };
            });
        }

        function startDrawingLoop() {
            if (isDrawing) return;
            isDrawing = true;

            function drawFrame() {
                if (!isDrawing || !videoEl.videoWidth || !videoEl.videoHeight) {
                    if (isDrawing) requestAnimationFrame(drawFrame);
                    return;
                }

                try {
                    // Clear canvas
                    ctx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);

                    // Save context for rotation
                    ctx.save();

                    // Apply rotation to the entire canvas content
                    if (rotationAngle !== 0) {
                        const centerX = overlayCanvas.width / 2;
                        const centerY = overlayCanvas.height / 2;
                        ctx.translate(centerX, centerY);
                        ctx.rotate(rotationAngle * Math.PI / 180);
                        ctx.translate(-centerX, -centerY);
                    }

                    // Vẽ video frame
                    ctx.drawImage(videoEl, 0, 0, overlayCanvas.width, overlayCanvas.height);

                    // Restore context
                    ctx.restore();

                
                    drawScoreboard();

                } catch (e) {
                    console.error('Lỗi khi vẽ frame:', e);
                }

                if (isDrawing) {
                    requestAnimationFrame(drawFrame);
                }
            }

            drawFrame();
        }

        function stopDrawingLoop() {
            isDrawing = false;
        }

        async function createPeer() {
            pc = new RTCPeerConnection({
                iceServers: [
                    { urls: 'stun:stun.l.google.com:19302' }
                ]
            });

            startDrawingLoop();
            await new Promise(resolve => setTimeout(resolve, 500));

            const canvasStream = overlayCanvas.captureStream(30);
            const videoTracks = canvasStream.getVideoTracks();
            const audioTracks = localStream.getAudioTracks();

            log(`Canvas stream - Video tracks: ${videoTracks.length}, Audio tracks: ${audioTracks.length}`);

            if (videoTracks.length === 0) {
                throw new Error('Không thể capture video từ canvas');
            }

            videoTracks.forEach(track => {
                log(`Adding video track: ${track.kind}, state: ${track.readyState}`);
                pc.addTrack(track, canvasStream);
            });

            audioTracks.forEach(track => {
                log(`Adding audio track: ${track.kind}, state: ${track.readyState}`);
                pc.addTrack(track, localStream);
            });

            // Ưu tiên codec H.264
            const transceivers = pc.getTransceivers();
            transceivers.forEach(transceiver => {
                if (transceiver.sender.track && transceiver.sender.track.kind === "video") {
                    const capabilities = RTCRtpSender.getCapabilities("video");
                    if (capabilities && capabilities.codecs) {
                        const codecs = capabilities.codecs;
                        const h264Codecs = codecs.filter(c => c.mimeType.toLowerCase() === "video/h264");
                        const otherCodecs = codecs.filter(c => c.mimeType.toLowerCase() !== "video/h264");

                        if (h264Codecs.length > 0) {
                            transceiver.setCodecPreferences([...h264Codecs, ...otherCodecs]);
                            log('Set H.264 codec preference');
                        }
                    }
                }
            });

            pc.onconnectionstatechange = () => {
                log(`Connection state: ${pc.connectionState}`);
            };

            pc.oniceconnectionstatechange = () => {
                log(`ICE connection state: ${pc.iceConnectionState}`);
            };

            return pc;
        }

        async function publish() {
            try {
                log('Đang khởi tạo camera...');
                await openMedia();

                log('Đang tạo peer connection...');
                await createPeer();

                log('Đang tạo offer...');
                const offer = await pc.createOffer({
                    offerToReceiveAudio: false,
                    offerToReceiveVideo: false
                });

                await pc.setLocalDescription(offer);

                log('Đang gửi offer đến server...');
                const auth = btoa(`${admin}:${adminPassword}`);
                const headers = {
                    'Content-Type': 'application/sdp',
                    'Authorization': `Basic ${auth}`
                };

                const res = await fetch(whipEndpoint, {
                    method: 'POST',
                    headers,
                    body: offer.sdp
                });

                if (!res.ok) {
                    const errorText = await res.text();
                    throw new Error(`WHIP POST failed: ${res.status} ${res.statusText} - ${errorText}`);
                }

                log('Đang xử lý answer...');
                const answerSdp = await res.text();
                await pc.setRemoteDescription({ type: 'answer', sdp: answerSdp });

                resourceUrl = res.headers.get('Location');
                if (!resourceUrl) {
                    resourceUrl = whipEndpoint;
                }

                log(`Resource URL: ${resourceUrl}`);

                pc.onicecandidate = async (evt) => {
                    if (!evt.candidate || !resourceUrl) return;

                    try {
                        const headers = {
                            'Content-Type': 'application/trickle-ice-sdpfrag',
                            'Authorization': `Basic ${auth}`
                        };

                        const ufrag = pc.localDescription.sdp.match(/ice-ufrag:([^\r\n]+)/)?.[1];
                        const pwd = pc.localDescription.sdp.match(/ice-pwd:([^\r\n]+)/)?.[1];

                        if (ufrag && pwd) {
                            const frag = `a=ice-ufrag:${ufrag}\r\n` +
                                `a=ice-pwd:${pwd}\r\n` +
                                `a=candidate:${evt.candidate.candidate}`;

                            await fetch(resourceUrl, {
                                method: 'PATCH',
                                headers,
                                body: frag
                            });
                        }
                    } catch (e) {
                        console.warn('ICE candidate error:', e);
                    }
                };

                btnStart.disabled = true;
                btnStop.disabled = false;
                resSel.disabled = true;
                log('✅ Đang phát livestream thành công!');

                const connection = new signalR.HubConnectionBuilder()
                    .withUrl("/chathub")
                    .build();
                await connection.start().then(() => {
                    console.log("Admin SignalR connected");
                });
                await connection.invoke("StartLive")
                    .catch(err => console.error(err.toString()));
            } catch (error) {
                log('❌ Lỗi khi bắt đầu livestream: ' + error.message, true);
                await stop();
                throw error;
            }
        }

        async function stop() {
            log('Đang dừng livestream...');

            try {
                stopDrawingLoop();

                if (resourceUrl) {
                    const headers = {};
                    if (bearerToken) {
                        headers['Authorization'] = `Bearer ${bearerToken}`;
                    } else if (admin && adminPassword) {
                        headers['Authorization'] = `Basic ${btoa(`${admin}:${adminPassword}`)}`;
                    }

                    await fetch(resourceUrl, {
                        method: 'DELETE',
                        headers
                    });
                }
            } catch (e) {
                console.warn('WHIP DELETE error:', e);
            }

            try {
                if (pc) {
                    pc.close();
                    pc = null;
                }
            } catch (e) {
                console.warn('PC close error:', e);
            }

            try {
                if (localStream) {
                    localStream.getTracks().forEach(t => t.stop());
                    localStream = null;
                }
            } catch (e) {
                console.warn('Media stop error:', e);
            }

            if (ctx) {
                ctx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
            }

            btnStart.disabled = false;
            btnStop.disabled = true;
            resSel.disabled = false;
            resourceUrl = null;

            log('✅ Đã dừng livestream.');
            getMp4Files();
        }

        // Setup controls với thêm nút xoay màn hình
        const setupScoreboardControls = () => {
            const toggleBtn = document.getElementById('toggleScoreboard');
            const increaseGreenBtn = document.getElementById('increaseGreen');
    
            const increaseRedBtn = document.getElementById('increaseRed');
      
            const increasePairBtn = document.getElementById('increasePair');
            const decreasePairBtn = document.getElementById('decreasePair');

            // Rotation controls
            const rotateBtn = document.getElementById('rotateScreen');
            const landscapeBtn = document.getElementById('landscapeMode');

            if (toggleBtn) {
                toggleBtn.onclick = () => {
                    showScoreboard = !showScoreboard;
                    log(`Bảng điểm: ${showScoreboard ? 'BẬT' : 'TẮT'}`);
                };
            }

            // Rotation controls
            if (rotateBtn) {
                rotateBtn.onclick = () => {
                    rotationAngle = (rotationAngle + 90) % 360;
                    rotateStream(rotationAngle);
                };
            }

            if (landscapeBtn) {
                landscapeBtn.onclick = () => {
                    // Toggle between portrait (0°) and landscape (90°)
                    rotationAngle = rotationAngle === 0 ? 90 : 0;
                    rotateStream(rotationAngle);
                };
            }

            if (increaseGreenBtn) {
                increaseGreenBtn.onchange = (value) => {
                    greenScore = value;
                    log(`Xanh: ${greenScore}`);
                };
            }

            increaseGreenBtn?.addEventListener('change', e => {
                greenScore = e.target.value;
                log(`Đỏ: ${greenScore}`);
            });

            increaseRedBtn?.addEventListener('change', e => {
                redScore = e.target.value;
                log(`Đỏ: ${redScore}`);
            });


          

            if (increasePairBtn) {
                increasePairBtn.onclick = () => {
                    pairScore++;
                    log(`Cặp: ${pairScore}`);
                };
            }

            if (decreasePairBtn) {
                decreasePairBtn.onclick = () => {
                    pairScore = Math.max(0, pairScore - 1);
                    log(`Cặp: ${pairScore}`);
                };
            }
        };

        // Event handlers
        btnStart.onclick = async () => {
            btnStart.disabled = true;
            try {
                await publish();
            } catch (e) {
                btnStart.disabled = false;
            }
        };

        btnStop.onclick = stop;

        setupScoreboardControls();

        // Initialize camera preview
        openMedia().catch(err => {
            log('❌ Không mở được camera: ' + err.message, true);
        });

        // Screen orientation change detection
        if (screen.orientation) {
            screen.orientation.addEventListener('change', () => {
                const angle = screen.orientation.angle;
                log(`Screen orientation changed: ${angle}°`);
                // Có thể tự động xoay theo orientation của device
                // rotateStream(angle);
            });
        }
    }

    window.initWhipPublisher = (opts) => start(opts);

    async function getMp4Files() {
        try {
            const response = await fetch('/Articles/SaveArticles');
            if (!response.ok) {
                console.warn("Không thể lấy danh sách file MP4");
                return;
            }

            const files = await response.json();
            const list = document.getElementById("fileList");

            if (list && files && files.length > 0) {
                list.innerHTML = '';

                files.forEach(f => {
                    const li = document.createElement("li");
                    li.textContent = f;
                    li.style.padding = "5px";
                    li.style.borderBottom = "1px solid #eee";
                    list.appendChild(li);
                });

                console.log(`Đã tải ${files.length} file MP4`);
            }
        } catch (error) {
            console.warn('Lỗi khi lấy danh sách MP4:', error);
        }
    }
})();