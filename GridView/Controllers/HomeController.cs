using GridView.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GridView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            var categories = new[] { "الکترونیک", "پوشاک", "خانه" };
            var suppliers = new[] { "تامین‌کننده A", "تامین‌کننده B", "تامین‌کننده C" };
            var statusList = new[] { "فعال", "غیرفعال" };
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
                var a = requestDto.SearchTerm;
            }
            // پاس پارامتر ها به ليست
            IQueryable<ProductSaleModel> list = GetBaseDataFromSql();

            //GridResultDto<ProductSaleModel> result = list.ToGridResultIQuarable();
            GridResultDto<ProductSaleModel> result = list.ToEorc_GridResultEnumarable();
            return Json(result);
        }

        private static IQueryable<ProductSaleModel> GetBaseDataFromSql()
        {
            var list = new List<ProductSaleModel>();

            string connectionString = "Server=SAP-16;Database=GridViewSample;User ID=sa;Password=137011;TrustServerCertificate=True;";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = $"SELECT * FROM ProductSales"; // تصادفی بخونه

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new ProductSaleModel
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    Supplier = reader.GetString(reader.GetOrdinal("Supplier")),
                                    TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                                    Currency = reader.GetString(reader.GetOrdinal("Currency")),
                                    Customer = reader.GetString(reader.GetOrdinal("Customer")),
                                    Region = reader.GetString(reader.GetOrdinal("Region")),
                                    SaleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate")).ToShortDateString(),
                                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Notes = reader.GetString(reader.GetOrdinal("Notes")),
                                    SalesPerson = reader.GetString(reader.GetOrdinal("SalesPerson"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return list.AsQueryable();
        }



        private JsonResult GetGridData_PersonModel(GridRequest request)
        {
            Random random = new Random();

            var firstNames = new[] { "علی", "زهرا", "حسین", "سمیه", "رضا", "لیلا", "مهدی", "سارا", "امیر", "فاطمه" };
            var lastNames = new[] { "محمدی", "رضایی", "کریمی", "حسینی", " احمدی", "فراهانی", "نجفی", "سعیدی", "عباسی", "صادقی" };
            var genders = new[] { "مرد", "زن" };
            var cities = new[] { "تهران", "مشهد", "شیراز", "اصفهان", "تبریز", "کرج", "قم", "کرمان", "اراک", "اهواز" };


            var list = new List<PersonModel>();
            for (int i = 1; i <= 1000; i++)
            {
                var person = new PersonModel
                {
                    Id = i,
                    FirstName = firstNames[random.Next(firstNames.Length)],
                    LastName = lastNames[random.Next(lastNames.Length)],
                    Age = random.Next(18, 70),
                    Gender = genders[random.Next(genders.Length)],
                    Email = $"user{i}@example.com",
                    Phone = $"09{random.Next(10000000, 99999999)}",
                    City = cities[random.Next(cities.Length)]
                };
                list.Add(person);
            }

            // --- فیلتر ---
            if (request.Filters != null && request.Filters.Any())
            {
                //foreach (var f in request.Filters)
                //{
                //    if (!string.IsNullOrEmpty(f.Value))
                //    {
                //        // پیدا کردن Property بدون حساسیت به حروف بزرگ/کوچک
                //        var prop = typeof(PersonModel).GetProperties()
                //                    .FirstOrDefault(p => string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase));

                //        if (prop != null)
                //        {
                //            list = list.Where(x =>
                //            {
                //                var val = prop.GetValue(x)?.ToString() ?? "";
                //                return val.Contains(f.Value, StringComparison.OrdinalIgnoreCase);
                //            }).ToList();
                //        }
                //    }
                //}


                foreach (var f in request.Filters)
                {
                    var prop = typeof(PersonModel).GetProperties()
                                .FirstOrDefault(p => string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase));

                    //if (prop != null)
                    //{
                    //    string type = f.Value.Type ?? "contains";
                    //    string val = f.Value.Value ?? "";

                    //    list = list.Where(x =>
                    //    {
                    //        var propVal = prop.GetValue(x)?.ToString() ?? "";

                    //        switch (type)
                    //        {
                    //            case "eq": return string.Equals(propVal, val, StringComparison.OrdinalIgnoreCase);
                    //            case "neq": return !string.Equals(propVal, val, StringComparison.OrdinalIgnoreCase);
                    //            case "gt":
                    //                if (double.TryParse(propVal, out var n1) && double.TryParse(val, out var n2))
                    //                    return n1 > n2;
                    //                return false;
                    //            case "lt":
                    //                if (double.TryParse(propVal, out var n3) && double.TryParse(val, out var n4))
                    //                    return n3 < n4;
                    //                return false;
                    //            case "startswith": return propVal.StartsWith(val, StringComparison.OrdinalIgnoreCase);
                    //            case "endswith": return propVal.EndsWith(val, StringComparison.OrdinalIgnoreCase);
                    //            case "contains":
                    //            default: return propVal.Contains(val, StringComparison.OrdinalIgnoreCase);
                    //        }
                    //    }).ToList();
                    //}
                }

            }
            // --- سورت ---
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var prop = typeof(PersonModel).GetProperty(request.SortColumn);
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
                var prop = typeof(PersonModel).GetProperty(request.GroupBy);
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
            int page = 0;
            int pageSize = 0;
            List<PersonModel> data;

            // --- پیجینگ ---
            if (request.enablePaging)
            {
                page = request.Page <= 0 ? 1 : request.Page;
                pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
                data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                page = 1;
                pageSize = list.Count();
                data = list;
            }
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

        public IActionResult Index_GridViewInfiniteScroll()
        {
            List<ProductSaleModel> model = new List<ProductSaleModel>();
            return View(model);
        }
    }
}