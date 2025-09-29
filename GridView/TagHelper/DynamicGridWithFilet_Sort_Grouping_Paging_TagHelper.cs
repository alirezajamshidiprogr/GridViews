using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace YourProject.TagHelpers
{
    [HtmlTargetElement("dynamic-grid")]
    public class DynamicGridWithFilet_Sort_Grouping_Paging_TagHelper : TagHelper
    {
        public List<object> Items { get; set; } = new List<object>();
        public bool EnablePaging { get; set; } = true;
        public bool EnableFiltering { get; set; } = true;
        public bool EnableSorting { get; set; } = true;
        public bool EnableGrouping { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "dynamic-grid-container");

            var html = "<h3>گرید داینامیک</h3>";

            if (Items == null || !Items.Any())
            {
                html += "<div>هیچ داده‌ای وجود ندارد.</div>";
                output.Content.SetHtmlContent(html);
                return;
            }

            var firstItem = Items.First();
            var props = firstItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Controls
            if (EnableFiltering || EnableGrouping)
            {
                html += "<div class='controls'>";
                if (EnableGrouping)
                {
                    html += "<div class='left'><label>گروه‌بندی بر اساس:";
                    html += "<select id='groupBySelector'><option value=''>— بدون گروه‌بندی —</option>";
                    foreach (var p in props)
                        html += $"<option value='{p.Name}'>{p.Name}</option>";
                    html += "</select></label></div>";
                }

                if (EnableFiltering)
                {
                    html += @"<div class='right'>
                        <div id='gridSummary'>در حال بارگذاری...</div>
                        <button id='refreshBtn' class='btn primary'>تازه‌سازی</button>
                    </div>";
                }
                html += "</div>";
            }

            // جدول
            html += "<div class='grid-container' id='gridContainer'>";
            html += "<div class='grid-header'>";
            foreach (var p in props)
                html += $"<div class='grid-cell' data-column='{p.Name}'>{p.Name} ▲▼</div>";
            html += "<div class='grid-cell'>عملیات</div></div>";

            if (EnableFiltering)
            {
                html += "<div class='grid-filters'>";
                foreach (var p in props)
                    html += $"<input type='text' placeholder='جستجو {p.Name}' id='filter{p.Name}' />";
                html += "</div>";
            }

            // داده‌ها به صورت JSON
            var jsonData = JsonSerializer.Serialize(Items);
            html += $"<script id='gridData' type='application/json'>{jsonData}</script>";

            html += "</div>"; // پایان grid-container

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
                    <button id='prevPage' class='btn'>قبلی</button>
                    <span id='pageInfo'></span>
                    <button id='nextPage' class='btn'>بعدی</button>
                </div>";
            }

            output.Content.SetHtmlContent(html);
        }
    }
}
