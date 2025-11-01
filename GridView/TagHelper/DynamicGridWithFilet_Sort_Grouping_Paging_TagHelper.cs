using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace GridView.TagHelpers
{
    public class DynamicGridBuilder
    {
        private List<object> _items = new List<object>();
        private readonly string _gridName;
        private string _url;
        private bool? _enablePaging;
        private bool? _enableEditButton;
        private bool? _enableDeleteButton ;
        private bool? _enableFiltering;
        private bool? _enableInlineEdit ;
        private bool? _enableSorting ;
        private bool? _enableFooter ;
        private bool? _enableGrouping ;
        private bool? _enableExcelExport;
        private bool? _enablePrint = true;
        private bool? _enableShowHiddenColumns;
        private bool? _enableAdvancedFilter ;

        public DynamicGridBuilder(string gridName)
        {
            _gridName = gridName;
        }

        public DynamicGridBuilder Items(List<object> items) { _items = items; return this; }
        //public DynamicGridBuilder GridName(string name) { _gridName = name; return this; }
        public DynamicGridBuilder Url(string url) { _url = url; return this; }
        public DynamicGridBuilder EnablePaging(bool val) { _enablePaging = val; return this; }
        public DynamicGridBuilder EnableFiltering(bool val) { _enableFiltering = val; return this; }
        public DynamicGridBuilder EnableSorting(bool val) { _enableSorting = val; return this; }
        public DynamicGridBuilder EnableFooter(bool val) { _enableFooter = val; return this; }
        public DynamicGridBuilder EnableGrouping(bool val) { _enableGrouping = val; return this; }
        public DynamicGridBuilder EnableExcelExport(bool val) { _enableExcelExport = val; return this; }
        public DynamicGridBuilder EnablePrint(bool val) { _enablePrint = val; return this; }
        public DynamicGridBuilder EnableEditButton(bool val) { _enableEditButton = val; return this; }
        public DynamicGridBuilder EnableDeleteButton(bool val) { _enableDeleteButton = val; return this; }
        public DynamicGridBuilder EnableInlineEdit(bool val) { _enableInlineEdit = val; return this; }
        public DynamicGridBuilder EnableShowHiddenColumns(bool val) { _enableShowHiddenColumns = val; return this; }
        public DynamicGridBuilder EnableAdvancedFilter(bool val) { _enableAdvancedFilter = val; return this; }

        public IHtmlContent Build()
        {
            string html = "";
            // اعتبارسنجی ورودی‌ها قبل از تولید HTML
            // اعتبارسنجی ورودی‌ها قبل از تولید HTML
            string htmlError = string.Empty;
            if (_items == null || !_items.Any())
            {
                htmlError += $"<div class='alert alert-danger'>آدرس URL گريد ويو مشخص نشده است . </div>";
            }

            if (string.IsNullOrEmpty(_gridName))
            {
                htmlError += $"<div class='alert alert-danger'>نام گرید مشخص نشده است! Eorc_Grid(null value is not valid)</div>";
            }

            if (string.IsNullOrEmpty(_url))
            {
                htmlError += $"<div class='alert alert-danger'>آدرس گرید مشخص نشده است! : URL(null value is not valid) </div>";
            }

            // بررسی تمام فیلدهای Nullable<bool> و دادن خطا اگر مشخص نشده باشند
            if (_enablePaging == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enablePaging مشخص نشده است!</div>";
            }

            if (_enableEditButton == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableEditButton مشخص نشده است!</div>";
            }

            if (_enableDeleteButton == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableDeleteButton مشخص نشده است!</div>";
            }

            if (_enableFiltering == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableFiltering مشخص نشده است!</div>";
            }

            if (_enableInlineEdit == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableInlineEdit مشخص نشده است!</div>";
            }

            if (_enableSorting == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableSorting مشخص نشده است!</div>";
            }

            if (_enableFooter == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableFooter مشخص نشده است!</div>";
            }

            if (_enableGrouping == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableGrouping مشخص نشده است!</div>";
            }

            if (_enableExcelExport == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableExcelExport مشخص نشده است!</div>";
            }

            if (_enablePrint == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enablePrint مشخص نشده است!</div>";
            }

            if (_enableShowHiddenColumns == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableShowHiddenColumns مشخص نشده است!</div>";
            }

            if (_enableAdvancedFilter == null)
            {
                htmlError += "<div class='alert alert-danger'>گزینه _enableAdvancedFilter مشخص نشده است!</div>";
            }

            if (htmlError.Length > 0)
            {
                return new HtmlString(htmlError);
            }


            html = $"<div id='{_gridName}' class='dynamic-grid-container'>";
            html += $@"
                    <div id='gridData'
                         data-url='{_url ?? ""}'
                         data-enable-paging='{_enablePaging.ToString().ToLower()}'
                         data-edit-button='{_enableEditButton.ToString().ToLower()}'
                         data-delete-button='{_enableDeleteButton.ToString().ToLower()}'
                         style='display:none;'>
                    </div>";


            //html += $"<div id='gridSettings' data-enable-paging='{_enablePaging.ToString().ToLower()}'></div>";
            //html += $"<div id='enabelEditButton' data-edit-button='{_enableEditButton.ToString().ToLower()}'></div>";
            //html += $"<div id='enabelDeleteButton' data-edit-button='{_enableDeleteButton.ToString().ToLower()}'></div>";

            html += "<div id='gridContainerWrapper'>";
            html += "<div class='row controls controls-bar'>";
            if (_enablePaging.HasValue && _enablePaging.Value)
            {
                html += $"<div class='col-1' id='grd-pageSizeSelector'>" +
                   $"<label>تعداد در هر صفحه:" +
                   $"<select id='pageSizeSelector'>" +
                   $"<option value='5'>5</option>" +
                   $"<option value='10'>10</option>" +
                   $"<option value='15'>15</option>" +
                   $"<option value='20'>20</option>" +
                   $"<option value='25'>25</option>" +
                   $"<option value='30' selected>30</option>" +
                   $"</select>" +
                   $"</label></div>";
            }


            if (_enableGrouping.HasValue && _enableGrouping.Value)
            {
                // استخراج ستون‌های قابل گروه‌بندی
                var groupableColumns = _items.First()
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new
                    {
                        Prop = p,
                        Attr = p.GetCustomAttribute<GridColumnAttribute>()
                    })
                    .Where(x => x.Attr != null && (x.Attr.EnableGrouping == true)) // فقط اونایی که grouping=true دارن
                    .ToList();

                html += "<div class='col-2 groupby-wrapper'><label>گروه‌بندی بر اساس:";
                html += "<select id='groupBySelector'>";
                html += "<option value=''>— بدون گروه‌بندی —</option>";

                foreach (var col in groupableColumns)
                {
                    var header = col.Attr.Header ?? col.Prop.Name;
                    html += $"<option value='{col.Prop.Name}'>{header}</option>";
                }

                html += "</select></label></div>";
            }

            if (_enableExcelExport.HasValue && _enableExcelExport.Value)
            {
                html += "<div class='grid-print-col'><button id='ExcelGridBtn' onclick='exportGridToExcelXlsx()' class='grid-print-btn'> خروجي اكسل<i style='margin-right: 6px;font-size: 20px;' class='fa fa-file-excel-o'></i> </button></div>";
            }
            if (_enablePrint.HasValue && _enablePrint.Value)
            {
                html += "<div class='grid-print-col'> <button id='printGridBtn' onclick='printDynamicGrid()' class='grid-print-btn'>پرینت  <i style='margin-right: 6px;font-size: 20px;' class='fa fa-print'></i></button></div>";
            }
            if (_enableShowHiddenColumns.HasValue && _enableShowHiddenColumns.Value)
            {
                html += "<div class='grid-print-col'> <button id='displayGridColumns' onclick='displayGridColumns()' class='grid-print-btn'>نمايش ستون<i style='margin-right: 6px;font-size: 20px;' class='fa fa-columns'></i></button></div>";
            }
            if (_enableAdvancedFilter.HasValue && _enableAdvancedFilter.Value)
            {
                html += "<div class='grid-print-col'> <button id='displayGridColumns' onclick='displayAdvancedFilter()' class='grid-print-btn'>فيلتر پيشرفته<i style='margin-right: 6px;font-size: 20px;' class='fa fa-search'></i></button></div>";
            }

            html += "</div>"; // controls

            html += "<div id='gridContainer' class='grid-container'>";

            if (_items.Any())
            {
                var firstItem = _items.First();
                var props = firstItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (_enableAdvancedFilter.HasValue && _enableAdvancedFilter.Value)
                {
                    html += @"
                        <div id='advancedFilterPopup' class='advanced-filter-popup' style='display:none;position:fixed;top:50%;left:50%;transform:translate(-50%, -50%);width:600px;max-width:90vw;min-width:400px;max-height:80vh;background:#fff;border:1px solid #ccc;border-radius:8px;box-shadow:0 4px 16px rgba(0,0,0,0.25);overflow-y:auto;padding:0;z-index:10000;'>
                            <!-- هدر پاپ‌آپ -->
                            <div class='popup-header' style='background:#3498db;color:#fff;font-weight:bold;padding:12px 16px;border-top-left-radius:8px;border-top-right-radius:8px;font-size:16px;'>
                                فیلتر پیشرفته
                                <span onclick='closeAdvancedFilter()' style='float:right;cursor:pointer;font-weight:bold;'>×</span>
                            </div>
                            <!-- محتوای فیلدها -->
                            <div class='popup-body' style='padding:12px;'>
                    ";
                    foreach (var prop in props)
                    {
                        var attr = prop.GetCustomAttribute<GridColumnAttribute>();
                        if (attr == null || !attr.EnableFiltering) continue; // فقط فیلدهایی که فیلتر دارند

                        string fieldName = prop.Name;
                                            string header = attr.Header ?? fieldName;

                        html += $@"
        <div class='filter-row' style='margin-bottom:12px;display:flex;flex-direction:column;'>
            <label style='font-weight:bold;margin-bottom:4px;'>{header}</label>
            <div style='display:flex;gap:8px;'>
                <input type='text' id='from{fieldName}' name='from{fieldName}' placeholder='از {header}' class='form-control' />
                <input type='text' id='to{fieldName}' name='to{fieldName}' placeholder='تا {header}' class='form-control' />
            </div>
        </div>
        <hr style='margin:6px 0;' />
    ";
                    }
                    html += @"
    </div> <!-- پایان popup-body -->
    <div class='popup-footer' style='text-align:right;padding:12px;border-top:1px solid #ccc;background:#f9f9f9;border-bottom-left-radius:8px;border-bottom-right-radius:8px;'>
        <button type='button' onclick='applyAdvancedFilter()' class='btn btn-primary' style='margin-left:8px;'>اعمال فیلتر</button>
        <button type='button' onclick='closeAdvancedFilter()' class='btn btn-secondary'>بستن</button>
    </div>
</div>
";
                }

                var columnsMeta = props
                    .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<GridColumnAttribute>() })
                    .Where(x => x.Attr != null)
                    .ToList();

                // Header
                html += "<div class='grid-header'>";
                foreach (var col in columnsMeta)
                {
                    var style = col.Attr.Visible ? "" : "style='display:none;'";
                    var className = col.Attr.Visible ? "grid-cell" : "grid-cell grid-cell-hidden";

                    var sortIcons = (_enableSorting.HasValue && _enableSorting.Value) && col.Attr.EnableSorting ? " ▲▼" : "";
                    html += $"<div class='{className}' {style} data-column='{col.Prop.Name}'>{col.Attr.Header}{sortIcons}</div>";
                }

                //  ستون عملیات
                if ((_enableEditButton.HasValue && _enableEditButton.Value) || (_enableDeleteButton.HasValue && _enableDeleteButton.Value))
                {
                    html += "<div class='grid-cell'>عملیات</div>";
                }

                html += "</div>";

                // Filters
                if (_enableFiltering.HasValue && _enableFiltering.Value)
                {
                    html += "<div class='grid-filters'>";
                    foreach (var col in columnsMeta)
                    {
                        if (!col.Attr.EnableFiltering) continue;
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        var className = col.Attr.Visible ? "grid-cell" : "grid-cell grid-cell-hidden";

                        html += $"<div class='{className}' {style} data-column='{col.Prop.Name}' style='position:relative;'>" +
                                $"<input type='text'  class='filter-input' data-prop='{col.Prop.Name}' placeholder='جستجو {col.Attr.Header}' />" +
                                $"<span class='filter-icon'>&#128269;</span>" +
                                $"<ul class='filter-menu'>" +
                                $"<li data-type='eq'  data-icon='='> =مساوي با </li>" +
                                $"<li data-type='neq' data-icon='≠'> !=نا مساوي با </li>" +
                                $"<li data-type='gt'  data-icon='<' > &gt;بزرگتر از </li>" +
                                $"<li data-type='lt'  data-icon='>'> &lt;كوچكتر از </li>" +
                                $"<li data-type='startswith' data-icon='*%'>شروع با * </li>" +
                                $"<li data-type='endswith' data-icon='%*'>* پايان با  </li>" +
                                $"<li data-type='contains' data-icon=''>شامل</li>" +
                                $"</ul>" +
                                "</div>";
                    }
                    //html += "<div class='grid-cell'></div>";
                    html += "</div>";
                }

                // Body
                html += "<div class='grid-body'>";
                foreach (var item in _items)
                {
                    html += "<div class='grid-row'>";
                    foreach (var col in columnsMeta)
                    {
                        var value = System.Net.WebUtility.HtmlEncode(col.Prop.GetValue(item)?.ToString() ?? "");
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        html += $"<div class='grid-cell' data-cell='{col.Prop.Name}' {style}>{value}</div>";
                    }

                    // ✅ فقط اگر یکی از دکمه‌ها فعال باشد، نمایش داده شود
                    if ((_enableEditButton == true) || (_enableDeleteButton == true))
                    {
                        html += "<div class='grid-cell grid-cell-Buttons'>";
                        if (_enableEditButton == true)
                            html += "<button class='btn primary edit-btn'>ویرایش</button>";
                        if (_enableDeleteButton == true)
                            html += "<button class='btn danger delete-btn'>حذف</button>";
                        html += "</div>";
                    }

                    html += "</div>";
                }
                html += "</div>";

                // Footer
                if (_enableFooter.HasValue && _enableFooter.Value)
                {
                    html += "<div class='grid-footer'><div class='grid-row footer-row'>";
                    foreach (var col in columnsMeta)
                    {
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        bool isNumeric = col.Prop.PropertyType == typeof(int) ||
                                         col.Prop.PropertyType == typeof(double) ||
                                         col.Prop.PropertyType == typeof(decimal) ||
                                         col.Prop.PropertyType == typeof(float) ||
                                         col.Prop.PropertyType == typeof(long);

                        string footerValue = "";
                        if (isNumeric)
                        {
                            var sum = _items.Sum(item => Convert.ToDecimal(col.Prop.GetValue(item) ?? 0));
                            footerValue = sum.ToString("N0");
                        }

                        html += $"<div class='grid-cell disabled' data-footer={col.Prop.Name} {style}>" +
                                $"<input type='text' class='footer-input' value='{footerValue}' readonly />" +
                                (isNumeric ?
                                    "<span class='footer-icon' data-icon-id='calc'>Σ</span>" +
                                    "<ul class='footer-menu' style='display: none;'>" +
                                    " <li data-calc='sum'>➕ جمع</li>" +
                                    " <li data-calc='avg'>📊 میانگین</li>" +
                                    " <li data-calc='count'>🔢 تعداد</li>" +
                                    " <li data-calc='max'>⬆️ بیشترین</li>" +
                                    " <li data-calc='min'>⬇️ کمترین</li>" +
                                    "</ul>" : "") +
                                "</div>";
                    }

                    // ✅ ستون عملیات در Footer
                    if ((_enableEditButton == true) || (_enableDeleteButton == true))
                    {
                        html += "<div class='grid-cell' data-footer='Actions'><input type='text' class='footer-input' value='0' readonly /></div>";
                    }

                }

                var columnsJson = props.Select(p =>
                {
                    var attr = p.GetCustomAttribute<GridColumnAttribute>();
                    return new
                    {
                        prop = p.Name,
                        header = attr?.Header ?? p.Name,
                        visible = attr?.Visible ?? true,
                        filtering = attr?.EnableFiltering ?? true,
                        sorting = attr?.EnableSorting ?? true,
                        grouping = attr?.EnableGrouping ?? true
                    };
                }).ToList();

                var jsonData = JsonSerializer.Serialize(new
                {
                    items = _items.Select(i => i.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .ToDictionary(p => p.Name, p => p.GetValue(i))),
                    columns = columnsJson
                });

                // ✅ جلوگیری از شکستن تگ script
                var safeJson = jsonData.Replace("</script>", "<\\/script>");

                html += $"<script id='gridDataLocal' type='application/json'>{safeJson}</script>";
            }

            html += "</div>"; // پایان gridContainer
            html += "</div>"; // پایان gridContainerWrapper

            // Paging
            if (_enablePaging.HasValue && _enablePaging.Value)
            {
                html += @"<div class='pagination'>
                    <button id='prevPage' class='btn pagination-btn'>قبلی<svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke=' currentColor' stroke-width='2'><polyline points='9 6 15 12 9 18'></polyline></svg></button>
                    <span id='pageInfo'></span>
                    <button id='nextPage' class='btn pagination-btn'>بعدی<svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><polyline points='15 18 9 12 15 6'></polyline></svg></svg></button>
                </div>";
            }

            html += "</div>"; // پایان dynamic-grid-container

            return new HtmlString(html);
        }
    }

    public static class DynamicGridExtensions
    {
        public static DynamicGridBuilder Eorc_Grid(string gridName)
                => new DynamicGridBuilder(gridName);
    }
}
