Perfect 👍
Here is the same README, rewritten fully in English, clean, professional, and suitable for GitHub, enterprise projects, and technical books.

📊 EORC Dynamic GridView (ASP.NET Core)
Overview

EORC Dynamic GridView is a powerful, flexible, and fully customizable dynamic GridView for ASP.NET Core MVC / Razor Pages, built using a Fluent Builder Pattern and TagHelper-style extensions.

It provides enterprise-level grid features without relying on third-party commercial libraries such as Kendo UI or DevExpress.

✨ Features

✅ Paging

✅ Sorting

✅ Column Filtering

✅ Advanced Filtering

✅ Grouping

✅ Inline Editing

✅ Edit & Delete Action Buttons

✅ Lazy Loading

✅ Calculated Footer (Sum / Avg / Count / Min / Max)

✅ Export:

Excel

PDF

Print

✅ Show / Hide Columns

✅ Custom Toolbar HTML

✅ RTL Support

✅ Attribute-based Column Configuration

✅ Fully Client-State Driven

🧱 Requirements

Ensure the following dependencies are included in your project:

ASP.NET Core 6+

jQuery

Font Awesome

Grid-specific CSS & JS files

<img width="642" height="130" alt="image" src="https://github.com/user-attachments/assets/a8b36db5-7755-466d-858f-5c448a62f373" />

🏗️ Model Definition

Columns are defined using a custom attribute (GridColumnAttribute):

<img width="672" height="333" alt="image" src="https://github.com/user-attachments/assets/c276f125-5ce6-49ce-9f0f-5343752a5c58" />

🚀 Usage Example
Razor View

<img width="654" height="531" alt="image" src="https://github.com/user-attachments/assets/b27ef079-9d8f-4bc1-b394-638f167eb205" />

⚙️ Configuration Options
Method	Description
EnablePaging(bool)	Enables pagination
PageSize(int)	Records per page (must be multiple of 5)
EnableSorting(bool)	Enables column sorting
EnableFiltering(bool)	Enables simple filtering
EnableAdvancedFilter(bool)	Enables advanced filtering
EnableGrouping(bool)	Enables grouping
EnableFooter(bool)	Enables footer calculations
EnableLazyLoading(bool)	Enables lazy loading
EnableEditButton(bool)	Shows edit button
EnableDeleteButton(bool)	Shows delete button
AddCustomHtml(string)	Adds custom HTML to toolbar

🧠 Built-in Validation

The grid performs strict validation during Build():

Grid name must be specified

Data source URL is required

All feature flags must be explicitly defined

PageSize must be divisible by 5

Any misconfiguration results in a clear HTML error message.

🧩 Internal Architecture

Column metadata resolved via Reflection

Grid state stored per instance in:

Safe JSON serialization to avoid <script> breaking

Fully decoupled client-side rendering

🛡️ Advantages

❌ No dependency on commercial grid libraries

✔ Full control over markup, behavior, and styling

✔ Enterprise-ready and extensible

✔ Easy debugging and customization

✔ Clean Architecture friendly

📌 Notes

Use Lazy Loading for large datasets

Attributes define all column behaviors

PageSize must be a multiple of 5

📄 License

This project is intended for internal or custom enterprise use and can be freely extended.

**cshtml :**

زبز
يسيي
@using GeneralModal.Models;
@using GeneralModal.TagHelper;
@using GridView.Enums;
@using GridView.ViewModel;
@using GridView.TagHelpers
@using static GridView.TagHelpers.DynamicGridExtensions


        @{
    var model = new List<ProductSaleDto>();  // مدل دلخواه شما طبق فرمت گريد
    var gridUrl1 = Url.Action("GetGridViewDataByCodeFirst", "Home"); // اكشن و كنترلر شما
        }
        @(
            Eorc_Grid<ProductSaleDto>("grdProduct2") // نام گريد شما
                .Url(gridUrl1)
                .Items(model.Cast<object>().ToList()) // اين خط الزامي است
                .PageSize(25) // پيج سايز اوليه
                .EnablePaging(true) // فعال سازي پيجينگ گريد 
                .EnableFiltering(true) // فعال سازي فليتر گريد
                .EnableSorting(true) //  فعال سازي سورتينگ گريد 
                .EnableFooter(true) // فعال سازي فوتر گريد 
                .EnableGrouping(true) // فعال سازي گروه بندي گريد
                .EnableExcelExport(true) // فعال سازي خروجي به اكسل
                .EnablePDFExport(true) // فعال سازي خروجي به پي دي اف
                .EnablePrint(true) // فعال سازي پرينت 
                .EnableEditButton(true)// فعال سازي دكمه ويرايش
                .EnableDeleteButton(true) // فعال سازي دكمه حذف
                .EnableLazyLoading(false) // غير فعال نمودن لو با اسكرول 
                .EnableAdvancedFilter(true) // فعال سازي فيلتر پيشرفته
                .EnableInlineEdit(true) // فعال سازي ويرايش داخل گريد فعلا غير فعال است
                .EnableShowHiddenColumns(true) // نمايش دكمه براي مخفي يا نمايش ستون هاي گريد
                .EditJavaScriptFunction("OpenModalSearch()") // تابع جاوا اسكريپت براي ويرايش (به صورت پيش فرض تابعي با نام گريد ايجاد ميشود و اين مورد كاستومايز كردن است)
                @*.EditJavaScriptFunction("InsUpd_grdProduct1_Item(this)") // تابع جاوا اسكريپت براي ويرايش (به صورت پيش فرض تابعي با نام گريد ايجاد ميشود و اين مورد كاستومايز كردن است)*@
                .DeleteJavaScriptFunction("CloseModalSearch()")// تابع جاوا اسكريپت براي حذف (به صورت پيش فرض تابعي با نام گريد ايجاد ميشود و اين مورد كاستومايز كردن است)
            .AddCustomHtml("<button class='grid-action-button-class' onclick='openFilterPopup()'>style</button>")// افزودن المان هاي اچ تي ام ال سفارشي
                .Build() // اين خط لازم است 
        )


        **Controller **
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

**Sample Grid**

<img width="1918" height="777" alt="image" src="https://github.com/user-attachments/assets/5561048f-5cf4-4bdd-9d21-688e47715d18" />


**sample procedure sql **
USE [GridViewSample]
GO
/****** Object:  StoredProcedure [dbo].[GetProductSalesPaged]    Script Date: 22/12/2025 12:14:30 ب.ظ ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetProductSalesPaged]
    @Page INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(100) = NULL,
    @SortAsc BIT = 1,
    @EnablePaging BIT = 1,
    @Filters NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Sql NVARCHAR(MAX) = N'SELECT * FROM ProductSales WHERE 1=1';
    DECLARE @CountSql NVARCHAR(MAX) = N'SELECT COUNT(*) FROM ProductSales WHERE 1=1';

    -- فیلترها
    IF @Filters IS NOT NULL
    BEGIN
        DECLARE @FilterTable TABLE ([Key] NVARCHAR(100), Type NVARCHAR(20), Value NVARCHAR(200));
        INSERT INTO @FilterTable([Key], Type, Value)
        SELECT [key], JSON_VALUE([value],'$.Type'), JSON_VALUE([value],'$.Value')
        FROM OPENJSON(@Filters);

        DECLARE @key NVARCHAR(100), @type NVARCHAR(20), @val NVARCHAR(200);
        DECLARE cur CURSOR FOR SELECT [Key], Type, Value FROM @FilterTable;
        OPEN cur;
        FETCH NEXT FROM cur INTO @key, @type, @val;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF @type = 'eq'
            BEGIN
                SET @Sql += ' AND ' + QUOTENAME(@key) + ' = ' + QUOTENAME(@val, '''');
                SET @CountSql += ' AND ' + QUOTENAME(@key) + ' = ' + QUOTENAME(@val, '''');
            END
            ELSE IF @type = 'neq'
            BEGIN
                SET @Sql += ' AND ' + QUOTENAME(@key) + ' <> ' + QUOTENAME(@val, '''');
                SET @CountSql += ' AND ' + QUOTENAME(@key) + ' <> ' + QUOTENAME(@val, '''');
            END
            ELSE IF @type = 'contains'
            BEGIN
                SET @Sql += ' AND ' + QUOTENAME(@key) + ' LIKE ''%' + @val + '%''';
                SET @CountSql += ' AND ' + QUOTENAME(@key) + ' LIKE ''%' + @val + '%''';
            END
            ELSE IF @type = 'startswith'
            BEGIN
                SET @Sql += ' AND ' + QUOTENAME(@key) + ' LIKE ''' + @val + '%''';
                SET @CountSql += ' AND ' + QUOTENAME(@key) + ' LIKE ''' + @val + '%''';
            END
            ELSE IF @type = 'endswith'
            BEGIN
                SET @Sql += ' AND ' + QUOTENAME(@key) + ' LIKE ''%' + @val + '''';
                SET @CountSql += ' AND ' + QUOTENAME(@key) + ' LIKE ''%' + @val + '''';
            END

            FETCH NEXT FROM cur INTO @key, @type, @val;
        END

        CLOSE cur;
        DEALLOCATE cur;
    END

    -- مرتب سازی
    IF @SortColumn IS NOT NULL AND @SortColumn <> ''
        SET @Sql += ' ORDER BY ' + QUOTENAME(@SortColumn) + CASE WHEN @SortAsc = 1 THEN ' ASC' ELSE ' DESC' END;
    ELSE
        SET @Sql += ' ORDER BY Id ASC';

    -- Paging
    IF @EnablePaging = 1
        SET @Sql += ' OFFSET ' + CAST((@Page-1)*@PageSize AS NVARCHAR(10)) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR(10)) + ' ROWS ONLY';

    -- اجرای دو SELECT: ابتدا داده‌ها، سپس تعداد کل
    EXEC sp_executesql @Sql;
    EXEC sp_executesql @CountSql;
END


