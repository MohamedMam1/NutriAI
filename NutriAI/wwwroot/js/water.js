let waterChart;

document.addEventListener('DOMContentLoaded', async () => {
    document.querySelectorAll('.water-add-btn').forEach(btn => {
        btn.addEventListener('click', () => addWater(parseInt(btn.dataset.ml, 10)));
    });
    document.getElementById('customWaterForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const ml = parseInt(document.getElementById('customWaterMl').value, 10);
        if (ml > 0) await addWater(ml);
    });
    await refreshWater();
});

async function refreshWater() {
    try {
        const data = await NutriAI.fetchJson('/Water/GetStatus');
        updateUi(data);
    } catch {
        NutriAI.showToast('Failed to load water data', 'danger');
    }
}

async function addWater(amountMl) {
    try {
        const data = await NutriAI.fetchJson('/Water/Add', {
            method: 'POST',
            body: JSON.stringify({ amountMl })
        });
        if (data.success === false) {
            NutriAI.showToast(data.message || 'Invalid water amount.', 'danger');
            return;
        }
        updateUi(data);
        NutriAI.showToast(data.message || ('+' + amountMl + 'ml added'));
    } catch (err) {
        NutriAI.showToast(err.payload?.message || 'Failed to add water', 'danger');
    }
}

function updateUi(data) {
    document.getElementById('waterCurrent').textContent = data.currentMl;
    document.getElementById('waterGoal').textContent = data.goalMl;
    document.getElementById('waterPercent').textContent = data.percent + '%';
    document.getElementById('waterBar').style.width = data.percent + '%';
    const rec = document.getElementById('waterRecommendation');
    if (rec && data.recommendation) rec.textContent = data.recommendation;
    drawCircle(data.percent);
}

function drawCircle(percent) {
    const canvas = document.getElementById('waterCircle');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const size = 180;
    const line = 14;
    const radius = (size - line) / 2;
    const center = size / 2;

    ctx.clearRect(0, 0, size, size);
    ctx.beginPath();
    ctx.arc(center, center, radius, 0, Math.PI * 2);
    ctx.strokeStyle = '#e9ecef';
    ctx.lineWidth = line;
    ctx.stroke();

    const angle = (percent / 100) * Math.PI * 2 - Math.PI / 2;
    ctx.beginPath();
    ctx.arc(center, center, radius, -Math.PI / 2, angle);
    ctx.strokeStyle = '#2196F3';
    ctx.lineWidth = line;
    ctx.lineCap = 'round';
    ctx.stroke();
}
