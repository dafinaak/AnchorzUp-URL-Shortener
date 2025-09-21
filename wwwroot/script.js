class UrlShortener {
    constructor() {
        this.apiBaseUrl = '/api/url';
        this.selectedExpiration = null;
        this.initializeEventListeners();
        this.loadUrls();
        this.startExpirationChecker();
    }

    initializeEventListeners() {
        const shortenBtn = document.getElementById('shorten-btn');
        const urlInput = document.getElementById('url-input');
        const expirationToggle = document.getElementById('expiration-toggle');
        const expirationOptions = document.getElementById('expiration-options');
        const options = document.querySelectorAll('.option');

        shortenBtn.addEventListener('click', () => this.shortenUrl());
        urlInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.shortenUrl();
            }
        });

        expirationToggle.addEventListener('click', () => {
            expirationOptions.classList.toggle('show');
            expirationToggle.classList.toggle('active');
        });

        options.forEach(option => {
            option.addEventListener('click', () => {

                options.forEach(opt => opt.classList.remove('selected'));
                
                option.classList.add('selected');

                this.selectedExpiration = parseInt(option.dataset.minutes);

                expirationToggle.querySelector('span:first-child').textContent = option.textContent;
                
                expirationOptions.classList.remove('show');
                expirationToggle.classList.remove('active');
            });
        });

        document.addEventListener('click', (e) => {
            if (!expirationToggle.contains(e.target) && !expirationOptions.contains(e.target)) {
                expirationOptions.classList.remove('show');
                expirationToggle.classList.remove('active');
            }
        });
    }

    async shortenUrl() {
        const urlInput = document.getElementById('url-input');
        const shortenBtn = document.getElementById('shorten-btn');
        const url = urlInput.value.trim();

        if (!url) {
            this.showError('Please enter a URL to shorten');
            return;
        }

        try {
            new URL(url);
        } catch {
            this.showError('Please enter a valid URL');
            return;
        }

        shortenBtn.disabled = true;
        shortenBtn.innerHTML = '<div class="loading"></div>';

        try {
            const response = await fetch(`${this.apiBaseUrl}/shorten`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    url: url,
                    expirationMinutes: this.selectedExpiration
                })
            });

            const data = await response.json();

            if (response.ok) {
                this.showSuccess(`URL shortened successfully: ${data.shortUrl}`);
                urlInput.value = '';
                this.selectedExpiration = null;
                this.resetExpirationDropdown();
                this.loadUrls(); 
            } else {
                this.showError(data.error || 'Failed to shorten URL');
            }
        } catch (error) {
            console.error('Error:', error);
            this.showError('Network error. Please try again.');
        } finally {

            shortenBtn.disabled = false;
            shortenBtn.innerHTML = 'Shorten URL';
        }
    }

    async loadUrls() {
        try {
            const response = await fetch(this.apiBaseUrl);
            const urls = await response.json();

            const urlList = document.getElementById('url-list');
            
            const qrStates = {};
            urls.forEach(url => {
                const qrSection = document.getElementById(`qr-section-${url.shortCode}`);
                if (qrSection) {
                    qrStates[url.shortCode] = qrSection.style.display;
                }
            });
            
            urlList.innerHTML = '';

            if (urls.length === 0) {
                urlList.innerHTML = '<div style="color: #5f6368; font-size: 14px; text-align: center; padding: 20px;">No shortened URLs yet</div>';
                return;
            }

            urls.forEach(url => {
                const urlItem = document.createElement('div');
                urlItem.className = 'url-item';
                
                const isExpired = url.isExpired || false;

                const wasQrVisible = qrStates[url.shortCode] === 'block';
                const cachedQrCode = localStorage.getItem(`qr_${url.shortCode}`);
                
                urlItem.innerHTML = `
                    <div style="flex: 1;">
                        <a href="/api/url/${url.shortCode}" target="_blank" class="url-link" onclick="urlShortener.trackClick('${url.shortCode}')">${url.shortUrl}</a>
                        <div class="click-count">This link has been clicked ${url.clickCount} times</div>
                        ${isExpired ? '<div class="expired-indicator">⚠️ EXPIRED</div>' : ''}
                        <div class="qr-code-section" id="qr-section-${url.shortCode}" style="display: ${wasQrVisible ? 'block' : 'none'};">
                            <div class="qr-code-container">
                                <img id="qr-image-${url.shortCode}" src="${cachedQrCode || ''}" alt="QR Code" class="qr-code-image">
                                <div class="qr-actions">
                                    <button class="qr-download-btn" onclick="urlShortener.downloadQrCode('${url.shortCode}')" title="Download QR Code">
                                        📥 Download
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="url-actions">
                        <button class="qr-toggle-btn" onclick="urlShortener.toggleQrCode('${url.shortCode}')" title="Show/Hide QR Code">
                            📱 QR
                        </button>
                        <button class="delete-btn" onclick="urlShortener.deleteUrl('${url.shortCode}')" title="Delete URL">
                            🗑️
                        </button>
                    </div>
                `;

                if (isExpired) {
                    urlItem.style.opacity = '0.6';
                }

                urlList.appendChild(urlItem);
            });
        } catch (error) {
            console.error('Error loading URLs:', error);
        }
    }

    async trackClick(shortCode) {
  
        setTimeout(() => {
            this.loadUrls();
        }, 1000);
    }

    startExpirationChecker() {

        setInterval(() => {
            this.checkForExpiredUrls();
        }, 10000);
    }

    checkForExpiredUrls() {


        this.loadUrls();
    }

    async deleteUrl(shortCode) {
        if (!confirm('Are you sure you want to delete this URL?')) {
            return;
        }

        try {
            const response = await fetch(`${this.apiBaseUrl}/${shortCode}`, {
                method: 'DELETE'
            });

            if (response.ok) {
                localStorage.removeItem(`qr_${shortCode}`);
                this.showSuccess('URL deleted successfully');
                this.loadUrls(); 
            } else {
                const data = await response.json();
                this.showError(data.error || 'Failed to delete URL');
            }
        } catch (error) {
            console.error('Error:', error);
            this.showError('Network error. Please try again.');
        }
    }

    resetExpirationDropdown() {
        const expirationToggle = document.getElementById('expiration-toggle');
        const options = document.querySelectorAll('.option');
        
        expirationToggle.querySelector('span:first-child').textContent = 'Add expiration date';
        options.forEach(opt => opt.classList.remove('selected'));
    }

    showSuccess(message) {
        this.showMessage(message, 'success');
    }

    showError(message) {
        this.showMessage(message, 'error');
    }

    showMessage(message, type) {
  
        const existingMessage = document.querySelector('.success-message, .error-message');
        if (existingMessage) {
            existingMessage.remove();
        }

        const messageDiv = document.createElement('div');
        messageDiv.className = `${type}-message`;
        messageDiv.textContent = message;

        const inputSection = document.querySelector('.input-section');
        inputSection.appendChild(messageDiv);

        setTimeout(() => {
            if (messageDiv.parentNode) {
                messageDiv.remove();
            }
        }, 5000);
    }

    async toggleQrCode(shortCode) {
        const qrSection = document.getElementById(`qr-section-${shortCode}`);
        const qrImage = document.getElementById(`qr-image-${shortCode}`);
        
        if (qrSection.style.display === 'none') {
            const cachedQrCode = localStorage.getItem(`qr_${shortCode}`);
            
            if (cachedQrCode) {
                qrImage.src = cachedQrCode;
            } else {
                try {
                    const response = await fetch(`${this.apiBaseUrl}/${shortCode}/qr?size=200`);
                    const data = await response.json();
                    
                    if (response.ok) {
                        const qrCodeDataUrl = `data:image/png;base64,${data.qrCode}`;
                        qrImage.src = qrCodeDataUrl;

                        localStorage.setItem(`qr_${shortCode}`, qrCodeDataUrl);
                    } else {
                        this.showError(data.error || 'Failed to generate QR code');
                        return;
                    }
                } catch (error) {
                    console.error('Error generating QR code:', error);
                    this.showError('Network error. Please try again.');
                    return;
                }
            }
            qrSection.style.display = 'block';
        } else {
            qrSection.style.display = 'none';
        }
    }

    async downloadQrCode(shortCode) {
        try {
            const response = await fetch(`${this.apiBaseUrl}/${shortCode}/qr/image?size=300`);
            
            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `qr-code-${shortCode}.png`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                this.showSuccess('QR code downloaded successfully');
            } else {
                const data = await response.json();
                this.showError(data.error || 'Failed to download QR code');
            }
        } catch (error) {
            console.error('Error downloading QR code:', error);
            this.showError('Network error. Please try again.');
        }
    }
}


const urlShortener = new UrlShortener();


