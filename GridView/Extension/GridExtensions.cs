using GridView.CoreServiceProviders;
using GridView.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

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

    public static GridResultDto<T> ToGridResult<T>(this IEnumerable<T> source)
    {
        // گرفتن IHttpContextAccessor از سرویس پرووایدر
        //var httpContextAccessor = CoreServiceProviders.serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

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
                    if (rawVal == null) return false; // 🚫 اگر مقدار null بود حذفش کن

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
}
