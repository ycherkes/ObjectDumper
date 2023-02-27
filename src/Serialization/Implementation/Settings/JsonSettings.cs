using Newtonsoft.Json;

namespace YellowFlavor.Serialization.Implementation.Settings;

internal class JsonSettings
{
    public bool IgnoreNullValues { get; set; } = true;

    public bool IgnoreDefaultValues { get; set; } = true;

    public int MaxDepth { get; set; } = 25;

    public TypeNameHandling TypeNameHandling { get; set; } = TypeNameHandling.None;

    public string NamingStrategy { get; set; } = "CamelCase";

    public bool SerializeEnumAsString { get; set; } = true;

    public DateTimeZoneHandling DateTimeZoneHandling { get; set; } = DateTimeZoneHandling.RoundtripKind;

}