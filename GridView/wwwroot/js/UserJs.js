function SearchGridViewByPopupData() {
    debugger
    const region = document.getElementById('popupRegion')?.value || null;
    const product = document.getElementById('popupProduct')?.value || null;

    const urlElement = document.getElementById('gridData');
    const localDataElement = document.getElementById('gridDataLocal');

    const groupBy = document.getElementById('groupBySelector')?.value || '';
    const currentPageLocal = currentPage || 1;

    // فقط وضعیت صفحه‌بندی و سورت فعلی را نگه می‌داریم
    const gridRequest = {
        Page: currentPageLocal,
        PageSize: pageSize || 30,
        SortColumn: sortColumn || '',
        SortAsc: sortAsc !== undefined ? sortAsc : true,
        Filters: {}, // خالی چون فیلترهای پاپ‌آپ مستقل هستند
        GroupBy: groupBy,
        enablePaging: true
    };

    const encodedGridRequest = btoa(unescape(encodeURIComponent(JSON.stringify(gridRequest))));

    fetch(urlElement.dataset.url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'GridRequest': encodedGridRequest
        },
        body: JSON.stringify({ region, product })
    })
        .then(res => res.json())
        .then(data => {
            // پاک کردن cache قبلی برای گروه‌بندی
            window.allItemsCache = [];

            const items = Array.isArray(data.items) ? data.items : [];
            window.allItemsCache = [...items];

            const columns = localDataElement ? (JSON.parse(localDataElement.textContent).columns || []) : null;

            if (groupBy && items.length) {
                renderGroupedRows(groupItems(items, groupBy), columns);
            } else {
                renderRows(items, columns);
            }

            // آپدیت صفحه‌بندی
            totalPage = Math.ceil(data.totalCount / data.pageSize);
            const pageInfo = document.getElementById('pageInfo');
            if (pageInfo) pageInfo.textContent = `صفحه ${currentPage} از ${totalPage}`;
        })
        .catch(err => console.error('Error fetching popup search data:', err));
}

function InsUpd_grdProduct1_Item(btn) {
    debugger
    var id = getGridDataCell(btn, 'ID');
    // و ادامه كدهاي خودتون
}

function Dlt_grdProduct_Item(btn) {
    var SupplieR = getGridDataCell(btn, 'SupplieR');


    // و ادامه كدهاي خودتون
    alert(SupplieR);
}


