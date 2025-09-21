// sessionTimeout.js - Session timeout management for NGO Web Application

class SessionTimeoutManager {
    constructor(options = {}) {
        this.timeoutMinutes = options.timeoutMinutes || 15;
        this.warningMinutes = options.warningMinutes || 2;
        this.checkInterval = options.checkInterval || 30000; // Check every 30 seconds
        this.logoutUrl = options.logoutUrl || '/Account/Logout';
        this.extendSessionUrl = options.extendSessionUrl || '/Account/ExtendSession';
        this.checkSessionUrl = options.checkSessionUrl || '/Account/CheckSession';

        this.warningShown = false;
        this.countdownInterval = null;
        this.sessionCheckInterval = null;

        this.init();
    }

    init() {
        // Only initialize for authenticated users
        if (this.isUserAuthenticated()) {
            this.startSessionChecking();
            this.bindActivityEvents();
            this.createWarningModal();
        }
    }

    isUserAuthenticated() {
        // Check if user is authenticated
        return document.body.getAttribute('data-authenticated') === 'true' ||
            document.querySelector('.user-info') !== null ||
            document.querySelector('[data-user]') !== null;
    }

    startSessionChecking() {
        this.sessionCheckInterval = setInterval(() => {
            this.checkSession();
        }, this.checkInterval);
    }

    async checkSession() {
        try {
            const response = await fetch(this.checkSessionUrl, {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const data = await response.json();

                if (data.sessionExpired || !data.isAuthenticated) {
                    this.handleSessionExpired();
                } else if (data.remainingTime) {
                    this.handleRemainingTime(data.remainingTime);
                }
            }
        } catch (error) {
            console.error('Session check failed:', error);
        }
    }

    handleRemainingTime(remainingTimeSeconds) {
        const remainingMinutes = Math.floor(remainingTimeSeconds / 60);

        // Show warning when approaching timeout
        if (remainingMinutes <= this.warningMinutes && !this.warningShown) {
            this.showTimeoutWarning(remainingTimeSeconds);
        }

        // Auto-hide warning if time extends
        if (remainingMinutes > this.warningMinutes && this.warningShown) {
            this.hideTimeoutWarning();
        }
    }

    showTimeoutWarning(remainingTimeSeconds) {
        this.warningShown = true;
        const modal = document.getElementById('sessionTimeoutModal');
        const countdownElement = document.getElementById('timeoutCountdown');

        if (modal && countdownElement) {
            this.startCountdown(remainingTimeSeconds, countdownElement);

            // Show modal using Bootstrap
            if (typeof bootstrap !== 'undefined') {
                const bootstrapModal = new bootstrap.Modal(modal, {
                    backdrop: 'static',
                    keyboard: false
                });
                bootstrapModal.show();
            } else {
                // Fallback for older Bootstrap or no Bootstrap
                modal.style.display = 'block';
                modal.classList.add('show');
            }
        }
    }

    startCountdown(initialSeconds, countdownElement) {
        let remainingSeconds = initialSeconds;

        this.countdownInterval = setInterval(() => {
            const minutes = Math.floor(remainingSeconds / 60);
            const seconds = remainingSeconds % 60;

            countdownElement.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;

            if (remainingSeconds <= 0) {
                this.handleSessionExpired();
                return;
            }

            remainingSeconds--;
        }, 1000);
    }

    hideTimeoutWarning() {
        this.warningShown = false;

        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
            this.countdownInterval = null;
        }

        const modal = document.getElementById('sessionTimeoutModal');
        if (modal) {
            if (typeof bootstrap !== 'undefined') {
                const bootstrapModal = bootstrap.Modal.getInstance(modal);
                if (bootstrapModal) {
                    bootstrapModal.hide();
                }
            } else {
                // Fallback
                modal.style.display = 'none';
                modal.classList.remove('show');
            }
        }
    }

    async extendSession() {
        try {
            const response = await fetch(this.extendSessionUrl, {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.hideTimeoutWarning();
                this.showNotification('Session extended successfully', 'success');
            } else {
                this.showNotification('Failed to extend session', 'error');
            }
        } catch (error) {
            console.error('Failed to extend session:', error);
            this.showNotification('Failed to extend session', 'error');
        }
    }

    handleSessionExpired() {
        // Clear intervals
        if (this.sessionCheckInterval) {
            clearInterval(this.sessionCheckInterval);
        }
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
        }

        // Show expiration message and redirect
        this.showNotification('Your session has expired due to inactivity. You will be redirected to the login page.', 'warning');

        setTimeout(() => {
            window.location.href = this.logoutUrl + '?sessionExpired=true';
        }, 3000);
    }

    bindActivityEvents() {
        // Track user activity to reset warning
        const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];

        events.forEach(event => {
            document.addEventListener(event, () => {
                // Reset warning if user becomes active
                if (this.warningShown) {
                    // Optionally auto-extend session on activity
                    this.extendSession();
                }
            });
        });
    }

    createWarningModal() {
        // Create timeout warning modal if it doesn't exist
        if (!document.getElementById('sessionTimeoutModal')) {
            const modalHtml = `
                <div class="modal fade" id="sessionTimeoutModal" tabindex="-1" aria-labelledby="sessionTimeoutModalLabel" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-warning text-dark">
                                <h5 class="modal-title" id="sessionTimeoutModalLabel">
                                    <i class="fas fa-exclamation-triangle me-2"></i>Session Timeout Warning
                                </h5>
                            </div>
                            <div class="modal-body text-center">
                                <i class="fas fa-clock fa-3x text-warning mb-3"></i>
                                <h6>Your session will expire in:</h6>
                                <h2 class="text-danger font-monospace" id="timeoutCountdown">0:00</h2>
                                <p class="mt-3">Click "Stay Logged In" to extend your session, or you will be automatically logged out.</p>
                            </div>
                            <div class="modal-footer justify-content-center">
                                <button type="button" class="btn btn-success" onclick="sessionManager.extendSession()">
                                    <i class="fas fa-refresh me-2"></i>Stay Logged In
                                </button>
                                <button type="button" class="btn btn-secondary" onclick="sessionManager.handleSessionExpired()">
                                    <i class="fas fa-sign-out-alt me-2"></i>Logout Now
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            document.body.insertAdjacentHTML('beforeend', modalHtml);
        }
    }

    showNotification(message, type = 'info') {
        // Create toast notification
        const toastId = 'toast-' + Date.now();
        const bgClass = type === 'success' ? 'bg-success' :
            type === 'error' ? 'bg-danger' :
                type === 'warning' ? 'bg-warning text-dark' : 'bg-primary';

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        // Create toast container if it doesn't exist
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);

        const toastElement = document.getElementById(toastId);

        if (typeof bootstrap !== 'undefined') {
            const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
            toast.show();

            // Remove toast element after it's hidden
            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        } else {
            // Fallback without Bootstrap
            toastElement.style.display = 'block';
            toastElement.classList.add('show');

            setTimeout(() => {
                toastElement.remove();
            }, 5000);
        }
    }

    destroy() {
        if (this.sessionCheckInterval) {
            clearInterval(this.sessionCheckInterval);
        }
        if (this.countdownInterval) {
            clearInterval(this.countdownInterval);
        }
    }
}

// Initialize session timeout manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Only initialize for authenticated users
    const isAuthenticated = document.body.getAttribute('data-authenticated') === 'true' ||
        document.querySelector('.user-info') !== null ||
        document.querySelector('[data-user]') !== null;

    if (isAuthenticated) {
        window.sessionManager = new SessionTimeoutManager({
            timeoutMinutes: 15,     // Total session timeout
            warningMinutes: 2,      // Show warning 2 minutes before timeout
            checkInterval: 30000,   // Check session every 30 seconds
        });
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', function () {
    if (window.sessionManager) {
        window.sessionManager.destroy();
    }
});

// Handle visibility change (when user switches tabs)
document.addEventListener('visibilitychange', function () {
    if (window.sessionManager && !document.hidden) {
        // User returned to tab, check session immediately
        window.sessionManager.checkSession();
    }
});