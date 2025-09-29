using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YourProject.TagHelpers
{
    [HtmlTargetElement("grid-inline-edit")]
    public class GridInlineEditTagHelper : TagHelper
    {
        public IEnumerable<object> Items { get; set; } = new List<object>();
        public string FetchUrl { get; set; } = "";
        public bool EnablePaging { get; set; } = true;
        public bool EnableFiltering { get; set; } = true;
        public bool EnableSorting { get; set; } = true;
        public bool EnableGrouping { get; set; } = true;
        public Type ModelType { get; set; }
        //
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // **حفظ تگ اصلی**
            // output.TagName = "div"; // حذف شد

            output.Attributes.SetAttribute("class", "dynamic-grid-container");
            output.Attributes.SetAttribute("fetch-url", FetchUrl); // fetch-url مستقیم

            var columnMeta = new List<(string Name, string Header, bool Visible)>();
            if (Items?.Any() == true)
            {
                var type = Items.First().GetType();
                foreach (var p in type.GetProperties())
                {
                    columnMeta.Add((p.Name, p.Name, true));
                }
            }
            else if (ModelType != null)
            {
                foreach (var p in ModelType.GetProperties())
                {
                    columnMeta.Add((p.Name, p.Name, true));
                }
            }

            var html = @"<div class='controls'>
                            <div class='left'>
                                <label>گروه‌بندی: 
                                    <select id='groupBySelector'><option value=''>بدون گروه‌بندی</option></select>
                                </label>
                            </div>
                            <div class='right'>
                                <button id='refreshBtn' class='btn primary'>تازه‌سازی</button>
                            </div>
                        </div>";

            html += "<div class='grid-wrapper'><div class='grid-table'>";

            // Header
            html += "<div class='grid-header'>";
            foreach (var col in columnMeta)
                html += $"<div data-column='{col.Name}'>{col.Header} ▲▼</div>";
            html += "<div>عملیات</div></div>";

            // Filters
            if (EnableFiltering)
            {
                html += "<div class='grid-filters'>";
                foreach (var col in columnMeta)
                    html += $"<input id='filter_{col.Name}' placeholder='جستجو {col.Header}' />";
                html += "<div></div></div>";
            }

            html += "<div class='grid-rows' id='gridRows'></div>";
            html += "</div></div>"; // grid-table + wrapper

            if (EnablePaging)
                html += "<div class='pagination' id='pagination'></div>";

            // JS config (می‌توانید این خط را هم نگه دارید)
            html += $"<script>var enablePaging={EnablePaging.ToString().ToLower()}; var enableFiltering={EnableFiltering.ToString().ToLower()}; var enableSorting={EnableSorting.ToString().ToLower()}; var enableGrouping={EnableGrouping.ToString().ToLower()};</script>";

            output.Content.SetHtmlContent(html);
        }
    }
}
