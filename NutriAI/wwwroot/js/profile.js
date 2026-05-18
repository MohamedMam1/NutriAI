document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('profileForm').addEventListener('submit', saveProfile);
});

async function saveProfile(e) {
    e.preventDefault();
    const form = e.target;
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const payload = {
        name: document.getElementById('name').value,
        age: parseInt(document.getElementById('age').value, 10),
        gender: document.getElementById('gender').value,
        height: parseFloat(document.getElementById('height').value),
        currentWeight: parseFloat(document.getElementById('currentWeight').value),
        goalWeight: parseFloat(document.getElementById('goalWeight').value),
        activityLevel: document.getElementById('activityLevel').value,
        dailyWaterTargetMl: parseInt(document.getElementById('dailyWater').value, 10)
    };

    try {
        await NutriAI.fetchJson('/Profile/Save', {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        NutriAI.showToast('Profile saved successfully');
    } catch {
        NutriAI.showToast('Failed to save profile', 'danger');
    }
}
