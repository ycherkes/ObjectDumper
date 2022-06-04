namespace ObjectFormatter.Settings;

internal class XmlSettings
{
    public bool IgnoreNullValues { get; set; } = true;

    public bool IgnoreDefaultValues { get; set; } = true;

    public int MaxDepth { get; set; } = 100;

    public bool UseFullTypeName { get; set; } = false;

    public string NamingStrategy { get; set; } = "Default";

    public bool SerializeEnumAsString { get; set; } = true;
}