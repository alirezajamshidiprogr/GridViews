namespace GridView.ViewModel
{
    public class ProductSaleDto
    {
        public int Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
        public string PaymentMethod { get; set; }

        // اطلاعات محصول
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCategory { get; set; }

        // اطلاعات مشتری
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerRegion { get; set; }

        // اطلاعات فروشنده
        public int SalesPersonId { get; set; }
        public string SalesPersonFullName { get; set; }
    }
}
