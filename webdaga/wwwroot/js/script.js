// DOM Content Loaded
document.addEventListener('DOMContentLoaded', function() {
    // Stream button functionality
    const streamButtons = document.querySelectorAll('.stream-btn');
    streamButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Remove active class from all buttons
            streamButtons.forEach(btn => btn.classList.remove('active'));
            // Add active class to clicked button
            this.classList.add('active');
            
            // Update live title based on selected stream
            const liveTitle = document.querySelector('.live-title');
            const streamName = this.textContent;
            liveTitle.textContent = `Xổ gà Server ${streamName} trực tiếp 18h ngày 4/8/25`;
        });
    });

    // Copy account number functionality
    const copyBtn = document.querySelector('.copy-btn');
    if (copyBtn) {
        copyBtn.addEventListener('click', function() {
            const accountNumber = '0300000000';
            navigator.clipboard.writeText(accountNumber).then(function() {
                // Show success message
                const originalText = copyBtn.textContent;
                copyBtn.textContent = 'Đã sao chép!';
                copyBtn.style.backgroundColor = '#28a745';
                
                setTimeout(() => {
                    copyBtn.textContent = originalText;
                    copyBtn.style.backgroundColor = '#ffd700';
                }, 2000);
            }).catch(function(err) {
                console.error('Could not copy text: ', err);
                // Fallback for older browsers
                const textArea = document.createElement('textarea');
                textArea.value = accountNumber;
                document.body.appendChild(textArea);
                textArea.select();
                document.execCommand('copy');
                document.body.removeChild(textArea);
                
                copyBtn.textContent = 'Đã sao chép!';
                setTimeout(() => {
                    copyBtn.textContent = 'Sao chép STK';
                }, 2000);
            });
        });
    }

    // Chat functionality
    const chatInput = document.querySelector('.chat-input input');
    const chatButton = document.querySelector('.chat-input button');



    if (chatButton && chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                chatButton.click();
            }
        });
    }

    // Video player functionality for replay videos
    const playOverlays = document.querySelectorAll('.play-overlay');
    playOverlays.forEach(button => {
        button.addEventListener('click', function() {
            const videoContainer = this.closest('.video-card');
            if (videoContainer) {
                // Simulate video loading for replay videos
                this.style.opacity = '0.7';
                setTimeout(() => {
                    this.style.opacity = '1';
                    alert('Video đang tải... (Đây là demo)');
                }, 500);
            }
        });
    });

    // Social buttons functionality
    const socialButtons = document.querySelectorAll('.social-btn');
    socialButtons.forEach(button => {
        button.addEventListener('click', function() {
            const buttonText = this.textContent.trim();
            let message = '';
            
            if (buttonText.includes('ZALO')) {
                message = 'Đang chuyển đến Zalo...';
            } else if (buttonText.includes('FACEBOOK')) {
                message = 'Đang chuyển đến Facebook...';
            }
            
            if (message) {
                alert(message + ' (Đây là demo)');
            }
        });
    });



    // Smooth scrolling for navigation links
    //const navLinks = document.querySelectorAll('.nav-link');
    //navLinks.forEach(link => {
    //    link.addEventListener('click', function(e) {
    //        //e.preventDefault();
    //        const targetId = this.getAttribute('href').substring(1);
    //        const targetSection = document.getElementById(targetId);
            
    //        if (targetSection) {
    //            targetSection.scrollIntoView({
    //                behavior: 'smooth'
    //            });
    //        }
    //    });
    //});



    // Auto-scroll chat to bottom
    setInterval(() => {
        if (chatMessages) {
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    }, 1000);

    // Add loading animation to video container
    const videoContainer = document.querySelector('.video-container');
    if (videoContainer) {
        videoContainer.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.01)';
        });
        
        videoContainer.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    }

    // Add hover effects to video cards
    const videoCards = document.querySelectorAll('.video-card');
    videoCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.boxShadow = '0 8px 25px rgba(255, 215, 0, 0.3)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.boxShadow = 'none';
        });
    });

    // Add click effects to buttons
    const allButtons = document.querySelectorAll('button');
    allButtons.forEach(button => {
        button.addEventListener('click', function() {
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });
    });

    console.log('CLB Gà Chọi website loaded successfully!');
}); 