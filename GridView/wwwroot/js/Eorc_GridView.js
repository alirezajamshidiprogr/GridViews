/// Grid Utility --

// خروجي PDF

function exportGridToPdf(gridName) {
    const grid = document.getElementById(gridName);
    if (!grid) {
        alert("هیچ گریدی پیدا نشد!");
        return;
    }

    const gridBody = grid.querySelector(`#${gridName}_grid-body`);
    if (!gridBody) {
        alert("بدنه گرید پیدا نشد!");
        return;
    }

    const rows = gridBody.querySelectorAll(".grid-row");
    if (rows.length > 1000) {
        alert("تعداد رکوردها بیش از حد مجاز است.");
        return;
    }

    const columns = Array.from(
        grid.querySelectorAll('.grid-header .grid-cell')
    ).filter(cell => {
        const text = cell.textContent.replace(/[▲▼]/g, '').trim().toLowerCase();
        return cell.style.display !== "none" && text !== "عملیات";
    });

    const popupId = `${gridName}_pdfSelectorPopup`;
    const confirmId = `${gridName}_confirmPdf`;
    const cancelId = `${gridName}_cancelPdf`;

    let selectorHtml = `
        <div style="padding:20px;">
            <h3 style="margin-bottom:15px;">ستون‌های قابل خروجی PDF:</h3>
            <div style="margin-bottom:15px;">
    `;

    columns.forEach((cell, i) => {
        const prop = cell.dataset.column || cell.getAttribute('data-cell') || i;
        const text = cell.textContent.replace(/[▲▼]/g, '').trim();
        selectorHtml += `
            <label style="display: block;
                  margin-bottom: 8px;
                  font-size: 14px;">
                <input type="checkbox" data-prop="${prop}" checked>
                ${text}
            </label>
        `;
    });

    selectorHtml += `
            </div>
            <div style="margin-top:10px;">
         <button id="${confirmId}"
                class="gridPopupBtnApplyGrid">
            <i class="fa-solid fa-file-pdf" style="margin-left:6px;"></i>
            تولید PDF
        </button>
                <button id="${cancelId}" class="gridPopupBtnCancelGrid">
                    <i class="fa fa-times"></i>
                    لغو
                </button>
            </div>
        </div>
    `;

    const popup = document.createElement("div");
    popup.id = popupId;
    popup.className = 'gridPopup';
    popup.setAttribute('data-eorc-grid-popup', '');
    popup.innerHTML = selectorHtml;

    document.body.appendChild(popup);

    document.getElementById(cancelId).addEventListener("click", () => popup.remove());

    document.getElementById(confirmId).addEventListener("click", () => {
        const selectedProps = [];
        popup.querySelectorAll("input[type=checkbox]").forEach(chk => {
            if (chk.checked) selectedProps.push(chk.dataset.prop);
        });
        if (selectedProps.length === 0) {
            alert("هیچ ستونی انتخاب نشده!");
            return;
        }
        popup.remove();

        const headers = columns
            .filter(c => selectedProps.includes(c.dataset.column || c.getAttribute('data-cell')))
            .map(c => c.textContent.replace(/[▲▼]/g, '').trim());

        const data = Array.from(rows).map(row => {
            return selectedProps.map(prop => {
                const cell = row.querySelector(`.grid-cell[data-cell="${prop}"]`);
                return cell ? cell.textContent.trim() : "";
            });
        });

        // ====== فونت فارسی Vazir ======
        if (!pdfMake.vfs || !pdfMake.vfs["Vazir.ttf"]) {
            pdfMake.vfs = { "Vazir.ttf": window.vazirBase64 };
            pdfMake.fonts = {
                Vazir: { normal: "Vazir.ttf", bold: "Vazir.ttf", italics: "Vazir.ttf", bolditalics: "Vazir.ttf" }
            };
        }

        const docDefinition = {
            defaultStyle: { font: "Vazir", alignment: "right" },
            background: (_, pageSize) => ({
                text: 'sam.eorc.ir',
                color: 'gray',
                opacity: 0.2,
                fontSize: 40,
                bold: true,
                alignment: 'center',
                rotation: -45,
                margin: [0, pageSize.height / 2 - 20]
            }),
            content: [
                { text: "گزارش خروجی سام", style: "header", margin: [0, 0, 0, 10] },
                {
                    table: {
                        headerRows: 1,
                        widths: Array(headers.length).fill("*"),
                        body: [headers, ...data]
                    },
                    layout: {
                        fillColor: rowIndex => rowIndex === 0 ? '#1976D2' : (rowIndex % 2 === 0 ? '#E3F2FD' : null),
                        hLineWidth: () => 0.5,
                        vLineWidth: () => 0.5,
                        hLineColor: () => '#B0B0B0',
                        vLineColor: () => '#B0B0B0'
                    }
                }
            ],
            styles: { header: { fontSize: 16, bold: true, color: "#000000" } },
            pageOrientation: "landscape",
            pageSize: "A4",
            pageMargins: [20, 20, 20, 20]
        };

        pdfMake.createPdf(docDefinition).download(`Grid-${gridName}.pdf`);
    });
}

// کمکی برای دسترسی امن به مقدار سلول
function getItemValue(item, prop) {
    if (!item)
        return '';
    if (prop in item)
        return item[prop]; // PascalCase
    const camel = prop.charAt(0).toLowerCase() + prop.slice(1);
    if (camel in item)
        return item[camel]; // camelCase

    for (const k of Object.keys(item)) {
        if (k.toLowerCase() === prop.toLowerCase()) return item[k]; // case-insensitive
    }
    return '';
}

// نرمال سازي متن
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

// افكت سطر انتخاب شده 
function enableGridRowSelection() {
    Object.keys(window.Grids).forEach(gridName => {
        const gridContainer = document.getElementById(gridName);
        if (!gridContainer) return;

        const bodyContainer = gridContainer.querySelector('.grid-body');
        if (!bodyContainer) return;

        // جلوگیری از دوباره بایند شدن
        $(bodyContainer).off('click', '.grid-row');

        $(bodyContainer).on('click', '.grid-row', function () {

            // ردیف‌های انتخاب شده قبلی را غیر فعال می‌کنیم
            $(bodyContainer).find('.grid-row.selected').removeClass('selected');

            // ردیف فعلی را انتخاب می‌کنیم
            $(this).addClass('selected');
        });
    });
}

// جزيئات رديف 
function enableRowDetailsPopup() {
    Object.keys(window.Grids).forEach(gridName => {

        const gridContainer = document.getElementById(gridName);
        if (!gridContainer) return;

        const bodyContainer = gridContainer.querySelector('.grid-body');
        if (!bodyContainer) return;

        $(bodyContainer).off('dblclick.rowdetails');

        $(bodyContainer).on('dblclick.rowdetails', '.grid-row', function (e) {

            if ($(this).closest('.grid-footer').length) return;
            if ($(e.target).closest('.grid-cell.disabled').length) return;
            if ($(e.target).is('input,button,select,textarea') ||
                $(e.target).closest('input,button,select,textarea').length) {
                return;
            }

            $('#' + gridName + 'rowDetailPopup').remove();
            const rowIndex = $(this).index();
            const popupId = `${gridName}_rowDetailPopup_${rowIndex}`;

            const headers = [];
            $(gridContainer).find('.grid-header .grid-cell').each(function () {
                const text = $(this).clone().children().remove().end()
                    .text().replace(/[▲▼]/g, '').trim();
                const prop = $(this).data('column') || $(this).attr('data-cell');
                if (text && text !== 'عملیات') headers.push({ text, prop });
            });

            const values = {};
            $(this).find('.grid-cell').each(function () {
                const prop = $(this).data('column') || $(this).attr('data-cell');
                values[prop] = $(this).attr('original-content') || '';
            });

            let popupHtml = `
                <div id="${popupId}" class="gridPopup popup-overlay show" data-eorc-grid-popup>
                <div class="popup-box">
                    <h3>اطلاعات بیشتر ردیف</h3>
                    <div class="details-container">`;

            headers.forEach(h => {
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
                    <div class="popup-buttons" style="margin-top:10px; display:flex; gap:8px; justify-content:flex-end;">
                        <button class="gridPopupBtnCancelGrid" title="بستن">
                             <i class="fa fa-times-circle"></i> بستن
                        </button>
                        <button class="export-btn btn secondary" title="خروجی PNG" 
                            style="border: 1px solid #b2d5fb;">
                            🖼️خروجي به عكس
                        </button>
                    </div>
                </div>
            </div>`;

            const $popup = $(popupHtml);
            $('body').append($popup);

            $popup.find('.gridPopupBtnCancelGrid').on('click', function () {
                $popup.remove();
            });

            $popup.find('.export-btn').on('click', function () {
                const container = $popup.find('.details-container')[0];
                if (container && window.html2canvas) {
                    html2canvas(container).then(canvas => {
                        const link = document.createElement('a');
                        link.download = `row-details.png`;
                        link.href = canvas.toDataURL('image/png');
                        link.click();
                    });
                } else {
                    alert('برای خروجی گرفتن، لطفاً کتابخانه html2canvas را اضافه کنید.');
                }
            });

        });
    });
}

/// نمايش يا عدم نمايش ستون هاي گريد 
function displayGridColumns(gridId) {
    const popupId = `${gridId}_columnSelectorPopup`;
    $('#' + popupId).remove();

    const grid = $('#' + gridId);
    if (!grid.length) return;

    // گرفتن همه ستون‌ها (چه مخفی چه نمایش داده شده) ولی ستون‌های کاربر مخفی شده را نادیده بگیر
    const columns = grid.find('.grid-header .grid-cell').filter(function () {
        const $this = $(this);
        const text = $this.clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
        const isHiddenPermanently = $this.hasClass('grid-cell-hidden');
        return text !== 'عملیات' && !isHiddenPermanently && text.length > 0;
    });

    // ساخت HTML Popup
    let selectorHtml = `<div style="padding: 20px;">
        <h3 style="margin-bottom: 15px;">ستون‌هایی که می‌خواهید نمایش داده شوند را انتخاب کنید:</h3>
        <div class="checkbox-grid">`;

    columns.each(function (i) {
        const $this = $(this);
        const prop = $this.data('column') || $this.attr('data-cell') || i;
        const text = $this.clone().children().remove().end().text().replace(/[▲▼]/g, '').trim();
        const visible = $this.css('display') !== 'none';
        selectorHtml += `<label style="display:flex; align-items:center; font-size:14px; padding:4px;">
            <input type="checkbox" data-prop="${prop}" ${visible ? 'checked' : ''}> ${text}
        </label>`;
    });

    const applyBtnId = `${gridId}_applyColumnsBtn`;
    const cancelBtnId = `${gridId}_cancelColumnsBtn`;

    selectorHtml += `</div>
        <div style="margin-top:15px;">
            <button id="${applyBtnId}"
                    class="gridPopupBtnApplyGrid">
                <i class="fa fa-check" style="margin-left:6px;"></i>
                اعمال
            </button>
<button id="${cancelBtnId}"
        class="gridPopupBtnCancelGrid">
    <i class="fa fa-times" style="margin-left:6px;"></i>
    لغو
</button>
        </div></div>`;

    const $popup = $('<div class="gridPopup" data-eorc-grid-popup></div>').attr('id', popupId).html(selectorHtml).css({ 'z-index': 99999 });

    $('body').append($popup);

    // بستن Popup
    $('#' + cancelBtnId).on('click', () => $popup.remove());

    // اعمال تغییرات ستون‌ها فقط روی همین گرید
    $('#' + applyBtnId).on('click', () => {
        $popup.find('input[type=checkbox]').each(function () {
            const prop = $(this).data('prop');
            const show = $(this).is(':checked');

            // هدر
            grid.find(`.grid-header .grid-cell[data-column="${prop}"]`).css('display', show ? 'flex' : 'none');
            // ردیف‌ها
            grid.find(`.grid-body .grid-cell[data-cell="${prop}"]`).css('display', show ? 'flex' : 'none');
            // فوترها
            grid.find(`[data-footer="${prop}"]`).css('display', show ? 'flex' : 'none');
            // فیلترها
            grid.find(`.grid-filters .grid-cell[data-column="${prop}"]`).css('display', show ? 'flex' : 'none');
        });

        $popup.remove();
    });
}

//تنظيم عرض ستون هاي گريد 
//function adjustGridColumnWidths(gridId) {
//    const grid = document.getElementById(gridId);
//    if (!grid) return;

//    // همه ردیف‌های هدر و بدنه
//    const headerCells = grid.querySelectorAll('.grid-header .grid-cell');
//    const bodyRows = grid.querySelectorAll('.grid-body .grid-row');

//    headerCells.forEach((cell, index) => {
//        // ایجاد span موقت برای اندازه‌گیری متن
//        const tempSpan = document.createElement('span');
//        tempSpan.style.visibility = 'hidden';
//        tempSpan.style.whiteSpace = 'nowrap';
//        tempSpan.style.font = window.getComputedStyle(cell).font;
//        tempSpan.textContent = cell.textContent.trim();
//        document.body.appendChild(tempSpan);

//        let maxWidth = tempSpan.offsetWidth + 16; // +16 برای padding تقریبی
//        document.body.removeChild(tempSpan);

//        // بررسی تمام سلول‌های بدنه برای این ستون
//        bodyRows.forEach(row => {
//            const bodyCell = row.children[index];
//            if (bodyCell) {
//                const tempSpanBody = document.createElement('span');
//                tempSpanBody.style.visibility = 'hidden';
//                tempSpanBody.style.whiteSpace = 'nowrap';
//                tempSpanBody.style.font = window.getComputedStyle(bodyCell).font;
//                tempSpanBody.textContent = bodyCell.textContent.trim();
//                document.body.appendChild(tempSpanBody);

//                maxWidth = Math.max(maxWidth, tempSpanBody.offsetWidth + 16);
//                document.body.removeChild(tempSpanBody);
//            }
//        });

//        // اعمال عرض محاسبه شده به ستون
//        cell.style.width = maxWidth + 'px';
//        cell.style.flex = '0 0 ' + maxWidth + 'px';

//        // اعمال به تمام سلول‌های بدنه
//        bodyRows.forEach(row => {
//            const bodyCell = row.children[index];
//            if (bodyCell) {
//                bodyCell.style.width = maxWidth + 'px';
//                bodyCell.style.flex = '0 0 ' + maxWidth + 'px';
//            }
//        });
//    });
//}

// تابع کوتاه کردن متن
function truncateText(text, maxLength = 19) {
    if (!text) return '';
    text = text.toString();
    return text.length > maxLength ? text.slice(0, maxLength) + '...' : text;
}

// نمايش مقدار سلولي كه كاربر ميخواد 
function getGridDataCell(btn, findValue) {
    const row = btn.closest('.grid-row');
    if (!row || !findValue) return null;

    findValue = findValue.toLowerCase();

    const cell = Array.from(row.querySelectorAll('.grid-cell'))
        .find(c =>
            !c.classList.contains('grid-cell-Buttons') &&
            c.getAttribute('data-cell')?.toLowerCase() === findValue
        );

    return cell
        ? (cell.getAttribute('original-content') ?? cell.textContent)
        : null;
}

// استايل دهي ردیف‌ها بر اساس نام
function setStyleRows(gridName) {
    const gridContainer = document.getElementById(gridName);
    if (gridContainer) {
        $(gridContainer).find('.grid-body .grid-row').each(function () {
            const $row = $(this);
            const productName = $row.find('.grid-cell[data-cell="ProductName"]').attr('original-content');
            if (productName === 'پرینتر') {

                $row.find('.grid-cell').attr('style', 'background-color: #e6ffcd !important; color: #721c24 !important;');
            }
        });
    }

}

// نمايش فيلتر استايل بندي رديف ها 
function openFilterPopup(gridName) {
    // حذف پاپ‌آپ قبلی اگر موجود بود
    const existing = document.getElementById('filterPopup');
    if (existing) existing.remove();

    // ساختار پاپ‌آپ
    const popup = document.createElement('div');
    popup.id = 'filterPopup';
    popup.style = `
        position:fixed; top:50%; left:50%; transform:translate(-50%, -50%);
        width:500px; max-width:90vw; background:#fff; border:1px solid #ccc;
        border-radius:8px; box-shadow:0 4px 16px rgba(0,0,0,0.25); padding:16px; z-index:10000;
    `;

    popup.innerHTML = `
        <h3>پاپ‌آپ فیلتر</h3>
        <div id="${gridName}_filterRowsContainer" class="gridPopup" data-eorc-grid-popup></div>
        <button id="${gridName}_addFilterBtn" style="margin-top:8px;">اضافه کردن شرط</button>
        <div style="margin-top:16px;">
            <button id="${gridName}_closeFilterBtn">بستن</button>
        </div>
    `;

    document.body.appendChild(popup);

    const filterRowsContainer = popup.querySelector('#' + gridName + '_filterRowsContainer');

    const columns = window.Grids[gridName]?.columns || [];

    function addFilterRow() {
        const rowDiv = document.createElement('div');
        rowDiv.style = 'margin-bottom:8px; display:flex; gap:8px;';

        // select ستون
        const filterColumn = document.createElement('select');
        columns.forEach(col => {
            const option = document.createElement('option');
            option.value = col.prop;
            option.textContent = col.header;
            filterColumn.appendChild(option);
        });

        // input مقدار
        const filterValue = document.createElement('input');
        filterValue.type = 'text';
        filterValue.placeholder = 'مقدار شرط';

        // دکمه حذف
        const removeBtn = document.createElement('button');
        removeBtn.textContent = '×';
        removeBtn.onclick = () => rowDiv.remove();

        rowDiv.appendChild(filterColumn);
        rowDiv.appendChild(filterValue);
        rowDiv.appendChild(removeBtn);

        filterRowsContainer.appendChild(rowDiv);
    }

    popup.querySelector('#' + gridName + '_addFilterBtn').onclick = addFilterRow;
    addFilterRow(); // یک ردیف پیش‌فرض

    popup.querySelector('#' + gridName + '_closeFilterBtn').onclick = () => popup.remove();
}

// پرينت 
function printDynamicGrid(gridName) {
    if (!gridName) return alert('نام گرید مشخص نشده است!');

    var grid = $(`#${gridName}`);
    if (!grid.length) {
        alert('گرید مورد نظر پیدا نشد!');
        return;
    }


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
        <button id="${gridName}_confirmPrint"
                class="gridPopupBtnApplyGrid">
            <i class="fa fa-print" style="margin-left:6px;"></i>
            پرینت
        </button>            <button id="${gridName}_cancelPrint"
                    class="gridPopupBtnCancelGrid">
                <i class="fa fa-times"></i>
                لغو
            </button>        </div></div>`;

    var popupId = `${gridName}_printColumnSelector`;

    var $popup = $(`<div id="${popupId}" class="gridPopup" data-eorc-grid-popup></div>`)
        .html(selectorHtml)
        .css({
            'border-radius': '8px',
        });


    $('body').append($popup);

    $('#' + gridName + '_cancelPrint').on('click', function () {
        $popup.remove();
    });

    $('#' + gridName + '_confirmPrint').on('click', function () {
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
function exportGridToExcelXlsx(gridName) {
    if (!gridName) return alert('نام گرید مشخص نشده است!');

    var grid = $(`#${gridName}`);
    if (!grid.length) return alert('گرید مورد نظر پیدا نشد!');

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

    // بررسی گروه‌بندی
    var groupBy = grid.find('#' + gridName + '_groupBySelector').val();
    if (groupBy && grid.find('.grid-group-header').length) {
        grid.find('.grid-group-header').each(function () {
            var groupTitle = $(this).text().trim();
            data.push([groupTitle]); // عنوان گروه

            var next = $(this).next();
            var firstGroup = data.length === 1;
            if (firstGroup) data.push(headers); // فقط برای اولین گروه

            while (next.length && !next.hasClass('grid-group-header')) {
                var rowData = [];
                selectedProps.forEach(function (prop) {
                    var cell = next.find(`.grid-cell[data-cell="${prop}"]`);
                    rowData.push(cell.length ? cell.text().trim() : '');
                });
                data.push(rowData);
                next = next.next();
            }
            data.push([]); // فاصله بین گروه‌ها
        });
    } else {
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

    ws['!cols'] = headers.map(() => ({ wch: 20 })); // عرض ستون‌ها

    // تنظیم فونت و رنگ‌ها
    var range = XLSX.utils.decode_range(ws['!ref']);
    for (var R = range.s.r; R <= range.e.r; ++R) {
        for (var C = range.s.c; C <= range.e.c; ++C) {
            var cellRef = XLSX.utils.encode_cell({ r: R, c: C });
            var cell = ws[cellRef];
            if (!cell) continue;
            if (!cell.s) cell.s = {};

            // فونت B Nazanin و راست‌چین
            cell.s.font = { name: "B Nazanin", sz: 12, color: { rgb: "000000" } };
            cell.s.alignment = { horizontal: "right", vertical: "center", readingOrder: 2 };

            // رنگ هدر آبی
            if (R === 0) {
                cell.s.fill = { patternType: "solid", fgColor: { rgb: "1976D2" } };
                cell.s.font.color = { rgb: "FFFFFF" };
                cell.s.font.bold = true;
                cell.s.alignment.horizontal = "center";
            }
            // رنگ سلول‌ها ملایم
            else {
                cell.s.fill = { patternType: "solid", fgColor: { rgb: "E3F2FD" } };
                cell.s.border = {
                    top: { style: "thin", color: { rgb: "B0B0B0" } },
                    bottom: { style: "thin", color: { rgb: "B0B0B0" } },
                    left: { style: "thin", color: { rgb: "B0B0B0" } },
                    right: { style: "thin", color: { rgb: "B0B0B0" } }
                };
            }
        }
    }

    XLSX.utils.book_append_sheet(wb, ws, "گزارش");
    XLSX.writeFile(wb, "گزارش_گرید.xlsx", { bookSST: true, cellStyles: true });
}

// advanced Filters 
function displayAdvancedFilter(gridName) {
    const grid = document.getElementById(gridName);
    if (!grid) return;

    const popup = grid.querySelector('.advanced-filter-popup');
    if (popup) popup.style.display = "block";
}

/// بستن و باز كردم فيلتر هر ستون با مساوي نامساوي و ...
// باز و بسته کردن منوی فیلتر
function initFilterIcons(gridName) {
    const grid = document.getElementById(gridName);
    if (!grid) return;

    const icons = grid.querySelectorAll('.filter-icon');
    icons.forEach((icon, index) => {
        icon.dataset.iconId = index;

        icon.addEventListener('click', e => {
            e.stopPropagation();
            const cell = e.target.closest('.grid-cell');
            const originalMenu = cell.querySelector('.eorc-grid-filter-menu');
            if (!originalMenu) return;

            // حذف منوهای باز قبلی فقط در این گرید
            grid.querySelectorAll('.eorc-grid-filter-menu.clone').forEach(m => m.remove());

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
}

// بروز رساني فوتر گريد 
function updateGridFooters(calcType = null, targetFooter = null) {
    // اگر فوتر خاصی ارسال شده باشد، فقط آن را محاسبه می‌کنیم
    const footers = targetFooter ? [targetFooter] : document.querySelectorAll('[data-footer]');

    footers.forEach(footer => {
        const field = footer.dataset.footer; // فیلد مربوط به فوتر
        const calc = calcType || footer.dataset.calcState || 'sum'; // نوع محاسبه

        // برای هر گرید، سلول‌های مرتبط با این فیلد را پیدا می‌کنیم
        const gridContainer = footer.closest('.eorc-dynamic-grid-container');
        const cells = Array.from(gridContainer.querySelectorAll(`[data-cell="${field}"]`))
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
            input.value = ''; // اگر هیچ مقداری وجود نداشت، ورودی خالی باشد
            return;
        }

        // محاسبه نتیجه با توجه به نوع calc
        let result = 0;
        switch (calc) {
            case 'avg': result = cells.reduce((a, b) => a + b, 0) / cells.length; break;
            case 'count': result = cells.length; break;
            case 'max': result = Math.max(...cells); break;
            case 'min': result = Math.min(...cells); break;
            default: result = cells.reduce((a, b) => a + b, 0); break; // پیش‌فرض جمع
        }

        // فرمت کردن نتیجه به فرمت عددی فارسی
        const formatted = result.toLocaleString('fa-IR', { maximumFractionDigits: 2 });

        // قرار دادن نتیجه در ورودی فوتر
        if (input) {
            let label = { sum: 'جمع', avg: 'میانگین', count: 'تعداد', max: 'بیشترین', min: 'کمترین' }[calc] || 'جمع';
            input.value = `${label} = ${formatted}`;
        }

        // ذخیره نوع محاسبه برای استفاده‌های بعدی
        footer.dataset.calcState = calc;
    });
}

// تغییر آیکن هنگام انتخاب گزینه از منو
document.addEventListener('click', function (e) {
    const li = e.target.closest('.eorc-grid-filter-menu li');
    if (!li) return;

    e.stopPropagation();

    const menu = li.closest('.eorc-grid-filter-menu');
    if (!menu) return;

    const iconId = menu.dataset.originalIconId;

    // پیدا کردن آیکن که منو بهش تعلق داره
    const icon = document.querySelector(`.filter-icon[data-icon-id="${iconId}"]`);
    if (!icon) return;

    // پیدا کردن گرید والد آیکن
    const gridContainer = icon.closest('.eorc-dynamic-grid-container');
    if (!gridContainer) return;

    const gridName = gridContainer.id; // id همان gridName است
    const gridState = window.Grids[gridName];
    if (!gridState) return;

    // تغییر نوع فیلتر و آیکن
    const filterType = li.dataset.type || 'contains';
    const selectedIcon = li.dataset.icon || '';
    icon.textContent = `🔍 ${selectedIcon}`;

    const cell = icon.closest('.grid-cell');
    if (!cell) return;

    const input = cell.querySelector('.filter-input');
    if (!input) return;

    input.dataset.filterType = filterType;

    // اجرا fetch فقط برای گرید مربوطه
    gridState.currentPage = 1;
    fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
});

//بستن فرم فيلتر پيشرفته 
function closeAdvancedFilter(gridName) {

    const popup = document.getElementById(`advancedFilterPopup_${gridName}`);
    if (popup) {
        popup.style.display = "none";
    }
}

//waiter for grid view 
function showGridWaiter(gridName, text = "در حال بارگذاری...") {
    // اگر قبلاً waiter ساخته شده بود کاری نکن
    let waiter = document.querySelector('[data-eorc-grid-waiter]');
    if (waiter) return;

    waiter = document.createElement('div');
    waiter.setAttribute('data-eorc-grid-waiter', '');
    waiter.innerHTML = `
        <div class="eorc-spinner"></div>
        <div class="waiter-text">${text}</div>
    `;

    // اضافه کردن مستقیم به body برای اینکه وسط صفحه باشه
    document.body.appendChild(waiter);
}


// حذف waiter
function hideGridWaiter() {
    const waiter = document.querySelector('[data-eorc-grid-waiter]');
    if (waiter) waiter.remove();
}

// يوزر استايل row
//function applyStylesToGridRows(styles, condition) {
//    const rows = document.querySelectorAll('.grid-row');

//    rows.forEach(row => {
//        let match = true;

//        if (condition) {
//            for (const key in condition) {
//                const keyLower = key.toLowerCase(); // ✅ همیشه lowercase
//                // جستجوی سلولی که data-cell برابر با key (بدون حساسیت به حروف) باشد
//                const cell = Array.from(row.querySelectorAll('[data-cell]'))
//                    .find(c => c.dataset.cell.toLowerCase() === keyLower);

//                const cellValue = cell ? cell.textContent.trim() : "";
//                const condValue = condition[key];

//                if (Array.isArray(condValue)) {
//                    if (!condValue.map(v => v.toString()).includes(cellValue)) {
//                        match = false;
//                        break;
//                    }
//                } else {
//                    if (cellValue !== condValue.toString()) {
//                        match = false;
//                        break;
//                    }
//                }
//            }
//        }

//        if (!match) return;

//        for (const property in styles) {
//            row.style.setProperty(property, styles[property], 'important');
//        }
//    });
//}

// _________________ End Utility _______________

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
function renderRows(items, columns = null, gridName = null, filters, sortColumn, sortAsc) {

    let filteredItems;

    if (!Array.isArray(items)) return;

    const container = gridName
        ? document.getElementById(gridName)
        : document.querySelector('.eorc-dynamic-grid-container');
    if (!container) return;

    const gridState = window.Grids[gridName] || { currentPage: 1, pageSize: 10, customRequestBody: {} };
    const bodyContainer = container.querySelector('.grid-body');
    if (!bodyContainer) return;

    var isLazyLoading = document.querySelector('#' + gridName + '_gridData')?.dataset.lazyLoading === 'true';
    // در حالت lazy loading باید همیشه body پاک شود
    if (!isLazyLoading) {
        bodyContainer.innerHTML = '';
    }

    if ((isLazyLoading) && filters && Object.keys(filters).length > 0) {
        const cacheObj = window.allItemsCache || {};

        // --- 1) Filtering ---
        if (filters && Object.keys(filters).length > 0) {
            filteredItems = Object.values(cacheObj)
                .flatMap(itemArray => Array.isArray(itemArray) ? itemArray : [itemArray])
                .filter(item => {
                    return Object.keys(filters).every(key => {
                        const filterObj = filters[key];
                        if (!filterObj || !filterObj.Value) return true;

                        let value = getItemValue(item, key);
                        value = value == null ? "" : value.toString().trim();

                        const filterValue = (filterObj.Value ?? "").toString().trim();

                        switch (filterObj.Type) {
                            case "contains": return value.includes(filterValue);
                            case "equals": return value === filterValue;
                            case "starts": return value.startsWith(filterValue);
                            default: return true;
                        }
                    });
                });
        }

        // --- 2) Sorting ---
        //if (sortColumn) {
        //    filteredItems.sort((a, b) => {
        //        let va = getItemValue(a, sortColumn);
        //        let vb = getItemValue(b, sortColumn);

        //        va = va == null ? "" : va;
        //        vb = vb == null ? "" : vb;

        //        const na = parseFloat(va);
        //        const nb = parseFloat(vb);

        //        const aIsNum = !isNaN(na);
        //        const bIsNum = !isNaN(nb);

        //        if (aIsNum && bIsNum) return sortAsc ? na - nb : nb - na;

        //        const sa = va.toString().toLowerCase();
        //        const sb = vb.toString().toLowerCase();

        //        if (sa < sb) return sortAsc ? -1 : 1;
        //        if (sa > sb) return sortAsc ? 1 : -1;
        //        return 0;
        //    });
        //}

        bodyContainer.innerHTML = '';
    }

    if (!columns) {
        const localDataElement = document.querySelector('#' + gridName + '_gridDataLocal');
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

    const gridData = document.querySelector('#' + gridName + '_gridData');
    const enableEditButton = gridData?.dataset.editButton === 'true';
    const enableDeleteButton = gridData?.dataset.deleteButton === 'true';

    // محاسبه تعداد کل صفحات
    const totalPages = gridState.totalPage || 1;

    const currentPage = gridState.currentPage || 1;

    if (!items || items.length === 0) {
        const emptyRow = document.createElement('div');
        emptyRow.className = 'grid-row no-records-row';

        emptyRow.innerHTML = `
        <div class="grid-cell" style="
            padding:26px;
            text-align:center;
            width:100%;
            font-size:18px;
            font-weight:600;
            color:#666;
            border:1px dashed #bbb;
            border-radius:10px;
        ">
            هیچ رکوردی یافت نشد
        </div>
        `;

        bodyContainer.appendChild(emptyRow);
        updateGridFooters();

        const pageInfo = container.querySelector('#pageInfo');
        if (pageInfo) pageInfo.textContent = `صفحه ${currentPage} از ${totalPages}`;

        const summary = container.querySelector('#gridSummary');
        if (summary) summary.textContent = `نمایش داده شده: 0`;

        return;
    }

    const rowsToRender = ((isLazyLoading) && filters && Object.keys(filters).length > 0) ? filteredItems : items;

    rowsToRender.forEach(item => {
        const row = document.createElement('div');
        row.className = 'grid-row';

        if (enableEditButton || enableDeleteButton) {
            const actions = document.createElement('div');
            actions.className = 'grid-cell grid-cell-Buttons';

            if (enableEditButton) {
                let insEditFunction = gridData?.dataset.editFunctionName || `InsUpd_${gridName}_Item(this)`;
                actions.innerHTML = `<button class="btn primary edit-btn" onclick='${insEditFunction}'> <i class="fa fa-edit"></i> ویرایش</button>`;
            }

            if (enableDeleteButton) {
                let deleteFunction = gridData?.dataset.deleteFunction || `Dlt_${gridName}_Item(this)`;
                actions.innerHTML += `<button class="btn danger delete-btn" onclick='${deleteFunction}'><i class="fa fa-trash"></i> حذف</button>`;
            }

            row.appendChild(actions);
        }

        columns.forEach(col => {
            const div = document.createElement('div');
            div.className = 'grid-cell';
            div.setAttribute('data-cell', col.prop);

            const originalValue = getItemValue(item, col.prop);
            div.setAttribute('original-content', originalValue);

            if (!isNaN(originalValue) && originalValue !== null && originalValue !== '') {
                div.textContent = Number(originalValue).toLocaleString();
            } else {
                div.textContent = (originalValue);
                //div.textContent = truncateText(originalValue);
            }

            if (!col.visible) {
                div.style.display = 'none';
                div.classList.add('grid-cell-hidden');
            }

            row.appendChild(div);
        });

        bodyContainer.appendChild(row);
    });

    updateGridFooters();

    const pageInfo = container.querySelector('#pageInfo');
    if (pageInfo) pageInfo.textContent = `صفحه ${currentPage} از ${totalPages}`;

    const summary = container.querySelector('#gridSummary');
    if (summary) summary.textContent = `نمایش داده شده: ${items.length}`;
}

// نمايش رديف ها با گروه بندي 
function renderGroupedRows(groups, columns, gridName = null) {
    const container = gridName
        ? document.getElementById(gridName)
        : document.querySelector('.eorc-dynamic-grid-container');
    if (!container) return;

    const bodyContainer = container.querySelector('.grid-body');
    if (!bodyContainer) return;

    const gridState = window.Grids[gridName] || { currentPage: 1, pageSize: 10 };
    if (gridState.currentPage === 1) {
        bodyContainer.innerHTML = '';
    }

    const gridData = document.querySelector('#' + gridName + '_gridData');
    const enableEditButton = gridData?.dataset.editButton === 'true';
    const enableDeleteButton = gridData?.dataset.deleteButton === 'true';

    groups.forEach(g => {
        const groupContainer = document.createElement('div');
        groupContainer.className = 'grid-group-container';

        const groupHeader = document.createElement('div');
        groupHeader.className = 'grid-group-header';

        const toggle = document.createElement('span');
        toggle.className = 'group-toggle';
        toggle.style.cursor = 'pointer';
        toggle.style.marginRight = '8px';
        toggle.style.color = '#007bff';
        toggle.style.fontWeight = 'bold';
        toggle.textContent = `[- ${g.group.length} ركورد]`;

        const title = document.createElement('span');
        title.style.marginRight = '6px';
        title.style.fontWeight = '500';
        title.textContent = g.key; // ← نمایش عنوان گروه

        groupHeader.appendChild(toggle);
        groupHeader.appendChild(title);
        groupContainer.appendChild(groupHeader);

        const rowsWrapper = document.createElement('div');
        rowsWrapper.className = 'grid-group-rows';
        rowsWrapper.style.display = 'block';

        g.group.forEach(item => {
            const row = document.createElement('div');
            row.className = 'grid-row';

            // دکمه‌ها
            if (enableEditButton || enableDeleteButton) {
                const actions = document.createElement('div');
                actions.className = 'grid-cell grid-cell-Buttons';

                if (enableEditButton) {
                    let insEditFunction = gridData?.dataset.editFunction || `InsUpd_${gridName}_Item(this)`;
                    actions.innerHTML = `<button class="btn primary edit-btn" onclick='${insEditFunction}'> <i class="fa fa-edit"></i> ویرایش</button>`;
                }

                if (enableDeleteButton) {
                    let deleteFunction = gridData?.dataset.deleteFunction || `Dlt_${gridName}_Item(this)`;
                    actions.innerHTML += `<button class="btn danger delete-btn" onclick='${deleteFunction}'><i class="fa fa-trash"></i> حذف</button>`;
                }

                row.appendChild(actions);
            }

            columns.forEach(col => {
                const div = document.createElement('div');
                div.className = 'grid-cell';
                div.setAttribute('data-cell', col.prop);

                const originalValue = getItemValue(item, col.prop);
                div.setAttribute('original-content', originalValue);
                div.textContent = !isNaN(originalValue) && originalValue !== null && originalValue !== ''
                    ? Number(originalValue).toLocaleString()
                    : truncateText(originalValue);

                if (!col.visible) {
                    div.style.display = 'none';
                    div.classList.add('grid-cell-hidden');
                }

                row.appendChild(div);
            });

            rowsWrapper.appendChild(row);
        });

        groupContainer.appendChild(rowsWrapper);
        bodyContainer.appendChild(groupContainer);

        let isExpanded = true;
        toggle.addEventListener('click', () => {
            isExpanded = !isExpanded;
            rowsWrapper.style.display = isExpanded ? 'block' : 'none';
            toggle.textContent = isExpanded
                ? `[- ${g.group.length} ركورد]`
                : `[+ ${g.group.length} ركورد]`;
        });
    });
}

//گروه بندي ركوردها (تايتل)
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
async function fetchGridData(page, size, customBody = null, gridName = null) {
    const container = document.getElementById(gridName);
    if (!container) return;

    showGridWaiter(gridName); // ✅ همین‌جا

    await new Promise(r => setTimeout(r, 600)); // delay دلخواه قبل از fetch

    const gridState = window.Grids[gridName] || {
        currentPage: 1,
        pageSize: 10,
        sortColumn: '',
        sortAsc: true,
        enablePaging: true,
        customRequestBody: {}
    };

    // بروز رسانی حالت Paging
    gridState.enablePaging = document.querySelector('#' + gridName + '_gridData')?.dataset.enablePaging === 'true';

    const urlElement = document.querySelector('#' + gridName + '_gridData');
    const localDataElement = document.querySelector('#' + gridName + '_gridDataLocal');

    // خواندن فیلترها
    const filters = {};
    container.querySelectorAll('.filter-input').forEach(input => {
        const key = input.dataset.prop;
        const val = normalizePersianText(input.value);
        const type = input.dataset.filterType || 'contains';
        if (val) {
            filters[key] = { Type: type, Value: val };
        }
    });

    const groupBy = document.querySelector('#' + gridName + '_groupBySelector')?.value || '';

    // حالت گروه‌بندی روی داده‌های لوکال
    if (localDataElement && groupBy) {
        let items = applyFilters(window.allItemsCache?.[gridName] || [], filters);
        const data = JSON.parse(localDataElement.textContent);
        let columns = data.columns || [];
        renderGroupedRows(groupItems(items, groupBy), columns, gridName);
        hideGridWaiter(gridName);
        return;
    }

    const gridData = document.querySelector('#' + gridName + '_gridData');
    var isLazyLoading = gridData?.dataset.lazyLoading === 'true';
    // حالت Paging معمولی یا Fetch از سرور

    if (urlElement && urlElement.dataset.url) {
        const gridRequest = {
            Page: page,
            PageSize: size,
            SortColumn: isLazyLoading ? "" : gridState.sortColumn,
            SortAsc: gridState.sortAsc,
            GroupBy: groupBy,
            Filters: isLazyLoading ? {} : filters,
            enablePaging: gridState.enablePaging,
            lazyLoading: isLazyLoading
        };

        const encodedGridRequest = btoa(unescape(encodeURIComponent(JSON.stringify(gridRequest))));
        const bodyToSend = customBody || gridState.customRequestBody || {};


        fetch(urlElement.dataset.url, {
            method: 'POST',
            body: JSON.stringify(bodyToSend),
            headers: {
                'Content-Type': 'application/json',
                'GridRequest': encodedGridRequest
            },
        })
            .then(res => res.json())
            .then(data => {
                gridState.totalPage = Math.ceil(data.totalCount / data.pageSize);
                const items = Array.isArray(data.items) ? data.items : [];

                // ذخیره داده‌ها در حافظه برای هر گرید
                window.allItemsCache = window.allItemsCache || {};
                window.allItemsCache[gridName] = [...(window.allItemsCache[gridName] || []), ...items];

                let columns = [];
                if (localDataElement) {
                    const d = JSON.parse(localDataElement.textContent);
                    columns = d.columns || [];
                }

                if (groupBy && items.length) {
                    renderGroupedRows(groupItems(items, groupBy), columns, gridName);
                } else {
                    window.allItemsCache[gridName];
                    renderRows(items, columns, gridName, filters, gridState.sortColumn, gridState.sortAsc);
                }
            })
            .catch(err => {
                console.error(`Error fetching grid data for ${gridName}:`, err);
            })
            .finally(() => {
                hideGridWaiter(gridName); // ✅ کافی و امن
            });
    }
    else {
        console.warn(`No data source found for grid ${gridName}.`);
    }

}

// initialize
// اين تابع ممكن با اسكرول داده ها تركيب بشن و دوباره بره سمت سرور بايد بررسي بشه بيشتر 
function initGrid(gridName, lazyLoading = true) {
    const gridState = window.Grids[gridName];
    if (!gridState) return;

    const grid = document.getElementById(gridName);
    if (!grid) return;

    // مقداردهی pageSize
    gridState.pageSize = parseInt(grid.dataset.pageSize || gridState.pageSize);

    // همگام‌سازی در صورت وجود
    if (typeof syncWith === 'function') syncWith(gridName);

    // بارگذاری اولیه داده
    if (typeof fetchGridData === 'function') {
        fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
    }

    const gridBody = grid.querySelector('.grid-body');
    if (lazyLoading === "true" && gridBody) {
        // تنظیم ارتفاع برای فعال کردن اسکرول
        gridBody.style.height = '70vh'; // می‌توانی مقدار دلخواه بدهی
        gridBody.style.overflowY = 'scroll';
        gridBody.style.overflowX = 'unset';

        // اضافه کردن رویداد scroll برای لیزی لود
        gridBody.addEventListener('scroll', () => {
            const threshold = 50;
            gridState.loading = false; // حالت اولیه
           if (gridBody.scrollTop + gridBody.clientHeight >= gridBody.scrollHeight - threshold) {
    if (!gridState.loading) {
        gridState.loading = true;
        gridState.currentPage++;
        fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
    }
}
        });
    } else {
        // مدیریت Paging عادی
        const nextBtn = grid.querySelector('#nextPage');
        const prevBtn = grid.querySelector('#prevPage');
        const pageSizeSelector = document.querySelector('#' + gridName + '_pageSizeSelector');
        const groupBySelector = grid.querySelector('#' + gridName + '_groupBySelector');
        const refreshBtn = grid.querySelector('#refreshBtn');

        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                gridState.currentPage++;
                fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
            });
        }
        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                if (gridState.currentPage > 1) {
                    gridState.currentPage--;
                    fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
                }
            });
        }
        if (pageSizeSelector) {
            pageSizeSelector.addEventListener('change', function () {
                gridState.pageSize = parseInt(this.value);
                gridState.currentPage = 1;
                fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
            });
        }
        if (groupBySelector) {
            groupBySelector.addEventListener('change', () => {
                fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
            });
        }
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                gridState.currentPage = 1;
                fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
            });
        }
    }

    // فیلترها
    grid.querySelectorAll('.filter-input').forEach(input => {
        input.addEventListener('input', () => {
            gridState.currentPage = 1;
            fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
        });
    });

    // مرتب‌سازی
    const enableSortingEl = document.querySelector('#' + gridName + '_gridEnableSorting');
    if (enableSortingEl && enableSortingEl.innerText.toLowerCase() === 'true') {
        grid.querySelectorAll('.grid-header [data-column]').forEach(h => {
            h.addEventListener('click', () => {
                const col = h.dataset.column;
                if (gridState.sortColumn === col) gridState.sortAsc = !gridState.sortAsc;
                else { gridState.sortColumn = col; gridState.sortAsc = true; }
                fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
            });
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    // مقداردهی همه گریدهای تعریف شده در window.Grids
    Object.keys(window.Grids).forEach(gridName => {
        const gridData = document.querySelector('#' + gridName + '_gridData');
        var isLazyLoading = gridData?.dataset.lazyLoading
        initGrid(gridName, isLazyLoading);
        initFilterIcons(gridName);
        //adjustGridColumnWidths(gridName);
        enableRowDetailsPopup(gridName);

    });

    enableGridRowSelection();
});

// بستن منو وقتی جای دیگه کلیک شد
document.addEventListener('click', e => {
    if (!e.target.closest('.filter-icon')) {
        document.querySelectorAll('.eorc-grid-filter-menu').forEach(menu => {
            menu.style.display = 'none';
        });
    }
});

// عمليات فوتر گريد جمع ، ميانگين و ...
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.eorc-dynamic-grid-container').forEach(gridContainer => {
        gridContainer.querySelectorAll('.grid-cell[data-footer]').forEach(cell => {
            const input = cell.querySelector('.footer-input');
            const icon = cell.querySelector('.footer-icon');
            const menu = cell.querySelector('.eorc-grid-footer-menu');
            const gridWrapper = document.querySelector('.gridContainerWrapper');
            if (!icon || !menu) return; // حذف gridWrapper از شرط
            if (cell.dataset.footer === 'Actions') return;

            // مقدار پیش‌فرض calcState
            if (!cell.dataset.calcState) {
                cell.dataset.calcState = 'sum';
                const original = cell.getAttribute('original-content') || '0';
                input.value = `جمع = ${original}`;
            }

            // باز کردن منو
            icon.addEventListener('click', e => {
                e.stopPropagation();

                // حذف منوهای باز قبلی
                gridContainer.querySelectorAll('.eorc-grid-footer-menu.clone').forEach(m => m.remove());

                // ساخت clone از منو
                const clone = menu.cloneNode(true);
                clone.classList.add('clone');
                clone.style.position = 'fixed';
                clone.style.display = 'block';
                clone.style.zIndex = 10000;
                clone.style.minWidth = '150px';
                clone.style.boxShadow = '0 4px 10px rgba(0,0,0,0.15)';
                clone.style.borderRadius = '8px';
                clone.style.background = '#fff';
                clone.style.padding = '6px 0';
                clone.style.listStyle = 'none';
                clone.style.cursor = 'pointer';
                clone.style.transition = 'opacity 0.15s ease, transform 0.15s ease';
                clone.style.opacity = '0';
                clone.style.transform = 'translateY(5px)';
                document.body.appendChild(clone);

                // محاسبه موقعیت دقیق
                const iconRect = icon.getBoundingClientRect();
                const popupRect = clone.getBoundingClientRect();
                const offset = 10;
                let left = iconRect.right - popupRect.width;
                let top = iconRect.top - popupRect.height - offset;

                // جلوگیری از خروج از viewport
                if (left < 4) left = 4;
                if (left + popupRect.width > window.innerWidth - 4)
                    left = window.innerWidth - popupRect.width - 4;
                if (top < 4) top = iconRect.bottom + offset;

                clone.style.left = `${left}px`;
                clone.style.top = `${top}px`;

                requestAnimationFrame(() => {
                    clone.style.opacity = '1';
                    clone.style.transform = 'translateY(0)';
                });

                // بستن هنگام کلیک بیرون
                const closeMenu = ev => {
                    if (!clone.contains(ev.target) && ev.target !== icon) {
                        clone.remove();
                        document.removeEventListener('click', closeMenu);
                    }
                };
                setTimeout(() => document.addEventListener('click', closeMenu), 100);

                // انتخاب گزینه‌ها
                clone.querySelectorAll('li').forEach(li => {
                    li.addEventListener('click', e => {
                        e.stopPropagation();
                        const calcType = li.dataset.calc;
                        cell.dataset.calcState = calcType;
                        updateGridFooters(calcType, cell);
                        clone.remove();
                    });
                });

                // بستن هنگام scroll
                if (gridWrapper) {
                    gridWrapper.addEventListener('scroll', () => clone.remove(), { once: true });
                }
            });
        });
    });
});

// بستن منو وقتی جای دیگه کلیک شد
document.addEventListener('click', e => {
    if (!e.target.classList.contains('footer-icon')) {
        const clonedMenus = document.querySelectorAll('.eorc-grid-footer-menu.clone');

        clonedMenus.forEach(menu => {
            const gridContainer = menu.closest('.eorc-dynamic-grid-container');
            if (gridContainer) {
                menu.remove();
            }
        });
    }
});

//تغيير عرض ستونهاي گريد
document.addEventListener("DOMContentLoaded", () => {
    const isRTL = document.documentElement.getAttribute("dir") === "rtl";

    document.querySelectorAll(".eorc-grid-container").forEach(grid => {
        const headerCells = grid.querySelectorAll(".grid-header .grid-cell");

        headerCells.forEach(cell => {
            cell.style.position = "relative";

            // ایجاد resizer
            const resizer = document.createElement("div");
            resizer.className = "resizer";
            resizer.style.position = "absolute";
            resizer.style.top = "0";
            resizer.style.width = "6px";
            resizer.style.height = "100%";
            resizer.style.cursor = "col-resize";
            resizer.style.zIndex = "5";
            resizer.style[isRTL ? "left" : "right"] = "0";

            cell.appendChild(resizer);

            let startX = 0;
            let startWidth = 0;
            let prop = "";
            let handleSide = isRTL ? "left" : "right";

            resizer.addEventListener("mousedown", e => {
                e.preventDefault();

                startX = e.pageX;
                startWidth = cell.offsetWidth;

                prop =
                    cell.dataset.column ||
                    cell.dataset.cell ||
                    cell.dataset.footer ||
                    cell.getAttribute("data-cell");

                // تشخیص موقعیت واقعی resizer
                const leftCss = window.getComputedStyle(resizer).left;
                const rightCss = window.getComputedStyle(resizer).right;

                if (leftCss && leftCss !== "auto" && leftCss !== "0px") {
                    handleSide = "left";
                } else if (rightCss && rightCss !== "auto" && rightCss !== "0px") {
                    handleSide = "right";
                }

                document.addEventListener("mousemove", resizeColumn);
                document.addEventListener("mouseup", stopResize);
            });

            function resizeColumn(e) {
                const delta = e.pageX - startX;

                const newWidth =
                    handleSide === "right" ? startWidth + delta : startWidth - delta;

                if (newWidth > 40) {
                    const newCss = {
                        width: newWidth + "px",
                        flex: `0 0 ${newWidth}px`
                    };

                    // فقط داخل همین گرید (بدون تداخل با بقیه)
                    const selector = `
                        .grid-header .grid-cell[data-cell="${prop}"],
                        .grid-header .grid-cell[data-column="${prop}"],
                        .grid-header .grid-cell[data-footer="${prop}"],
                        .grid-body .grid-cell[data-cell="${prop}"],
                        .grid-body .grid-cell[data-column="${prop}"],
                        .grid-body .grid-cell[data-footer="${prop}"],
                        .grid-filters .grid-cell[data-cell="${prop}"],
                        .grid-filters .grid-cell[data-column="${prop}"],
                        .grid-filters .grid-cell[data-footer="${prop}"],
                        .grid-footer .grid-cell[data-cell="${prop}"],
                        .grid-footer .grid-cell[data-column="${prop}"],
                        .grid-footer .grid-cell[data-footer="${prop}"]
                    `;

                    grid.querySelectorAll(selector).forEach(c => {
                        c.style.width = newCss.width;
                        c.style.flex = newCss.flex;
                    });
                }
            }

            function stopResize() {
                document.removeEventListener("mousemove", resizeColumn);
                document.removeEventListener("mouseup", stopResize);
            }
        });
    });
});

///////////////////////////////////////////

// لود با اسكرول
function initLazyLoading(gridName) {
    const gridData = document.querySelector('#' + gridName + '_gridData');
    if (!gridData || gridData.dataset.lazyLoading !== "true") return;

    const bodyContainer = container.querySelector('.grid-body');
    if (!bodyContainer) return;

    // بررسی آیا Lazy Loading فعال است
    const isLazy = container.dataset.lazyLoading === 'true';
    if (!isLazy) return;

    bodyContainer.addEventListener('scroll', () => {
        // اگر به انتهای scroll رسیدیم
        if (bodyContainer.scrollTop + bodyContainer.clientHeight >= bodyContainer.scrollHeight - 10) {
            const gridState = window.Grids[gridName];
            if (!gridState) return;

            // بررسی اینکه صفحه بعد وجود دارد
            if (gridState.currentPage < (gridState.totalPage || 1)) {
                gridState.currentPage++;
                fetchGridData(gridState.currentPage, gridState.pageSize, gridState.customRequestBody, gridName);
            }
        }
    });
}

