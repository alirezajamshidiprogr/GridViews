namespace GridView.ViewModel
{
    public class GridRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public bool SortAsc { get; set; }
        public Dictionary<string, string> Filters { get; set; }
        public string GroupBy { get; set; }
    }
}
