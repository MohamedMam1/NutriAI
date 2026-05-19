let weightChart;

document.addEventListener('DOMContentLoaded', async () => {
    document.getElementById('weightForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const weight = parseFloat(document.getElementById('weightInput').value);
        if (!weight) return;
        try {
            const result = await NutriAI.fetchJson('/Weight/Add', {
                method: 'POST',
                body: JSON.stringify({ weight })
            });
            if (!result.success) {
                NutriAI.showToast(result.message || 'Invalid weight value.', 'danger');
                return;
            }
            document.getElementById('weightInput').value = '';
            NutriAI.showToast(result.message || 'Weight saved');
            await loadWeightData();
        } catch (err) {
            NutriAI.showToast(err.payload?.message || 'Failed to save weight', 'danger');
        }
    });
    await loadWeightData();
});

async function loadWeightData() {
    try {
        const data = await NutriAI.fetchJson('/Weight/GetData');
        document.getElementById('displayCurrentWeight').textContent = data.currentWeight;
        document.getElementById('displayGoalWeight').textContent = data.goalWeight;
        const diff = (data.currentWeight - data.goalWeight).toFixed(1);
        document.getElementById('weightToGo').textContent = diff > 0 ? diff : '0';
        const insightEl = document.getElementById('weightAiInsight');
        if (insightEl && data.aiInsight) insightEl.textContent = data.aiInsight;

        renderHistory(data.history);
        renderChart(data.history, data.goalWeight);
    } catch {
        NutriAI.showToast('Failed to load weight data', 'danger');
    }
}

function renderHistory(history) {
    const list = document.getElementById('weightHistoryList');
    list.replaceChildren();
    [...history].reverse().forEach(entry => {
        const row = document.createElement('div');
        row.className = 'd-flex justify-content-between align-items-center py-2 border-bottom gap-2';
        const date = document.createElement('span');
        date.textContent = entry.date;
        const w = document.createElement('strong');
        w.textContent = entry.weight + ' kg';
        const btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-outline-danger';
        btn.innerHTML = '<i class="fas fa-trash"></i>';
        btn.addEventListener('click', () => deleteWeight(entry.id));
        row.appendChild(date);
        const right = document.createElement('div');
        right.className = 'd-flex align-items-center gap-2';
        right.appendChild(w);
        right.appendChild(btn);
        row.appendChild(right);
        list.appendChild(row);
    });
}

async function deleteWeight(id) {
    try {
        const result = await NutriAI.fetchJson('/Weight/Delete?id=' + id, { method: 'DELETE' });
        if (!result.success) {
            NutriAI.showToast(result.message || 'Could not delete entry.', 'danger');
            return;
        }
        NutriAI.showToast(result.message || 'Weight entry deleted');
        await loadWeightData();
    } catch {
        NutriAI.showToast('Failed to delete weight entry', 'danger');
    }
}

function renderChart(history, goalWeight) {
    const ctx = document.getElementById('weightChart');
    if (!ctx) return;
    const labels = history.map(h => h.date.slice(5));
    const values = history.map(h => h.weight);

    if (weightChart) weightChart.destroy();
    weightChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: [
                {
                    label: 'Weight (kg)',
                    data: values,
                    borderColor: '#4CAF50',
                    backgroundColor: 'rgba(76, 175, 80, 0.1)',
                    fill: true,
                    tension: 0.4
                },
                {
                    label: 'Goal',
                    data: Array(values.length).fill(goalWeight),
                    borderColor: '#FF9800',
                    borderDash: [6, 6],
                    pointRadius: 0
                }
            ]
        },
        options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
    });
}
