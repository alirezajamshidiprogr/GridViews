let currentPage = 1;
let pageSize = 10;
let sortColumn = '';
let sortAsc = true;

// کمکی برای دسترسی امن به مقدار سلول
function getItemValue(item, prop) {
    if (!item) return '';
    if (prop in item) return item[prop]; // PascalCase
    const camel = prop.charAt(0).toLowerCase() + prop.slice(1);
    if (camel in item) return item[camel]; // camelCase
    for (const k of Object.keys(item)) {
        if (k.toLowerCase() === prop.toLowerCase()) return item[k]; // case-insensitive
    }
    return '';
}

// اعمال فیلتر
function applyFilters(items, filters) {
    if (!Array.isArray(items)) return [];
    return items.filter(item => {
        return Object.keys(filters).every(key => {
            if (!filters[key]) return true;
            const val = getItemValue(item, key);
            return val.toString().toLowerCase().includes(filters[key].toLowerCase());
        });
    });
}

// رندر ردیف‌ها
function renderRows(items, columns = null) {
    if (!Array.isArray(items)) return;

    const container = document.getElementById('gridContainer');
    if (!container) return;

    const bodyContainer = container.querySelector('.grid-body');
    if (!bodyContainer) return;

    // فقط اگر ستون‌ها داده نشده باشند، از localDataElement بخوانیم
    if (!columns) {
        const localDataElement = document.getElementById('gridDataLocal');
        if (localDataElement) {
            try {
                const data = JSON.parse(localDataElement.textContent);
                columns = (data.columns || []).filter(c => c.visible);
            } catch {
                console.warn('renderRows: couldnt parse columns JSON');
                columns = [];
            }
        }
    }

    // پاک کردن ردیف‌های قبلی
    bodyContainer.querySelectorAll('.grid-row').forEach(r => r.remove());

    items.forEach(item => {
        const row = document.createElement('div');
        row.className = 'grid-row';

        columns.forEach(col => {
            const div = document.createElement('div');
            div.className = 'grid-cell';
            div.textContent = getItemValue(item, col.prop);
            row.appendChild(div);
        });

        const actions = document.createElement('div');
        actions.className = 'grid-cell';
        actions.innerHTML = `
            <button class="btn primary edit-btn">ویرایش</button>
            <button class="btn danger delete-btn">حذف</button>
        `;
        row.appendChild(actions);

        bodyContainer.appendChild(row);
    });

    // آپدیت اطلاعات صفحه‌بندی
    const totalPages = Math.max(1, Math.ceil(items.length / pageSize));
    const pageInfo = document.getElementById('pageInfo');
    if (pageInfo) pageInfo.textContent = `صفحه ${currentPage} از ${totalPages}`;
    const summary = document.getElementById('gridSummary');
    if (summary) summary.textContent = `نمایش داده شده: ${items.length}`;
}

// فراخوانی داده‌ها
function fetchGridData(page, size) {
    const urlElement = document.getElementById('gridData');
    const localDataElement = document.getElementById('gridDataLocal');

    const filters = {};
    document.querySelectorAll('.filter-input').forEach(input => {
        const key = input.dataset.prop;
        const val = normalizePersianText(input.value);
        if (val) filters[key] = val;
    });

    const groupBy = document.getElementById('groupBySelector')?.value || '';

    if (urlElement && urlElement.dataset.url) {
        fetch(urlElement.dataset.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Page: page, PageSize: size, SortColumn: sortColumn, SortAsc: sortAsc, GroupBy: groupBy, Filters: filters })
        })
            .then(res => res.json())
            .then(data => {
                const items = Array.isArray(data.items) ? data.items : [];
                const localDataElement = document.getElementById('gridDataLocal');
                let columns = null;
                if (localDataElement) {
                    const d = JSON.parse(localDataElement.textContent);
                    columns = (d.columns || []).filter(c => c.visible);
                }
                renderRows(items, columns);
            })
            .catch(err => console.error('Error fetching grid data:', err));
    } else if (localDataElement) {
        try {
            const data = JSON.parse(localDataElement.textContent);
            const items = applyFilters(data.items || [], filters);
            renderRows(items);
        } catch (err) {
            console.error('Error parsing local grid data:', err);
        }
    } else {
        console.warn('No data source found for grid.');
    }
}

function normalizePersianText(str) {
    if (!str) return '';

    return str
        // اعداد عربی به فارسی
        .replace(/[٠١٢٣٤٥٦٧٨٩]/g, d => '۰۱۲۳۴۵۶۷۸۹'['٠١٢٣٤٥٦٧٨٩'.indexOf(d)])
        // حروف عربی به فارسی
        .replace(/ك/g, 'ک')
        .replace(/ي/g, 'ی')
        // حذف نیم‌فاصله و فاصله‌های اضافی
        .replace(/\u200c/g, ' ')
        // تبدیل فاصله‌های چندتایی به یکی
        .replace(/\s+/g, ' ')
        // حذف کاراکترهای نامرئی
        .replace(/[\u200B-\u200F\u202A-\u202E]/g, '')
        // نرمال‌سازی حروف هم‌معنی
        .replace(/[ۀﻩﻪﻫ]/g, 'ه')
        .replace(/[ؤﺅ]/g, 'و')
        .replace(/[إأٱآا]/g, 'ا')
        .replace(/[ئ]/g, 'ی')
        // پاک کردن فاصله از ابتدا و انتها
        .trim();
}


// initialize
function initGrid() {
    fetchGridData(currentPage, pageSize);

    const nextBtn = document.getElementById('nextPage');
    const prevBtn = document.getElementById('prevPage');
    const pageSizeSelector = document.getElementById('pageSizeSelector');
    const groupBySelector = document.getElementById('groupBySelector');
    const refreshBtn = document.getElementById('refreshBtn');

    if (nextBtn) nextBtn.addEventListener('click', () => { currentPage++; fetchGridData(currentPage, pageSize); });
    if (prevBtn) prevBtn.addEventListener('click', () => { if (currentPage > 1) { currentPage--; fetchGridData(currentPage, pageSize); } });
    if (pageSizeSelector) pageSizeSelector.addEventListener('change', function () { pageSize = parseInt(this.value); currentPage = 1; fetchGridData(currentPage, pageSize); });
    if (groupBySelector) groupBySelector.addEventListener('change', () => { fetchGridData(currentPage, pageSize); });
    if (refreshBtn) refreshBtn.addEventListener('click', () => { currentPage = 1; fetchGridData(currentPage, pageSize); });

    document.querySelectorAll('.filter-input').forEach(input => {
        input.addEventListener('input', () => { currentPage = 1; fetchGridData(currentPage, pageSize); });
    });

    document.querySelectorAll('.grid-header [data-column]').forEach(h => {
        h.addEventListener('click', () => {
            const col = h.dataset.column;
            if (sortColumn === col) sortAsc = !sortAsc;
            else { sortColumn = col; sortAsc = true; }
            fetchGridData(currentPage, pageSize);
        });
    });
}

document.addEventListener('DOMContentLoaded', initGrid);
