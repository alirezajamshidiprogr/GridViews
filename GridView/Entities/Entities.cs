namespace GridView.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public ICollection<ProductSale> Sales { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public ICollection<ProductSale> Purchases { get; set; }
    }

    public class SalesPerson
    {
        internal string Name;
        public int Id { get; set; }
        public string FullName { get; set; }
        public ICollection<ProductSale> Sales { get; set; }
    }

    public class ProductSale
    {
        public int Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
        public string PaymentMethod { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int SalesPersonId { get; set; }
        public SalesPerson SalesPerson { get; set; }
    }

}
