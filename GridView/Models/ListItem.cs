namespace GeneralModal.Models
{
    public class ListItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; } = false;

        public ListItem(string value, string text, bool selected = false)
        {
            Value = value;
            Text = text;
            Selected = selected;
        }
    }
}
