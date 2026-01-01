namespace GridView.ViewModel
{
    public abstract class SelectBase : HtmlElement
    {
        public List<ListItem> Items { get; set; } = new();
        public bool UseSelect2 { get; set; } = false;
        public List<string> SelectedValues { get; set; } = new();

        protected string RenderOptions(bool isMulti)
        {
            return string.Join("", Items.Select(item =>
            {
                bool isSelected =
                    item.Selected ||
                    SelectedValues.Contains(item.Value);

                return $"<option value='{item.Value}' {(isSelected ? "selected" : "")}>{item.Text}</option>";
            }));
        }
    }
}
