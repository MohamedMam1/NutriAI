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
    document.getElementById('caloriesConsumed').textContent = data.caloriesConsumed;
    document.getElementById('caloriesGoal').textContent = data.caloriesGoal;
    const caloriePct = Math.min(100, (data.caloriesConsumed / data.caloriesGoal) * 100);
    document.getElementById('calorieProgress').style.width = caloriePct + '%';

    document.getElementById('currentWeight').textContent = data.currentWeight;
    document.getElementById('goalWeight').textContent = data.goalWeight;
    document.getElementById('waterMl').textContent = data.waterMl;
    document.getElementById('waterProgress').style.width = (data.waterMl / data.waterGoalMl * 100) + '%';
    document.getElementById('weeklyStreak').textContent = data.weeklyStreak;
    document.getElementById('aiInsight').textContent = data.aiInsight;
}

function renderLists(data) {
    const mealsEl = document.getElementById('recentMealsList');
    mealsEl.innerHTML = data.recentMeals.map(m => `
        <div class="d-flex justify-content-between align-items-center py-2 border-bottom">
            <div><strong>${m.name}</strong><br><span class="small-text text-muted">${m.time}</span></div>
            <span class="badge bg-success">${m.calories} cal</span>
        </div>`).join('');

    const plansEl = document.getElementById('savedPlansList');
    plansEl.innerHTML = data.savedPlans.map(p => `
        <div class="d-flex justify-content-between align-items-center py-2 border-bottom">
            <strong>${p.name}</strong>
            <span class="small-text text-muted">${p.days} days</span>
        </div>`).join('');
}

function initCharts(data) {
    const calorieCtx = document.getElementById('calorieChart');
    if (calorieCtx) {
        calorieChart = new Chart(calorieCtx, {
            type: 'bar',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                datasets: [{
                    label: 'Calories',
                    data: [1920, 1780, 2010, data.caloriesConsumed, 1900, 2100, 1750],
                    backgroundColor: 'rgba(76, 175, 80, 0.7)',
                    borderRadius: 8
                }]
            },
            options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
        });
    }

    const weightCtx = document.getElementById('weightMiniChart');
    if (weightCtx) {
        weightMiniChart = new Chart(weightCtx, {
            type: 'line',
            data: {
                labels: ['W1', 'W2', 'W3', 'W4', 'W5', 'W6', 'Now'],
                datasets: [{
                    label: 'Weight (kg)',
                    data: [79.2, 79.0, 78.8, 78.7, 78.6, 78.5, data.currentWeight],
                    borderColor: '#2196F3',
                    backgroundColor: 'rgba(33, 150, 243, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: { responsive: true, plugins: { legend: { display: false } } }
        });
    }
}
