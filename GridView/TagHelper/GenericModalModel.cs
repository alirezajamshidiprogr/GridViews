namespace GridView.TagHelper
{
    public class GenericModalModel
    {
        public string Id { get; set; } = "globalModal";
        public string Title { get; set; } = "";
        public string Size { get; set; } = "md"; // sm, md, lg, xl, xxl
        public string BodyHtml { get; set; } = ""; // برای محتوای بادی
        public string HeaderHtml { get; set; } = ""; // برای هدر دلخواه
        public string FooterHtml { get; set; } = ""; // برای فوتر دلخواه
        public bool EnableHeader { get; set; } = true;
        public bool EnableFooter { get; set; } = true;
    }
}
