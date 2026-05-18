// ============================================
// TripPlanner — Frontend JavaScript
// ============================================

const API = 'api';
let currentUser = null;
let currentEditId = null;
let currentEditType = null;
let cachedCities = [];
let cachedUsers = [];
let cachedLocations = [];

// ============================================
// Init
// ============================================
document.addEventListener('DOMContentLoaded', async () => {
    await refreshCache();
    populateAccountSelector();
});

// ============================================
// Account selector
// ============================================
function populateAccountSelector() {
    const sel = document.getElementById('account-selector');
    sel.innerHTML = cachedUsers.map(u =>
        `<option value="${u.userId}">${u.username} (${u.role === 'admin' ? 'Адмін' : 'Користувач'})</option>`
    ).join('');

    sel.addEventListener('change', () => {
        const userId = parseInt(sel.value);
        currentUser = cachedUsers.find(u => u.userId === userId);
        applyRole();
    });

    // Auto-select first user
    if (cachedUsers.length > 0) {
        currentUser = cachedUsers[0];
        sel.value = currentUser.userId;
        applyRole();
    }
}

function applyRole() {
    if (!currentUser) return;

    const isAdmin = currentUser.role === 'admin';
    document.body.classList.toggle('is-admin', isAdmin);

    const navLinks = document.getElementById('nav-links');

    if (isAdmin) {
        navLinks.innerHTML = `
            <a href="#" class="nav-link active" data-section="cities">Міста</a>
            <a href="#" class="nav-link" data-section="locations">Локації</a>
            <a href="#" class="nav-link" data-section="users">Користувачі</a>
            <a href="#" class="nav-link" data-section="all-reviews">Усі відгуки</a>
        `;
    } else {
        navLinks.innerHTML = `
            <a href="#" class="nav-link" data-section="cities">Міста</a>
            <a href="#" class="nav-link" data-section="locations">Локації</a>
            <a href="#" class="nav-link active" data-section="trips">Подорожі</a>
            <a href="#" class="nav-link" data-section="reviews">Відгуки</a>
        `;
    }

    // Rebind nav click events
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const section = link.dataset.section;
            document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            link.classList.add('active');
            document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
            document.getElementById(`${section}-section`).classList.add('active');
            loadSection(section);
        });
    });

    // Show first section
    document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
    const firstLink = navLinks.querySelector('.nav-link.active');
    if (firstLink) {
        document.getElementById(`${firstLink.dataset.section}-section`).classList.add('active');
        loadSection(firstLink.dataset.section);
    }

    showToast(`Ви увійшли як ${currentUser.username} (${isAdmin ? 'Адмін' : 'Користувач'})`);
}

function loadSection(section) {
    switch (section) {
        case 'cities': loadCities(); break;
        case 'locations': loadLocations(); break;
        case 'trips': loadTrips(); break;
        case 'reviews': loadReviews(); break;
        case 'users': loadUsers(); break;
        case 'all-reviews': loadAllReviews(); break;
    }
}

// ============================================
// Toast
// ============================================
function showToast(message, isError = false) {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = 'toast show' + (isError ? ' error' : '');
    setTimeout(() => toast.className = 'toast', 3000);
}

// ============================================
// Modal
// ============================================
function openModal(type, editData = null) {
    currentEditType = type;
    currentEditId = editData ? editData.id : null;
    const overlay = document.getElementById('modal-overlay');
    const title = document.getElementById('modal-title');
    const body = document.getElementById('modal-body');
    const isEdit = editData !== null;
    let html = '';

    switch (type) {
        case 'city':
            title.textContent = isEdit ? 'Редагувати місто' : 'Додати місто';
            html = `
                <div class="form-group"><label class="form-label">Назва</label>
                <input class="form-input" id="f-name" value="${isEdit ? editData.name : ''}" placeholder="Назва міста"></div>
                <div class="form-group"><label class="form-label">Опис</label>
                <textarea class="form-textarea" id="f-description" placeholder="Опис міста">${isEdit ? (editData.description || '') : ''}</textarea></div>
                <div class="form-group"><label class="form-label">Категорія</label>
                <input class="form-input" id="f-category" value="${isEdit ? (editData.category || '') : ''}" placeholder="Туристичне, Історичне..."></div>
                <div class="form-actions">
                    <button class="btn btn-secondary" onclick="closeModal()">Скасувати</button>
                    <button class="btn btn-primary" onclick="saveCity()">${isEdit ? 'Зберегти' : 'Додати'}</button>
                </div>`;
            break;
        case 'location':
            title.textContent = isEdit ? 'Редагувати локацію' : 'Додати локацію';
            html = `
                <div class="form-group"><label class="form-label">Місто</label>
                <select class="form-select" id="f-cityId">${getCityOptions(isEdit ? editData.cityId : null)}</select></div>
                <div class="form-group"><label class="form-label">Назва</label>
                <input class="form-input" id="f-name" value="${isEdit ? editData.name : ''}" placeholder="Назва локації"></div>
                <div class="form-group"><label class="form-label">Адреса</label>
                <input class="form-input" id="f-address" value="${isEdit ? (editData.address || '') : ''}" placeholder="Адреса"></div>
                <div class="form-group"><label class="form-label">Опис</label>
                <textarea class="form-textarea" id="f-description" placeholder="Опис">${isEdit ? (editData.description || '') : ''}</textarea></div>
                <div class="form-group"><label class="form-label">Категорія</label>
                <input class="form-input" id="f-category" value="${isEdit ? (editData.category || '') : ''}" placeholder="Визначне місце, Природа..."></div>
                <div class="form-actions">
                    <button class="btn btn-secondary" onclick="closeModal()">Скасувати</button>
                    <button class="btn btn-primary" onclick="saveLocation()">${isEdit ? 'Зберегти' : 'Додати'}</button>
                </div>`;
            break;
        case 'trip':
            title.textContent = isEdit ? 'Редагувати подорож' : 'Нова подорож';
            html = `
                <div class="form-group"><label class="form-label">Назва</label>
                <input class="form-input" id="f-name" value="${isEdit ? editData.name : ''}" placeholder="Назва подорожі"></div>
                <div class="form-group"><label class="form-label">Опис</label>
                <textarea class="form-textarea" id="f-description" placeholder="Опис">${isEdit ? (editData.description || '') : ''}</textarea></div>
                ${isEdit ? `<div class="form-group"><label class="form-label">Статус</label>
                <select class="form-select" id="f-status">
                    <option value="active" ${editData.status === 'active' ? 'selected' : ''}>Активна</option>
                    <option value="completed" ${editData.status === 'completed' ? 'selected' : ''}>Завершена</option>
                    <option value="cancelled" ${editData.status === 'cancelled' ? 'selected' : ''}>Скасована</option>
                </select></div>` : ''}
                <div class="form-actions">
                    <button class="btn btn-secondary" onclick="closeModal()">Скасувати</button>
                    <button class="btn btn-primary" onclick="saveTrip()">${isEdit ? 'Зберегти' : 'Створити'}</button>
                </div>`;
            break;
        case 'review':
            title.textContent = isEdit ? 'Редагувати відгук' : 'Новий відгук';
            html = `
                <div class="form-group"><label class="form-label">Локація</label>
                <select class="form-select" id="f-locationId">${getLocationOptions(isEdit ? editData.locationId : null)}</select></div>
                <div class="form-group"><label class="form-label">Рейтинг</label>
                <select class="form-select" id="f-rating">
                    ${[5,4,3,2,1].map(n => `<option value="${n}" ${isEdit && editData.rating === n ? 'selected' : ''}>${'★'.repeat(n)}${'☆'.repeat(5-n)}</option>`).join('')}
                </select></div>
                <div class="form-group"><label class="form-label">Коментар</label>
                <textarea class="form-textarea" id="f-comment" placeholder="Ваші враження...">${isEdit ? (editData.comment || '') : ''}</textarea></div>
                <div class="form-actions">
                    <button class="btn btn-secondary" onclick="closeModal()">Скасувати</button>
                    <button class="btn btn-primary" onclick="saveReview()">${isEdit ? 'Зберегти' : 'Опублікувати'}</button>
                </div>`;
            break;
        case 'user':
            title.textContent = isEdit ? 'Редагувати користувача' : 'Новий користувач';
            html = `
                <div class="form-group"><label class="form-label">Нікнейм</label>
                <input class="form-input" id="f-username" value="${isEdit ? editData.username : ''}" placeholder="Нікнейм"></div>
                <div class="form-group"><label class="form-label">Email</label>
                <input class="form-input" id="f-email" type="email" value="${isEdit ? editData.email : ''}" placeholder="email@example.com"></div>
                ${!isEdit ? `<div class="form-group"><label class="form-label">Пароль</label>
                <input class="form-input" id="f-password" type="password" placeholder="Пароль"></div>` : ''}
                <div class="form-group"><label class="form-label">Роль</label>
                <select class="form-select" id="f-role">
                    <option value="user" ${isEdit && editData.role === 'user' ? 'selected' : ''}>Користувач</option>
                    <option value="admin" ${isEdit && editData.role === 'admin' ? 'selected' : ''}>Адміністратор</option>
                </select></div>
                <div class="form-actions">
                    <button class="btn btn-secondary" onclick="closeModal()">Скасувати</button>
                    <button class="btn btn-primary" onclick="saveUser()">${isEdit ? 'Зберегти' : 'Додати'}</button>
                </div>`;
            break;
        case 'tripLocation':
            title.textContent = 'Додати локацію в подорож';
            html = `
                <div class="form-group"><label class="form-label">Локація</label>
                <select class="form-select" id="f-locationId">${getLocationOptions()}</select></div>
                <div class="form-group"><label class="form-label">Дата та час</label>
                <input class="form-input" id="f-visitDatetime" type="datetime-local"></div>
                <div class="form-actions">
                    <button class="btn btn-secondary" onclick="closeModal()">Скасувати</button>
                    <button class="btn btn-primary" onclick="saveTripLocation()">Додати</button>
                </div>`;
            break;
    }

    body.innerHTML = html;
    overlay.classList.add('open');
}

function closeModal() {
    document.getElementById('modal-overlay').classList.remove('open');
    currentEditId = null;
    currentEditType = null;
}

// ============================================
// Helpers
// ============================================
function getCityOptions(sel) {
    return cachedCities.map(c => `<option value="${c.cityId}" ${c.cityId === sel ? 'selected' : ''}>${c.name}</option>`).join('');
}
function getUserOptions(sel) {
    return cachedUsers.map(u => `<option value="${u.userId}" ${u.userId === sel ? 'selected' : ''}>${u.username}</option>`).join('');
}
function getLocationOptions(sel) {
    return cachedLocations.map(l => `<option value="${l.locationId}" ${l.locationId === sel ? 'selected' : ''}>${l.name}${l.city ? ' (' + l.city.name + ')' : ''}</option>`).join('');
}

async function refreshCache() {
    try {
        const [cities, users, locations] = await Promise.all([
            fetch(`${API}/Cities`).then(r => r.json()),
            fetch(`${API}/Users`).then(r => r.json()),
            fetch(`${API}/Locations`).then(r => r.json())
        ]);
        cachedCities = cities;
        cachedUsers = users;
        cachedLocations = locations;
        populateFilters();
    } catch (e) { console.error('Cache error:', e); }
}

function populateFilters() {
    const lcf = document.getElementById('location-city-filter');
    if (lcf) {
        const v = lcf.value;
        lcf.innerHTML = '<option value="">Усі міста</option>' + cachedCities.map(c => `<option value="${c.cityId}">${c.name}</option>`).join('');
        lcf.value = v;
    }
    const rlf = document.getElementById('review-location-filter');
    if (rlf) {
        const v = rlf.value;
        rlf.innerHTML = '<option value="">Усі локації</option>' + cachedLocations.map(l => `<option value="${l.locationId}">${l.name}</option>`).join('');
        rlf.value = v;
    }
}

// ============================================
// CITIES
// ============================================
async function loadCities() {
    try {
        const res = await fetch(`${API}/Cities`);
        const cities = await res.json();
        const grid = document.getElementById('cities-grid');
        const isAdmin = currentUser && currentUser.role === 'admin';

        if (!cities.length) { grid.innerHTML = `<div class="empty-state"><div class="empty-state-icon">🏙️</div><p class="empty-state-text">Міст ще немає</p></div>`; return; }

        grid.innerHTML = cities.map(c => `
            <div class="card">
                ${c.category ? `<span class="card-badge badge-category">${c.category}</span>` : ''}
                <h3 class="card-title">${c.name}</h3>
                ${c.description ? `<p class="card-text">${c.description}</p>` : ''}
                <div class="card-meta"><span class="card-meta-item"><strong>${c.locations ? c.locations.length : 0}</strong> локацій</span></div>
                ${isAdmin ? `<div class="btn-group">
                    <button class="btn btn-secondary btn-sm" onclick='openModal("city",${JSON.stringify({id:c.cityId,name:c.name,description:c.description,category:c.category})})'>Редагувати</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteCity(${c.cityId})">Видалити</button>
                </div>` : ''}
            </div>`).join('');
    } catch (e) { showToast('Помилка завантаження', true); }
}

async function saveCity() {
    const data = { name: document.getElementById('f-name').value.trim(), description: document.getElementById('f-description').value.trim() || null, category: document.getElementById('f-category').value.trim() || null };
    if (!data.name) { showToast('Введіть назву', true); return; }
    try {
        let res;
        if (currentEditId) { data.cityId = currentEditId; res = await fetch(`${API}/Cities/${currentEditId}`, { method: 'PUT', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        else { res = await fetch(`${API}/Cities`, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        if (res.ok || res.status === 201 || res.status === 204) { showToast(currentEditId ? 'Місто оновлено' : 'Місто додано'); closeModal(); loadCities(); refreshCache(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка збереження', true); }
}

async function deleteCity(id) {
    if (!confirm('Видалити місто?')) return;
    try {
        const res = await fetch(`${API}/Cities/${id}`, { method: 'DELETE' });
        if (res.ok || res.status === 204) { showToast('Видалено'); loadCities(); refreshCache(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

// ============================================
// LOCATIONS
// ============================================
async function loadLocations() {
    try {
        const cityF = document.getElementById('location-city-filter').value;
        const catF = document.getElementById('location-category-filter').value;
        let url = `${API}/Locations?`;
        if (cityF) url += `cityId=${cityF}&`;
        if (catF) url += `category=${catF}&`;

        const res = await fetch(url);
        const locs = await res.json();
        const grid = document.getElementById('locations-grid');
        const isAdmin = currentUser && currentUser.role === 'admin';

        const cats = [...new Set(cachedLocations.map(l => l.category).filter(Boolean))];
        const cs = document.getElementById('location-category-filter');
        const cv = cs.value;
        cs.innerHTML = '<option value="">Усі категорії</option>' + cats.map(c => `<option value="${c}">${c}</option>`).join('');
        cs.value = cv;

        if (!locs.length) { grid.innerHTML = `<div class="empty-state"><div class="empty-state-icon">📍</div><p class="empty-state-text">Локацій не знайдено</p></div>`; return; }

        grid.innerHTML = locs.map(l => `
            <div class="card">
                ${l.category ? `<span class="card-badge badge-category">${l.category}</span>` : ''}
                <h3 class="card-title">${l.name}</h3>
                <p class="card-subtitle">${l.city ? l.city.name : ''} ${l.address ? '· ' + l.address : ''}</p>
                ${l.description ? `<p class="card-text">${l.description}</p>` : ''}
                ${isAdmin ? `<div class="btn-group">
                    <button class="btn btn-secondary btn-sm" onclick='openModal("location",${JSON.stringify({id:l.locationId,cityId:l.cityId,name:l.name,address:l.address,description:l.description,category:l.category})})'>Редагувати</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteLocation(${l.locationId})">Видалити</button>
                </div>` : ''}
            </div>`).join('');
    } catch (e) { showToast('Помилка завантаження', true); }
}

async function saveLocation() {
    const data = { cityId: parseInt(document.getElementById('f-cityId').value), name: document.getElementById('f-name').value.trim(), address: document.getElementById('f-address').value.trim() || null, description: document.getElementById('f-description').value.trim() || null, category: document.getElementById('f-category').value.trim() || null };
    if (!data.name) { showToast('Введіть назву', true); return; }
    try {
        let res;
        if (currentEditId) { data.locationId = currentEditId; res = await fetch(`${API}/Locations/${currentEditId}`, { method: 'PUT', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        else { res = await fetch(`${API}/Locations`, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        if (res.ok || res.status === 201 || res.status === 204) { showToast(currentEditId ? 'Оновлено' : 'Додано'); closeModal(); loadLocations(); refreshCache(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

async function deleteLocation(id) {
    if (!confirm('Видалити локацію?')) return;
    try {
        const res = await fetch(`${API}/Locations/${id}`, { method: 'DELETE' });
        if (res.ok || res.status === 204) { showToast('Видалено'); loadLocations(); refreshCache(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

// ============================================
// TRIPS (user sees only their own)
// ============================================
async function loadTrips() {
    try {
        const res = await fetch(`${API}/Trips?userId=${currentUser.userId}`);
        const trips = await res.json();
        const grid = document.getElementById('trips-grid');

        if (!trips.length) { grid.innerHTML = `<div class="empty-state"><div class="empty-state-icon">🗺️</div><p class="empty-state-text">У вас ще немає подорожей</p></div>`; return; }

        const tripsData = await Promise.all(trips.map(async t => {
            const tlRes = await fetch(`${API}/TripLocations?tripId=${t.tripId}`);
            const tl = await tlRes.json();
            return { ...t, tripLocations: tl };
        }));

        grid.innerHTML = tripsData.map(t => {
            const sc = t.status === 'active' ? 'badge-active' : t.status === 'completed' ? 'badge-completed' : 'badge-cancelled';
            const st = t.status === 'active' ? 'Активна' : t.status === 'completed' ? 'Завершена' : 'Скасована';
            return `
            <div class="card">
                <span class="card-badge ${sc}">${st}</span>
                <h3 class="card-title">${t.name}</h3>
                ${t.description ? `<p class="card-text">${t.description}</p>` : ''}
                ${t.tripLocations.length ? `<ul class="trip-locations-list">${t.tripLocations.map(tl =>
                    `<li>${tl.location ? tl.location.name : 'Локація #' + tl.locationId}${tl.visitDatetime ? ' · ' + new Date(tl.visitDatetime).toLocaleDateString('uk-UA') : ''}</li>`
                ).join('')}</ul>` : '<p class="card-subtitle">Локації ще не додані</p>'}
                <div class="card-meta">
                    <span class="card-meta-item"><strong>${t.tripLocations.length}</strong> локацій</span>
                    <span class="card-meta-item">Створено: <strong>${new Date(t.createdAt).toLocaleDateString('uk-UA')}</strong></span>
                </div>
                <div class="btn-group">
                    <button class="btn btn-primary btn-sm" onclick="addLocationToTrip(${t.tripId})">+ Локація</button>
                    <button class="btn btn-secondary btn-sm" onclick='openModal("trip",${JSON.stringify({id:t.tripId,userId:t.userId,name:t.name,description:t.description,status:t.status})})'>Редагувати</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteTrip(${t.tripId})">Видалити</button>
                </div>
            </div>`;
        }).join('');
    } catch (e) { showToast('Помилка завантаження', true); }
}

function addLocationToTrip(tripId) { currentEditId = tripId; openModal('tripLocation'); }

async function saveTripLocation() {
    const dt = document.getElementById('f-visitDatetime').value;
    const data = { tripId: currentEditId, locationId: parseInt(document.getElementById('f-locationId').value), visitDatetime: dt ? new Date(dt).toISOString() : null };
    try {
        const res = await fetch(`${API}/TripLocations`, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) });
        if (res.ok || res.status === 201) { showToast('Локацію додано'); closeModal(); loadTrips(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

async function saveTrip() {
    const data = { userId: currentUser.userId, name: document.getElementById('f-name').value.trim(), description: document.getElementById('f-description').value.trim() || null, status: currentEditId ? document.getElementById('f-status').value : 'active' };
    if (!data.name) { showToast('Введіть назву', true); return; }
    try {
        let res;
        if (currentEditId) { data.tripId = currentEditId; res = await fetch(`${API}/Trips/${currentEditId}`, { method: 'PUT', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        else { res = await fetch(`${API}/Trips`, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        if (res.ok || res.status === 201 || res.status === 204) { showToast(currentEditId ? 'Оновлено' : 'Створено'); closeModal(); loadTrips(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

async function deleteTrip(id) {
    if (!confirm('Видалити подорож?')) return;
    try {
        const res = await fetch(`${API}/Trips/${id}`, { method: 'DELETE' });
        if (res.ok || res.status === 204) { showToast('Видалено'); loadTrips(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

// ============================================
// REVIEWS (user sees only their own)
// ============================================
async function loadReviews() {
    try {
        const res = await fetch(`${API}/Reviews`);
        const all = await res.json();
        const reviews = all.filter(r => r.userId === currentUser.userId);
        const grid = document.getElementById('reviews-grid');

        if (!reviews.length) { grid.innerHTML = `<div class="empty-state"><div class="empty-state-icon">💬</div><p class="empty-state-text">У вас ще немає відгуків</p></div>`; return; }

        grid.innerHTML = reviews.map(r => `
            <div class="card">
                <div class="stars">${'★'.repeat(r.rating)}${'☆'.repeat(5 - r.rating)}</div>
                <h3 class="card-title">${r.location ? r.location.name : 'Локація #' + r.locationId}</h3>
                ${r.comment ? `<p class="card-text">"${r.comment}"</p>` : ''}
                <div class="card-meta"><span class="card-meta-item">${new Date(r.createdAt).toLocaleDateString('uk-UA')}</span></div>
                <div class="btn-group">
                    <button class="btn btn-secondary btn-sm" onclick='openModal("review",${JSON.stringify({id:r.reviewId,userId:r.userId,locationId:r.locationId,rating:r.rating,comment:r.comment})})'>Редагувати</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteReview(${r.reviewId})">Видалити</button>
                </div>
            </div>`).join('');
    } catch (e) { showToast('Помилка завантаження', true); }
}

async function saveReview() {
    const data = { userId: currentUser.userId, locationId: parseInt(document.getElementById('f-locationId').value), rating: parseInt(document.getElementById('f-rating').value), comment: document.getElementById('f-comment').value.trim() || null };
    try {
        let res;
        if (currentEditId) { data.reviewId = currentEditId; res = await fetch(`${API}/Reviews/${currentEditId}`, { method: 'PUT', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        else { res = await fetch(`${API}/Reviews`, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        if (res.ok || res.status === 201 || res.status === 204) { showToast(currentEditId ? 'Оновлено' : 'Опубліковано'); closeModal(); loadReviews(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

async function deleteReview(id) {
    if (!confirm('Видалити відгук?')) return;
    try {
        const res = await fetch(`${API}/Reviews/${id}`, { method: 'DELETE' });
        if (res.ok || res.status === 204) { showToast('Видалено'); loadReviews(); loadAllReviews(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

// ============================================
// ALL REVIEWS (admin moderation)
// ============================================
async function loadAllReviews() {
    try {
        const locF = document.getElementById('review-location-filter').value;
        let url = `${API}/Reviews?`;
        if (locF) url += `locationId=${locF}&`;

        const res = await fetch(url);
        const reviews = await res.json();
        const grid = document.getElementById('all-reviews-grid');

        if (!reviews.length) { grid.innerHTML = `<div class="empty-state"><div class="empty-state-icon">💬</div><p class="empty-state-text">Відгуків немає</p></div>`; return; }

        grid.innerHTML = reviews.map(r => `
            <div class="card">
                <div class="stars">${'★'.repeat(r.rating)}${'☆'.repeat(5 - r.rating)}</div>
                <h3 class="card-title">${r.location ? r.location.name : 'Локація #' + r.locationId}</h3>
                <p class="card-subtitle">${r.user ? r.user.username : 'Користувач #' + r.userId}</p>
                ${r.comment ? `<p class="card-text">"${r.comment}"</p>` : ''}
                <div class="card-meta"><span class="card-meta-item">${new Date(r.createdAt).toLocaleDateString('uk-UA')}</span></div>
                <div class="btn-group">
                    <button class="btn btn-danger btn-sm" onclick="deleteReview(${r.reviewId})">Видалити</button>
                </div>
            </div>`).join('');
    } catch (e) { showToast('Помилка завантаження', true); }
}

// ============================================
// USERS (admin only)
// ============================================
async function loadUsers() {
    try {
        const res = await fetch(`${API}/Users`);
        const users = await res.json();
        const grid = document.getElementById('users-grid');

        if (!users.length) { grid.innerHTML = `<div class="empty-state"><div class="empty-state-icon">👤</div><p class="empty-state-text">Користувачів немає</p></div>`; return; }

        grid.innerHTML = users.map(u => `
            <div class="card">
                <span class="card-badge ${u.role === 'admin' ? 'badge-admin' : 'badge-user'}">${u.role === 'admin' ? 'Адмін' : 'Користувач'}</span>
                <h3 class="card-title">${u.username}</h3>
                <p class="card-subtitle">${u.email}</p>
                <div class="user-stats">
                    <div class="stat"><div class="stat-number">${u.trips ? u.trips.length : 0}</div><div class="stat-label">Подорожей</div></div>
                    <div class="stat"><div class="stat-number">${u.reviews ? u.reviews.length : 0}</div><div class="stat-label">Відгуків</div></div>
                </div>
                <div class="btn-group">
                    <button class="btn btn-secondary btn-sm" onclick='openModal("user",${JSON.stringify({id:u.userId,username:u.username,email:u.email,role:u.role})})'>Редагувати</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteUser(${u.userId})">Видалити</button>
                </div>
            </div>`).join('');
    } catch (e) { showToast('Помилка завантаження', true); }
}

async function saveUser() {
    const data = { username: document.getElementById('f-username').value.trim(), email: document.getElementById('f-email').value.trim(), role: document.getElementById('f-role').value };
    if (!data.username || !data.email) { showToast('Заповніть всі поля', true); return; }
    try {
        let res;
        if (currentEditId) { data.userId = currentEditId; data.passwordHash = 'unchanged'; res = await fetch(`${API}/Users/${currentEditId}`, { method: 'PUT', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        else { const pw = document.getElementById('f-password').value.trim(); if (!pw) { showToast('Введіть пароль', true); return; } data.passwordHash = pw; res = await fetch(`${API}/Users`, { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data) }); }
        if (res.ok || res.status === 201 || res.status === 204) { showToast(currentEditId ? 'Оновлено' : 'Додано'); closeModal(); loadUsers(); refreshCache(); populateAccountSelector(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}

async function deleteUser(id) {
    if (!confirm('Видалити користувача?')) return;
    try {
        const res = await fetch(`${API}/Users/${id}`, { method: 'DELETE' });
        if (res.ok || res.status === 204) { showToast('Видалено'); loadUsers(); refreshCache(); }
        else { const err = await res.json(); showToast(err.message || 'Помилка', true); }
    } catch (e) { showToast('Помилка', true); }
}