using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace GridView.TagHelpers
{
    public class Eorc_DynamicGridBuilder
    {
        private List<object> _items = new List<object>();
        private string _url;
        private string _editJsFunctionName;
        private Dictionary<string, string> _editFunctionParams = new();
        private string? _deleteJavaScriptFunction = string.Empty;
        private bool? _enablePaging;
        private bool? _enableEditButton;
        private bool? _enableDeleteButton;
        private bool? _enableFiltering;
        private bool? _enableInlineEdit;
        private bool? _enableSorting;
        private bool? _enableFooter;
        private bool? _enableGrouping;
        private bool? _enableExcelExport;
        private bool? _enablePdfExport;
        private bool? _enablePrint = true;
        private bool? _enableShowHiddenColumns;
        private bool? _enableAdvancedFilter;
        private int? _pageSize = 10;
        private bool _enableLazyLoading;

        private List<string> _customHtmlElements = new List<string>();

        private Type _modelType;
        private string _gridName;

        public Eorc_DynamicGridBuilder ModelType<T>(string gridName)
        {
            _modelType = typeof(T);
            _gridName = gridName;  // اینجا هم نام گرید ست می‌شود
            return this;
        }

        public Eorc_DynamicGridBuilder AddCustomHtml(string html)
        {
            _customHtmlElements.Add(html);
            return this;
        }

        public Eorc_DynamicGridBuilder Items(List<object> items) { _items = items; return this; }
        public Eorc_DynamicGridBuilder Url(string url) { _url = url; return this; }
        public Eorc_DynamicGridBuilder EnablePaging(bool val) { _enablePaging = val; return this; }
        public Eorc_DynamicGridBuilder EnableFiltering(bool val) { _enableFiltering = val; return this; }
        public Eorc_DynamicGridBuilder EnableSorting(bool val) { _enableSorting = val; return this; }
        public Eorc_DynamicGridBuilder EnableFooter(bool val) { _enableFooter = val; return this; }
        public Eorc_DynamicGridBuilder EnableGrouping(bool val) { _enableGrouping = val; return this; }
        public Eorc_DynamicGridBuilder EnableExcelExport(bool val) { _enableExcelExport = val; return this; }
        public Eorc_DynamicGridBuilder EnablePDFExport(bool val) { _enablePdfExport = val; return this; }
        public Eorc_DynamicGridBuilder EnablePrint(bool val) { _enablePrint = val; return this; }
        public Eorc_DynamicGridBuilder EnableEditButton(bool val) { _enableEditButton = val; return this; }
        public Eorc_DynamicGridBuilder EnableDeleteButton(bool val) { _enableDeleteButton = val; return this; }
        public Eorc_DynamicGridBuilder EnableInlineEdit(bool val) { _enableInlineEdit = val; return this; }
        public Eorc_DynamicGridBuilder EnableShowHiddenColumns(bool val) { _enableShowHiddenColumns = val; return this; }
        public Eorc_DynamicGridBuilder EnableAdvancedFilter(bool val) { _enableAdvancedFilter = val; return this; }
        public Eorc_DynamicGridBuilder EditJavaScriptFunction(string val) { _editJsFunctionName = val; return this; }
        public Eorc_DynamicGridBuilder EnableLazyLoading(bool val) { _enableLazyLoading = val; return this; }
        public Eorc_DynamicGridBuilder EditJavaScriptFunction(string funcName, object parameters)
        {
            _editJsFunctionName = funcName;

            if (parameters != null)
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    _editFunctionParams[prop.Name] = prop.GetValue(parameters)?.ToString() ?? "";
                }
            }

            return this;
        }
        public Eorc_DynamicGridBuilder DeleteJavaScriptFunction(string val) { _deleteJavaScriptFunction = val; return this; }
        public Eorc_DynamicGridBuilder PageSize(int val) { _pageSize = val; return this; }

        public IHtmlContent Build()
        {
            string html = "";
            string htmlError = string.Empty;

            // 🟥 1. بررسی خالی بودن گرید یا داده‌ها
            //if (_items == null || !_items.Any())
            //{
            //    htmlError += $"<div class='alert alert-danger'>هیچ داده‌ای برای نمایش در گرید ({_gridName ?? "بدون‌نام"}) وجود ندارد! (Items خالی یا null)</div>";
            //}

            // 🟥 2. بررسی نام گرید
            if (string.IsNullOrEmpty(_gridName))
            {
                htmlError += $"<div class='alert alert-danger'>نام گرید مشخص نشده است! Eorc_Grid(null value is not valid)</div>";
            }

            // 🟥 3. بررسی آدرس
            if (string.IsNullOrEmpty(_url))
            {
                htmlError += $"<div class='alert alert-danger'>آدرس گرید مشخص نشده است! : URL(null value is not valid) </div>";
            }

            // 🟥 4. بررسی تمام فیلدهای تنظیمات که مقدار ندارند
            if (_enablePaging == null) htmlError += "<div class='alert alert-danger'>گزینه _enablePaging مشخص نشده است!</div>";
            if (_enableEditButton == null) htmlError += "<div class='alert alert-danger'>گزینه _enableEditButton مشخص نشده است!</div>";
            if (_enableDeleteButton == null) htmlError += "<div class='alert alert-danger'>گزینه _enableDeleteButton مشخص نشده است!</div>";
            if (_enableFiltering == null) htmlError += "<div class='alert alert-danger'>گزینه _enableFiltering مشخص نشده است!</div>";
            if (_enableInlineEdit == null) htmlError += "<div class='alert alert-danger'>گزینه _enableInlineEdit مشخص نشده است!</div>";
            if (_enableSorting == null) htmlError += "<div class='alert alert-danger'>گزینه _enableSorting مشخص نشده است!</div>";
            if (_enableFooter == null) htmlError += "<div class='alert alert-danger'>گزینه _enableFooter مشخص نشده است!</div>";
            if (_enableGrouping == null) htmlError += "<div class='alert alert-danger'>گزینه _enableGrouping مشخص نشده است!</div>";
            if (_enableExcelExport == null) htmlError += "<div class='alert alert-danger'>گزینه _enableExcelExport مشخص نشده است!</div>";
            if (_enablePdfExport == null) htmlError += "<div class='alert alert-danger'>گزینه _enablePdfExport مشخص نشده است!</div>";
            if (_enablePrint == null) htmlError += "<div class='alert alert-danger'>گزینه _enablePrint مشخص نشده است!</div>";
            if (_enableShowHiddenColumns == null) htmlError += "<div class='alert alert-danger'>گزینه _enableShowHiddenColumns مشخص نشده است!</div>";
            if (_enableAdvancedFilter == null) htmlError += "<div class='alert alert-danger'>گزینه _enableAdvancedFilter مشخص نشده است!</div>";
            if (_pageSize % 5 != 0) htmlError += "<div class='alert alert-danger'>Page Size بايد مضربي از 5 باشد </div>";

            // 🟥 اگر هر خطایی وجود داشت — خروج
            if (htmlError.Length > 0)
            {
                return new HtmlString(htmlError);
            }

            // ✅ از اینجا به بعد کد اصلی گرید بدون تغییر ادامه دارد
            html = $"<div id='{_gridName}' class='eorc-dynamic-grid-container'>";
            html += $@"
            <div id='{_gridName}_gridData'
                 data-url='{_url ?? ""}'
                 data-enable-paging='{_enablePaging.ToString().ToLower()}'
                 data-edit-button='{(_enableEditButton ?? false).ToString().ToLower()}'
                 data-delete-button='{(_enableDeleteButton ?? false).ToString().ToLower()}'
                 data-edit-function-name='{_editJsFunctionName}'
                 data-delete-function='{_deleteJavaScriptFunction}'
                 data-lazy-loading='{(_enableLazyLoading).ToString().ToLower()}'
                 data-page-size='{_pageSize}'
            style='display:none;'";

            // اضافه کردن پارامترهای تابع Edit
            if (_editFunctionParams != null)
            {
                foreach (var kv in _editFunctionParams)
                {
                    html += $" data-edit-param-{kv.Key}='{kv.Value}'";
                }
            }

            html += "></div>";



            html += $"<div id='{_gridName}_gridContainerWrapper' class='gridContainerWrapper'>";
            html += "<div class='row controls controls-bar'>";

            if (!_enableLazyLoading && (_enablePaging.HasValue && _enablePaging.Value))
            {
                html += $"<div class='col-1' id='{_gridName}_grd-pageSizeSelector'>" +
                   $"<label>تعداد در هر صفحه:" +
                   $"<select id='{_gridName}_pageSizeSelector'>" +
                   $"<option value='5' {((_pageSize == 5 && !_enableLazyLoading)? "selected" : "")}>5</option>" +
                   $"<option value='10' {((_pageSize == 10 && !_enableLazyLoading )? "selected" : "")}>10</option>" +
                   $"<option value='15' {((_pageSize == 15 && !_enableLazyLoading )? "selected" : "")}>15</option>" +
                   $"<option value='20' {((_pageSize == 20 && !_enableLazyLoading )? "selected" : "")}>20</option>" +
                   $"<option value='25' {(_pageSize == 25 || _enableLazyLoading ? "selected" : "")}>25</option>" +
                   $"<option value='30' {((_pageSize == 30 && !_enableLazyLoading) ? "selected" : "")}>30</option>" +
                   $"</select>" +
                   $"</label></div>";
            }


            if (!_enableLazyLoading && (_enableGrouping.HasValue && _enableGrouping.Value))
            {
                //  گرفتن نوع داده‌ها — اگر آیتمی وجود ندارد از ModelType استفاده می‌کنیم
                var type = (_items != null && _items.Any())
                    ? _items.First().GetType()
                    : _modelType;

                if (type != null)
                {
                    var groupableColumns = type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(p => new
                        {
                            Prop = p,
                            Attr = p.GetCustomAttribute<GridColumnAttribute>()
                        })
                        // 🟩 پیش‌فرض EnableGrouping = true است
                        .Where(x => x.Attr != null && (x.Attr.EnableGrouping != false))
                        .ToList();

                    html += "<div class='col-2 groupby-wrapper'><label>گروه‌بندی بر اساس:";
                    html += $"<select id='{_gridName}_groupBySelector'>";
                    html += "<option value='' selected>— بدون گروه‌بندی —</option>";

                    foreach (var col in groupableColumns)
                    {
                        var header = col.Attr.DisplayName ?? col.Prop.Name;
                        html += $"<option value='{col.Prop.Name}'>{header}</option>";
                    }

                    html += "</select></label></div>";
                }
            }

            if (_enablePdfExport.HasValue && _enablePdfExport.Value)
            {
                html += $"<div class='col-md-1 grid-buttons-col'><button id='PdfGridBtn' onclick='exportGridToPdf(\"{_gridName}\")' class='grid-action-button-class'> خروجي pdf <i style='margin-right: 6px;font-size: 20px;' class='fa fa-file-pdf-o'></i> </button></div>";
            }
            if (_enableExcelExport.HasValue && _enableExcelExport.Value)
            {
                html += $"<div class='col-md-1 grid-buttons-col'><button id='ExcelGridBtn' onclick='exportGridToExcelXlsx(\"{_gridName}\")' class='grid-action-button-class'> خروجي اكسل<i style='margin-right: 6px;font-size: 20px;' class='fa fa-file-excel-o'></i> </button></div>";
            }
            if (_enablePrint.HasValue && _enablePrint.Value)
            {
                html += $"<div class='col-md-1 grid-buttons-col'> <button id='printGridBtn' onclick='printDynamicGrid(\"{_gridName}\")' class='grid-action-button-class'>پرینت  <i style='margin-right: 6px;font-size: 20px;' class='fa fa-print'></i></button></div>";
            }
            if (_enableShowHiddenColumns.HasValue && _enableShowHiddenColumns.Value)
            {
                html += $"<div class='col-md-1 grid-buttons-col'> <button id='displayGridColumns' onclick='displayGridColumns(\"{_gridName}\")' class='grid-action-button-class'>نمايش ستون<i style='margin-right: 6px;font-size: 20px;' class='fa fa-columns'></i></button></div>";
            }
            if (_enableAdvancedFilter.HasValue && _enableAdvancedFilter.Value)
            {
                html += $"<div class='col-md-1 grid-buttons-col'> <button id='displayGridColumns' onclick='displayAdvancedFilter(\"{_gridName}\")' class='grid-action-button-class'>فيلتر پيشرفته<i style='margin-right: 6px;font-size: 20px;' class='fa fa-search'></i></button></div>";
            }
            if (_customHtmlElements.Any())
            {
                foreach (var customHtml in _customHtmlElements)
                {
                    html += $"<div class='col-md-1 grid-buttons-col'>{customHtml}</div>";
                }
            }

            html += "</div>"; // controls

            html += $"<div id='{_gridName}_gridContainer' class='eorc-grid-container'>";

            // استخراج Propertyها
            PropertyInfo[] props;
            if (_items != null && _items.Any())
            {
                props = _items.First().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
            else if (_modelType != null)
            {
                props = _modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                props = new PropertyInfo[0];
            }

            // ساخت Metadata ستون‌ها از GridColumnAttribute
            var columnsMeta = props
                .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<GridColumnAttribute>() })
                .Where(x => x.Attr != null)
                .ToList();

            // Advanced Filter (اگر فعال است)
            if (_enableAdvancedFilter.HasValue && _enableAdvancedFilter.Value)
            {
                html += @$"
        <div id='advancedFilterPopup_{_gridName}' class='advanced-filter-popup' style='display:none;position:fixed;top:50%;left:50%;transform:translate(-50%, -50%);width:600px;max-width:90vw;min-width:400px;max-height:80vh;background:#fff;border:1px solid #ccc;border-radius:8px;box-shadow:0 4px 16px rgba(0,0,0,0.25);overflow-y:auto;padding:0;z-index:10000;'>
        <div class='popup-header' style='background:#3498db;color:#fff;font-weight:bold;padding:12px 16px;border-top-left-radius:8px;border-top-right-radius:8px;font-size:16px;'>
            فیلتر پیشرفته
            <span onclick='closeAdvancedFilter()' style='float:right;cursor:pointer;font-weight:bold;'>×</span>
        </div>
        <div class='popup-body' style='padding:12px;'>";
                foreach (var col in columnsMeta.Where(c => c.Attr.EnableFiltering))
                {
                    string fieldName = col.Prop.Name;
                    string header = col.Attr.DisplayName ?? fieldName;

                    html += $@"
        <div class='filter-row' style='margin-bottom:12px;display:flex;flex-direction:column;'>
            <label style='font-weight:bold;margin-bottom:4px;'>{header}</label>
            <div style='display:flex;gap:8px;'>
                <input type='text' id='from{fieldName}' name='from{fieldName}' placeholder='از {header}' class='form-control' />
                <input type='text' id='to{fieldName}' name='to{fieldName}' placeholder='تا {header}' class='form-control' />
            </div>
        </div>
        <hr style='margin:6px 0;' />";
                }
                html += @$"
        </div> <!-- پایان popup-body -->
        <div class='popup-footer' style='text-align:right;padding:12px;border-top:1px solid #ccc;background:#f9f9f9;border-bottom-left-radius:8px;border-bottom-right-radius:8px;'>
            <button type='button' onclick='applyAdvancedFilter(""{_gridName}"")' class='btn gridPopupBtnApplyGrid' style='margin-left:8px;'> <i class=""fa fa-check"" style=""margin-left:6px;""></i>اعمال فیلتر</button>
           <button type='button' onclick='closeAdvancedFilter(""{_gridName}"")' class='btn gridPopupBtnCancelGrid'>بستن   <i class=""fa fa-times"" style=""margin-left:6px;""></i> </button>
        </div>
    </div>";
            }

            // Header
            html += "<div class='grid-header'>";

            // ستون عملیات اول
            if ((_enableEditButton == true) || (_enableDeleteButton == true))
            {
                html += "<div class='grid-cell grid-cell-Actions'>عملیات</div>";
            }

            // سپس بقیه ستون‌ها
            foreach (var col in columnsMeta)
            {
                var style = col.Attr.Visible ? "" : "style='display:none;'";
                var className = col.Attr.Visible ? "grid-cell" : "grid-cell grid-cell-hidden";
                var sortIcons = (_enableSorting.HasValue && _enableSorting.Value) && col.Attr.EnableSorting ? "  <i class='fa fa-sort' style='margin-left: 7px;'> </i> " : "";
                html += $"<div class='{className}' {style} data-column='{col.Prop.Name}'>{sortIcons} {col.Attr.DisplayName}</div>";
            }

           
            html += "</div>";

            // Filters
            if (_enableFiltering == true)
            {
                html += "<div class='grid-filters'>";

                // اگر ستون عملیات اول است، یک div خالی برایش اضافه کنید تا جا پر شود
                if ((_enableEditButton == true) || (_enableDeleteButton == true))
                {
                    html += "<div class='grid-cell grid-cell-Buttons' style='pointer-events:none;'></div>";
                }

                foreach (var col in columnsMeta.Where(c => c.Attr.EnableFiltering))
                {
                    var style = col.Attr.Visible ? "" : "style='display:none;'";
                    var className = col.Attr.Visible ? "grid-cell" : "grid-cell grid-cell-hidden";

                    html += $"<div class='{className}' {style} data-column='{col.Prop.Name}' style='position:relative;'>" +
                            $"<input type='text' class='filter-input' data-prop='{col.Prop.Name}' placeholder='جستجو' />" +
                            $"<span class='filter-icon'>&#128269;</span>" +
                            $"<ul class='eorc-grid-filter-menu'>" +
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
                html += "</div>";
            }

            // Body
            html += $"<div id='{_gridName}_grid-body' class='grid-body'>";
            if (_items.Any())
            {
                foreach (var item in _items)
                {
                    html += "<div class='grid-row'>";

                    // ستون عملیات اول
                    if ((_enableEditButton == true) || (_enableDeleteButton == true))
                    {
                        html += "<div class='grid-cell grid-cell-Buttons'>";
                        if (_enableEditButton == true) html += "<button class='btn primary edit-btn'>ویرایش</button>";
                        if (_enableDeleteButton == true) html += "<button class='btn danger delete-btn'>حذف</button>";
                        html += "</div>";
                    }

                    foreach (var col in columnsMeta)
                    {
                        var value = System.Net.WebUtility.HtmlEncode(col.Prop.GetValue(item)?.ToString() ?? "");
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        html += $"<div class='grid-cell' data-cell='{col.Prop.Name}' {style}>{value}</div>";
                    }

                    html += "</div>";
                }
            }
            else
            {
                // ردیف خالی برای اسکلت گرید
                html += "<div class='grid-row'>";
                foreach (var col in columnsMeta)
                {
                    var style = col.Attr.Visible ? "" : "style='display:none;'";
                    html += $"<div class='grid-cell' data-cell='{col.Prop.Name}' {style}></div>";
                }
                if ((_enableEditButton == true) || (_enableDeleteButton == true))
                {
                    html += "<div class='grid-cell grid-cell-Buttons'></div>";
                }
                html += "</div>";
            }
            html += "</div>"; // پایان grid-body

            // Footer (همانند قبل)
            if (_enableFooter == true)
            {
                html += "<div class='grid-footer'><div class='grid-row footer-row'>";

                // ستون عملیات اول
                if ((_enableEditButton == true) || (_enableDeleteButton == true))
                {
                    html += "<div class='grid-cell grid-cell-Buttons'></div>";
                }

                foreach (var col in columnsMeta)
                {
                    var style = col.Attr.Visible ? "" : "style='display:none;'";
                    bool isNumeric = col.Prop.PropertyType == typeof(int) ||
                                     col.Prop.PropertyType == typeof(double) ||
                                     col.Prop.PropertyType == typeof(decimal) ||
                                     col.Prop.PropertyType == typeof(float) ||
                                     col.Prop.PropertyType == typeof(long);

                    string footerValue = "";
                    if (isNumeric && _items.Any())
                    {
                        var sum = _items.Sum(item => Convert.ToDecimal(col.Prop.GetValue(item) ?? 0));
                        footerValue = sum.ToString("N0");
                    }

                    html += $"<div class='grid-cell disabled' data-footer={col.Prop.Name} {style}>" +
                            $"<input type='text' class='footer-input' value='{footerValue}' readonly />" +
                            (isNumeric ?
                                "<span class='footer-icon' data-icon-id='calc'>Σ</span>" +
                                "<ul class='eorc-grid-footer-menu' style='display: none;'>" +
                                " <li data-calc='sum'>➕ جمع</li>" +
                                " <li data-calc='avg'>📊 میانگین</li>" +
                                " <li data-calc='count'>🔢 تعداد</li>" +
                                " <li data-calc='max'>⬆️ بیشترین</li>" +
                                " <li data-calc='min'>⬇️ کمترین</li>" +
                                "</ul>" : "") +
                            "</div>";
                }

                html += "</div></div>";
            }


            var columnsJson = props.Select(p =>
            {
                var attr = p.GetCustomAttribute<GridColumnAttribute>();
                return new
                {
                    prop = p.Name,
                    header = attr?.DisplayName ?? p.Name,
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
            html += $"<script id='{_gridName}_gridDataLocal' type='application/json'>{safeJson}</script>";
            html += $"<script id='{_gridName}_gridEnableSorting' type='application/json'>{_enableSorting}</script>";

            html += "</div>"; // پایان gridContainer
            html += "</div>"; // پایان gridContainerWrapper

            // Paging
            if (!_enableLazyLoading && (_enablePaging.HasValue && _enablePaging.Value))
            {
                html += @"<div class='pagination'>
                    <button id='prevPage' class='btn pagination-btn'>قبلی<svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke=' currentColor' stroke-width='2'><polyline points='9 6 15 12 9 18'></polyline></svg></button>
                    <span id='pageInfo'></span>
                    <button id='nextPage' class='btn pagination-btn'>بعدی<svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><polyline points='15 18 9 12 15 6'></polyline></svg></svg></button>
                </div>";
            }

            // 🔹 اضافه کردن JS مخصوص State هر گرید
                            html += $@"
                <script>
                    // اطمینان از وجود آبجکت اصلی
                    window.Grids = window.Grids || {{}};

                    // وضعیت گرید {_gridName}
                    window.Grids['{_gridName}'] = {{
                        currentPage: 1,
                        pageSize: {_pageSize},
                        totalPage: 0,
                        sortColumn: '',
                        sortAsc: true,
                        enablePaging: {(_enablePaging.HasValue && _enablePaging.Value).ToString().ToLower()},
                        customRequestBody: {{}}
                    }};

                    
                </script>";



            html += "</div>"; // پایان eorc-dynamic-grid-container

            return new HtmlString(html);
        }
    }

    public static class DynamicGridExtensions
    {
        // اگر میخوای مستقیماً با مدل و نام گرید صدا بزنی
        public static Eorc_DynamicGridBuilder Eorc_Grid<T>(string gridName, string customHtml = null)
        {
            var builder = new Eorc_DynamicGridBuilder()
                              .ModelType<T>(gridName);

            if (!string.IsNullOrWhiteSpace(customHtml))
            {
                builder.AddCustomHtml(customHtml);
            }

            return builder;
        }


    }
}
