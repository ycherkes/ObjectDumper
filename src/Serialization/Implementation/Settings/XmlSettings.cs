namespace YellowFlavor.Serialization.Implementation.Settings;

internal class XmlSettings
{
    public bool IgnoreNullValues { get; set; } = true;

    public int MaxDepth { get; set; } = 25;
}