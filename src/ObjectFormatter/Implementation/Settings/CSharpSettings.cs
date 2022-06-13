namespace ObjectFormatter.Implementation.Settings;

internal class CSharpSettings
{
    public bool ConvertDateTimeToUtc { get; set; } = true;
    public bool IgnoreNullValues { get; set; } = true;

    public bool IgnoreDefaultValues { get; set; } = true;

    public int MaxDepth { get; set; } = 100;

    public bool UseFullTypeName { get; set; } = false;
}