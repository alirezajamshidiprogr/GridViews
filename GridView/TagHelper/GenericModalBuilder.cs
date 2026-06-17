using Microsoft.AspNetCore.Html;
using System.Text;
using System.Xml.Linq;
using GridView.TagHelper;
using GridView.ViewModel;
using GridView.ViewModel.Enums;

namespace GeneralModal.TagHelper
{
    public class Eorc_ModalBuilder
    {
        private readonly GenericModalModel _model;
        private bool _enableValidation = true;
        private int _columns = 1;

        public Eorc_ModalBuilder(string id)
        {
            _model = new GenericModalModel { Id = id };
        }

        public Eorc_ModalBuilder EnableValidation(bool enable = true)
        {
            _enableValidation = enable;
            return this;
        }

        public Eorc_ModalBuilder Columns(int columns)
        {
            _columns = Math.Max(1, columns);
            return this;
        }
        public Eorc_ModalBuilder Title(string title)
        {
            _model.Title = title;
            return this;
        }

        public Eorc_ModalBuilder Size(ModalSize size)
        {
            _model.Size = size.ToString();
            return this;
        }


        public Eorc_ModalBuilder HeaderHtml(string html)
        {
            _model.HeaderHtml = html;
            return this;
        }

        public Eorc_ModalBuilder FooterHtml(string html)
        {
            _model.FooterHtml = html;
            return this;
        }

        // پذیرش چند HtmlElement
        public Eorc_ModalBuilder BodyHtml(params HtmlElement[] elements)
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
// ----------intitialSelect2 Controls---------- 
function initSelect2_(modalId){{

    var modal = document.getElementByUd(modalId) ;
    if (!modal) return ;


    $modal.find('select.select2').each(function () {{
        if ($select.hasClass('select2-hidden-accessible')){{
        $(this).select2({{
        dropdownParent : $modal,
        width:'100%' ,
        placeholder: $select.data('placeholder') || '' ,
        assowClear: true 
        }});
    }}
 }});
}}

// به دست آوردن مقادير پاپ آپ
function getInputValuesModal(modalIdOrElement) {{
//نحوه استفاده :
//var values = getInputValuesModal('myModal');

    // بررسی اینکه ورودی یک عنصر HTML است یا یک شناسه
    var modal = typeof modalIdOrElement === 'string' 
        ? document.getElementById(modalIdOrElement) 
        : modalIdOrElement;
    
    if (!modal) return {{}};

    var inputs = modal.querySelectorAll('input, select, textarea');
    var model = {{}};

    inputs.forEach(function(input) {{
        // فقط input هایی که name یا id دارند
        var key = input.name || input.id;
        if (!key) return;

        // مقدار input را بگیریم
        if (input.type === 'checkbox') {{
            model[key] = input.checked;
        }} else if (input.type === 'radio') {{
            if (input.checked) model[key] = input.value;
        }} else if (input.tagName.toLowerCase() === 'select' && input.multiple) {{
            model[key] = Array.from(input.selectedOptions).map(o => o.value);
        }} else {{
            model[key] = input.value;
        }}
    }});

    return model;
}}


// ---------- Open And Close Modal ----------
function openModal_{_model.Id}(id) {{
    var modal = document.getElementById(id);
    if (!modal) return;

    modal.classList.add('fade');
    modal.style.display = 'block';

    setTimeout(() => {{
        modal.classList.add('show');
       // initSelect2Controls('{_model.Id}');
    }}, 10);

    document.body.classList.add('modal-open');

    document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());

    var backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop fade show';
    // وقتی روی بک‌دراپ کلیک شد مدال بسته شود
    backdrop.onclick = function () {{ closeModal_{_model.Id}(modal); }};
    document.body.appendChild(backdrop);

    // لود سلكت ها
    //initSelect2('{_model.Id}'); 

     $('.clockpicker-with-callbacks').clockpicker({{donetext: 'Done',}})
            
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
// وليديشن فرم ها
function submitForm_modal(modalId){{
    var container = document.getElementById(modalId) ; 
    if (!container) return ;

    //Validation
    var fields = container.querySelectorAll('input[isrequired],textarea[isrequired],select[isrequired]');
    var formIsValid = true ;

    fields.forEach(function (field) {{
        if (!validateField(field)) formIsValid = false ;
        }});

    if (!formIsValid) {{
        swal({{
                title: 'خطا',
                type: 'error',
                text: 'لطفاً تمام فيلد هاي اجباري را كامل كنيد ' , 
                icon: 'error',
                confirmButtonText : 'باشه'
            }});
         return false;
        }}

return true  ;
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
