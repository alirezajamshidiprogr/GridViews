using GridView.Entities;
using GridView.ViewModel;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexStoreProcedure()
        {
            return View("IndexStoreProcedure");
        } 
        
        public IActionResult IndexStoreProcedureCodeFirst()
        {
            return View("IndexStoreProcedureCodeFirst");
        }

        public IActionResult IndexForm()
        {
            return View("IndexForm");
        }

        [HttpPost]
        public async Task<IActionResult> GetGridViewData_WithStoreProcedure()
        {
            // در صورتي كه در گريد در body مقداري  هست بخوان 
            var gridSearchUser = await GridExtensions.ReadRequestBodyAsync<CustomGridRequestDto>(Request);

            if (gridSearchUser.SearchTerm != null)
            {
                var searchTerm = gridSearchUser.SearchTerm; // به عنوان مثال می‌توانید از این مقدار برای فیلتر استفاده کنید
            }

            string connection = "Server=SAP-16;Database=GridViewSample;User ID=sa;Password=137011;TrustServerCertificate=True;";
            GridResultDto<ProductSaleModel> result = await GridExtensions.GetGridDataFromSPAsync<ProductSaleModel>("GetProductSalesPaged", connection, gridSearchUser);
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
                    SaleDate = ps.SaleDate.Date,
                    PaymentMethod = ps.PaymentMethod,

                    ProductId = ps.ProductId,
                    ProductName = ps.Product.Name,
                    ProductCategory = ps.Product.Category,

                    CustomerId = ps.CustomerId,
                    CustomerName = ps.Customer.Name,
                    CustomerRegion = ps.Customer.Region,

                    SalesPersonId = ps.SalesPersonId,
                    SalesPersonFullName = ps.SalesPerson.FullName
                });
               


            // صدا زدن متد EF Core Grid
            var result = await GridExtensions.GetGridDataEfCoreAsync(query);

            return Json(result);
        }
    }
}