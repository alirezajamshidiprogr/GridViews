using GridView.Entities;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static void Initialize(SalesDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.ProductSales.Any())
            return; // قبلاً داده وجود دارد

        var random = new Random();

        // نام‌های محصول فارسی نمونه
        string[] productNames = { "کفش ورزشی", "کتاب داستان", "موبایل", "لپ‌تاپ", "خودکار", "دفتر", "کیف", "لباس", "تلویزیون", "یخچال" };
        string[] categories = { "الکترونیک", "ورزش", "کتاب", "لوازم‌التحریر", "پوشاک", "خانه و آشپزخانه" };

        // نام‌های مشتری فارسی
        string[] customerNames = { "علی", "محمد", "زهرا", "فاطمه", "رضا", "سارا", "امیر", "حسین", "مریم", "مجید" };
        string[] regions = { "تهران", "اصفهان", "شیراز", "مشهد", "تبریز", "قم" };

        // نام‌های فروشنده فارسی
        string[] salesPersonNames = { "سعید", "مهدی", "نیلوفر", "امیرحسین", "سارا", "پرهام", "مونا", "کامران", "بهاره", "علی رضا" };

        // Products - 52000 رکورد
        var products = new List<Product>();
        for (int i = 1; i <= 52000; i++)
        {
            products.Add(new Product
            {
                Name = productNames[random.Next(productNames.Length)] + $" {i}",
                Category = categories[random.Next(categories.Length)]
            });
        }
        context.Products.AddRange(products);
        context.SaveChanges();

        // Customers - 500 رکورد
        var customers = new List<Customer>();
        for (int i = 1; i <= 500; i++)
        {
            customers.Add(new Customer
            {
                Name = customerNames[random.Next(customerNames.Length)] + $" {i}",
                Region = regions[random.Next(regions.Length)]
            });
        }
        context.Customers.AddRange(customers);
        context.SaveChanges();

        // SalesPersons - 50 رکورد
        var salesPersons = new List<SalesPerson>();
        for (int i = 1; i <= 50; i++)
        {
            salesPersons.Add(new SalesPerson
            {
                FullName = salesPersonNames[random.Next(salesPersonNames.Length)] + $" {i}"
            });
        }
        context.SalesPersons.AddRange(salesPersons);
        context.SaveChanges();

        // ProductSales - رکوردها به صورت تصادفی از جدول‌های بالا
        var sales = new List<ProductSale>();
        for (int i = 1; i <= 52000; i++)
        {
            var product = products[random.Next(products.Count)];
            var customer = customers[random.Next(customers.Count)];
            var salesPerson = salesPersons[random.Next(salesPersons.Count)];
            var quantity = random.Next(1, 20);
            var unitPrice = random.Next(10000, 1000000);

            sales.Add(new ProductSale
            {
                ProductId = product.Id,
                CustomerId = customer.Id,
                SalesPersonId = salesPerson.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = quantity * unitPrice,
                SaleDate = DateTime.Now.AddDays(-random.Next(0, 365)),
                PaymentMethod = random.Next(0, 2) == 0 ? "نقدی" : "کارت"
            });

            // هر 1000 رکورد یک بار SaveChanges
            if (i % 1000 == 0)
            {
                context.ProductSales.AddRange(sales);
                context.SaveChanges();
                sales.Clear();
            }
        }

        if (sales.Any())
        {
            context.ProductSales.AddRange(sales);
            context.SaveChanges();
        }
    }
}
