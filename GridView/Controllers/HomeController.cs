using GridView.Entities;
using GridView.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GridView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SalesDbContext _context;
        public HomeController(ILogger<HomeController> logger, SalesDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index_GridWith_Filter_Sort_Paging_Grouping()
        {
            return View("Index_GridWith_Filter_Sort_Paging_Grouping");
        }

        [HttpPost]
        public async Task<IActionResult> GetGridViewData1()
        {
            // در صورتي كه در گريد در body مقداري  هست بخوان 
            var requestDto = await GridExtensions.ReadRequestBodyAsync<CustomGridRequestDto>(Request);

            if (requestDto.SearchTerm != null)
            {
                var searchTerm = requestDto.SearchTerm; // به عنوان مثال می‌توانید از این مقدار برای فیلتر استفاده کنید
            }

            var result = await GridExtensions.GetGridDataFromSPAsync<ProductSaleModel>("GetProductSalesPaged");
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetGridViewDataByCodeFirst()
        {
            var requestDto = await GridExtensions.ReadRequestBodyAsync<CustomGridRequestDto>(Request);
           
            if (requestDto.SearchTerm != null)
            {
                var searchTerm = requestDto.SearchTerm; // می‌توانی برای فیلتر اضافی استفاده کنی
            }

            // ساخت query با Include برای روابط
            var query = _context.ProductSales
                .Select(ps => new ProductSaleDto
                {
                    Id = ps.Id,
                    UnitPrice = ps.UnitPrice,
                    Quantity = ps.Quantity,
                    TotalPrice = ps.TotalPrice,
                    SaleDate = ps.SaleDate,
                    PaymentMethod = ps.PaymentMethod,

                    ProductId = ps.ProductId,
                    ProductName = ps.Product.Name,
                    ProductCategory = ps.Product.Category,

                    CustomerId = ps.CustomerId,
                    CustomerName = ps.Customer.Name,
                    CustomerRegion = ps.Customer.Region,

                    SalesPersonId = ps.SalesPersonId,
                    SalesPersonFullName = ps.SalesPerson.FullName
                })
                .AsQueryable();


            // صدا زدن متد EF Core Grid
            var result = await GridExtensions.GetGridDataEfCoreAsync(query);

            return Json(result);
        }



        //private static IQueryable<ProductSaleModel> GetBaseDataFromSql()
        //{
        //    var list = new List<ProductSaleModel>();

        //    string connectionString = "Server=SAP-16;Database=GridViewSample;User ID=sa;Password=137011;TrustServerCertificate=True;";
        //    try
        //    {
        //        using (var conn = new SqlConnection(connectionString))
        //        {
        //            conn.Open();

        //            string sql = $"SELECT * FROM ProductSales"; // تصادفی بخونه

        //            using (var cmd = new SqlCommand(sql, conn))
        //            {
        //                using (var reader = cmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        list.Add(new ProductSaleModel
        //                        {
        //                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
        //                            Category = reader.GetString(reader.GetOrdinal("Category")),
        //                            Supplier = reader.GetString(reader.GetOrdinal("Supplier")),
        //                            TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
        //                            Currency = reader.GetString(reader.GetOrdinal("Currency")),
        //                            Customer = reader.GetString(reader.GetOrdinal("Customer")),
        //                            Region = reader.GetString(reader.GetOrdinal("Region")),
        //                            SaleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate")).ToShortDateString(),
        //                            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
        //                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
        //                            PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
        //                            Status = reader.GetString(reader.GetOrdinal("Status")),
        //                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
        //                            SalesPerson = reader.GetString(reader.GetOrdinal("SalesPerson"))
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return list.AsQueryable();
        //}

    }
}