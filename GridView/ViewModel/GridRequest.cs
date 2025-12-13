using Microsoft.AspNetCore.Mvc.Filters;

namespace GridView.ViewModel
{
    public class GridRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public bool SortAsc { get; set; }
        public bool lazyLoading { get; set; } = false; // ← این اضافه شد
        public Dictionary<string, GridFilter> Filters { get; set; } = new();
        public string GroupBy { get; set; }
        public bool enablePaging { get; set; }
    }

    public class GridFilter
    {
        public string Type { get; set; } = "contains";
        public string Value { get; set; } = "";
    }


}
