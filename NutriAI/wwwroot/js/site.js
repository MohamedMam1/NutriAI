// NutriAI shared utilities
const NutriAI = {
    showToast(message, type = 'success') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed top-0 end-0 m-3 fade-in`;
        toast.style.zIndex = '9999';
        toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>${this.escapeHtml(message)}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    },

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    getCsrfToken() {
        return document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
    },

    async fetchJson(url, options = {}) {
        const method = (options.method || 'GET').toUpperCase();
        const headers = { 'Content-Type': 'application/json', ...options.headers };

        if (method !== 'GET' && method !== 'HEAD') {
            const token = this.getCsrfToken();
            if (token) headers['X-CSRF-TOKEN'] = token;
        }

        const response = await fetch(url, { ...options, headers });
        const data = await response.json().catch(() => ({}));
        if (!response.ok) {
            const err = new Error(data.message || data.title || 'Request failed');
            err.payload = data;
            throw err;
        }
        return data;
    },

    formatNumber(num, decimals = 0) {
        return Number(num).toLocaleString(undefined, { maximumFractionDigits: decimals });
    }
};
