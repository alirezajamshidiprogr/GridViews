let allItems = [];
let resultRows = [];
let currentPage = 1;
let pageSize = 10;
let sortColumn = '';
let sortAsc = true;

function initGrid() {
    const hidden = document.getElementById('gridData');
    if (!hidden) return;
    allItems = JSON.parse(hidden.textContent);
    resultRows = [...allItems];
    attachEvents();
    renderPage();
}

function applyFilterAndSort() {
    resultRows = allItems.filter(item => {
        let include = true;
        document.querySelectorAll('.grid-filters input').forEach(input => {
            const col = input.id.replace('filter', '');
            if (input.value && !(item[col] ?? '').toString().toLowerCase().includes(input.value.toLowerCase())) {
                include = false;
            }
        });
        return include;
    });

    if (sortColumn) {
        resultRows.sort((a, b) => {
            let A = a[sortColumn] ?? '';
            let B = b[sortColumn] ?? '';
            if (!isNaN(Date.parse(A)) && !isNaN(Date.parse(B))) {
                A = new Date(A); B = new Date(B);
            } else { A = A.toString().toLowerCase(); B = B.toString().toLowerCase(); }
            if (A > B) return sortAsc ? 1 : -1;
            if (A < B) return sortAsc ? -1 : 1;
            return 0;
        });
    }
}

function renderPage() {
    const container = document.getElementById('gridContainer');
    if (!container) return;

    container.querySelectorAll('.grid-row, .group-row').forEach(r => r.remove());

    pageSize = parseInt(document.getElementById('pageSizeSelector')?.value || 10);
    const totalPages = Math.max(1, Math.ceil(resultRows.length / pageSize));
    if (currentPage > totalPages) currentPage = totalPages;
    const start = (currentPage - 1) * pageSize;
    const pageItems = resultRows.slice(start, start + pageSize);

    const groupBy = document.getElementById('groupBySelector')?.value || '';
    if (groupBy) {
        const grouped = {};
        pageItems.forEach(item => {
            const key = item[groupBy] ?? '';
            if (!grouped[key]) grouped[key] = [];
            grouped[key].push(item);
        });
        Object.keys(grouped).sort().forEach(k => {
            const groupRow = document.createElement('div');
            groupRow.className = 'group-row';
            groupRow.textContent = `${groupBy}: ${k} (${grouped[k].length})`;
            container.appendChild(groupRow);
            grouped[k].forEach(item => container.appendChild(createRow(item)));
        });
    } else {
        pageItems.forEach(item => container.appendChild(createRow(item)));
    }

    document.getElementById('pageInfo').textContent = `صفحه ${currentPage} از ${totalPages}`;
    document.getElementById('gridSummary').textContent = `کل: ${allItems.length} | نمایش داده شده: ${resultRows.length}`;
}

function createRow(item) {
    const row = document.createElement('div');
    row.className = 'grid-row';
    Object.keys(item).forEach(p => {
        const div = document.createElement('div');
        div.className = 'grid-cell';
        div.textContent = item[p];
        row.appendChild(div);
    });
    const actions = document.createElement('div');
    actions.className = 'grid-cell';
    actions.innerHTML = `<button class="btn primary edit-btn">ویرایش</button>
                         <button class="btn danger delete-btn">حذف</button>`;
    row.appendChild(actions);
    return row;
}

function attachEvents() {
    document.querySelectorAll('.grid-header [data-column]').forEach(h => {
        h.addEventListener('click', () => {
            const col = h.dataset.column;
            if (sortColumn === col) sortAsc = !sortAsc;
            else { sortColumn = col; sortAsc = true; }
            applyFilterAndSort(); renderPage();
        });
    });

    document.querySelectorAll('.grid-filters input').forEach(input => {
        input.addEventListener('input', () => { currentPage = 1; applyFilterAndSort(); renderPage(); });
    });

    document.getElementById('pageSizeSelector')?.addEventListener('change', () => { currentPage = 1; renderPage(); });
    document.getElementById('prevPage')?.addEventListener('click', () => { if (currentPage > 1) { currentPage--; renderPage(); } });
    document.getElementById('nextPage')?.addEventListener('click', () => { const totalPages = Math.max(1, Math.ceil(resultRows.length / pageSize)); if (currentPage < totalPages) { currentPage++; renderPage(); } });

    document.getElementById('groupBySelector')?.addEventListener('change', () => { renderPage(); });
    document.getElementById('refreshBtn')?.addEventListener('click', () => {
        const hidden = document.getElementById('gridData');
        allItems = JSON.parse(hidden.textContent); currentPage = 1; applyFilterAndSort(); renderPage();
    });
}

document.addEventListener('DOMContentLoaded', initGrid);
