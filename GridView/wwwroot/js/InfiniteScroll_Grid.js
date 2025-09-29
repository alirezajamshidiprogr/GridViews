let page = 0;
const pageSize = 50;
let loading = false;
let sortColumn = "";
let sortAsc = true;
let filters = {};

document.addEventListener("DOMContentLoaded", () => {
    const container = document.getElementById("gridContainer");
    if (!container) return;

    attachEvents();
    loadData(true);
});

async function loadData(reset = false) {
    if (loading) return;
    loading = true;

    const fetchUrl = document.getElementById("fetchUrl")?.value;
    if (!fetchUrl) return;

    const rowsContainer = document.querySelector(".grid-rows");

    if (reset) {
        page = 0;
        rowsContainer.innerHTML = "";
    }

    const params = new URLSearchParams();
    params.append("page", page);
    params.append("pageSize", pageSize);
    if (sortColumn) {
        params.append("sortBy", sortColumn);
        params.append("sortAsc", sortAsc);
    }
    Object.keys(filters).forEach(k => {
        if (filters[k]) params.append(`filter_${k}`, filters[k]);
    });

    try {
        const res = await fetch(`${fetchUrl}?${params.toString()}`);
        const data = await res.json();
        if (!data || data.length === 0) return;

        data.forEach(item => rowsContainer.appendChild(createRowElement(item)));
        page++;
    } catch (err) {
        console.error(err);
    }

    loading = false;
}

function createRowElement(item) {
    const row = document.createElement("div");
    row.className = "grid-row";

    // hiddenColumns را یک بار به lowercase تبدیل می‌کنیم
    const hiddenCols = hiddenColumns.map(c => c.toLowerCase());

    Object.keys(item).forEach(k => {
        const div = document.createElement("div");
        div.className = "grid-cell";
        div.textContent = item[k];

        // بررسی اینکه این ستون مخفی است
        if (hiddenCols.includes(k.toLowerCase())) {
            div.classList.add("grid-cell-hidden");
        }

        row.appendChild(div);
    });

    // ستون عملیات
    const actions = document.createElement("div");
    actions.className = "grid-cell actions-cell";
    actions.innerHTML = `<button class="btn primary edit-btn">ویرایش</button>
                         <button class="btn danger delete-btn">حذف</button>`;
    row.appendChild(actions);

    return row;
}

function attachEvents() {
    const container = document.getElementById("gridContainer");
    const rowsContainer = document.querySelector(".grid-rows");

    // sort
    container.addEventListener("click", e => {
        const h = e.target.closest("[data-column]");
        if (!h) return;
        const col = h.dataset.column;
        if (sortColumn === col) sortAsc = !sortAsc;
        else { sortColumn = col; sortAsc = true; }
        loadData(true);
    });

    // infinite scroll
    rowsContainer.addEventListener("scroll", () => {
        if (!loading && rowsContainer.scrollTop + rowsContainer.clientHeight >= rowsContainer.scrollHeight - 20) {
            loadData(false);
        }
    });

    // refresh button
    document.getElementById("refreshBtn")?.addEventListener("click", () => loadData(true));

    // filters
    document.querySelectorAll(".grid-filters input").forEach(input => {
        input.addEventListener("input", () => {
            const col = input.id.replace("filter_", "");
            filters[col] = input.value;
            loadData(true);
        });
    });
}
