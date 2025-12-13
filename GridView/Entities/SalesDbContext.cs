using Microsoft.EntityFrameworkCore;

namespace GridView.Entities
{
    public class SalesDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesPerson> SalesPersons { get; set; }
        public DbSet<ProductSale> ProductSales { get; set; }

        public SalesDbContext(DbContextOptions<SalesDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تنظیم رابطه‌ها
            modelBuilder.Entity<ProductSale>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.Sales)
                .HasForeignKey(ps => ps.ProductId);

            modelBuilder.Entity<ProductSale>()
                .HasOne(ps => ps.Customer)
                .WithMany(c => c.Purchases)
                .HasForeignKey(ps => ps.CustomerId);

            modelBuilder.Entity<ProductSale>()
                .HasOne(ps => ps.SalesPerson)
                .WithMany(sp => sp.Sales)
                .HasForeignKey(ps => ps.SalesPersonId);
        }
    }
}
