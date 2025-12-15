namespace GridView.ViewModel
{
    public class ProductSaleDto
    {
        [GridColumn("شناسه",visible:false)]

        public int Id { get; set; }


        [GridColumn("شناسه شخص", visible: false)]
        public int SalesPersonId { get; set; }

        [GridColumn("شناسه مشتري", visible: false)]
        public int CustomerId { get; set; }

        [GridColumn("واحد پول")]
        public decimal UnitPrice { get; set; }
        [GridColumn("مقدار")]
        public int Quantity { get; set; }
        [GridColumn("مجموع")]
        public decimal TotalPrice { get; set; }
        [GridColumn("تاريخ")]
        public DateTime SaleDate { get; set; }
        [GridColumn("نوع پرداخت")]
        public string PaymentMethod { get; set; }
        [GridColumn("شناسه محصول")]
        public int ProductId { get; set; }
        [GridColumn("نام محصول")]
        public string ProductName { get; set; }
        [GridColumn("دسته بندي")]
        public string ProductCategory { get; set; }
        [GridColumn("مشتري")]
        public string CustomerName { get; set; }
        [GridColumn("منطقه")]
        public string CustomerRegion { get; set; }
        [GridColumn("نام شخص")]
        public string SalesPersonFullName { get; set; }
    }
}
