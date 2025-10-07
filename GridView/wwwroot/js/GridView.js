let currentPage = 1;
let pageSize = 10 ; // allItems = تعداد کل رکوردها
let totalPage = 0; // allItems = تعداد کل رکوردها
let sortColumn = '';
let sortAsc = true;
let enablePaging; 

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

            let val = getItemValue(item, key);

            // اگر عدد است، جداکننده حذف و تبدیل به string انگلیسی
            if (!isNaN(val) && val !== null && val !== '') {
                val = Number(val).toString();
            }

            // نرمال‌سازی متن (اعداد فارسی -> انگلیسی)
            val = val.replace(/[۰-۹]/g, d => '0123456789'['۰۱۲۳۴۵۶۷۸۹'.indexOf(d)]);

            const filterVal = filters[key].replace(/[۰-۹]/g, d => '0123456789'['۰۱۲۳۴۵۶۷۸۹'.indexOf(d)]);

            return val.toString().toLowerCase().includes(filterVal.toLowerCase());
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
            div.setAttribute('data-cell', col.prop); // اضافه شدن data-cell
            div.textContent = getItemValue(item, col.prop);
            let value = getItemValue(item, col.prop);

            // اگر عدد بود، جداکننده اضافه کن
            //if (!isNaN(value) && value !== '' && value !== null) {
            //    // parseFloat برای اطمینان از عدد بودن
            //    const num = parseFloat(value);
            //    // به صورت عدد فارسی با جداکننده
            //    value = num.toLocaleString('fa-IR');
            //}

            div.textContent = value;
            row.appendChild(div);
        });

        const actions = document.createElement('div');
        actions.className = 'grid-cell';
        actions.innerHTML = `
            <button class="btn primary edit-btn"><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><path d='M12 20h9'></path><path d='M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z'></path></svg>ويرايش</button>
            <button class="btn danger delete-btn"><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><polyline points='3 6 5 6 21 6'></polyline><path d='M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6'></path><path d='M10 11v6'></path><path d='M14 11v6'></path></svg> حذف</button>
        `;
        row.appendChild(actions);

        bodyContainer.appendChild(row);
    });

    updateGridFooters();
    // آپدیت اطلاعات صفحه‌بندی
    const totalPages = Math.max(1, Math.ceil(items.length / pageSize));
    const pageInfo = document.getElementById('pageInfo');
    if (pageInfo) pageInfo.textContent = `صفحه ${currentPage} از ${totalPage}`;
    const summary = document.getElementById('gridSummary');
    if (summary) summary.textContent = `نمایش داده شده: ${items.length}`;
}

// فراخوانی داده‌ها
function fetchGridData(page, size) {
    enablePaging = document.getElementById('gridSettings')?.dataset.enablePaging === 'true';

    const urlElement = document.getElementById('gridData');
    const localDataElement = document.getElementById('gridDataLocal');

    const filters = {};
    document.querySelectorAll('.filter-input').forEach(input => {
        const key = input.dataset.prop;
        const val = normalizePersianText(input.value);
        if (val) filters[key] = val;
    });

    const groupBy = document.getElementById('groupBySelector')?.value || '';

    // اگر Paging فعال نیست، size = تعداد کل رکوردها
    //if (!enablePaging && localDataElement) {
    //    try {
    //        const data = JSON.parse(localDataElement.textContent);
    //        let items = applyFilters(data.items || [], filters);

    //        // همه رکوردها در یک صفحه
    //        renderRows(items);

    //        // آپدیت اطلاعات صفحه‌بندی
    //        const pageInfo = document.getElementById('pageInfo');
    //        if (pageInfo) pageInfo.textContent = `نمایش همه ${items.length} رکورد`;

    //        return; // از ادامه اجرای Paging جلوگیری کن
    //    } catch (err) {
    //        console.error('Error parsing local grid data:', err);
    //        return;
    //    }
    //}

    // حالت معمولی Paging یا Fetch از سرور
    if (urlElement && urlElement.dataset.url) {
        fetch(urlElement.dataset.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Page: page, PageSize: size, SortColumn: sortColumn, SortAsc: sortAsc, GroupBy: groupBy, Filters: filters, enablePaging: enablePaging })
        })
            .then(res => res.json())
            .then(data => {
                debugger
                totalPage = Math.ceil(data.totalCount / data.pageSize);

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
            totalPage = Math.ceil(data.totalCount / data.pageSize);
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


/// بستن و باز كردم فيلتر هر ستون با مساوي نامساوي و ...
// باز و بسته کردن منوی فیلتر
document.querySelectorAll('.filter-icon').forEach((icon, index) => {
    // هر آیکن شناسه منحصربه‌فرد دارد
    icon.dataset.iconId = index;

    icon.addEventListener('click', e => {
        e.stopPropagation();
        const cell = e.target.parentElement;
        const originalMenu = cell.querySelector('.filter-menu');
        if (!originalMenu) return;

        // حذف منوهای باز قبلی
        document.querySelectorAll('.filter-menu.clone').forEach(m => m.remove());

        // clone منو
        const menu = originalMenu.cloneNode(true);
        menu.classList.add('clone');
        menu.dataset.originalIconId = icon.dataset.iconId;
        document.body.appendChild(menu);

        const rect = cell.getBoundingClientRect();
        menu.style.position = 'absolute';
        menu.style.top = rect.bottom + 'px';
        menu.style.left = rect.left + 'px';
        menu.style.display = 'block';
        menu.style.zIndex = 9999;

        // بستن وقتی بیرون کلیک شد
        const clickOutside = event => {
            if (!menu.contains(event.target) && !icon.contains(event.target)) {
                menu.remove();
                document.removeEventListener('click', clickOutside);
            }
        };
        document.addEventListener('click', clickOutside);
    });
});

// تغییر آیکن هنگام انتخاب گزینه از منو
document.addEventListener('click', function (e) {
    const li = e.target.closest('.filter-menu li');
    if (!li) return;

    e.stopPropagation();

    const menu = li.closest('.filter-menu');
    const iconId = menu.dataset.originalIconId;
    const icon = document.querySelector(`.filter-icon[data-icon-id="${iconId}"]`);
    if (!icon) return;

    // مقدار آیکن جدید (علامت شرط)
    const selectedIcon = li.getAttribute('data-icon'); // || li.textContent // متن نندازه;

    // نمایش 🔍 همیشه ثابت + آیکن شرط انتخابی
    icon.innerHTML = `🔍 ${selectedIcon}`;

    // بستن منو
    menu.remove();
});

// بستن منو وقتی جای دیگه کلیک شد
document.addEventListener('click', e => {
    if (!e.target.classList.contains('filter-icon')) {
        document.querySelectorAll('.filter-menu').forEach(menu => menu.style.display = 'none');
    }
});


// عمليات فوتر گريد جمع ، ميانگين و ...

document.querySelectorAll('.grid-cell[data-footer]').forEach(cell => {
    const input = cell.querySelector('.footer-input');
    const icon = cell.querySelector('.footer-icon');
    const menu = cell.querySelector('.footer-menu');

    // مقدار پیش‌فرض calcState
    if (!cell.dataset.calcState) {
        cell.dataset.calcState = 'sum';
        input.value = `جمع = 0`;
    }

    if (!icon || !menu) return;

    // باز کردن منو
    icon.addEventListener('click', e => {
        e.stopPropagation();

        // حذف منوهای باز قبلی
        document.querySelectorAll('.footer-menu.clone').forEach(m => m.remove());

        const clone = menu.cloneNode(true);
        clone.classList.add('clone');
        document.body.appendChild(clone);

        const rect = icon.getBoundingClientRect();
        clone.style.position = 'absolute';
        clone.style.top = (rect.bottom + window.scrollY) + 'px';
        clone.style.left = (rect.left + window.scrollX) + 'px';
        clone.style.display = 'block';
        clone.style.zIndex = 9999;

        // بستن وقتی بیرون کلیک شد
        const closeMenu = ev => {
            if (!clone.contains(ev.target) && ev.target !== icon) {
                clone.remove();
                document.removeEventListener('click', closeMenu);
            }
        };
        document.addEventListener('click', closeMenu);

        // انتخاب گزینه برای همین footer
        clone.querySelectorAll('li').forEach(li => {
            li.addEventListener('click', e => {
                e.stopPropagation();
                const calcType = li.dataset.calc;
                cell.dataset.calcState = calcType;

                updateGridFooters(calcType, cell);

                clone.remove();
            });
        });
    });
});


function updateGridFooters(calcType = null, targetFooter = null) {
    const footers = targetFooter ? [targetFooter] : document.querySelectorAll('[data-footer]');

    footers.forEach(footer => {
        const field = footer.dataset.footer;
        const calc = calcType || footer.dataset.calcState || 'sum';

        const cells = Array.from(document.querySelectorAll(`[data-cell="${field}"]`))
            .map(c => {
                let val = c.textContent.trim();

                // تبدیل اعداد فارسی به انگلیسی
                val = val.replace(/[۰-۹]/g, d => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d));

                // حذف جداکننده هزارگان
                val = val.replace(/,/g, '');

                return val;
            })
            .filter(v => /^[0-9]+(\.[0-9]+)?$/.test(v)) // فقط اعداد صحیح یا اعشاری
            .map(Number); // تبدیل به عدد

        const input = footer.querySelector('.footer-input');

        if (cells.length === 0) {
            input.value = '';
            return;
        }

        let result = 0;
        switch (calc) {
            case 'avg': result = cells.reduce((a, b) => a + b, 0) / cells.length; break;
            case 'count': result = cells.length; break;
            case 'max': result = Math.max(...cells); break;
            case 'min': result = Math.min(...cells); break;
            default: result = cells.reduce((a, b) => a + b, 0);
        }

        const formatted = result.toLocaleString('fa-IR', { maximumFractionDigits: 2 });
        if (input) {
            let label = { sum: 'جمع', avg: 'میانگین', count: 'تعداد', max: 'بیشترین', min: 'کمترین' }[calc] || 'جمع';
            input.value = `${label} = ${formatted}`;
        }

        footer.dataset.calcState = calc;
    });
}


// بستن منو وقتی جای دیگه کلیک شد
document.addEventListener('click', e => {
    if (!e.target.classList.contains('footer-icon')) {
        document.querySelectorAll('.footer-menu.clone').forEach(menu => menu.remove());
    }
});





document.addEventListener('DOMContentLoaded', initGrid);
