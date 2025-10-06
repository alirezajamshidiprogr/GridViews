using GridView.Models;
using GridView.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;


namespace GridView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static List<ProductModel> AllProducts = GenerateProducts(); // داده رندوم اولیه
        private static List<ProductModel> _products = new List<ProductModel>();

        private static readonly string[] Products = { "Laptop", "Phone", "Tablet", "Monitor", "Keyboard", "Mouse" };
        private static readonly string[] Categories = { "Electronics", "Accessories", "Office" };
        private static readonly string[] Suppliers = { "SupplierA", "SupplierB", "SupplierC", "SupplierD" };
        private static readonly string[] Customers = { "Customer1", "Customer2", "Customer3", "Customer4" };
        private static readonly string[] Regions = { "North", "South", "East", "West" };
        private static readonly string[] PaymentMethods = { "Cash", "Credit Card", "Bank Transfer" };
        private static readonly string[] Statuses = { "Pending", "Completed", "Canceled" };
        private static readonly string[] SalesPersons = { "Alice", "Bob", "Charlie", "Diana" };
        private static readonly string[] NotesList = { "", "Urgent", "Delayed", "Special Request" };
        private static readonly string[] Currencies = { "USD", "EUR", "IRR" };
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            var categories = new[] { "الکترونیک", "پوشاک", "خانه" };
            var suppliers = new[] { "تامین‌کننده A", "تامین‌کننده B", "تامین‌کننده C" };
            var statusList = new[] { "فعال", "غیرفعال" };

            var rand = new Random();
            for (int i = 1; i <= 100; i++)
            {
                _products.Add(new ProductModel
                {
                    Id = i,
                    Name = $"محصول {i}",
                    Category = categories[i % 3],
                    Price = Math.Round((decimal)(rand.NextDouble() * 1000), 2),
                    Stock = rand.Next(1, 50),
                    Supplier = suppliers[i % 3],
                    DateAdded = DateTime.Now.AddDays(-rand.Next(0, 365)),
                    Status = statusList[i % 2]
                });
            }
        }

        [HttpGet]
        public IActionResult GetMoreProducts(int page = 1, int pageSize = 50)
        {
            var data = AllProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Json(data);
        }

        private static List<ProductModel> GenerateProducts()
        {
            var list = new List<ProductModel>();
            var rnd = new Random();
            for (int i = 1; i <= 3000; i++)
            {
                list.Add(new ProductModel
                {
                    Id = i,
                    Name = "محصول " + i,
                    Category = "دسته " + (i % 10),
                    Price = rnd.Next(1000, 5000),
                    Stock = rnd.Next(0, 100),
                    Supplier = "تامین کننده " + (i % 5),
                    DateAdded = DateTime.Now.AddDays(-rnd.Next(0, 100)),
                    Status = (i % 2 == 0) ? "فعال" : "غیرفعال"
                });
            }
            return list;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Index_GridWith_Filter_Sort_Paging_Grouping()
        {
            var rnd = new Random();
            var products = new[] { "لپ‌تاپ", "موبایل", "مانیتور", "پرینتر", "تبلت", "کیبورد", "موس", "هدفون" };
            var categories = new[] { "الکترونیک", "لوازم جانبی", "کامپیوتر" };
            var suppliers = new[] { "شرکت الف", "شرکت ب", "شرکت ج" };
            var customers = new[] { "علی", "زهرا", "حسین", "مهدی", "سمیه" };
            var regions = new[] { "تهران", "اصفهان", "مشهد", "شیراز", "تبریز" };
            var paymentMethods = new[] { "کارت", "نقد", "چک", "آنلاین" };
            var statuses = new[] { "تکمیل شده", "در انتظار", "لغو شده" };
            var salesPersons = new[] { "سارا", "امیر", "رضا", "لیلا" };

            // ده ردیف اول
            var list = new List<ProductSaleModel>();
            for (int i = 1; i <= 10; i++) // <--- فقط 10 ردیف
            {
                var qty = rnd.Next(1, 20);
                var price = rnd.Next(100, 5000);
                list.Add(new ProductSaleModel
                {
                    Id = i,
                    ProductName = products[rnd.Next(products.Length)],
                    Category = categories[rnd.Next(categories.Length)],
                    Supplier = suppliers[rnd.Next(suppliers.Length)],
                    UnitPrice = price,
                    Quantity = qty,
                    TotalPrice = price * qty,
                    Currency = "IRR",
                    Customer = customers[rnd.Next(customers.Length)],
                    Region = regions[rnd.Next(regions.Length)],
                    SaleDate = DateTime.Now.AddDays(-rnd.Next(0, 365)),
                    PaymentMethod = paymentMethods[rnd.Next(paymentMethods.Length)],
                    Status = statuses[rnd.Next(statuses.Length)],
                    Notes = "مثال " + i,
                    SalesPerson = salesPersons[rnd.Next(salesPersons.Length)]
                });
            }

            // داده اولیه ده ردیف به view ارسال می‌شود
            return View("Index_GridWith_Filter_Sort_Paging_Grouping", list);
        }

        [HttpPost]
        public IActionResult GetGridDataIndex_GridWith_Filter_Sort_Paging_Grouping([FromBody] GridRequest request)
        {
            var rnd = new Random();
            var products = new[] { "لپ‌تاپ", "موبایل", "مانیتور", "پرینتر", "تبلت", "کیبورد", "موس", "هدفون" };
            var categories = new[] { "الکترونیک", "لوازم جانبی", "کامپیوتر" };
            var suppliers = new[] { "شرکت الف", "شرکت ب", "شرکت ج" };
            var customers = new[] { "علی", "زهرا", "حسین", "مهدی", "سمیه" };
            var regions = new[] { "تهران", "اصفهان", "مشهد", "شیراز", "تبریز" };
            var paymentMethods = new[] { "کارت", "نقد", "چک", "آنلاین" };
            var statuses = new[] { "تکمیل شده", "در انتظار", "لغو شده" };
            var salesPersons = new[] { "سارا", "امیر", "رضا", "لیلا" };

            var list = new List<ProductSaleModel>();
            for (int i = 1; i <= 3000; i++)
            {
                var qty = rnd.Next(1, 20);
                var price = rnd.Next(100, 5000);
                list.Add(new ProductSaleModel
                {
                    Id = i,
                    ProductName = products[rnd.Next(products.Length)],
                    Category = categories[rnd.Next(categories.Length)],
                    Supplier = suppliers[rnd.Next(suppliers.Length)],
                    UnitPrice = price,
                    Quantity = qty,
                    TotalPrice = price * qty,
                    Currency = "IRR",
                    Customer = customers[rnd.Next(customers.Length)],
                    Region = regions[rnd.Next(regions.Length)],
                    SaleDate = DateTime.Now.AddDays(-rnd.Next(0, 365)),
                    PaymentMethod = paymentMethods[rnd.Next(paymentMethods.Length)],
                    Status = statuses[rnd.Next(statuses.Length)],
                    Notes = "مثال " + i,
                    SalesPerson = salesPersons[rnd.Next(salesPersons.Length)]
                });
            }

            // --- فیلتر ---
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var f in request.Filters)
                {
                    if (!string.IsNullOrEmpty(f.Value))
                    {
                        // پیدا کردن Property بدون حساسیت به حروف بزرگ/کوچک
                        var prop = typeof(ProductSaleModel).GetProperties()
                                    .FirstOrDefault(p => string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase));

                        if (prop != null)
                        {
                            list = list.Where(x =>
                            {
                                var val = prop.GetValue(x)?.ToString() ?? "";
                                return val.Contains(f.Value, StringComparison.OrdinalIgnoreCase);
                            }).ToList();
                        }
                    }
                }
            }
            // --- سورت ---
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var prop = typeof(ProductSaleModel).GetProperty(request.SortColumn);
                if (prop != null)
                {
                    list = request.SortAsc
                        ? list.OrderBy(x => prop.GetValue(x)).ToList()
                        : list.OrderByDescending(x => prop.GetValue(x)).ToList();
                }
            }

            var totalCount = list.Count;

            // --- گروه‌بندی ---
            if (!string.IsNullOrEmpty(request.GroupBy))
            {
                var prop = typeof(ProductSaleModel).GetProperty(request.GroupBy);
                if (prop != null)
                {
                    var grouped = list
                        .GroupBy(x => prop.GetValue(x))
                        .Select(g => new
                        {
                            Key = g.Key?.ToString(),
                            Count = g.Count(),
                            Items = g.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList()
                        })
                        .ToList();

                    return Json(new
                    {
                        TotalCount = totalCount,
                        GroupBy = request.GroupBy,
                        Groups = grouped
                    });
                }
            }

            // --- پیجینگ ---
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // --- خروجی نهایی ---
            return Json(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = data
            });
        }

        public IActionResult GridInlineEdit()
        {
            return View();
        }


        // اکشن Fetch داده‌ها
        [HttpPost]
        public IActionResult GetGridData([FromBody] GridRequest request)
        {
            IQueryable<ProductModel> query = _products.AsQueryable();

            // فیلترها
            if (request.Filters != null)
            {
                foreach (var f in request.Filters)
                {
                    if (!string.IsNullOrEmpty(f.Value))
                    {
                        var prop = typeof(ProductModel).GetProperty(f.Key);
                        if (prop != null)
                        {
                            var propType = prop.PropertyType;
                            if (propType == typeof(string))
                                query = query.Where($"{f.Key}.Contains(@0)", f.Value);
                            else
                                query = query.Where($"{f.Key}.ToString().Contains(@0)", f.Value);
                        }
                        else
                        {
                            // ستون پیدا نشد، می‌توانید لاگ بگیرید یا از ادامه فیلتر بگذرید
                            continue;
                        }
                    }
                }
            }

            // مرتب‌سازی
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var sortExpr = request.SortAsc ? request.SortColumn : request.SortColumn + " descending";
                query = query.OrderBy(sortExpr);
            }

            // گروه‌بندی (برای ارسال به JS فقط نام گروه‌ها)
            List<object> dataList;
            if (!string.IsNullOrEmpty(request.GroupBy))
            {
                dataList = query
                 .AsEnumerable() // بعد از این خط داده‌ها در حافظه هستند
                 .GroupBy(x => {
                     var prop = x.GetType().GetProperty(request.GroupBy);
                     return prop != null ? prop.GetValue(x) : null;
                 })
                 .SelectMany(g => new object[] { new { __group = true, name = g.Key ?? "بدون مقدار" } }.Concat(g))
                 .ToList();
            }
            else
            {
                dataList = query.ToList<object>();
            }

            int totalPages = (int)Math.Ceiling((double)query.Count() / request.PageSize);

            // ستون‌ها برای GroupBy dropdown
            var columns = typeof(ProductModel).GetProperties()
                            .Select(p => new { name = p.Name, header = p.Name })
                            .ToList();

            return Json(new { data = dataList, totalPages, groupColumns = columns });
        }

        [HttpPost]
        public IActionResult Save([FromBody] ProductModel updated)
        {
            var existing = _products.FirstOrDefault(p => p.Id == updated.Id);
            if (existing != null)
            {
                existing.Name = updated.Name;
                existing.Category = updated.Category;
                existing.Price = updated.Price;
                existing.Stock = updated.Stock;
                existing.Supplier = updated.Supplier;
                existing.DateAdded = updated.DateAdded;
                existing.Status = updated.Status;
            }
            return Ok();
        }

        public IActionResult Index_GridViewInfiniteScroll()
        {
            List<ProductSaleModel> model = new List<ProductSaleModel>();
            return View(model);
        }


        [HttpGet]
        public IActionResult GetDataGrid(int page = 0, int pageSize = 50,
                                 string sortBy = "", bool sortAsc = true,
                                 string filter_Name = "", string filter_Category = "", string filter_Supplier = "")
        {
            List<ProductSaleModel> data = SetGridViewData(page, pageSize, sortBy, sortAsc, filter_Name, filter_Category);

            return Json(data);
        }

        private static List<ProductSaleModel> SetGridViewData(int page, int pageSize, string sortBy, bool sortAsc, string filter_Name, string filter_Category)
        {
            var rand = new Random();
            var list = new List<ProductSaleModel>();

            for (int i = 1; i <= 5000; i++)
            {
                var quantity = rand.Next(1, 20);
                var unitPrice = Math.Round((decimal)(rand.NextDouble() * 1000 + 50), 2);

                list.Add(new ProductSaleModel
                {
                    Id = i,
                    ProductName = Products[rand.Next(Products.Length)],
                    Category = Categories[rand.Next(Categories.Length)],
                    Supplier = Suppliers[rand.Next(Suppliers.Length)],
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    TotalPrice = unitPrice * quantity,
                    Currency = Currencies[rand.Next(Currencies.Length)],
                    Customer = Customers[rand.Next(Customers.Length)],
                    Region = Regions[rand.Next(Regions.Length)],
                    SaleDate = DateTime.Now.AddDays(-rand.Next(0, 365)),
                    PaymentMethod = PaymentMethods[rand.Next(PaymentMethods.Length)],
                    Status = Statuses[rand.Next(Statuses.Length)],
                    Notes = NotesList[rand.Next(NotesList.Length)],
                    SalesPerson = SalesPersons[rand.Next(SalesPersons.Length)]
                });
            }

            var query = list.AsQueryable();

            // فیلتر
            if (!string.IsNullOrEmpty(filter_Name))
                query = query.Where(x => x.ProductName.Contains(filter_Name));
            if (!string.IsNullOrEmpty(filter_Category))
                query = query.Where(x => x.Category.Contains(filter_Category));

            // سورت
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "Price")
                    query = sortAsc ? query.OrderBy(x => x.TotalPrice) : query.OrderByDescending(x => x.TotalPrice);
                else if (sortBy == "Name")
                    query = sortAsc ? query.OrderBy(x => x.ProductName) : query.OrderByDescending(x => x.ProductName);
                else if (sortBy == "SaleDate")
                    query = sortAsc ? query.OrderBy(x => x.SaleDate) : query.OrderByDescending(x => x.SaleDate);
            }

            // صفحه‌بندی
            List<ProductSaleModel> data = query.Skip(page * pageSize).Take(pageSize).ToList();
            return data;
        }
    }
}