using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using System;

namespace YourProject.TagHelpers
{
    [HtmlTargetElement("gridview-infinite-scroll")]
    public class GridViewInfiniteScrollTagHelper : TagHelper
    {
        public IEnumerable<object> Items { get; set; } = new List<object>();
        public string[] Columns { get; set; } = new string[0];
        public bool EnableFiltering { get; set; } = true;
        public bool EnableSorting { get; set; } = true;
        public bool EnableGrouping { get; set; } = true;
        public string FetchUrl { get; set; } = "";
        public Type ModelType { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "dynamic-grid-container");

            var columnMeta = new List<(string Name, string Header, bool Visible)>();

            if (Items?.Any() == true)
            {
                var type = Items.First().GetType();
                foreach (var p in type.GetProperties())
                {
                    var attr = (GridColumnAttribute?)Attribute.GetCustomAttribute(p, typeof(GridColumnAttribute));
                    string header = attr?.DisplayName ?? p.Name;
                    bool visible = attr?.Visible ?? true;
                    columnMeta.Add((p.Name, header, visible));
                }
            }
            else if (ModelType != null)
            {
                foreach (var p in ModelType.GetProperties())
                {
                    var attr = (GridColumnAttribute?)Attribute.GetCustomAttribute(p, typeof(GridColumnAttribute));
                    string header = attr?.DisplayName ?? p.Name;
                    bool visible = attr?.Visible ?? true;
                    columnMeta.Add((p.Name, header, visible));
                }
            }
            else
            {
                columnMeta = Columns.Select(c => (c, c, true)).ToList();
            }

            var html = "<h3>گرید داینامیک</h3>";

            if (EnableFiltering || EnableGrouping)
            {
                html += "<div class='controls'>";
                if (EnableGrouping)
                {
                    html += "<div class='left'><label>گروه‌بندی بر اساس:";
                    html += "<select id='groupBySelector'><option value=''>— بدون گروه‌بندی —</option>";
                    foreach (var col in columnMeta.Where(c => c.Visible))
                        html += $"<option value='{col.Name}'>{col.Header}</option>";
                    html += "</select></label></div>";
                }

                if (EnableFiltering)
                {
                    html += $@"<div class='right'>
                        <div id='gridSummary'>در حال بارگذاری...</div>
                        <button id='refreshBtn' class='btn primary'>تازه‌سازی</button>
                    </div>";
                }

                html += "</div>"; // controls
            }

            // grid-wrapper با overflow-x مشترک
            html += "<div class='grid-wrapper' id='gridContainer'>";

            // header
            html += "<div class='grid-header'>";
            foreach (var col in columnMeta.Where(c => c.Visible))
                html += $"<div data-column='{col.Name}'>{col.Header} ▲▼</div>";
            html += "<div>عملیات</div></div>";

            // filters
            if (EnableFiltering)
            {
                html += "<div class='grid-filters'>";
                foreach (var col in columnMeta.Where(c => c.Visible))
                    html += $"<input type='text' placeholder='جستجو {col.Header}' id='filter_{col.Name}' />";
                html += "</div>";
            }

            // rows container با overflow-y فقط
            html += "<div class='grid-rows'></div>";
            html += "</div>"; // grid-wrapper

            if (!string.IsNullOrEmpty(FetchUrl))
                html += $"<input type='hidden' id='fetchUrl' value='{FetchUrl}' />";

            // خروجی hiddenColumns برای JS
            var hiddenCols = columnMeta.Where(c => !c.Visible).Select(c => c.Name).ToList();
            html += $"<script>var hiddenColumns = {System.Text.Json.JsonSerializer.Serialize(hiddenCols)};</script>";

            output.Content.SetHtmlContent(html);
        }
    }
}
