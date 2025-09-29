document.addEventListener("DOMContentLoaded", () => {
    const gridTag = document.querySelector("grid-inline-edit");
    if (!gridTag) return;

    // fetchUrl درست گرفته می‌شود
    const fetchUrl = gridTag.getAttribute("fetch-url") || "";

    let page = 1;
    const pageSize = 20;
    let sortColumn = "";
    let sortAsc = true;
    let filters = {};
    let groupBy = "";

    const groupSelect = gridTag.querySelector("#groupBySelector");

    function renderGroupOptions(columns) {
        groupSelect.innerHTML = '<option value="">بدون گروه‌بندی</option>';
        columns.forEach(col => {
            const opt = document.createElement("option");
            opt.value = col.name;
            opt.textContent = col.header;
            groupSelect.appendChild(opt);
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
                renderRows(result.data);
                renderPagination(result.totalPages);
                if (result.groupColumns) renderGroupOptions(result.groupColumns);
            })
            .catch(err => console.error(err));
    }

    function renderRows(data) {
        const rowsContainer = gridTag.querySelector("#gridRows");
        rowsContainer.innerHTML = "";

        data.forEach(item => {
            if (item.__group) {
                const groupRow = document.createElement("div");
                groupRow.className = "grid-row group-row";
                groupRow.textContent = item.name;
                rowsContainer.appendChild(groupRow);
            } else {
                const row = document.createElement("div");
                row.className = "grid-row";
                row.dataset.id = item.id;

                let html = "";
                Object.keys(item).forEach((key, index) => {
                    if (key !== "id") {
                        if (key.toLowerCase() === "category") {
                            html += `<div><select>
                                        <option>الکترونیک</option>
                                        <option>پوشاک</option>
                                        <option>خانه</option>
                                     </select></div>`;
                        } else {
                            html += `<div contenteditable="false">${item[key]}</div>`;
                        }
                    } else {
                        html += `<div>${item[key]}</div>`;
                    }
                });
                html += `<div><button class="btn primary">ویرایش</button></div>`;
                row.innerHTML = html;
                rowsContainer.appendChild(row);

                const editBtn = row.querySelector("button");
                editBtn.addEventListener("click", () => {
                    const cells = row.children;
                    const idCell = row.dataset.id;
                    const nameCell = cells[1];
                    const categoryCell = cells[2].querySelector("select");
                    const priceCell = cells[3];
                    const statusCell = cells[4];

                    if (row.dataset.editing === "true") {
                        const payload = {
                            id: parseInt(idCell),
                            name: nameCell.textContent,
                            category: categoryCell.value,
                            price: priceCell.textContent,
                            status: statusCell.textContent
                        };
                        fetch(`${fetchUrl}/save`, {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(payload)
                        }).then(() => loadData());

                        row.dataset.editing = "false";
                        editBtn.textContent = "ویرایش";
                        [nameCell, priceCell, statusCell, categoryCell].forEach(c => {
                            c.contentEditable = "false";
                            c.style.backgroundColor = "";
                        });
                    } else {
                        row.dataset.editing = "true";
                        editBtn.textContent = "ذخیره";
                        [nameCell, priceCell, statusCell].forEach(c => {
                            c.contentEditable = "true";
                            c.style.backgroundColor = "white";
                        });
                        categoryCell.style.backgroundColor = "white";
                    }
                });
            }
        });
    }

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

    // Sorting
    gridTag.querySelectorAll(".grid-header div[data-column]").forEach(div => {
        div.addEventListener("click", () => {
            const col = div.dataset.column;
            if (sortColumn === col) sortAsc = !sortAsc;
            else { sortColumn = col; sortAsc = true; }
            loadData();
        });
    });

    // Filtering
    gridTag.querySelectorAll(".grid-filters input").forEach(input => {
        input.addEventListener("input", () => {
            const col = input.id.replace("filter_", "");
            filters[col] = input.value;
            page = 1;
            loadData();
        });
    });

    // Refresh
    gridTag.querySelector("#refreshBtn").addEventListener("click", () => {
        page = 1;
        filters = {};
        sortColumn = "";
        sortAsc = true;
        groupBy = "";
        groupSelect.value = "";
        loadData();
    });

    // **لود اولیه**
    loadData();
});
