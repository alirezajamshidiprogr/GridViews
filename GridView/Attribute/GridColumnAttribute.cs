[AttributeUsage(AttributeTargets.Property)]
public class GridColumnAttribute : Attribute
{
    public string Header { get; }
    public bool Editable { get; set; } 

    public bool Visible { get; }

    public GridColumnAttribute(string displayName, bool visible = true, bool editable = false)
    {
        Header = displayName;
        Visible = visible;
        Visible = visible;
    }
}
