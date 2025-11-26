[AttributeUsage(AttributeTargets.Property)]
public class GridColumnAttribute : Attribute
{
    public string Header { get; }
    public bool Editable { get; set; }
    public bool FirstDisplay { get; set; } = true;
    public bool Visible { get; } // نمايش يا عدم نمايش ستون
    public bool EnableFiltering { get; set; } = true; // فيلتر سرچ داشته باشه يا نه 
    public bool EnableSorting { get; set; } = true; // مرتب سازي داشته باشه يا نه
    public bool EnableGrouping { get; set; } = true;  // گروه بندي داشته باشه يا نه 
    public GridColumnAttribute(string displayName, bool visible = true, bool editable = false,bool filtering = true, bool sorting = true, bool grouping = true ,bool firstDisplay = true)
    {
        Header = displayName;
        Visible = visible;
        Visible = visible;
        Editable = editable;
        EnableGrouping= grouping;
        EnableSorting= sorting;
        EnableFiltering= filtering;
        FirstDisplay = firstDisplay;
    }
}
