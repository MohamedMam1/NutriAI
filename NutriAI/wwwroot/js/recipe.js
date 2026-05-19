document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('recipeForm').addEventListener('submit', analyzeRecipe);
});

async function analyzeRecipe(e) {
    e.preventDefault();
    const text = document.getElementById('recipeText').value.trim();
    if (!text) return;

    const loading = document.getElementById('recipeLoading');
    loading.classList.add('active');

    try {
        const data = await NutriAI.fetchJson('/Recipe/Analyze', {
            method: 'POST',
            body: JSON.stringify({ recipeText: text })
        });
        if (!data.success) {
            NutriAI.showToast(data.message || 'Could not analyze recipe.', 'danger');
            return;
        }
        if (data.message && data.dataSource === 'database') {
            NutriAI.showToast(data.message, 'warning');
        }
        renderResults(data);
        document.getElementById('recipeResults').classList.remove('d-none');
    } catch {
        NutriAI.showToast('Failed to analyze recipe. Please wait for the AI response.', 'danger');
    } finally {
        loading.classList.remove('active');
    }
}

function renderResults(data) {
    const summary = document.getElementById('recipeSummary');
    summary.replaceChildren();

    const cards = [
        { label: 'Total Calories', value: data.totalCalories, color: 'green' },
        { label: 'Per Serving', value: data.perServing.calories + ' cal', color: 'blue' },
        { label: 'Protein', value: data.perServing.protein + 'g', color: 'orange' },
        { label: 'Servings', value: data.servings, color: 'green' }
    ];

    cards.forEach(c => {
        const col = document.createElement('div');
        col.className = 'col-6 col-md-3';
        col.innerHTML = '<div class="text-center p-2"><p class="card-title mb-1">' + c.label +
            '</p><p class="stat-value">' + c.value + '</p></div>';
        summary.appendChild(col);
    });

    const tbody = document.querySelector('#ingredientTable tbody');
    tbody.replaceChildren();
    data.ingredients.forEach(ing => {
        const tr = document.createElement('tr');
        tr.innerHTML = '<td>' + ing.name + '</td><td>' + ing.amount + '</td><td>' + ing.calories + '</td>';
        tbody.appendChild(tr);
    });

    const altList = document.getElementById('alternativesList');
    altList.replaceChildren();
    data.alternatives.forEach(alt => {
        const li = document.createElement('li');
        li.className = 'mb-2 small-text';
        li.textContent = alt;
        altList.appendChild(li);
    });
}
