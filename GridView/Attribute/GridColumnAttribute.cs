[AttributeUsage(AttributeTargets.Property)]
public class GridColumnAttribute : Attribute
{
    public string Header { get; }
    public bool Visible { get; }

    public GridColumnAttribute(string displayName, bool visible = true)
    {
        Header = displayName;
        Visible = visible;
    }
}
