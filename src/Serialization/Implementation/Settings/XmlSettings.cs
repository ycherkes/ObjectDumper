using Newtonsoft.Json;

namespace YellowFlavor.Serialization.Implementation.Settings;

internal class XmlSettings
{
    public bool IgnoreNullValues { get; set; } = true;
    public bool IgnoreDefaultValues { get; set; } = true;
    public int MaxDepth { get; set; } = 25;
    public string NamingStrategy { get; set; } = "Default";
    public DateTimeZoneHandling DateTimeZoneHandling { get; set; } = DateTimeZoneHandling.RoundtripKind;
}