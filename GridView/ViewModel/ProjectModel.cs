namespace GridView.ViewModel
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Owner { get; set; }
        public string Start { get; set; } // یا DateTime اگر خواستی
        public string End { get; set; }
        public string Status { get; set; }    // وضعیت (فعال، در انتظار، انجام‌شده)
    }
}
