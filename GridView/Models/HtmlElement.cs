namespace GeneralModal.Models
{
    public abstract partial class HtmlElement
    {
        public string Id { get; set; } = "";
        public string Class { get; set; } = "";
        public string Style { get; set; } = "";
        public string Name { get; set; } = "";
        public string Placeholder { get; set; } = "";
        public string LabelText { get; set; } = ""; // متن label خودکار
        public string OnClick { get; set; } = "";

        // ویژگی‌های Validation
        public bool Required { get; set; } = false;
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string Pattern { get; set; } = "";
        public string ValidationMessage { get; set; } = "";
        public HtmlElement SetId(string id) { Id = id; return this; }
        public HtmlElement SetClass(string cls) { Class = cls; return this; }
        public HtmlElement SetStyle(string style) { Style = style; return this; }
        public HtmlElement SetName(string name) { Name = name; return this; }
        public HtmlElement SetPlaceholder(string ph) { Placeholder = ph; return this; }
        public HtmlElement SetLabel(string label) { LabelText = label; return this; }
        public HtmlElement OnClickAction(string js) { OnClick = js; return this; }

        // Validation Fluent
        public HtmlElement IsRequired(bool value = true) { Required = value; return this; }
        public HtmlElement SetMinLength(int len) { MinLength = len; return this; }
        public HtmlElement SetMaxLength(int len) { MaxLength = len; return this; }
        public HtmlElement SetPattern(string p) { Pattern = p; return this; }
        public HtmlElement SetValidationMessage(string msg) { ValidationMessage = msg; return this; }

        public virtual string RenderValidationAttributes()
        {
            string attrs = "";

            bool hasValidation =
                Required ||
                MinLength.HasValue ||
                MaxLength.HasValue ||
                !string.IsNullOrEmpty(Pattern);

            if (Required) attrs += " required";
            if (MinLength.HasValue) attrs += $" minlength='{MinLength.Value}'";
            if (MaxLength.HasValue) attrs += $" maxlength='{MaxLength.Value}'";
            if (!string.IsNullOrEmpty(Pattern)) attrs += $" pattern='{Pattern}'";
            if (!string.IsNullOrEmpty(ValidationMessage)) attrs += $" title='{ValidationMessage}'";

            // اگر هر نوع اعتبارسنجی فعال بود → isrequired اضافه شود
            if (hasValidation) attrs += " isrequired='true'";

            return attrs;
        }

        public virtual string RenderAttributes() => $"id='{Id}' name='{Name}' class='{Class}' style='{Style}' placeholder='{Placeholder}'";

        // رندر المان و Label داخل یک div
        public virtual string Render()
        {
            return RenderWrapper(RenderElementHtml());
        }

        protected virtual string RenderWrapper(string elementHtml)
        {
            string labelHtml = !string.IsNullOrEmpty(LabelText) ? $"<label for='{Id}' class='form-label'>{LabelText}</label>" : "";
            string wrapperClass = this is CheckBox || this is RadioButton ? "form-check mb-3" : "mb-3";
            return $"<div class='{wrapperClass}'>{labelHtml}{elementHtml}</div>";
        }

        protected abstract string RenderElementHtml();
    }


    public class TextBox : HtmlElement
    {
        public string Value { get; set; } = "";

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-control" : Class;
            return $"<input type='text' value='{Value}' {RenderAttributes()} class='{cls}' {RenderValidationAttributes()} />";
        }
    }
    //public class MultiSelect : SelectBase
    //{
    //    public bool Multiple { get; set; } = true; // حالت چند انتخابی

    //    protected override string RenderElementHtml()
    //    {
    //        // کلاس پیش‌فرض
    //        string cls = string.IsNullOrWhiteSpace(Class) ? "form-select" : Class;
    //        if (UseSelect2) cls += " select2";

    //        // name باید آرایه‌ای باشد برای ارسال چند مقدار
    //        string nameAttr = Name;
    //        if (!string.IsNullOrEmpty(Name) && Multiple)
    //            nameAttr += "[]";

    //        // گزینه‌ها
    //        string options = RenderOptions(true);

    //        // تگ select
    //        string selectHtml = $"<select id='{Id}' name='{nameAttr}' class='{cls}' multiple {RenderValidationAttributes()}>{options}</select>";

    //        // Label و Wrapper مانند سایر کنترل‌ها
    //        //string labelHtml = !string.IsNullOrEmpty(LabelText) ? $"<label for='{Id}' class='form-label'>{LabelText}</label>" : "";
    //        string wrapperClass = "mb-3"; // مانند TextBox و TextArea

    //        return $"<div class='{wrapperClass}'>{selectHtml}</div>";
    //    }
    //}

    public class MultiSelectSelect2 : HtmlElement
    {
        public List<ListItem> Items { get; set; } = new List<ListItem>();
        public List<string> SelectedValues { get; set; } = new List<string>();
        public int Width { get; set; } = 300;
        public int MarginTop { get; set; } = 50;

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-select" : Class;
            cls += " select2";

            string nameAttr = string.IsNullOrEmpty(Name) ? "" : Name + "[]";

            string optionsHtml = "";
            foreach (var item in Items)
            {
                string selected = SelectedValues.Contains(item.Value) ? "selected" : "";
                optionsHtml += $"<option value='{item.Value}' {selected}>{item.Text}</option>";
            }
            return $"<select multiple id='{Id}' name='{nameAttr}' class='{cls}' style='width:100%' {RenderValidationAttributes()}>{optionsHtml}</select>";
        }
    }

    public class MultiSelect : SelectBase
    {
        public bool UseSelect2 { get; set; } = true; // فعال کردن Select2

        protected override string RenderElementHtml()
        {
            // کلاس پیش‌فرض
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-select" : Class;
            if (UseSelect2)
            {
                cls += " select2"; // کلاس Select2 برای JS
            }

            // name باید آرایه‌ای باشد برای ارسال چند مقدار
            string nameAttr = Name;
            if (!string.IsNullOrEmpty(Name))
                nameAttr += "[]";

            // گزینه‌ها
            string options = RenderOptions(true);

            // تگ select
            string selectHtml = $"<select multiple id='{Id}' name='{nameAttr}' class='{cls}' {RenderValidationAttributes()}>{options}</select>";

            // Label
            //string labelHtml = !string.IsNullOrEmpty(LabelText) ? $"<label for='{Id}' class='form-label'>{LabelText}</label>" : "";

            // Wrapper مشابه سایر کنترل‌ها
            string wrapperClass = "mb-3";

            return $"<div class='{wrapperClass}'>{selectHtml}</div>";
        }
    }



    public class PasswordBox : HtmlElement
    {
        public string Value { get; set; } = "";

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-control" : Class;
            return $"<input type='password' value='{Value}' {RenderAttributes()} class='{cls}' {RenderValidationAttributes()} />";
        }
    }

    public class TextArea : HtmlElement
    {
        public string Value { get; set; } = "";
        public int Rows { get; set; } = 3;

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-control" : Class;
            return $"<textarea rows='{Rows}' {RenderAttributes()} class='{cls}' {RenderValidationAttributes()}>{Value}</textarea>";
        }
    }

    public class Select : SelectBase
    {
        public bool UseSelect2 { get; set; } = false; // فعال کردن select2
        public string Placeholder { get; set; } = ""; // متن placeholder
        public bool EnableValidation { get; set; } = true; // فعال یا غیرفعال کردن ولیدیشن
        public string Width { get; set; } = "100%"; // عرض select

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-select" : Class;
            if (UseSelect2) cls += " select2";

            string style = string.IsNullOrWhiteSpace(Style) ? $"width:{Width};" : Style;

            string optionsHtml = RenderOptions(false);

            // اضافه کردن ویژگی‌های ولیدیشن در صورت فعال بودن
            string validationAttrs = EnableValidation ? RenderValidationAttributes() : "";

            return $"<select id='{Id}' name='{Name}' class='{cls}' style='{style}' {validationAttrs}>{optionsHtml}</select>";
        }
    }


    public class CheckBox : HtmlElement
    {
        public bool Checked { get; set; } = false;
        public string Value { get; set; } = "1";
        public string Label { get; set; } = "";   // متن لیبل
        public string LabelClass { get; set; } = "form-check-label";

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-check-input" : Class;

            string isChecked = Checked ? "checked" : "";

            string inputHtml =
                $"<input type='checkbox' value='{Value}' id='{Id}' {RenderAttributes()} " +
                $"class='{cls}' {isChecked} {RenderValidationAttributes()} />";

            string labelHtml = !string.IsNullOrWhiteSpace(Label)
                ? $"<label for='{Id}' class='{LabelClass}'>{Label}</label>"
                : "";

                        return $@"
            <div class='form-check mb-3 custom-check'>
                {inputHtml}
                {labelHtml}
            </div>";
        }
    }


    public class RadioButton : HtmlElement
    {
        public string Value { get; set; } = "";
        public bool Checked { get; set; } = false;

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-check-input" : Class;
            string isChecked = Checked ? "checked" : "";
            return $"<input type='radio' value='{Value}' {RenderAttributes()} class='{cls}' {isChecked} {RenderValidationAttributes()} />";
        }
    }
    public class Button : HtmlElement
    {
        public string Type { get; set; } = "button";
        public string Text { get; set; } = "Button";
        public string OnClick { get; set; } = "";
        // آیکن دلخواه مثل: "fa fa-save" یا "<i class='fa fa-plus'></i>"
        public string Icon { get; set; } = "";

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "btn btn-primary" : Class;
            string onclickAttr = string.IsNullOrWhiteSpace(OnClick) ? "" : $"onclick='{OnClick}'";

            string iconHtml = "";
            if (!string.IsNullOrWhiteSpace(Icon))
            {
                // اگر فقط کلاس آیکن فرستاده شود
                if (!Icon.Trim().StartsWith("<"))
                    iconHtml = $"<i class='{Icon}'></i> ";
                else
                    iconHtml = Icon + " ";
            }

            return $"<button id='{Id}' type='{Type}' class='{cls}' {onclickAttr}>{iconHtml}{Text}</button>";
        }
    }

    public class DatePicker : HtmlElement
    {
        public string Value { get; set; } = "";

        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-control datepicker" : Class;
            return $"<input type='text' value='{Value}' {RenderAttributes()} class='{cls}' {RenderValidationAttributes()} />";
        }
    }

    public class FileInput : HtmlElement
    {
        protected override string RenderElementHtml()
        {
            string cls = string.IsNullOrWhiteSpace(Class) ? "form-control" : Class;
            return $"<input type='file' {RenderAttributes()} class='{cls}' {RenderValidationAttributes()} />";
        }
    }

    // رندر همزمان چند المان
    public static class HtmlElementExtensions
    {
        public static string RenderAll(params HtmlElement[] elements)
        {
            string html = "";
            foreach (var e in elements)
            {
                html += e.Render();
            }
            return html;
        }
    }
}
