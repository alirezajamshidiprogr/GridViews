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
            html += $"<div id='gridSettings' data-enable-paging='{EnablePaging.ToString().ToLower()}'></div>";
            

            // Wrapper کلی
            html += "<div id='gridContainerWrapper'>";

            // Controls گروه‌بندی
            if (EnableGrouping)
            {
                html += "<div class='controls'>";
                html += $"<div id='grd-pageSizeSelector'>" +
                $"<label>تعداد در هر صفحه:" +
                $"<select id='pageSizeSelector'>" +
                $"<option value='5'>5</option>" +
                $"<option value='10' selected>10</option>" +
                $"<option value='20'>20</option>" +
                $"</select>" +
                $"</label></div>";
                html += "<div class='left'><label>گروه‌بندی بر اساس:";
                html += "<select id='groupBySelector'><option value=''>— بدون گروه‌بندی —</option></select>";
                html += "</label></div>";
                html += "</div>";
            }

            // div اصلی gridContainer که JS روی آن کار می‌کند
            html += "<div id='gridContainer' class='grid-container'>";

            //if (Items == null || !Items.Any())
            //{
            //    //html += "<div>هیچ داده‌ای وجود ندارد.</div>";
            //}
            //else
            //{
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
                        html += $"<div class='grid-cell' data-cell={col.Prop.Name} {style}>{value}</div>";
                    }
                    html += "<div class='grid-cell'><button class='btn primary edit-btn'>ویرایش ...</button>" +
                            "<button class='btn danger delete-btn'>حذف ...</button></div>";
                    html += "</div>"; // پایان grid-row
                }
                html += "</div>"; // پایان grid-body

            // Footer (دقیقاً بعد از grid-body)
            if (EnableFooter)
            {
                html += "<div class='grid-footer'>";
                html += "<div class='grid-row footer-row'>";

                foreach (var col in columnsMeta)
                {
                    var style = col.Attr.Visible ? "" : "style='display:none;'";
                    bool isNumeric = false;
                    var propType = col.Prop.PropertyType;

                    if (propType == typeof(int) || propType == typeof(double) ||
                        propType == typeof(decimal) || propType == typeof(float) ||
                        propType == typeof(long))
                    {
                        isNumeric = true;
                    }
                    else if (propType == typeof(string))
                    {
                        // فقط اگر همه مقادیر ستون کاملاً عدد باشند و شامل / یا حروف نشوند
                        isNumeric = Items.All(item =>
                        {
                            var value = col.Prop.GetValue(item)?.ToString();
                            return !string.IsNullOrEmpty(value) &&
                                   value.All(c => char.IsDigit(c)); // فقط ارقام
                        });
                    }

                    string footerValue = "";
                    if (isNumeric)
                    {
                        var sum = Items.Sum(item =>
                        {
                            var value = col.Prop.GetValue(item)?.ToString();
                            return decimal.TryParse(value, out var num) ? num : 0;
                        });
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
                                "</ul>"
                                : ""
                            ) +
                            "</div>";
                }

                html += "<div class='grid-cell'></div>";
                html += "</div>";
                html += "</div>";
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
            //}

            html += "</div>"; // پایان gridContainer

            html += "</div>"; // پایان gridContainerWrapper

            // Paging
            if (EnablePaging)
            {
                html += @"<div class='pagination'>
                    <button id='prevPage' class='btn pagination-btn'>قبلی<svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke=' currentColor' stroke-width='2'><polyline points='9 6 15 12 9 18'></polyline></svg></button>
                    <span id='pageInfo'></span>
                    <button id='nextPage' class='btn pagination-btn'>بعدی <svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><polyline points='15 18 9 12 15 6'></polyline></svg></svg></button>
                </div>";


            }



        

            output.Content.SetHtmlContent(html);
        }
    }
}
