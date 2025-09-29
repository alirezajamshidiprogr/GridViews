document.addEventListener("DOMContentLoaded", () => {
    const gridTag = document.querySelector(".dynamic-grid-container");
    if (!gridTag) return;

    const fetchUrl = window.fetchUrl || "";
    let page = 1;
    const pageSize = 20;
    let sortColumn = "";
    let sortAsc = true;
    let filters = {};
    let groupBy = "";

    const groupSelect = document.getElementById("groupBySelector");
    const columns = window.columnMeta || [];

    // پر کردن گزینه‌های گروه‌بندی
    function renderGroupOptions() {
        groupSelect.innerHTML = '<option value="">بدون گروه‌بندی</option>';
        columns.forEach(col => {
            if (col.Visible) {
                const opt = document.createElement("option");
                opt.value = col.Name;
                opt.textContent = col.Header;
                groupSelect.appendChild(opt);
            }
        });
    }

    groupSelect.addEventListener("change", () => {
        groupBy = groupSelect.value;
        page = 1;
        loadData();
    });

    function loadData() {
        const params = { page, pageSize, sortColumn, sortAsc, filters, groupBy };

        fetch(fetchUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(params)
        })
            .then(res => res.json())
            .then(result => {
                renderHeader();
                if (window.enableFiltering) renderFilters();
                renderRows(result.data);
                if (window.enablePaging) renderPagination(result.totalPages);
                if (window.enableGrouping && result.groupColumns) renderGroupOptions();
            })
            .catch(err => console.error(err));
    }

    // هدر جدول
    function renderHeader() {
        const header = gridTag.querySelector(".grid-header");
        header.innerHTML = "";
        columns.forEach(col => {
            if (!col.Visible) return;
            const div = document.createElement("div");
            div.dataset.column = col.Name;
            div.textContent = `${col.Header} ▲▼`;
            header.appendChild(div);

            if (window.enableSorting) {
                div.addEventListener("click", () => {
                    if (sortColumn === col.Name) sortAsc = !sortAsc;
                    else { sortColumn = col.Name; sortAsc = true; }
                    loadData();
                });
            }
        });
        header.appendChild(document.createElement("div")).textContent = "عملیات";
    }

    // فیلترها
    function renderFilters() {
        const filterRow = gridTag.querySelector(".grid-filters");
        filterRow.innerHTML = "";
        columns.forEach(col => {
            if (!col.Visible) return;
            const input = document.createElement("input");
            input.id = `filter_${col.Name}`;
            input.placeholder = `جستجو ${col.Header}`;
            input.addEventListener("input", () => {
                filters[col.Name] = input.value;
                page = 1;
                loadData();
            });
            filterRow.appendChild(input);
        });
        filterRow.appendChild(document.createElement("div"));
    }

    // ردیف‌ها
    function renderRows(data) {
        const rowsContainer = gridTag.querySelector("#gridRows");
        rowsContainer.innerHTML = "";

        // paging
        const totalPages = Math.ceil(data.length / pageSize);
        if (page > totalPages) page = totalPages || 1;
        const start = (page - 1) * pageSize;
        const end = start + pageSize;
        const pageData = data.slice(start, end);

        pageData.forEach(item => {
            if (item.__group) {
                const groupRow = document.createElement("div");
                groupRow.className = "grid-row group-row";
                groupRow.textContent = item.name;
                rowsContainer.appendChild(groupRow);
            } else {
                const row = document.createElement("div");
                row.className = "grid-row";
                const idCol = columns.find(c => c.Name.toLowerCase() === "id");
                if (idCol && item[idCol.Name] !== undefined) row.dataset.id = item[idCol.Name];

                columns.forEach(col => {
                    if (!col.Visible) return;
                    const cell = document.createElement("div");
                    cell.dataset.column = col.Name;
                    if (col.Editable) {
                        // contentEditable فقط برای ویرایش متنی
                        cell.contentEditable = "false";
                    }
                    cell.textContent = item[col.Name] ?? "";
                    row.appendChild(cell);
                });

                const actionCell = document.createElement("div");
                const editBtn = document.createElement("button");
                editBtn.className = "btn primary";
                editBtn.textContent = "ویرایش";
                actionCell.appendChild(editBtn);
                row.appendChild(actionCell);

                editBtn.addEventListener("click", () => {
                    if (row.dataset.editing === "true") {
                        // ذخیره به سرور
                        const payload = { Id: row.dataset.id };
                        columns.forEach(col => {
                            if (col.Visible && col.Editable) {
                                const cell = row.querySelector(`div[data-column='${col.Name}']`);
                                cell.contentEditable = "true";
                                cell.style.backgroundColor = "white";
                            }
                        });
                        fetch(`${fetchUrl}/save`, {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(payload)
                        }).then(() => loadData());

                        row.dataset.editing = "false";
                        editBtn.textContent = "ویرایش";
                        row.querySelectorAll("[contenteditable='true']").forEach(c => {
                            c.contentEditable = "false";
                            c.style.backgroundColor = "";
                        });
                    } else {
                        row.dataset.editing = "true";
                        editBtn.textContent = "ذخیره";
                        columns.forEach(col => {
                            if (col.Visible && col.Editable) {
                                const cell = row.querySelector(`div[data-column='${col.Name}']`);
                                cell.contentEditable = "true";
                                cell.style.backgroundColor = "white";
                            }
                        });
                    }
                });

                rowsContainer.appendChild(row);
            }
        });

        // صفحه‌بندی
        if (window.enablePaging) renderPagination(totalPages);
    }

    // صفحه‌بندی
    function renderPagination(totalPages) {
        const container = gridTag.querySelector("#pagination");
        container.innerHTML = "";

        if (page > 1) {
            const prev = document.createElement("button");
            prev.textContent = "قبلی";
            prev.className = "btn";
            prev.onclick = () => { page--; loadData(); };
            container.appendChild(prev);
        }
        if (page < totalPages) {
            const next = document.createElement("button");
            next.textContent = "بعدی";
            next.className = "btn";
            next.onclick = () => { page++; loadData(); };
            container.appendChild(next);
        }
    }

    // دکمه تازه‌سازی
    document.getElementById("refreshBtn").addEventListener("click", () => {
        page = 1;
        filters = {};
        sortColumn = "";
        sortAsc = true;
        groupBy = "";
        groupSelect.value = "";
        loadData();
    });

    // لود اولیه
    loadData();
});