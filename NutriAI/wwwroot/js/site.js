// NutriAI shared utilities
const NutriAI = {
    showToast(message, type = 'success') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed top-0 end-0 m-3 fade-in`;
        toast.style.zIndex = '9999';
        toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>${message}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    },

    async fetchJson(url, options = {}) {
        const response = await fetch(url, {
            headers: { 'Content-Type': 'application/json', ...options.headers },
            ...options
        });
        if (!response.ok) throw new Error('Request failed');
        return response.json();
    },

    formatNumber(num, decimals = 0) {
        return Number(num).toLocaleString(undefined, { maximumFractionDigits: decimals });
    }
};
