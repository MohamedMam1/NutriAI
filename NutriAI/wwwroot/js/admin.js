let currentPage = 1;
let activityPage = 1;
let activePanel = 'meals';
let searchTimeout;
let userModal;

const panelTitles = { meals: 'Meal logs', recipes: 'Recipe analyses', reports: 'Weekly reports' };
const panelEndpoints = {
    meals: '/Admin/GetMealLogs',
    recipes: '/Admin/GetRecipeAnalyses',
    reports: '/Admin/GetWeeklyReports'
};

document.addEventListener('DOMContentLoaded', async () => {
    userModal = new bootstrap.Modal(document.getElementById('userModal'));
    await loadStats();
    await loadUsers();
    await loadActivityPanel();

    document.getElementById('userSearch').addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            currentPage = 1;
            loadUsers(e.target.value);
        }, 300);
    });

    document.getElementById('btnAddUser').addEventListener('click', () => openUserModal());
    document.getElementById('btnSaveUser').addEventListener('click', saveUser);

    document.querySelectorAll('#adminSideNav button').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('#adminSideNav button').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            activePanel = btn.dataset.panel;
            activityPage = 1;
            document.getElementById('activityPanelTitle').textContent = panelTitles[activePanel];
            loadActivityPanel();
        });
    });
});

async function loadStats() {
    try {
        const data = await NutriAI.fetchJson('/Admin/GetStats');
        const container = document.getElementById('adminStats');
        const items = [
            { label: 'Total Users', value: data.totalUsers, icon: 'fa-users', color: 'green' },
            { label: 'Active Users', value: data.activeUsers, icon: 'fa-user-check', color: 'blue' },
            { label: 'Banned Users', value: data.bannedUsers, icon: 'fa-user-slash', color: 'orange' },
            { label: 'Meal Logs', value: data.totalMealLogs, icon: 'fa-utensils', color: 'green' },
            { label: 'Recipes', value: data.totalRecipes, icon: 'fa-book', color: 'blue' },
            { label: 'Status', value: data.status, icon: 'fa-heartbeat', color: 'green' }
        ];

        container.replaceChildren();
        items.forEach(item => {
            const col = document.createElement('div');
            col.className = 'col-md-6 col-lg-4 col-xl-2';
            const card = document.createElement('div');
            card.className = 'card-nutri admin-stat-card hover-scale text-center';
            card.innerHTML =
                '<div class="stat-icon ' + item.color + ' mx-auto mb-2"><i class="fas ' + item.icon + '"></i></div>' +
                '<p class="card-title">' + item.label + '</p><p class="stat-value">' +
                (typeof item.value === 'number' ? NutriAI.formatNumber(item.value) : item.value) + '</p>';
            col.appendChild(card);
            container.appendChild(col);
        });
    } catch {
        NutriAI.showToast('Failed to load admin stats', 'danger');
    }
}

async function loadUsers(search = '') {
    try {
        const q = '?page=' + currentPage + (search ? '&search=' + encodeURIComponent(search) : '');
        const data = await NutriAI.fetchJson('/Admin/GetUsers' + q);
        renderUsersTable(data.users);
        renderPagination(data.page, data.totalPages, 'usersPagination', (p) => {
            currentPage = p;
            loadUsers(document.getElementById('userSearch').value);
        });
    } catch {
        NutriAI.showToast('Failed to load users', 'danger');
    }
}

function renderUsersTable(users) {
    const tbody = document.getElementById('usersTableBody');
    tbody.replaceChildren();
    users.forEach(u => {
        const tr = document.createElement('tr');
        const statusClass = u.isBanned ? 'danger' : u.status === 'Active' ? 'success' : 'secondary';
        const statusLabel = u.isBanned ? 'Banned' : u.status;
        tr.innerHTML =
            '<td>' + escapeHtml(u.name) + '</td>' +
            '<td>' + escapeHtml(u.email) + '</td>' +
            '<td><span class="badge bg-' + statusClass + '">' + statusLabel + '</span></td>' +
            '<td>' + u.mealCount + '</td>' +
            '<td>' + u.joined + '</td>' +
            '<td class="text-nowrap"></td>';
        const actions = tr.lastElementChild;

        const editBtn = document.createElement('button');
        editBtn.className = 'btn btn-sm btn-outline-primary me-1';
        editBtn.innerHTML = '<i class="fas fa-edit"></i>';
        editBtn.addEventListener('click', () => openUserModal(u));

        const banBtn = document.createElement('button');
        banBtn.className = 'btn btn-sm btn-outline-warning me-1';
        banBtn.title = u.isBanned ? 'Unban' : 'Ban';
        banBtn.innerHTML = '<i class="fas fa-ban"></i>';
        banBtn.addEventListener('click', () => toggleBan(u));

        const delBtn = document.createElement('button');
        delBtn.className = 'btn btn-sm btn-outline-danger';
        delBtn.innerHTML = '<i class="fas fa-trash"></i>';
        delBtn.addEventListener('click', () => deleteUser(u));

        actions.appendChild(editBtn);
        actions.appendChild(banBtn);
        if (!u.roles?.includes('Admin')) actions.appendChild(delBtn);
        tbody.appendChild(tr);
    });
}

function openUserModal(user) {
    document.getElementById('userModalTitle').textContent = user ? 'Edit user' : 'Add user';
    document.getElementById('userId').value = user?.id || '';
    document.getElementById('userFullName').value = user?.name || '';
    document.getElementById('userEmail').value = user?.email || '';
    document.getElementById('userPassword').value = '';
    document.getElementById('passwordField').style.display = user ? 'none' : 'block';
    userModal.show();
}

async function saveUser() {
    const id = document.getElementById('userId').value;
    const fullName = document.getElementById('userFullName').value.trim();
    const email = document.getElementById('userEmail').value.trim();
    const password = document.getElementById('userPassword').value;

    try {
        let result;
        if (id) {
            result = await NutriAI.fetchJson('/Admin/UpdateUser/' + id, {
                method: 'PUT',
                body: JSON.stringify({ fullName, email })
            });
        } else {
            result = await NutriAI.fetchJson('/Admin/CreateUser', {
                method: 'POST',
                body: JSON.stringify({ fullName, email, password })
            });
        }
        if (!result.success) {
            NutriAI.showToast(result.message || 'Could not save user.', 'danger');
            return;
        }
        NutriAI.showToast(result.message || 'User saved');
        userModal.hide();
        await loadUsers(document.getElementById('userSearch').value);
    } catch {
        NutriAI.showToast('Failed to save user', 'danger');
    }
}

async function toggleBan(user) {
    const banned = !user.isBanned;
    const msg = banned ? 'Ban this user?' : 'Unban this user?';
    if (!confirm(msg)) return;
    try {
        const result = await NutriAI.fetchJson('/Admin/SetBan/' + user.id, {
            method: 'POST',
            body: JSON.stringify({ banned })
        });
        if (!result.success) {
            NutriAI.showToast(result.message || 'Action failed.', 'danger');
            return;
        }
        NutriAI.showToast(result.message);
        await loadUsers(document.getElementById('userSearch').value);
    } catch {
        NutriAI.showToast('Failed to update ban status', 'danger');
    }
}

async function deleteUser(user) {
    if (!confirm('Delete user ' + user.email + '? This cannot be undone.')) return;
    try {
        const result = await NutriAI.fetchJson('/Admin/DeleteUser/' + user.id, { method: 'DELETE' });
        if (!result.success) {
            NutriAI.showToast(result.message || 'Could not delete user.', 'danger');
            return;
        }
        NutriAI.showToast(result.message || 'User deleted');
        await loadUsers(document.getElementById('userSearch').value);
    } catch {
        NutriAI.showToast('Failed to delete user', 'danger');
    }
}

async function loadActivityPanel(userId) {
    try {
        const endpoint = panelEndpoints[activePanel];
        let q = '?page=' + activityPage;
        if (userId) q += '&userId=' + encodeURIComponent(userId);
        const data = await NutriAI.fetchJson(endpoint + q);
        renderActivityItems(data.items);
        renderPagination(data.page, data.totalPages, 'activityPagination', (p) => {
            activityPage = p;
            loadActivityPanel(userId);
        });
    } catch {
        NutriAI.showToast('Failed to load activity data', 'danger');
    }
}

function renderActivityItems(items) {
    const container = document.getElementById('activityPanelContent');
    container.replaceChildren();
    if (!items?.length) {
        container.textContent = 'No records found.';
        return;
    }

    items.forEach(item => {
        const block = document.createElement('div');
        block.className = 'border-bottom py-2';
        if (activePanel === 'meals') {
            block.innerHTML =
                '<strong>' + escapeHtml(item.userName) + '</strong> <span class="text-muted">(' + escapeHtml(item.userEmail) + ')</span><br>' +
                escapeHtml(item.description) + ' · ' + item.calories + ' cal<br>' +
                '<span class="text-muted">' + item.loggedAt + '</span>';
        } else if (activePanel === 'recipes') {
            block.innerHTML =
                '<strong>' + escapeHtml(item.userName) + '</strong><br>' +
                escapeHtml(item.recipeName) + ' · ' + item.totalCalories + ' cal · ' + item.servings + ' servings<br>' +
                '<span class="text-muted">' + item.analyzedAt + '</span>';
        } else {
            block.innerHTML =
                '<strong>' + escapeHtml(item.userName) + '</strong><br>' +
                'Weight: ' + item.weightChangeKg + ' kg · Avg cal: ' + item.averageCalories +
                ' · Hydration: ' + item.hydrationScore + '%<br>' +
                'Best day: ' + escapeHtml(item.bestDay) + ' · Worst day: ' + escapeHtml(item.worstDay) + '<br>' +
                '<span class="text-muted">' + item.generatedAt + '</span>';
        }
        container.appendChild(block);
    });
}

function renderPagination(page, totalPages, elementId, onPage) {
    const ul = document.getElementById(elementId);
    ul.replaceChildren();
    if (totalPages <= 1) return;
    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement('li');
        li.className = 'page-item' + (i === page ? ' active' : '');
        const a = document.createElement('a');
        a.className = 'page-link';
        a.href = '#';
        a.textContent = i;
        a.addEventListener('click', (e) => {
            e.preventDefault();
            onPage(i);
        });
        li.appendChild(a);
        ul.appendChild(li);
    }
}

function escapeHtml(text) {
    const d = document.createElement('div');
    d.textContent = text ?? '';
    return d.innerHTML;
}

