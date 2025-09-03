// DOM Content Loaded
document.addEventListener('DOMContentLoaded', function () {
    window.onscroll = function () {
        const btn = document.getElementById("backToTop");
        if (document.body.scrollTop > 200 || document.documentElement.scrollTop > 200) {
            btn.style.display = "block";
        } else {
            btn.style.display = "none";
        }
    };

    // Scroll lên đầu trang
    document.getElementById("backToTop").onclick = function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    // Mobile Navigation Toggle
    const navToggle = document.getElementById('navToggle');
    const navMenu = document.getElementById('navMenu');

    if (navToggle && navMenu) {
        navToggle.addEventListener('click', function () {
            navToggle.classList.toggle('active');
            navMenu.classList.toggle('active');
        });

        // Close menu when clicking outside


        // Close menu when clicking on a link
        const navLinks = navMenu.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', function () {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
            });
        });
    }

    // Add click event listeners to navigation links
    const allNavLinks = document.querySelectorAll('.nav-link');
    allNavLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href && href !== '#') {
               
                // For debugging, let's try to navigate programmatically
                try {
                    window.location.href = href;
                } catch (error) {
                   
                }
            }
        });
    });

    // Stream button functionality
    const streamButtons = document.querySelectorAll('.stream-btn');
    streamButtons.forEach(button => {
        button.addEventListener('click', function () {
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
        copyBtn.addEventListener('click', function () {
            const accountNumber = '0721 0006 39727';
            navigator.clipboard.writeText(accountNumber).then(function () {
                // Show success message
                const originalText = copyBtn.textContent;
                copyBtn.textContent = 'Đã sao chép!';
                copyBtn.style.backgroundColor = '#28a745';

                setTimeout(() => {
                    copyBtn.textContent = originalText;
                    copyBtn.style.backgroundColor = '#ffd700';
                }, 2000);
            }).catch(function (err) {
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
    const chatMessages = document.querySelector('.chat-messages');

    function addMessage(username, message) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'message';
        messageDiv.innerHTML = `
            <span class="username">${username}:</span>
            <span class="message-text">${message}</span>
        `;
        chatMessages.appendChild(messageDiv);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Video player functionality for replay videos
    const playOverlays = document.querySelectorAll('.play-overlay');
    playOverlays.forEach(button => {
        button.addEventListener('click', function () {
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
        button.addEventListener('click', function () {
            const buttonText = this.textContent.trim();

            if (buttonText.includes('NHÓM VIP ZALO')) {
                window.open('https://zalo.me/g/ymtmoh673', '_blank');
            }
            else if (buttonText.includes('ZALO')) {
                window.open('https://zalo.me/0335501179', '_blank');
            }
            else if (buttonText.includes('FACEBOOK')) {
                window.open('https://web.facebook.com/tptuyhoa.vuahoatet', '_blank');

            }

           
        });
    });

    // Chat now button
    const chatNowBtn = document.querySelector('.chat-btn');
    if (chatNowBtn) {
        chatNowBtn.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }



 

    // Add loading animation to video container
    const videoContainer = document.querySelector('.video-container');
    if (videoContainer) {
        videoContainer.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.01)';
        });

        videoContainer.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    }

    // Add hover effects to video cards
    const videoCards = document.querySelectorAll('.video-card');
    videoCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.boxShadow = '0 8px 25px rgba(255, 215, 0, 0.3)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.boxShadow = 'none';
        });
    });

    // Add click effects to buttons
    const allButtons = document.querySelectorAll('button');
    allButtons.forEach(button => {
        button.addEventListener('click', function () {
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });
    });


    const phoneBtn = document.querySelector('.phone-btn');
    const zaloBtn = document.querySelector('.zalo-btn');
    const vipBtn = document.querySelector('.vip-btn');

    // Phone button click handler
    if (phoneBtn) {
        phoneBtn.addEventListener('click', function () {
            window.location.href = 'tel:+84335501179';
        });
    }

    // Zalo button click handler
    if (zaloBtn) {
        zaloBtn.addEventListener('click', function () {
            window.open('https://zalo.me/0335501179', '_blank');
        });
    }

    // VIP button click handler
    if (vipBtn) {
        vipBtn.addEventListener('click', function () {
            window.open('https://zalo.me/g/ymtmoh673', '_blank');

        });
    }

    // Notification function
    function showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close">&times;</button>
            </div>
        `;

        // Add styles
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8'};
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
            z-index: 10000;
            max-width: 300px;
            transform: translateX(100%);
            transition: transform 0.3s ease;
        `;

        // Add to page
        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Close button functionality
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', () => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        });

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (document.body.contains(notification)) {
                notification.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    if (document.body.contains(notification)) {
                        document.body.removeChild(notification);
                    }
                }, 300);
            }
        }, 5000);
    }

    // Add hover sound effect (optional)
    const floatingBtns = document.querySelectorAll('.floating-btn');
    floatingBtns.forEach(btn => {
        btn.addEventListener('mouseenter', function () {
            // Add a subtle animation effect
            this.style.animation = 'pulse 0.3s ease';
        });

        btn.addEventListener('mouseleave', function () {
            this.style.animation = 'none';
        });
    });

    // Add pulse animation keyframes
    const style = document.createElement('style');
    style.textContent = `
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.05); }
            100% { transform: scale(1.1); }
        }
    `;
    document.head.appendChild(style);
}); 