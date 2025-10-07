namespace GridView.ViewModel
{
    public class PersonModel
    {
        [GridColumn("تامين كننده",visible: false)]

        public int Id { get; set; }
        [GridColumn("تامين كننده")]
        // شناسه
        public string FirstName { get; set; }      // نام
        [GridColumn("تامين كننده")]

        public string LastName { get; set; }       // نام خانوادگی
        [GridColumn("تامين كننده")]

        public int Age { get; set; }               // سن
        [GridColumn("تامين كننده")]

        public string Gender { get; set; }         // جنسیت
        [GridColumn("تامين كننده")]

        public string Email { get; set; }          // ایمیل
        [GridColumn("تامين كننده")]

        public string Phone { get; set; }          // شماره تماس
        [GridColumn("تامين كننده")]

        public string City { get; set; }           // شهر محل سکونت
    }
}
