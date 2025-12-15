using Microsoft.AspNetCore.Html;
using GeneralModal.Models;
using System.Text;
using GridView.Models;
using System.Xml.Linq;
using GridView.Enums;

namespace GeneralModal.TagHelper
{
    public class GenericModalBuilder
    {
        private readonly GenericModalModel _model;
        private bool _enableValidation = true;
        private int _columns = 1;

        public GenericModalBuilder(string id)
        {
            _model = new GenericModalModel { Id = id };
        }

        public GenericModalBuilder EnableValidation(bool enable = true)
        {
            _enableValidation = enable;
            return this;
        }

        public GenericModalBuilder Columns(int columns)
        {
            _columns = Math.Max(1, columns); 
            return this;
        }
        public GenericModalBuilder Title(string title)
        {
            _model.Title = title;
            return this;
        }

        public GenericModalBuilder Size(ModalSize size)
        {
            _model.Size = size.ToString(); 
            return this;
        }


        public GenericModalBuilder HeaderHtml(string html)
        {
            _model.HeaderHtml = html;
            return this;
        }

        public GenericModalBuilder FooterHtml(string html)
        {
            _model.FooterHtml = html;
            return this;
        }

        // پذیرش چند HtmlElement
        public GenericModalBuilder BodyHtml(params HtmlElement[] elements)
        {
            var sb = new StringBuilder();
            if (_columns <= 1)
            {
                // حالت تک‌ستونه
                foreach (var el in elements)
                {
                    // هر HtmlElement شامل <label> و <input> خودش باشد
                                sb.AppendLine($@"
                    <div class='mb-3'>
                        {el.Render()}
                    </div>");
                }
            }
            else
            {
                // حالت چند ستونه
                int count = 0;
                sb.AppendLine("<div class='row'>");
                foreach (var el in elements)
                {
                    sb.AppendLine($"<div class='col-md-{12 / _columns} mb-3'>{el.Render()}</div>");
                    count++;
                    if (count % _columns == 0 && count < elements.Length)
                    {
                        sb.AppendLine("</div><div class='row'>");
                    }
                }
                sb.AppendLine("</div>");
            }


            _model.BodyHtml = sb.ToString();
            return this;
        }


        public IHtmlContent Build()
        {
            ModalSize modalSizeEnum;
            Enum.TryParse(_model.Size, out modalSizeEnum);

            string modalSizeClass = modalSizeEnum switch
            {
                ModalSize.Small => "modal-sm",
                ModalSize.Large => "modal-lg",
                ModalSize.ExtraLarge => "modal-xl",
                ModalSize.ExtraExtraLarge => "modal-xxl",
                _ => ""
            };


            // Header
            string headerHtml = "";
            if (!string.IsNullOrEmpty(_model.HeaderHtml))
            {
                headerHtml = $@"
                    <div class='modal-header'>
                <button type=""button"" class=""close"" onclick=""closeModal_{_model.Id}(this)"">    <i class=""fa fa-times""></i></button>      {_model.HeaderHtml}</div>";
                            }
                            else if (!string.IsNullOrEmpty(_model.Title))
                            {
                                headerHtml = $@"
                                <div class='modal-header'>
                            <button type=""button"" class=""close"" onclick=""closeModal_{_model.Id}(this)"">    <i class=""fa fa-times""></i></button>
                                    <h5 class='modal-title'>{_model.Title}</h5>
                                </div>";
            }


            // Footer
            string footerHtml = "";
            if (!string.IsNullOrEmpty(_model.FooterHtml))
            {
                footerHtml = $"<div class='modal-footer'>{_model.FooterHtml}</div>";
            }

            // Body
            string bodyHtml = $"<div class='modal-body'>{_model.BodyHtml}</div>";

            // --- Script ولیدیشن + Open/Close Bootstrap 4 compatible ---
            string script = $@"
<script>

// ---------- Open And Close Modal ----------
function openModal_{_model.Id}(id) {{
    var modal = document.getElementById(id);
    if (!modal) return;

    modal.classList.add('fade');
    modal.style.display = 'block';

    setTimeout(() => {{
        modal.classList.add('show');
        initSelect2Controls('{_model.Id}');
    }}, 10);

    document.body.classList.add('modal-open');

    document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());

    var backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop fade show';
    // وقتی روی بک‌دراپ کلیک شد مدال بسته شود
    backdrop.onclick = function () {{ closeModal_{_model.Id}(modal); }};
    document.body.appendChild(backdrop);
}}

function closeModal_{_model.Id}() {{
    var modal = document.getElementById('{_model.Id}');
    if (!modal) return;

    modal.classList.remove('show');
    document.body.classList.remove('modal-open');

    setTimeout(() => {{
        modal.style.display = 'none';
        document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
    }}, 150);
}}



// ---------- Validation ONLY if enabled ----------
document.addEventListener('DOMContentLoaded', function () {{
    {(_enableValidation ? $@"
    var modal = document.getElementById('{_model.Id}');
    if (!modal) return;

    var fields = modal.querySelectorAll('input[isrequired], textarea[isrequired], select[isrequired]');

    fields.forEach(function(field) {{
        field.addEventListener('blur', function() {{ validateField(field); }});
        field.addEventListener('input', function() {{
            if(field.classList.contains('is-invalid')) 
                validateField(field);
        }});
    }});

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
    " : "// Validation disabled")}
}});

// ---------- ValidationModal ----------
function validateField(field) {{
    if (!field.checkValidity()) {{
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
        return false;
    }}
    field.classList.remove('is-invalid');
    field.classList.add('is-valid');
    return true;
}}

// ---------- CheckIsValidForm ----------
function IsValidForm_myModal(modalOrId) {{

    // اگر نوع string بود، تبدیل به عنصر DOM کن
    var modal = (typeof modalOrId === 'string')
        ? document.getElementById(modalOrId)
        : modalOrId;

    if (!modal) return false;

    var fields = modal.querySelectorAll('input[isrequired], textarea[isrequired], select[isrequired]');
    var formIsValid = true;

    fields.forEach(function(field) {{
        if (!field.checkValidity()) {{
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
            formIsValid = false;
        }} else {{
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        }}
    }});

    return formIsValid;
}}
// ______________ load Select2 __________
function initSelect2Controls(modalId) {{
    $('#' + modalId).find('.select2').each(function () {{
        if (!$(this).hasClass(""select2-hidden-accessible"")) {{
            $(this).select2({{
                dropdownParent: $('#' + modalId),  // مهم برای مدال
                width: ""100%"",
                placeholder: $(this).attr(""placeholder"") || """",
                allowClear: true
            }});
        }}
    }});
}}
// ---------- Submit ----------
function submitModalForm_{_model.Id}() {{
    {(_enableValidation ? $@"
    var modal = document.getElementById('{_model.Id}');
    if (!modal) return;

    var fields = modal.querySelectorAll('input[isrequired], textarea[isrequired], select[isrequired]');
    var formIsValid = true;

    fields.forEach(function(field) {{
        if (!field.checkValidity()) {{
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
            formIsValid = false;
        }} else {{
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        }}
    }});

    if (!formIsValid) {{
        alert('لطفاً تمام فیلدهای اجباری را کامل کنید!');
        return;
    }}

    alert('فرم معتبر است!');
    " : @"
    console.log('Validation disabled, form submitted');
    ")}
}}

</script>";


            string html = $@"
<div class='modal fade Eorc_Modal' id='{_model.Id}' tabindex='-1' aria-hidden='true'>
    <div class='modal-dialog {modalSizeClass} modal-dialog-centered'>
        <div class='modal-content'>
            {headerHtml}
            {bodyHtml}
            {footerHtml}
        </div>
    </div>
</div>
{script}";

            return new HtmlString(html);
        }
    }
}
