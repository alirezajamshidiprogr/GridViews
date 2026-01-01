using Microsoft.AspNetCore.Html;
using System.Text;
using GridView.ViewModel;

namespace GridView.TagHelper
{
    public class GenericCollapsibleFormBuilder
    {
        private readonly string _id;
        private readonly List<HtmlElement> _elements = new();
        private int _columns = 1;
        private string _title = "Form";
        private bool _enableValidation = true;

        public GenericCollapsibleFormBuilder(string id)
        {
            _id = id;
        }

        public GenericCollapsibleFormBuilder Title(string title) { _title = title; return this; }
        public GenericCollapsibleFormBuilder Columns(int cols) { _columns = Math.Max(1, cols); return this; }
        public GenericCollapsibleFormBuilder EnableValidation(bool enable = true) { _enableValidation = enable; return this; }
        public GenericCollapsibleFormBuilder AddElements(params HtmlElement[] elements) { _elements.AddRange(elements); return this; }

        public IHtmlContent Build()
        {
            var sb = new StringBuilder();

            // Card Header & Body
            sb.AppendLine($@"
<div class='card mb-3' id='{_id}' style='direction:rtl;text-align:right;'>
    <div class='card-header' style='cursor:pointer;' onclick=""toggleCollapse_{_id}()"">
        <h5 class='mb-0'>{_title}</h5>
    </div>
    <div class='card-body collapse show' id='body_{_id}' dir='rtl'>
");

            // Body: ستون‌بندی
            if (_columns <= 1)
            {
                foreach (var el in _elements)
                {
                    sb.AppendLine($@"<div class='col-12 mb-3'>{el.Render()}</div>");
                }
            }
            else
            {
                sb.AppendLine("<div class='row'>");

                int count = 0;
                foreach (var el in _elements)
                {
                    string colClass = !string.IsNullOrWhiteSpace(el.ColClass)
                                        ? el.ColClass
                                        : $"col-md-{12 / _columns}";

                                    sb.AppendLine($@"
                        <div class='{colClass} mb-3'>
                            {el.Render()}
                        </div>
                    ");

                    count++;
                    if (string.IsNullOrWhiteSpace(el.ColClass) &&
                        count % _columns == 0 &&
                        count < _elements.Count)
                    {
                        sb.AppendLine("</div><div class='row'>");
                    }
                }

                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div></div>"); // Close card-body & card

            // JS
            sb.AppendLine($@"
<script>
function toggleCollapse_{_id}() {{
    var body = document.getElementById('body_{_id}');
    if(!body) return;
    body.classList.toggle('show');
}}

function getInputValuesForm_{_id}() {{
    var container = document.getElementById('body_{_id}');
    if(!container) return {{}};
    var inputs = container.querySelectorAll('input, select, textarea');
    var model = {{}};
    inputs.forEach(function(input){{
        var key = input.name || input.id;
        if(!key) return;
        if(input.type==='checkbox') model[key] = input.checked;
        else if(input.type==='radio') {{ if(input.checked) model[key] = input.value; }}
        else if(input.tagName.toLowerCase()==='select' && input.multiple) model[key]=Array.from(input.selectedOptions).map(o=>o.value);
        else model[key]=input.value;
    }});
    return model;
}}

// Validation
function validateField(field) {{
    if(!field.checkValidity()) {{
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
        return false;
    }}
    field.classList.remove('is-invalid');
    field.classList.add('is-valid');
    return true;
}}

// --- Auto-validation on input ---
document.addEventListener('DOMContentLoaded', function() {{
    {(_enableValidation ? $@"
    var container = document.getElementById('{_id}');
    if(container){{
        var fields = container.querySelectorAll('input[isrequired], textarea[isrequired], select[isrequired]');
        fields.forEach(function(field){{
            field.addEventListener('blur', function() {{ validateField(field); }});
            field.addEventListener('input', function() {{
                if(field.classList.contains('is-invalid')) validateField(field);
            }});
        }});
    }}
    " : "// Validation disabled")}
}});

function isValidForm_{_id}() {{
    var container = document.getElementById('body_{_id}');
    if(!container) return false;
    var fields = container.querySelectorAll('input[isrequired], select[isrequired], textarea[isrequired]');
    var valid = true;
    fields.forEach(f=>{{ if(!validateField(f)) valid=false; }});
    return valid;
}}

function submitForm_myForm(modalId) {{
debugger 
    var container = document.getElementById(modalId);
    if (!container) return;

    // Validation
    var fields = container.querySelectorAll('input[isrequired], textarea[isrequired], select[isrequired]');
    var formIsValid = true;

    fields.forEach(function (field) {{
        if (!validateField(field)) formIsValid = false;
    }});

    if (!formIsValid) {{
        Swal.fire({{
            icon: 'error',
            title: 'خطا',
            text: 'لطفاً تمام فیلدهای اجباری را کامل کنید!',
            confirmButtonText: 'باشه'
        }});
        return;
    }}

}}

</script>
");

            return new HtmlString(sb.ToString());
        }
    }
}
