let currentPage = 1;
let pageSize = 30; // allItems = تعداد کل رکوردها
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
            const filterObj = filters[key];
            if (!filterObj || !filterObj.Value) return true;

            let val = getItemValue(item, key);

            if (!isNaN(val) && val !== null && val !== '') {
                val = Number(val).toString();
            }

            val = val.replace(/[۰-۹]/g, d => '0123456789'['۰۱۲۳۴۵۶۷۸۹'.indexOf(d)]);

            const filterVal = filterObj.Value.replace(/[۰-۹]/g, d => '0123456789'['۰۱۲۳۴۵۶۷۸۹'.indexOf(d)]);

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

    // پاک کردن ردیف‌ها و هدرهای گروه قبلی
    bodyContainer.innerHTML = '';

    // فقط اگر ستون‌ها داده نشده باشند، از localDataElement بخوانیم
    if (!columns) {
        const localDataElement = document.getElementById('gridDataLocal');
        if (localDataElement) {
            try {
                const data = JSON.parse(localDataElement.textContent);
                columns = data.columns || [];
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
            div.setAttribute('data-cell', col.prop);
            div.textContent = getItemValue(item, col.prop);
            // اگر ستون مخفی است
            if (!col.visible) {
                div.style.display = 'none';
            }

            row.appendChild(div);
        });

        const gridElement = $('.dynamic-grid-container').first().attr('id');;
        const actions = document.createElement('div');
        actions.className = 'grid-cell';

        actions.innerHTML = `
            <button class="btn primary edit-btn" onclick='InsUpd_${gridElement}_Item(this)'><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><path d='M12 20h9'></path><path d='M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z'></path></svg>ويرايش</button>
            <button class="btn danger delete-btn" onclick='Dlt_${gridElement}_Item(this)'><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><polyline points='3 6 5 6 21 6'></polyline><path d='M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6'></path><path d='M10 11v6'></path><path d='M14 11v6'></path></svg> حذف</button>
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

function groupItems(items, groupByField) {
    if (!groupByField) return items;

    const groups = {};
    items.forEach(item => {
        const key = getItemValue(item, groupByField) || '— بدون مقدار —';
        if (!groups[key]) groups[key] = [];
        groups[key].push(item);
    });

    return Object.entries(groups).map(([key, group]) => ({ key, group }));
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
        const type = input.dataset.filterType || 'contains'; // پیش‌فرض contains
        if (val) {
            // ارسال به صورت رشته JSON برای هر فیلد
            filters[key] = { Type: type, Value: val };
        }
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


    if (localDataElement && groupBy) {
        let items = applyFilters(window.allItemsCache || [], filters); // تمام رکوردها
        const data = JSON.parse(localDataElement.textContent);
        let columns = data.columns || [];

        renderGroupedRows(groupItems(items, groupBy), columns);

        return; // از ادامه جلوگیری می‌کنیم
    }

    // حالت معمولی Paging یا Fetch از سرور
    if (urlElement && urlElement.dataset.url) {
        fetch(urlElement.dataset.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Page: page,
                PageSize: size,
                SortColumn: sortColumn,
                SortAsc: sortAsc,
                GroupBy: groupBy,
                Filters: filters,
                enablePaging: enablePaging
            })
        })
            .then(res => res.json())
            .then(data => {
                totalPage = Math.ceil(data.totalCount / data.pageSize);
                const items = Array.isArray(data.items) ? data.items : [];

                // ✅ اینجا کل داده‌ها را در حافظه نگه می‌داریم برای گروه‌بندی بعدی
                window.allItemsCache = window.allItemsCache || [];
                window.allItemsCache = [...window.allItemsCache, ...items];

                let columns = null;
                if (localDataElement) {
                    const d = JSON.parse(localDataElement.textContent);
                    columns = d.columns || [];
                }
                if (groupBy && items.length) {
                    if (!columns) columns = []; // جلوگیری از خطای null
                    renderGroupedRows(groupItems(items, groupBy), columns);
                } else {
                    renderRows(items, columns);
                }
            })
            .catch(err => console.error('Error fetching grid data:', err));
    } else {
        console.warn('No data source found for grid.');
    }
}

function renderGroupedRows(groups, columns) {
    const container = document.getElementById('gridContainer');
    const bodyContainer = container.querySelector('.grid-body');
    bodyContainer.innerHTML = '';

    groups.forEach(g => {
        // هدر گروه
        const groupHeader = document.createElement('div');
        groupHeader.className = 'grid-group-header';

        // محاسبه جمع (مثال روی فیلد Quantity)
        const total = g.group.reduce((sum, item) => sum + Number(getItemValue(item, 'Quantity') || 0), 0);

        // آیکن collapse/expand + تعداد رکورد
        const toggle = document.createElement('span');
        toggle.textContent = ` [- ${g.group.length}] `; // پیش‌فرض باز و تعداد رکوردها
        toggle.style.cursor = 'pointer';
        toggle.style.marginRight = '8px';

        groupHeader.appendChild(toggle);

        const title = document.createElement('span');
        title.textContent = `${g.key} (جمع: ${total.toLocaleString('fa-IR')})`;
        groupHeader.appendChild(title);

        bodyContainer.appendChild(groupHeader);

        // ردیف‌های گروه
        g.group.forEach(item => {
            const row = document.createElement('div');
            row.className = 'grid-row';
            columns.forEach(col => {
                const div = document.createElement('div');
                div.className = 'grid-cell';
                div.setAttribute('data-cell', col.prop);
                div.textContent = getItemValue(item, col.prop);
                // اگر ستون مخفی است
                if (!col.visible) {
                    div.style.display = 'none';
                }
                row.appendChild(div);
            });
            bodyContainer.appendChild(row);
        });

        // رفتار collapse/expand
        toggle.addEventListener('click', () => {
            let sibling = groupHeader.nextElementSibling;
            while (sibling && !sibling.classList.contains('grid-group-header')) {
                sibling.style.display = sibling.style.display === 'none' ? 'flex' : 'none';
                sibling = sibling.nextElementSibling;
            }
            // تغییر متن toggle با تعداد
            const isCollapsed = toggle.textContent.startsWith(' [-');
            toggle.textContent = isCollapsed ? ` [+ ${g.group.length}] ` : ` [- ${g.group.length}] `;
        });
    });
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

    // تغییر نوع فیلتر و آیکن
    const filterType = li.dataset.type; // eq, neq, gt, lt, ...
    const selectedIcon = li.dataset.icon;
    icon.textContent = `🔍 ${selectedIcon}`;

    const cell = icon.closest('.grid-cell');
    if (!cell) return;

    const input = cell.querySelector('.filter-input');
    if (!input) return;

    input.dataset.filterType = filterType; // ست کردن Type جدید

    // ✅ فوراً fetch اجرا شود
    currentPage = 1; // برگردیم به صفحه اول
    fetchGridData(currentPage, pageSize);

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
            .filter(c => c.style.display !== 'none') // فقط سلول‌های قابل مشاهده
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

// تنظيم ارتفاع گريد
//function adjustGridHeight() {
//    const gridBody = document.querySelector('#gridContainer .grid-body');
//    const rows = gridBody.querySelectorAll('.grid-row');
//    const rowHeight = rows[0] ? rows[0].offsetHeight : 30; // ارتفاع تقریبی هر ردیف
//    const pageSizeSelector = document.getElementById('pageSizeSelector');

//    if (!gridBody) return;

//    // ارتفاع کل مانیتور یا بخشی از صفحه که میخوای استفاده بشه
//    const maxAvailableHeight = window.innerHeight * 0.6; // مثلا 60% صفحه

//    // ارتفاع مورد نیاز برای تمام ردیف‌ها
//    let neededHeight = rows.length * rowHeight;

//    // ارتفاع نهایی = کوچیک‌تر بین نیاز و فضای موجود
//    gridBody.style.height = Math.min(neededHeight, maxAvailableHeight) + 'px';

//    // حذف overflow خود grid
//    gridBody.style.overflowY = 'visible';
//}

//// فراخوانی هنگام بارگذاری و تغییر صفحه
//document.addEventListener('DOMContentLoaded', adjustGridHeight);
//window.addEventListener('resize', adjustGridHeight);

//// وقتی تعداد ردیف‌ها تغییر کرد (مثلا بعد از fetch یا تغییر pageSize)
//function renderRows(items, columns = null) {
//    // ... کد خودت
//    adjustGridHeight(); // اینجا اضافه کن
//}


// پرينت 
function printDynamicGrid() {
    var grid = $('.dynamic-grid-container').first();
    if (!grid.length) return alert('هیچ گریدی پیدا نشد!');

    var rows = grid.find('.grid-row');
    if (rows.length > 300) return alert('تعداد رکوردها بیش از 300 است و امکان پرینت وجود ندارد.');

    // گرفتن ستون‌های هدر و حذف ستون عملیات
    var columns = grid.find('.grid-header .grid-cell').filter(function () {
        var text = $(this).clone().children().remove().end().text().trim().toLowerCase();
        return $(this).css('display') !== 'none' && text !== 'عملیات';
    });

    // ایجاد پاپ‌آپ انتخاب ستون‌ها
    var selectorHtml = '<div style="padding: 20px;">';
    selectorHtml += '<h3 style="margin-bottom: 15px;">ستون‌هایی که می‌خواهید چاپ شوند را انتخاب کنید:</h3>';
    columns.each(function (i) {
        var prop = $(this).data('column') || $(this).attr('data-cell') || i;
        var text = $(this).clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
        selectorHtml += `<label style="display:block; margin-bottom: 8px; font-size: 14px;">
            <input type="checkbox" data-prop="${prop}" checked> ${i + 1}. ${text}
        </label>`;
    });
    selectorHtml += `
        <div style="margin-top:15px;">
            <button id="confirmPrint" style="padding:8px 15px; background-color:#007bff; color:white; border:none; border-radius:4px; cursor:pointer;">پرینت</button>
            <button id="cancelPrint" style="padding:8px 15px; margin-left:10px; background-color:#ccc; border:none; border-radius:4px; cursor:pointer;">لغو</button>
        </div></div>`;

    var $popup = $('<div id="columnSelectorPopup"></div>').html(selectorHtml).css({
        'border-radius': '8px',
    });
    $('body').append($popup);

    $('#cancelPrint').on('click', function () {
        $popup.remove();
    });

    $('#confirmPrint').on('click', function () {
        var selectedProps = [];
        $popup.find('input[type=checkbox]').each(function () {
            if ($(this).is(':checked')) selectedProps.push($(this).data('prop'));
        });
        $popup.remove();

        if (!selectedProps.length) return alert('هیچ ستونی برای چاپ انتخاب نشده است.');

        // ساخت HTML چاپ
        var html = '<html><head><title>پرینت گرید</title>';
        html += `<style>
            body {direction: rtl; font-family: 'vazir-light', Tahoma, sans-serif; margin: 20px;}
            table {border-collapse: collapse; width: 100%; font-size: 12px; border:1px solid #ccc;}
            th, td {border:1px solid #ccc; padding: 8px; text-align: right;}
            th {background-color: #f0f0f0;}
            tfoot td {border: none; padding-top: 20px; text-align: center; font-style: italic; font-size: 10px;}
            .header-print {display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;
                border: 1px solid #ccc; padding: 10px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);}
            .header-print img {height: 50px;}
            .header-print .title {font-weight: bold; font-size: 16px; text-align: center; flex:1;}
            .header-print .datetime {font-size: 12px; color: #555; text-align: left; flex:1;}
        </style>`;
        html += '</head><body>';

        // هدر چاپ با لوگو، عنوان و تاریخ
        var now = new Date();
        var date = now.toLocaleDateString('fa-IR');
        var time = now.toLocaleTimeString('fa-IR');
        html += `<div class="header-print">
            <div style="flex:1; text-align: right;"><img src="/logo/Logo43.png" alt=" شركت پالايش نفت اصفهان"></div>
            <div class="title">چاپ گزارش</div>
            <div class="datetime">${date} ${time}</div>
        </div>`;

        // جدول
        html += '<table><thead><tr>';
        columns.each(function () {
            var prop = $(this).data('column') || $(this).attr('data-cell');
            var text = $(this).clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
            if (selectedProps.includes(prop)) html += '<th>' + text + '</th>';
        });
        html += '</tr></thead><tbody>';

        rows.each(function () {
            html += '<tr>';
            selectedProps.forEach(function (prop) {
                var cell = $(this).find(`.grid-cell[data-cell="${prop}"]`);
                html += '<td>' + (cell.length ? cell.clone().children().remove().end().text().trim() : '') + '</td>';
            }, this);
            html += '</tr>';
        });

        html += '</tbody><tfoot><tr><td colspan="' + selectedProps.length + '">نرم افزار سام</td></tr></tfoot>';
        html += '</table></body></html>';

        var printWindow = window.open('', '', 'width=900,height=600');
        printWindow.document.write(html);
        printWindow.document.close();
        printWindow.focus();
        printWindow.print();
    });

}


//  خروجي اكسل
function exportGridToExcelXlsx() {
    var grid = $('.dynamic-grid-container').first();
    if (!grid.length) return alert('هیچ گریدی پیدا نشد!');

    var rows = grid.find('.grid-row');
    if (rows.length > 1000) return alert('تعداد رکوردها بیش از حد مجاز است.');

    var columns = grid.find('.grid-header .grid-cell').filter(function () {
        var text = $(this).clone().children().remove().end().text().trim().toLowerCase();
        return $(this).css('display') !== 'none' && text !== 'عملیات';
    });

    var selectedProps = [];
    var headers = [];
    columns.each(function () {
        var prop = $(this).data('column') || $(this).attr('data-cell');
        selectedProps.push(prop);
        headers.push($(this).clone().children().remove().end().text().replace(/[▲▼]/g, '').trim());
    });

    var data = [];

    // بررسی اینکه آیا گروه‌بندی فعال است؟
    var groupBy = $('#groupBySelector').val();
    if (groupBy && $('.grid-group-header').length) {
        // استخراج گروه‌ها از DOM
        $('.grid-group-header').each(function () {
            var groupTitle = $(this).text().trim();
            data.push([groupTitle]); // ردیف عنوان گروه

            // ردیف‌های گروه
            var groupRows = [];
            var next = $(this).next();
            while (next.length && !next.hasClass('grid-group-header')) {
                var rowData = [];
                selectedProps.forEach(function (prop) {
                    var cell = next.find(`.grid-cell[data-cell="${prop}"]`);
                    rowData.push(cell.length ? cell.text().trim() : '');
                });
                groupRows.push(rowData);
                next = next.next();
            }

            // اضافه کردن هدر فقط برای اولین گروه
            if (data.length === 1) data.push(headers);

            // اضافه کردن ردیف‌های گروه به داده‌ها
            groupRows.forEach(r => data.push(r));

            // ردیف خالی برای فاصله بین گروه‌ها
            data.push([]);
        });
    } else {
        // بدون گروه‌بندی
        data.push(headers);
        rows.each(function () {
            var row = [];
            selectedProps.forEach(function (prop) {
                var cell = $(this).find(`.grid-cell[data-cell="${prop}"]`);
                row.push(cell.length ? cell.text().trim() : '');
            }, this);
            data.push(row);
        });
    }

    // --- ساخت اکسل ---
    var wb = XLSX.utils.book_new();
    var ws = XLSX.utils.aoa_to_sheet(data);

    // راست‌به‌چپ و استایل‌دهی
    var range = XLSX.utils.decode_range(ws['!ref']);
    for (var R = range.s.r; R <= range.e.r; ++R) {
        for (var C = range.s.c; C <= range.e.c; ++C) {
            var cellRef = XLSX.utils.encode_cell({ r: R, c: C });
            var cell = ws[cellRef];
            if (!cell) continue;
            if (!cell.s) cell.s = {};

            cell.s.alignment = { horizontal: "right", vertical: "center" };
            cell.s.font = { name: "B Nazanin", sz: 12 };

            // اگر این ردیف گروه است، رنگ پس‌زمینه متفاوت
            if (data[R].length === 1) {
                cell.s.fill = { patternType: "solid", fgColor: { rgb: "FFF9C4" } }; // زرد روشن
                cell.s.font.bold = true;
            } else if (R === 1 && !groupBy) {
                cell.s.fill = { patternType: "solid", fgColor: { rgb: "E0E0E0" } }; // هدر
                cell.s.font.bold = true;
            } else {
                cell.s.border = {
                    top: { style: "thin", color: { rgb: "000000" } },
                    bottom: { style: "thin", color: { rgb: "000000" } },
                    left: { style: "thin", color: { rgb: "000000" } },
                    right: { style: "thin", color: { rgb: "000000" } }
                };
            }
        }
    }

    ws['!cols'] = headers.map(() => ({ wch: 20 }));
    XLSX.utils.book_append_sheet(wb, ws, "گزارش");

    // فایل خروجی
    XLSX.writeFile(wb, "گزارش_گرید.xlsx");
}


//
function enableRowDetailsPopup() {
    $(document).on('dblclick', '.grid-row', function () {
        $('#rowDetailPopup').remove();

        var headers = [];
        $('.grid-header .grid-cell').each(function () {
            var text = $(this).clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
            var prop = $(this).data('column') || $(this).attr('data-cell');
            if (text && text !== 'عملیات') {
                headers.push({ text: text, prop: prop });
            }
        });

        var values = {};
        $(this).find('.grid-cell').each(function () {
            var prop = $(this).data('column') || $(this).attr('data-cell');
            values[prop] = $(this).clone().children().remove().end().text().trim();
        });

        var popupHtml = `<div id="rowDetailPopup">
            <h3>جزئیات ردیف انتخاب‌شده</h3>
            <div class="details-container">`;

        headers.forEach(function (h) {
            if (values[h.prop]) {
                popupHtml += `
                <div class="detail-row">
                    <span class="label">${h.text}:</span>
                    <span class="value">${values[h.prop]}</span>
                </div>`;
            }
        });

        popupHtml += `
            </div>
            <button class="close-btn">بستن</button>
        </div>`;

        var $popup = $(popupHtml);
        $('body').append($popup);
        setTimeout(() => $popup.addClass('show'), 10);

        $popup.find('.close-btn').on('click', function () {
            $popup.remove();
        });
    });
}



/// نمايش يا عدم نمايش ستون هاي گريد 
function displayGridColumns() {
    $('#columnSelectorPopup').remove();

    // گرفتن همه ستون‌ها (چه مخفی چه نمایش داده شده)
    const columns = $('.grid-header .grid-cell').filter(function () {
        const text = $(this).clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
        return text !== 'عملیات';
    });

    // ساخت HTML Popup
    let selectorHtml = `<div style="padding: 20px;">
        <h3 style="margin-bottom: 15px;">ستون‌هایی که می‌خواهید نمایش داده شوند را انتخاب کنید:</h3>`;

    columns.each(function (i) {
        const prop = $(this).data('column') || $(this).attr('data-cell') || i;
        const text = $(this).clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
        const visible = $(this).css('display') !== 'none';
        selectorHtml += `<label style="display:block; margin-bottom: 8px; font-size: 14px;">
            <input type="checkbox" data-prop="${prop}" ${visible ? 'checked' : ''}> ${i + 1}. ${text}
        </label>`;
    });

    selectorHtml += `
        <div style="margin-top:15px;">
            <button id="applyColumnsBtn" style="padding:8px 15px; background-color:#007bff; color:white; border:none; border-radius:4px; cursor:pointer;">اعمال</button>
            <button id="cancelColumnsBtn" style="padding:8px 15px; margin-left:10px; background-color:#ccc; border:none; border-radius:4px; cursor:pointer;">لغو</button>
        </div></div>`;

    const $popup = $('<div id="columnSelectorPopup"></div>').html(selectorHtml).css({
        'border-radius': '8px',
        'position': 'fixed',
        'top': '50%',
        'left': '50%',
        'transform': 'translate(-50%, -50%)',
        'background': '#fff',
        'z-index': 99999,
        'box-shadow': '0 4px 16px rgba(0,0,0,0.25)'
    });

    $('body').append($popup);

    // بستن Popup
    $('#cancelColumnsBtn').on('click', () => $popup.remove());

    // اعمال تغییرات ستون‌ها
    $('#applyColumnsBtn').on('click', () => {
        $popup.find('input[type=checkbox]').each(function () {
            const prop = $(this).data('prop');
            const show = $(this).is(':checked');

            // هدر
            $(`.grid-header .grid-cell[data-column="${prop}"]`).css('display', show ? 'flex' : 'none');
            // ردیف‌ها
            $(`.grid-body .grid-cell[data-cell="${prop}"]`).css('display', show ? 'flex' : 'none');
            // فوترها
            $(`[data-footer="${prop}"]`).css('display', show ? 'flex' : 'none');
            // فیلترها
            $(`.grid-filters .grid-cell[data-column="${prop}"]`).css('display', show ? 'flex' : 'none');
        });
    });
}
///////////////////////////////////////////

document.addEventListener('DOMContentLoaded', initGrid);

