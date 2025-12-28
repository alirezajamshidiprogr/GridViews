using GridView.CoreServiceProviders;
using GridView.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public static class GridExtensions
{
    // نرمال‌سازی فارسی
    public static string NormalizePersian(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string normalized = input;

        normalized = normalized
            .Trim()
            .Replace('ي', 'ی')
            .Replace('ى', 'ی')
            .Replace('ئ', 'ی')
            .Replace('ك', 'ک')
            .Replace('ة', 'ه')
            .Replace('ۀ', 'ه')
            .Replace('أ', 'ا')
            .Replace('إ', 'ا')
            .Replace('آ', 'ا')
            .Replace('ؤ', 'و')
            .Replace("‌", "")
            .Replace("\u200C", "")
            .Replace("\u200F", "")
            .Replace("\u202B", "")
            .Replace("\u202A", "")
            .Replace("\u202C", "")
            .ToLowerInvariant();

        normalized = normalized
            .Replace('٠', '۰')
            .Replace('١', '۱')
            .Replace('٢', '۲')
            .Replace('٣', '۳')
            .Replace('٤', '۴')
            .Replace('٥', '۵')
            .Replace('٦', '۶')
            .Replace('٧', '۷')
            .Replace('٨', '۸')
            .Replace('٩', '۹');

        var diacritics = new[] { '\u064B', '\u064C', '\u064D', '\u064E', '\u064F', '\u0650', '\u0651', '\u0652' };
        foreach (var d in diacritics)
            normalized = normalized.Replace(d.ToString(), "");

        normalized = normalized
            .Replace("\u200C", " ")
            .Replace("\u200F", "")
            .Replace("\u202A", "")
            .Replace("\u202B", "")
            .Replace("\u202C", "")
            .Replace("\u202D", "")
            .Replace("\u202E", "")
            .Trim();

        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized;
    }

    // Extension برای تبدیل JSON به آبجکت
    public static T FromJson<T>(this string json)
    {
        if (string.IsNullOrEmpty(json)) return default;
        return JsonSerializer.Deserialize<T>(json);
    }

    // Extension جدید برای تبدیل هدر GridRequest به آبجکت با کنترل خطا
    // 
    public static GridRequest FromGridRequestHeader(this string requestJson)
    {
        if (!string.IsNullOrEmpty(requestJson))
        {
            try
            {
                // 🔐 Decode Base64 → UTF8 → JSON string
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(requestJson));
                return JsonSerializer.Deserialize<GridRequest>(json) ?? new GridRequest();
            }
            catch
            {
                return new GridRequest();
            }
        }
        return new GridRequest();
    }

    public static GridResultDto<T> ToEorc_GridResultEnumarable<T>(this IEnumerable<T> source)
    {
        var httpContextAccessor = CoreServiceProviders.serviceProvider.GetService<IHttpContextAccessor>();
        var headerValue = httpContextAccessor?.HttpContext?.Request.Headers["GridRequest"].FirstOrDefault();
        var request = headerValue.FromGridRequestHeader();

        if (source == null) source = Enumerable.Empty<T>();

        var query = source.AsEnumerable();

        // فیلترها
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (request.Filters != null)
        {
            foreach (var f in request.Filters)
            {
                if (f.Value == null || string.IsNullOrEmpty(f.Value.Value)) continue;

                // 🧩 نرمال‌سازی نام ستون برای پیدا شدن در مدل
                var prop = props.FirstOrDefault(p =>
                    string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name.Replace("_", ""), f.Key.Replace("_", ""), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name.Replace("-", ""), f.Key.Replace("-", ""), StringComparison.OrdinalIgnoreCase));

                if (prop == null) continue;

                query = query.Where(x =>
                {
                    var rawVal = prop.GetValue(x);
                    if (rawVal == null) return false;

                    var val = rawVal.ToString().NormalizePersian();
                    var filterVal = (f.Value.Value ?? "").NormalizePersian();

                    return f.Value.Type switch
                    {
                        "eq" => val.Equals(filterVal, StringComparison.OrdinalIgnoreCase),
                        "neq" => !val.Equals(filterVal, StringComparison.OrdinalIgnoreCase),
                        "gt" => decimal.TryParse(val, out var v1) && decimal.TryParse(filterVal, out var f1) && v1 > f1,
                        "lt" => decimal.TryParse(val, out var v2) && decimal.TryParse(filterVal, out var f2) && v2 < f2,
                        "startswith" => val.StartsWith(filterVal, StringComparison.OrdinalIgnoreCase),
                        "endswith" => val.EndsWith(filterVal, StringComparison.OrdinalIgnoreCase),
                        "contains" => val.Contains(filterVal, StringComparison.OrdinalIgnoreCase),
                        _ => true
                    };
                });
            }
        }

        // سورت
        if (!string.IsNullOrEmpty(request.SortColumn))
        {
            var prop = typeof(T).GetProperty(request.SortColumn);
            if (prop != null)
            {
                query = request.SortAsc
                ? query.OrderBy(x => prop.GetValue(x) ?? "")
                : query.OrderByDescending(x => prop.GetValue(x) ?? "");
            }
        }

        var totalCount = query.Count();

        int page = request.Page <= 0 ? 1 : request.Page;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        // گروه‌بندی
        if (!string.IsNullOrEmpty(request.GroupBy))
        {
            var prop = typeof(T).GetProperty(request.GroupBy);
            if (prop != null)
            {
                var grouped = query
                   .GroupBy(x => prop.GetValue(x) ?? "")
                    .Select(g => new GridGroupDto<T>
                    {
                        Key = g.Key?.ToString() ?? "(ناشناخته)",
                        Count = g.Count(),
                        Items = g.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                    })
                    .ToList();

                return new GridResultDto<T>
                {
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    GroupBy = request.GroupBy,
                    Groups = grouped
                };
            }
        }

        // پیجینگ
        var items = request.enablePaging
            ? query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            : query.ToList();

        return new GridResultDto<T>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }
    public static async Task<GridResultDto<T>> GetGridDataEfCoreAsync<T>(IQueryable<T> query) where T : class
    {
        try
        {
            var httpContextAccessor = CoreServiceProviders.serviceProvider.GetService<IHttpContextAccessor>();
            var headerValue = httpContextAccessor?.HttpContext?.Request.Headers["GridRequest"].FirstOrDefault();
            var request = headerValue?.FromGridRequestHeader();


            if (query == null) query = Enumerable.Empty<T>().AsQueryable();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // فیلترها
            if (request.Filters != null)
            {
                foreach (var f in request.Filters)
                {
                    if (f.Value == null || string.IsNullOrEmpty(f.Value.Value)) continue;

                    var prop = props.FirstOrDefault(p => string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop == null) continue;

                    var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (underlyingType == typeof(string))
                    {
                        var val = f.Value.Value.NormalizePersian();
                        switch (f.Value.Type)
                        {
                            case "contains":
                                query = query.Where(x => EF.Functions.Like(EF.Property<string>(x, prop.Name), $"%{val}%"));
                                break;
                            case "eq":
                                query = query.Where(x => EF.Property<string>(x, prop.Name) == val);
                                break;
                            case "neq":
                                query = query.Where(x => EF.Property<string>(x, prop.Name) != val);
                                break;
                            case "startswith":
                                query = query.Where(x => EF.Functions.Like(EF.Property<string>(x, prop.Name), $"{val}%"));
                                break;
                            case "endswith":
                                query = query.Where(x => EF.Functions.Like(EF.Property<string>(x, prop.Name), $"%{val}"));
                                break;
                        }
                    }
                    else if (underlyingType == typeof(int))
                    {
                        if (int.TryParse(f.Value.Value, out var val))
                        {
                            switch (f.Value.Type)
                            {
                                case "eq": query = query.Where(x => EF.Property<int>(x, prop.Name) == val); break;
                                case "neq": query = query.Where(x => EF.Property<int>(x, prop.Name) != val); break;
                                case "gt": query = query.Where(x => EF.Property<int>(x, prop.Name) > val); break;
                                case "lt": query = query.Where(x => EF.Property<int>(x, prop.Name) < val); break;
                            }
                        }
                    }
                    else if (underlyingType == typeof(decimal))
                    {
                        if (decimal.TryParse(f.Value.Value, out var val))
                        {
                            switch (f.Value.Type)
                            {
                                case "eq": query = query.Where(x => EF.Property<decimal>(x, prop.Name) == val); break;
                                case "neq": query = query.Where(x => EF.Property<decimal>(x, prop.Name) != val); break;
                                case "gt": query = query.Where(x => EF.Property<decimal>(x, prop.Name) > val); break;
                                case "lt": query = query.Where(x => EF.Property<decimal>(x, prop.Name) < val); break;
                            }
                        }
                    }
                    else if (underlyingType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(f.Value.Value, out var val))
                        {
                            switch (f.Value.Type)
                            {
                                case "eq": query = query.Where(x => EF.Property<DateTime>(x, prop.Name) == val); break;
                                case "neq": query = query.Where(x => EF.Property<DateTime>(x, prop.Name) != val); break;
                                case "gt": query = query.Where(x => EF.Property<DateTime>(x, prop.Name) > val); break;
                                case "lt": query = query.Where(x => EF.Property<DateTime>(x, prop.Name) < val); break;
                            }
                        }
                    }
                }
            }

            // سورت
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var prop = typeof(T).GetProperty(request.SortColumn);
                if (prop != null)
                {
                    query = request.SortAsc
                        ? query.OrderBy(x => EF.Property<object>(x, prop.Name))
                        : query.OrderByDescending(x => EF.Property<object>(x, prop.Name));
                }
            }

            var totalCount = await query.CountAsync();

            int page = request.Page <= 0 ? 1 : request.Page;
            int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var items = request.enablePaging
                ? await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()
                : await query.ToListAsync();

            return new GridResultDto<T>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = items
            };
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    public static async Task<GridResultDto<T>> GetGridDataFromSPAsync<T>(string spName,string connection, object? searchModel = null) where T : class, new()
    {
        try
        {
            // استفاده از connection pool به جای باز و بسته کردن مکرر اتصال
            using SqlConnection conn = new SqlConnection(connection);
            await conn.OpenAsync();

            var httpContextAccessor = CoreServiceProviders.serviceProvider.GetService<IHttpContextAccessor>();
            var headerValue = httpContextAccessor?.HttpContext?.Request.Headers["GridRequest"].FirstOrDefault();

            // اگر headerValue وجود داشته باشد، اطلاعات را از آن می‌گیریم
            var request = headerValue?.FromGridRequestHeader();

            if (request == null)
            {
                // اگر هیچ درخواست معتبر نداشتیم، می‌توانیم یک response خالی یا خطا برگردانیم
                return new GridResultDto<T> { Items = new List<T>(), TotalCount = 0, Page = 1, PageSize = 10 };
            }

            using var cmd = new SqlCommand(spName, conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            // اضافه کردن پارامترها به SP
            cmd.Parameters.AddWithValue("@Page", request.Page <= 0 ? 1 : request.Page);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize <= 0 ? 10 : request.PageSize);
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn ?? "");
            cmd.Parameters.AddWithValue("@SortAsc", request.SortAsc);
            cmd.Parameters.AddWithValue("@Filters", JsonSerializer.Serialize(request.Filters ?? new Dictionary<string, GridFilter>()));

            // ---------- Dynamic Search Parameters User ----------
            if (searchModel != null)
            {
                var properties = searchModel.GetType().GetProperties();

                foreach (var prop in properties)
                {
                    var value = prop.GetValue(searchModel);

                    // اگر null بود، ارسال نکن
                    if (value == null)
                        continue;

                    // اگر string خالی بود => null (DBNull)
                    if (value is string str && string.IsNullOrWhiteSpace(str))
                        value = DBNull.Value;

                    var paramName = "@" + prop.Name;
                    cmd.Parameters.AddWithValue(paramName, value);
                }
            }

            var list = new List<T>();
            int totalCount = 0;

            using var reader = await cmd.ExecuteReaderAsync();

            // خواندن نتایج داده‌ها
            while (await reader.ReadAsync())
            {
                var obj = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull) continue;

                    var value = reader[prop.Name];
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    try
                    {
                        // تبدیل مقدار SQL به نوع Property
                        var safeValue = Convert.ChangeType(value, propType);
                        prop.SetValue(obj, safeValue);
                    }
                    catch
                    {
                        // اگر تبدیل نشد می‌توانیم مقدار پیش‌فرض بدیم یا خطا نگیریم
                        prop.SetValue(obj, prop.PropertyType.IsValueType ? Activator.CreateInstance(propType) : null);
                    }
                }
                list.Add(obj);
            }

            // فرض بر این که SP نتیجه TotalCount رو به صورت جداگانه می‌ده
            if (reader.NextResult())
            {
                if (await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }
            }

            return new GridResultDto<T>
            {
                Items = list,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                GroupBy = request.GroupBy,
            };
        }
        catch (Exception ex)
        {

            throw;
        }
    }


    // اکستنشن کمکی برای بررسی ستون در SqlDataReader
    public static bool HasColumn(this SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    public static async Task<T> ReadRequestBodyAsync<T>(HttpRequest request) where T : new()
    {
        if (request == null || request.ContentLength == null || request.ContentLength == 0)
            return new T();

        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
            return new T();

        try
        {
            return JsonSerializer.Deserialize<T>(body) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    public static GridResultDto<T> ToGridResultIQuarable<T>(this IEnumerable<T> source)
    {
        // تبدیل به IQueryable اگر ممکن باشه
        var queryable = source as IQueryable<T>;

        var httpContextAccessor = CoreServiceProviders.serviceProvider.GetService<IHttpContextAccessor>();
        var headerValue = httpContextAccessor?.HttpContext?.Request.Headers["GridRequest"].FirstOrDefault();
        var request = headerValue.FromGridRequestHeader();

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        bool isEfQueryable = queryable != null;

        // فیلترها
        if (request.Filters != null)
        {
            foreach (var f in request.Filters)
            {
                if (f.Value == null || string.IsNullOrEmpty(f.Value.Value)) continue;

                var prop = props.FirstOrDefault(p =>
                    string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase));

                if (prop == null) continue;

                // رشته
                if (prop.PropertyType == typeof(string))
                {
                    if (isEfQueryable)
                    {
                        // EF Core translation
                        var filterVal = f.Value.Value;
                        switch (f.Value.Type)
                        {
                            case "eq":
                                queryable = queryable.Where(x => EF.Property<string>(x, prop.Name) == filterVal);
                                break;
                            case "neq":
                                queryable = queryable.Where(x => EF.Property<string>(x, prop.Name) != filterVal);
                                break;
                            case "contains":
                                queryable = queryable.Where(x => EF.Functions.Like(EF.Property<string>(x, prop.Name), $"%{filterVal}%"));
                                break;
                            case "startswith":
                                queryable = queryable.Where(x => EF.Functions.Like(EF.Property<string>(x, prop.Name), $"{filterVal}%"));
                                break;
                            case "endswith":
                                queryable = queryable.Where(x => EF.Functions.Like(EF.Property<string>(x, prop.Name), $"%{filterVal}"));
                                break;
                        }
                    }
                    else
                    {
                        // IEnumerable / in-memory filtering
                        var filterVal = f.Value.Value.NormalizePersian();
                        switch (f.Value.Type)
                        {
                            case "eq":
                                source = source.Where(x => (prop.GetValue(x)?.ToString().NormalizePersian() ?? "") == filterVal);
                                break;
                            case "neq":
                                source = source.Where(x => (prop.GetValue(x)?.ToString().NormalizePersian() ?? "") != filterVal);
                                break;
                            case "contains":
                                source = source.Where(x => (prop.GetValue(x)?.ToString().NormalizePersian() ?? "").Contains(filterVal));
                                break;
                            case "startswith":
                                source = source.Where(x => (prop.GetValue(x)?.ToString().NormalizePersian() ?? "").StartsWith(filterVal));
                                break;
                            case "endswith":
                                source = source.Where(x => (prop.GetValue(x)?.ToString().NormalizePersian() ?? "").EndsWith(filterVal));
                                break;
                        }
                    }
                }
                // عدد
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(double))
                {
                    if (decimal.TryParse(f.Value.Value, out var filterVal))
                    {
                        if (isEfQueryable)
                        {
                            switch (f.Value.Type)
                            {
                                case "eq":
                                    queryable = queryable.Where(x => EF.Property<decimal>(x, prop.Name) == filterVal);
                                    break;
                                case "neq":
                                    queryable = queryable.Where(x => EF.Property<decimal>(x, prop.Name) != filterVal);
                                    break;
                                case "gt":
                                    queryable = queryable.Where(x => EF.Property<decimal>(x, prop.Name) > filterVal);
                                    break;
                                case "lt":
                                    queryable = queryable.Where(x => EF.Property<decimal>(x, prop.Name) < filterVal);
                                    break;
                            }
                        }
                        else
                        {
                            switch (f.Value.Type)
                            {
                                case "eq":
                                    source = source.Where(x => Convert.ToDecimal(prop.GetValue(x) ?? 0) == filterVal);
                                    break;
                                case "neq":
                                    source = source.Where(x => Convert.ToDecimal(prop.GetValue(x) ?? 0) != filterVal);
                                    break;
                                case "gt":
                                    source = source.Where(x => Convert.ToDecimal(prop.GetValue(x) ?? 0) > filterVal);
                                    break;
                                case "lt":
                                    source = source.Where(x => Convert.ToDecimal(prop.GetValue(x) ?? 0) < filterVal);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // سورت
        if (!string.IsNullOrEmpty(request.SortColumn))
        {
            var prop = typeof(T).GetProperty(request.SortColumn);
            if (prop != null)
            {
                if (isEfQueryable)
                {
                    queryable = request.SortAsc
                        ? queryable.OrderBy(x => EF.Property<object>(x, prop.Name))
                        : queryable.OrderByDescending(x => EF.Property<object>(x, prop.Name));
                }
                else
                {
                    source = request.SortAsc
                        ? source.OrderBy(x => prop.GetValue(x))
                        : source.OrderByDescending(x => prop.GetValue(x));
                }
            }
        }

        var totalCount = isEfQueryable ? queryable.Count() : source.Count();
        int page = request.Page <= 0 ? 1 : request.Page;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        // گروه‌بندی
        if (!string.IsNullOrEmpty(request.GroupBy))
        {
            var prop = typeof(T).GetProperty(request.GroupBy);
            if (prop != null)
            {
                var grouped = (isEfQueryable ? queryable.AsEnumerable() : source)
                    .GroupBy(x => prop.GetValue(x) ?? "")
                    .Select(g => new GridGroupDto<T>
                    {
                        Key = g.Key?.ToString() ?? "(ناشناخته)",
                        Count = g.Count(),
                        Items = g.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                    })
                    .ToList();

                return new GridResultDto<T>
                {
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    GroupBy = request.GroupBy,
                    Groups = grouped
                };
            }
        }

        // پیجینگ
        var items = request.enablePaging
            ? (isEfQueryable ? queryable.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                            : source.Skip((page - 1) * pageSize).Take(pageSize).ToList())
            : (isEfQueryable ? queryable.ToList() : source.ToList());

        return new GridResultDto<T>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }
}
