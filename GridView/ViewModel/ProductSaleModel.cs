namespace GridView.ViewModel
{
    public class ProductSaleModel
    {
        [GridColumn(displayName: "شناسه محصول",visible:false, grouping: false,Editable =false,EnableSorting = true,EnableFiltering =true ,EnableGrouping = true )]
        public int Id { get; set; }
        [GridColumn("نام محصول", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public string ProductName { get; set; }
        [GridColumn("دسته بندي", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public string Category { get; set; }
        [GridColumn("تامين كننده", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public string Supplier { get; set; }
        [GridColumn("مجموع", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public decimal TotalPrice { get; set; }
        [GridColumn("واحد پول", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true , FirstDisplay = false )]
        public string Currency { get; set; }
        [GridColumn("مشتري", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public string Customer { get; set; }
        [GridColumn("منطقه", grouping: false)]
        public string Region { get; set; }
        [GridColumn("تاريخ فروش")]
        public string SaleDate { get; set; }
        [GridColumn("قيمت", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public decimal UnitPrice { get; set; }
        [GridColumn("تعداد", grouping: false, Editable = false, EnableSorting = true, EnableFiltering = true, EnableGrouping = true)]
        public int Quantity { get; set; }
        [GridColumn("نوع پرداخت")]
        public string PaymentMethod { get; set; }
        [GridColumn("وضعيت")]
        public string Status { get; set; }
        [GridColumn("يادداشت")]
        public string Notes { get; set; }
        [GridColumn("فرروشنده")]
        public string SalesPerson { get; set; }

    }
}
