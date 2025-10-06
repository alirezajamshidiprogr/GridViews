using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace YourProject.TagHelpers
{
    [HtmlTargetElement("dynamic-grid")]
    public class DynamicGridWithFilter_Sort_Grouping_Paging_TagHelper : TagHelper
    {
        public List<object> Items { get; set; } = new List<object>();
        public bool EnablePaging { get; set; } = true;
        public bool EnableFiltering { get; set; } = true;
        public bool EnableSorting { get; set; } = true;
        public bool EnableFooter { get; set; } = false;
        public bool EnableGrouping { get; set; } = true;
        [HtmlAttributeName("url")]
        public string DataUrl { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "dynamic-grid-container");

            var html = "<h3>گرید داینامیک</h3>";
            html += $"<div id='gridData' data-url='{DataUrl ?? ""}' style='display:none;'></div>";

            // Wrapper کلی
            html += "<div id='gridContainerWrapper'>";

            // Controls گروه‌بندی
            if (EnableGrouping)
            {
                html += "<div class='controls'>";
                html += "<div class='left'><label>گروه‌بندی بر اساس:";
                html += "<select id='groupBySelector'><option value=''>— بدون گروه‌بندی —</option></select>";
                html += "</label></div>";
                html += "</div>";
            }

            // div اصلی gridContainer که JS روی آن کار می‌کند
            html += "<div id='gridContainer' class='grid-container'>";

            if (Items == null || !Items.Any())
            {
                html += "<div>هیچ داده‌ای وجود ندارد.</div>";
            }
            else
            {
                var firstItem = Items.First();
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
                    var sortIcons = EnableSorting && col.Attr.EnableSorting ? " ▲▼" : "";
                    html += $"<div class='grid-cell' {style} data-column='{col.Prop.Name}'>{col.Attr.Header}{sortIcons}</div>";
                }
                html += "<div class='grid-cell'>عملیات</div>";
                html += "</div>"; // پایان grid-header

                // Filters زیر Header
                if (EnableFiltering)
                {
                    html += "<div class='grid-filters'>";
                    foreach (var col in columnsMeta)
                    {
                        if (!col.Attr.EnableFiltering) continue;
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        html += $"<div class='grid-cell' {style} data-column='{col.Prop.Name}'>" +
                                $"<input type='text'  class='filter-input' data-prop='{col.Prop.Name}' placeholder='جستجو {col.Attr.Header}' />" +
                                "</div>";
                    }
                    html += "<div class='grid-cell'></div>"; // ستون عملیات خالی
                    html += "</div>"; // پایان grid-filters
                }

                // Body
                html += "<div class='grid-body'>";
                foreach (var item in Items)
                {
                    html += "<div class='grid-row'>";
                    foreach (var col in columnsMeta)
                    {
                        var value = col.Prop.GetValue(item)?.ToString() ?? "";
                        var style = col.Attr.Visible ? "" : "style='display:none;'";
                        html += $"<div class='grid-cell' data-column='{col.Prop.Name}' {style}>{value}</div>";
                    }
                    html += "<div class='grid-cell'><button class='btn primary edit-btn'>ویرایش</button> " +
                            "<button class='btn danger delete-btn'>حذف</button></div>";
                    html += "</div>"; // پایان grid-row
                }
                html += "</div>"; // پایان grid-body

                // JSON برای JS
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
                    items = Items.Select(i => i.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .ToDictionary(p => p.Name, p => p.GetValue(i))),
                    columns = columnsJson
                });
                html += $"<script id='gridDataLocal' type='application/json'>{jsonData}</script>";
            }

            html += "</div>"; // پایان gridContainer

            html += "</div>"; // پایان gridContainerWrapper

            // Paging
            if (EnablePaging)
            {
                html += @"<div class='pagination'>
                    <label>تعداد در هر صفحه:
                        <select id='pageSizeSelector'>
                            <option value='5'>5</option>
                            <option value='10' selected>10</option>
                            <option value='20'>20</option>
                        </select>
                    </label>
                    <button id='prevPage' class='btn pagination-btn'>قبلی<svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke=' currentColor' stroke-width='2'><polyline points='9 6 15 12 9 18'></polyline>
</svg></button>
                    <span id='pageInfo'></span>
                    <button id='nextPage' class='btn pagination-btn'>بعدی <svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'>
            <polyline points='15 18 9 12 15 6'></polyline>
        </svg></svg></button>
                </div>";
            }

            // Paging
            if (EnableFooter)
            {
                html += @"<div class='footer'>

                </div>";
            }

            output.Content.SetHtmlContent(html);
        }
    }
}
