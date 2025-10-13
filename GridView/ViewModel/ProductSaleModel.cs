namespace GridView.ViewModel
{
    public class ProductSaleModel
    {
        [GridColumn("شناسه محصول",visible:false, grouping: false)]
        public int Id { get; set; }
        [GridColumn("نام محصول")]
        public string ProductName { get; set; }
        [GridColumn("دسته بندي")]
        public string Category { get; set; }
        [GridColumn("تامين كننده")]
        public string Supplier { get; set; }
        [GridColumn("قيمت", grouping: false)]
        public decimal UnitPrice { get; set; }
        [GridColumn("تعداد", grouping: false)]
        public int Quantity { get; set; }
        [GridColumn("مجموع", grouping: false)]
        public decimal TotalPrice { get; set; }
        [GridColumn("واحد پول", grouping: false)]
        public string Currency { get; set; }
        [GridColumn("مشتري")]
        public string Customer { get; set; }
        [GridColumn("منطقه")]
        public string Region { get; set; }
        [GridColumn("تاريخ فروش")]
        public string SaleDate { get; set; }
        [GridColumn("نوع پرداخت")]
        public string PaymentMethod { get; set; }
        [GridColumn("وضعيت")]
        public string Status { get; set; }
        [GridColumn("يادداشت", grouping: false)]
        public string Notes { get; set; }
        [GridColumn("فرروشنده")]
        public string SalesPerson { get; set; }

    }
}
