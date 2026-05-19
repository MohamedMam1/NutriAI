document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('plannerForm').addEventListener('submit', generatePlan);
});

async function generatePlan(e) {
    e.preventDefault();
    const loading = document.getElementById('plannerLoading');
    loading.classList.add('active');
    const results = document.getElementById('mealPlanResults');
    results.replaceChildren();

    const payload = {
        goalWeight: parseFloat(document.getElementById('goalWeight').value),
        timelineWeeks: parseInt(document.getElementById('timeline').value, 10),
        dietaryPreference: document.getElementById('dietaryPreference').value
    };

    try {
        const data = await NutriAI.fetchJson('/MealPlanner/Generate', {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        if (!data.success) {
            NutriAI.showToast(data.message || 'Could not generate meal plan.', 'danger');
            return;
        }
        if (data.message && data.dataSource === 'database') {
            NutriAI.showToast(data.message, 'warning');
        }
        renderPlan(data);
    } catch {
        NutriAI.showToast('Failed to generate meal plan. Please wait for the AI response.', 'danger');
    } finally {
        loading.classList.remove('active');
    }
}

function renderPlan(data) {
    const container = document.getElementById('mealPlanResults');
    const header = document.createElement('div');
    header.className = 'card-nutri mb-4 insight-card';
    header.innerHTML = '<h3 class="h5 mb-2">Your Weekly Plan</h3><p class="small-text mb-0">Goal: ' +
        data.goalWeight + ' kg in ' + data.timelineWeeks + ' weeks · ' + data.preference + ' diet</p>';
    container.appendChild(header);

    data.weeklyPlan.forEach(day => {
        const dayCard = document.createElement('div');
        dayCard.className = 'card-nutri mb-3';
        const title = document.createElement('h3');
        title.className = 'h6 mb-3';
        title.textContent = day.day;
        dayCard.appendChild(title);

        day.meals.forEach(meal => {
            const typeClass = meal.type.toLowerCase();
            const mealEl = document.createElement('div');
            mealEl.className = 'meal-plan-card card-nutri mb-2 ' + typeClass;
            mealEl.innerHTML =
                '<div class="d-flex justify-content-between"><strong>' + meal.type + ': ' + meal.name + '</strong>' +
                '<span class="badge bg-success">' + meal.calories + ' cal</span></div>' +
                '<p class="small-text text-muted mb-1">P: ' + meal.protein + 'g · C: ' + meal.carbs + 'g · F: ' + meal.fat + 'g</p>' +
                '<p class="small-text mb-0"><i class="fas fa-utensils me-1"></i>' + meal.instructions + '</p>';
            dayCard.appendChild(mealEl);
        });

        container.appendChild(dayCard);
    });
}
