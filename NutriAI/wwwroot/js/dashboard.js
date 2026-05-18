let calorieChart, weightMiniChart;

document.addEventListener('DOMContentLoaded', async () => {
    try {
        const data = await NutriAI.fetchJson('/Dashboard/GetSummary');
        renderStats(data);
        renderLists(data);
        initCharts(data);
    } catch {
        NutriAI.showToast('Failed to load dashboard data', 'danger');
    }
});

function renderStats(data) {
    document.getElementById('caloriesConsumed').textContent = data.caloriesConsumed ?? 0;
    document.getElementById('caloriesGoal').textContent = data.caloriesGoal ?? 0;
    const calorieGoal = data.caloriesGoal || 1;
    const caloriePct = Math.min(100, (data.caloriesConsumed / calorieGoal) * 100);
    document.getElementById('calorieProgress').style.width = caloriePct + '%';

    document.getElementById('currentWeight').textContent = data.currentWeight ?? 0;
    document.getElementById('goalWeight').textContent = data.goalWeight ?? 0;
    document.getElementById('waterMl').textContent = data.waterMl ?? 0;
    const waterGoal = data.waterGoalMl || 1;
    document.getElementById('waterProgress').style.width = Math.min(100, (data.waterMl / waterGoal) * 100) + '%';
    document.getElementById('weeklyStreak').textContent = data.weeklyStreak ?? 0;
    document.getElementById('aiInsight').textContent = data.aiInsight ?? '';

    if (data.latestReportBestDay && data.latestReportWorstDay) {
        const row = document.getElementById('weeklyReportRow');
        const summary = document.getElementById('weeklyReportSummary');
        if (row && summary) {
            row.style.display = '';
            summary.textContent = `Best day: ${data.latestReportBestDay}. Day to improve: ${data.latestReportWorstDay}. View full details in Reports.`;
        }
    }
}

function renderLists(data) {
    const mealsEl = document.getElementById('recentMealsList');
    const plansEl = document.getElementById('savedPlansList');

    if (!data.recentMeals?.length) {
        mealsEl.innerHTML = '<p class="small-text text-muted mb-0">No meals logged today yet.</p>';
    } else {
        mealsEl.innerHTML = data.recentMeals.map(m => `
            <div class="dashboard-list-item">
                <div><strong>${escapeHtml(m.name)}</strong><br><span class="small-text text-muted">${escapeHtml(m.time)}</span></div>
                <span class="badge bg-success">${m.calories} cal</span>
            </div>`).join('');
    }

    if (!data.savedPlans?.length) {
        plansEl.innerHTML = '<p class="small-text text-muted mb-0">No saved meal plans yet.</p>';
    } else {
        plansEl.innerHTML = data.savedPlans.map(p => `
            <div class="dashboard-list-item">
                <strong>${escapeHtml(p.name)}</strong>
                <span class="small-text text-muted">${p.days} days</span>
            </div>`).join('');
    }
}

function initCharts(data) {
    const calorieCtx = document.getElementById('calorieChart');
    const calorieLabels = data.weeklyCalories?.map(p => p.label) ?? [];
    const calorieValues = data.weeklyCalories?.map(p => p.calories) ?? [];

    if (calorieCtx && calorieLabels.length) {
        calorieChart = new Chart(calorieCtx, {
            type: 'bar',
            data: {
                labels: calorieLabels,
                datasets: [{
                    label: 'Calories',
                    data: calorieValues,
                    backgroundColor: 'rgba(76, 175, 80, 0.7)',
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });
    }

    const weightCtx = document.getElementById('weightMiniChart');
    const weightLabels = data.weightTrend?.map(p => p.label) ?? [];
    const weightValues = data.weightTrend?.map(p => p.weight ?? null) ?? [];

    if (weightCtx && weightLabels.length) {
        weightMiniChart = new Chart(weightCtx, {
            type: 'line',
            data: {
                labels: weightLabels,
                datasets: [{
                    label: 'Weight (kg)',
                    data: weightValues,
                    borderColor: '#2196F3',
                    backgroundColor: 'rgba(33, 150, 243, 0.1)',
                    fill: true,
                    tension: 0.4,
                    spanGaps: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } }
            }
        });
    }
}

function escapeHtml(text) {
    const div = document.createElement('label');
    div.textContent = text ?? '';
    return div.innerHTML;
}
