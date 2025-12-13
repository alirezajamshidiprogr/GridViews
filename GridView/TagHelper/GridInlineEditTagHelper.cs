using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

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

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("class", "dynamic-grid-container");
            output.Attributes.SetAttribute("fetch-url", FetchUrl);

            var columnMeta = new List<object>();
            Type type = Items?.Any() == true ? Items.First().GetType() : ModelType;

            if (type != null)
            {
                foreach (var prop in type.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<GridColumnAttribute>();

                    // تبدیل نام پراپرتی به camelCase
                    var camelName = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);

                    columnMeta.Add(new
                    {
                        Name = prop.Name, // بدون تغییر به camelCase
                        Header = attr?.DisplayName ?? prop.Name,
                        Visible = attr?.Visible ?? true,
                        Editable = attr?.Editable ?? false
                    });
                }
            }

            var columnsJson = JsonSerializer.Serialize(columnMeta);

            var html = $@"
<div class='grid-wrapper'>
    <div class='controls'>
        <div class='left'>
            <label>گروه‌بندی: 
                <select id='groupBySelector'><option value=''>بدون گروه‌بندی</option></select>
            </label>
        </div>
        <div class='right'>
            <button id='refreshBtn' class='btn primary'>تازه‌سازی</button>
        </div>
    </div>

    <div class='grid-table'>
        <div class='grid-header'></div>
        {(EnableFiltering ? "<div class='grid-filters'></div>" : "")}
        <div class='grid-rows' id='gridRows'></div>
    </div>

    {(EnablePaging ? "<div class='pagination' id='pagination'></div>" : "")}
</div>

<script>
window.columnMeta = {columnsJson};
window.enablePaging = {EnablePaging.ToString().ToLower()};
window.enableFiltering = {EnableFiltering.ToString().ToLower()};
window.enableSorting = {EnableSorting.ToString().ToLower()};
window.enableGrouping = {EnableGrouping.ToString().ToLower()};
window.fetchUrl = '{FetchUrl}';
</script>
<script src='/js/grid-inline-edit.js'></script>
<link href='/css/grid-inline-edit.css' rel='stylesheet' />
";

            output.Content.SetHtmlContent(html);
        }
    }
}
