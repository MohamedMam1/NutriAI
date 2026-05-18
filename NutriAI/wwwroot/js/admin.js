let currentPage = 1;
let searchTimeout;

document.addEventListener('DOMContentLoaded', async () => {
    await loadStats();
    await loadUsers();

    document.getElementById('userSearch').addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            currentPage = 1;
            loadUsers(e.target.value);
        }, 300);
    });
});

async function loadStats() {
    try {
        const data = await NutriAI.fetchJson('/Admin/GetStats');
        const container = document.getElementById('adminStats');
        const items = [
            { label: 'Total Users', value: data.totalUsers, icon: 'fa-users', color: 'green' },
            { label: 'Active Users', value: data.activeUsers, icon: 'fa-user-check', color: 'blue' },
            { label: 'Meal Logs', value: data.totalMealLogs, icon: 'fa-utensils', color: 'orange' },
            { label: 'Recipes Analyzed', value: data.totalRecipes, icon: 'fa-book', color: 'green' }
        ];

        container.replaceChildren();
        items.forEach(item => {
            const col = document.createElement('div');
            col.className = 'col-md-6 col-lg-3';
            const card = document.createElement('div');
            card.className = 'card-nutri admin-stat-card hover-scale text-center';
            card.innerHTML =
                '<motion class="stat-icon ' + item.color + ' mx-auto mb-2"><i class="fas ' + item.icon + '"></i></motion>' +
                '<p class="card-title">' + item.label + '</p><p class="stat-value">' +
                NutriAI.formatNumber(item.value) + '</p>';
            card.innerHTML = card.innerHTML.split('motion').join('div');
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
        renderPagination(data.page, data.totalPages);
    } catch {
        NutriAI.showToast('Failed to load users', 'danger');
    }
}

function renderUsersTable(users) {
    const tbody = document.getElementById('usersTableBody');
    tbody.replaceChildren();
    users.forEach(u => {
        const tr = document.createElement('tr');
        const statusClass = u.status === 'Active' ? 'success' : 'secondary';
        tr.innerHTML =
            '<td>' + u.id + '</td><td>' + u.name + '</td><td>' + u.email + '</td>' +
            '<td><span class="badge bg-' + statusClass + '">' + u.status + '</span></td><td>' + u.joined + '</td>';
        tbody.appendChild(tr);
    });
}

function renderPagination(page, totalPages) {
    const ul = document.getElementById('usersPagination');
    ul.replaceChildren();
    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement('li');
        li.className = 'page-item' + (i === page ? ' active' : '');
        const a = document.createElement('a');
        a.className = 'page-link';
        a.href = '#';
        a.textContent = i;
        a.addEventListener('click', (e) => {
            e.preventDefault();
            currentPage = i;
            loadUsers(document.getElementById('userSearch').value);
        });
        li.appendChild(a);
        ul.appendChild(li);
    }
}
