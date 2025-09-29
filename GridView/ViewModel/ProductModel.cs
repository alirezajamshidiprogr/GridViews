namespace GridView.ViewModel
{
    public class ProductModel
    {
        [GridColumn("شناسه", editable: false, visible: false)]
        public int Id { get; set; }

        [GridColumn("نام محصول", editable: true, visible: true)]
        public string Name { get; set; }

        [GridColumn("دسته‌بندی", editable: true, visible: true)]
        public string Category { get; set; }

        [GridColumn("قیمت", editable: true, visible: true)]
        public decimal Price { get; set; }

        [GridColumn("موجودی", visible: true)]
        public int Stock { get; set; }

        [GridColumn("تامین‌کننده", visible: true)]
        public string Supplier { get; set; }

        [GridColumn("تاریخ اضافه شدن", visible: true)]
        public DateTime DateAdded { get; set; }

        [GridColumn("وضعیت", visible: true)]
        public string Status { get; set; }
    }
}


