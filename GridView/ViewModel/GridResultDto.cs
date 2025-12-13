namespace GridView.ViewModel
{
    public class GridResultDto<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new();
        public string? GroupBy { get; set; }
        public bool enableLazyLoading { get; set; } = false; 
        public List<GridGroupDto<T>>? Groups { get; set; }
    }

    public class GridGroupDto<T>
    {
        public string? Key { get; set; }
        public int Count { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
