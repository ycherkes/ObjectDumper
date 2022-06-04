namespace ObjectFormatter.Settings;

internal class VbSettings
{
    public bool IgnoreNullValues { get; set; } = true;

    public bool IgnoreDefaultValues { get; set; } = true;

    public int MaxDepth { get; set; } = 100;

    public bool UseFullTypeName { get; set; } = false;
}