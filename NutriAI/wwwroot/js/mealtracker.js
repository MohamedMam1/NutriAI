document.addEventListener('DOMContentLoaded', () => {
    loadMeals();
    document.getElementById('mealForm').addEventListener('submit', submitMeal);
    window.deleteMeal = deleteMeal;
});

async function loadMeals() {
    try {
        const meals = await NutriAI.fetchJson('/MealTracker/GetMeals');
        renderHistory(meals);
    } catch {
        NutriAI.showToast('Failed to load meals', 'danger');
    }
}

async function submitMeal(e) {
    e.preventDefault();
    const input = document.getElementById('mealInput');
    const description = input.value.trim();
    if (!description) return;

    appendUserMessage(description);
    input.value = '';
    showLoading(true);

    try {
        const result = await NutriAI.fetchJson('/MealTracker/Analyze', {
            method: 'POST',
            body: JSON.stringify({ description })
        });

        if (!result.success) {
            NutriAI.showToast(result.message || 'Could not analyze this meal.', 'danger');
            return;
        }

        if (result.message && result.dataSource === 'database') {
            NutriAI.showToast(result.message, 'warning');
        }

        appendAiMessage(result);
        loadMeals();
    } catch {
        NutriAI.showToast('Failed to analyze meal. Please wait for the AI response and try again.', 'danger');
    } finally {
        showLoading(false);
    }
}

function appendUserMessage(text) {
    const container = document.getElementById('chatContainer');
    const wrap = document.createElement('div');
    wrap.className = 'chat-message user d-flex';
    const bubble = document.createElement('div');
    bubble.className = 'chat-bubble';
    bubble.textContent = text;
    wrap.appendChild(bubble);
    container.appendChild(wrap);
    container.scrollTop = container.scrollHeight;
}

function appendAiMessage(result) {
    const m = result.meal;
    if (!m) return;

    const container = document.getElementById('chatContainer');
    const wrap = document.createElement('div');
    wrap.className = 'chat-message ai d-flex';

    const bubble = document.createElement('div');
    bubble.className = 'chat-bubble';

    const p = document.createElement('p');
    p.className = 'mb-2';
    p.textContent = result.aiResponse;
    bubble.appendChild(p);

    [['calories', m.calories + ' cal', 'calories'],
     ['protein', 'P: ' + m.protein + 'g', 'protein'],
     ['carbs', 'C: ' + m.carbs + 'g', 'carbs'],
     ['fat', 'F: ' + m.fat + 'g', 'fat']].forEach(([, label, cls]) => {
        const span = document.createElement('span');
        span.className = 'macro-badge ' + cls;
        span.textContent = label;
        bubble.appendChild(span);
    });

    wrap.appendChild(bubble);
    container.appendChild(wrap);
    container.scrollTop = container.scrollHeight;
}

function renderHistory(meals) {
    const list = document.getElementById('mealHistoryList');
    list.replaceChildren();

    if (!meals.length) {
        const empty = document.createElement('p');
        empty.className = 'small-text text-muted';
        empty.textContent = 'No meals logged today.';
        list.appendChild(empty);
        return;
    }

    meals.forEach(m => {
        const row = document.createElement('div');
        row.className = 'd-flex justify-content-between align-items-start py-2 border-bottom';

        const info = document.createElement('div');
        const title = document.createElement('strong');
        title.className = 'small-text';
        title.textContent = m.description;
        const meta = document.createElement('span');
        meta.className = 'small-text text-muted';
        meta.innerHTML = '<br>' + m.time + ' · ' + m.calories + ' cal';
        info.appendChild(title);
        info.appendChild(meta);

        const btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-outline-danger';
        btn.innerHTML = '<i class="fas fa-trash"></i>';
        btn.addEventListener('click', () => deleteMeal(m.id));

        row.appendChild(info);
        row.appendChild(btn);
        list.appendChild(row);
    });
}

async function deleteMeal(id) {
    try {
        await NutriAI.fetchJson('/MealTracker/Delete?id=' + id, { method: 'DELETE' });
        loadMeals();
        NutriAI.showToast('Meal deleted');
    } catch {
        NutriAI.showToast('Failed to delete meal', 'danger');
    }
}

function showLoading(show) {
    document.getElementById('aiLoading').classList.toggle('active', show);
}
