let calorieChart, weightChart, hydrationChart;

document.addEventListener('DOMContentLoaded', async () => {
    try {
        const data = await NutriAI.fetchJson('/Report/GetWeeklyData');
        renderStats(data);
        renderCharts(data);
        if (data.message && data.dataSource === 'database') {
            NutriAI.showToast(data.message, 'warning');
        }
        renderRecommendations(data.aiRecommendations || []);
    } catch {
        NutriAI.showToast('Failed to load report', 'danger');
    }
});

function renderStats(data) {
    const stats = [
        { icon: 'fa-weight-scale', color: 'green', label: 'Weight Change', value: data.weightChange + ' kg' },
        { icon: 'fa-fire', color: 'orange', label: 'Avg Calories', value: data.avgCalories },
        { icon: 'fa-droplet', color: 'blue', label: 'Hydration Score', value: data.hydrationScore + '%' },
        { icon: 'fa-star', color: 'green', label: 'Best Day', value: data.bestDay },
        { icon: 'fa-arrow-down', color: 'orange', label: 'Worst Day', value: data.worstDay }
    ];

    const container = document.getElementById('reportStats');
    container.replaceChildren();
    stats.forEach(s => {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg';
        col.innerHTML =
            '<div class="card-nutri hover-scale">' +
            '<div class="d-flex align-items-center gap-3">' +
            '<div class="stat-icon ' + s.color + '"><i class="fas ' + s.icon + '"></i></div>' +
            '<div><p class="card-title mb-0">' + s.label + '</p><p class="stat-value mb-0">' + s.value + '</p></div>' +
            '</div></div>';
        container.appendChild(col);
    });
}

function renderCharts(data) {
    const chartOpts = { responsive: true, plugins: { legend: { display: false } } };

    calorieChart = new Chart(document.getElementById('reportCalorieChart'), {
        type: 'bar',
        data: {
            labels: data.dailyLabels,
            datasets: [{ data: data.dailyCalories, backgroundColor: 'rgba(76, 175, 80, 0.7)', borderRadius: 6 }]
        },
        options: chartOpts
    });

    weightChart = new Chart(document.getElementById('reportWeightChart'), {
        type: 'line',
        data: {
            labels: data.dailyLabels,
            datasets: [{ data: data.weightTrend, borderColor: '#2196F3', tension: 0.4, fill: false }]
        },
        options: chartOpts
    });

    hydrationChart = new Chart(document.getElementById('reportHydrationChart'), {
        type: 'bar',
        data: {
            labels: data.dailyLabels,
            datasets: [{ data: data.hydrationDays, backgroundColor: 'rgba(33, 150, 243, 0.7)', borderRadius: 6 }]
        },
        options: chartOpts
    });
}

function renderRecommendations(items) {
    const list = document.getElementById('aiRecommendations');
    list.replaceChildren();
    if (!items.length) {
        const li = document.createElement('li');
        li.className = 'small-text text-muted';
        li.textContent = 'This information is not available right now.';
        list.appendChild(li);
        return;
    }
    items.forEach(text => {
        const li = document.createElement('li');
        li.className = 'mb-2 small-text';
        li.textContent = text;
        list.appendChild(li);
    });
}

