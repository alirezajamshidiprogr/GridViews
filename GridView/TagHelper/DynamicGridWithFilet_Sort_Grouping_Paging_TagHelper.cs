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
        private string _gridName;
        private string _url;
        private bool _enablePaging = true;
        private bool _enableFiltering = true;
        private bool _enableSorting = true;
        private bool _enableFooter = false;
        private bool _enableGrouping = true;
        private bool _enableExcelExport = true;
        private bool _enablePrint = true;
        private bool _enableShowHiddenColumns = true;

        public DynamicGridBuilder Items(List<object> items) { _items = items; return this; }
        public DynamicGridBuilder GridName(string name) { _gridName = name; return this; }
        public DynamicGridBuilder Url(string url) { _url = url; return this; }
        public DynamicGridBuilder EnablePaging(bool val) { _enablePaging = val; return this; }
        public DynamicGridBuilder EnableFiltering(bool val) { _enableFiltering = val; return this; }
        public DynamicGridBuilder EnableSorting(bool val) { _enableSorting = val; return this; }
        public DynamicGridBuilder EnableFooter(bool val) { _enableFooter = val; return this; }
        public DynamicGridBuilder EnableGrouping(bool val) { _enableGrouping = _enablePaging ? false : val; return this; }
        public DynamicGridBuilder EnableExcelExport(bool val) { _enableExcelExport = val; return this; }
        public DynamicGridBuilder EnablePrint(bool val) { _enablePrint = val; return this; }
        public DynamicGridBuilder EnableShowHiddenColumns(bool val) { _enableShowHiddenColumns = val; return this; }

        public IHtmlContent Build()
        {
            string html = "";
            if (string.IsNullOrEmpty(_url))
            {
                return new HtmlString("براي گريد آدرس دهي اوليه نشده است !");
            }
            if (string.IsNullOrEmpty(_gridName))
            {
                return new HtmlString("نامي براي گريد خود انتخاب كنيد");
            }

            html = $"<div id='{_gridName}' class='dynamic-grid-container'>";

            html += $"<div id='gridData' data-url='{_url ?? ""}' style='display:none;'></div>";

            html += $"<div id='gridSettings' data-enable-paging='{_enablePaging.ToString().ToLower()}'></div>";

            html += "<div id='gridContainerWrapper'>";
            html += "<div class='row controls controls-bar'>";
            if (_enablePaging)
            {
                html += $"<div class='col-2' id='grd-pageSizeSelector'>" +
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


            if (_enableGrouping)
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

            if (_enableExcelExport)
            {
                html += "<div class='col-1' style='max-width:400px;'><button id='ExcelGridBtn' onclick='exportGridToExcelXlsx()' class='btn btn-primary full-width-btn'> خروجي اكسل<i style='margin-right: 6px;font-size: 20px;' class='fa fa-file-excel-o'></i> </button></div>";
            }
            if (_enablePrint)
            {
                html += "<div class='col-1' style='max-width:400px;'> <button id='printGridBtn' onclick='printDynamicGrid()' class='btn btn-primary full-width-btn'>پرینت  <i style='margin-right: 6px;font-size: 20px;' class='fa fa-print'></i></button></div>";
            } 
            if (_enableShowHiddenColumns)
            {
                html += "<div class='col-1' style='max-width:400px;'> <button id='displayGridColumns' onclick='displayGridColumns()' class='btn btn-primary full-width-btn'>نمايش ستون<i style='margin-right: 6px;font-size: 20px;' class='fa fa-columns'></i></button></div>";
            }

            html += "</div>"; // controls

            html += "<div id='gridContainer' class='grid-container'>";

            if (_items.Any())
            {
                var firstItem = _items.First();
                var props = firstItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var columnsMeta = props
                    .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<GridColumnAttribute>() })
                    .Where(x => x.Attr != null)
                    .ToList();

                // Header
                html += "<div class='grid-header'>";
                foreach (var col in columnsMeta)
                {
                    var style = col.Attr.Visible ? "" : "style='display:none;'";
                    var sortIcons = _enableSorting && col.Attr.EnableSorting ? " ▲▼" : "";
                    html += $"<div class='grid-cell' {style} data-column='{col.Prop.Name}'>{col.Attr.Header}{sortIcons}</div>";
                }
                html += "<div class='grid-cell'>عملیات</div>";
                html += "</div>";

                // Filters
                if (_enableFiltering)
                {
                    html += "<div class='grid-filters'>";
                    foreach (var col in columnsMeta)
                    {
                        if (!col.Attr.EnableFiltering) continue;
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        html += $"<div class='grid-cell' {style} data-column='{col.Prop.Name}' style='position:relative;'>" +
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
                    html += "<div class='grid-cell'></div>";
                    html += "</div>";
                }

                // Body
                html += "<div class='grid-body'>";
                foreach (var item in _items)
                {
                    html += "<div class='grid-row'>";
                    foreach (var col in columnsMeta)
                    {
                        var value = col.Prop.GetValue(item)?.ToString() ?? "";
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        html += $"<div class='grid-cell' data-cell={col.Prop.Name} {style}>{value}</div>";
                    }
                    html += "<div class='grid-cell'><button class='btn primary edit-btn'>ویرایش</button>" +
                            "<button class='btn danger delete-btn'>حذف</button></div>";
                    html += "</div>";
                }
                html += "</div>";

                // Footer
                if (_enableFooter)
                {
                    html += "<div class='grid-footer'><div class='grid-row footer-row'>";
                    foreach (var col in columnsMeta)
                    {
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        bool isNumeric = col.Prop.PropertyType == typeof(int) || col.Prop.PropertyType == typeof(double) ||
                                         col.Prop.PropertyType == typeof(decimal) || col.Prop.PropertyType == typeof(float) ||
                                         col.Prop.PropertyType == typeof(long);

                        string footerValue = "";
                        if (isNumeric)
                        {
                            var sum = _items.Sum(item => Convert.ToDecimal(col.Prop.GetValue(item) ?? 0));
                            footerValue = sum.ToString("N0");
                        }

                        html += $"<div class='grid-cell disabled' data-footer={col.Prop.Name} {style}>" +
                                $"<input type='text' class='footer-input' placeholder='نوع عمليات' value='{footerValue}' readonly />" +
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
                    html += "<div class='grid-cell'></div></div></div>";
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

                html += $"<script id='gridDataLocal' type='application/json'>{jsonData}</script>";
            }

            html += "</div>"; // پایان gridContainer
            html += "</div>"; // پایان gridContainerWrapper

            // Paging
            if (_enablePaging)
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
        public static DynamicGridBuilder Jamshidi_Grid() => new DynamicGridBuilder();
    }
}
