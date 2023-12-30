using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation.Json;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation;

internal class JsonSerializer : ISerializer
{
    private static readonly JsonConverter StringEnumConverter = new StringEnumConverter();
    private static JsonSerializerSettings JsonSettings => new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        ContractResolver = new SpecificContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
            ExcludeTypes = ["Avro.Schema"]
        },
        Formatting = Formatting.Indented,
        Converters = { StringEnumConverter, new IpAddressConverter() },
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
        MaxDepth = 25
    };

    private static JsonSerializerSettings GetJsonSettings(string settings)
    {
        var newSettings = JsonSettings;
        if (settings == null) return newSettings;

        var jsonSettings = JsonConvert.DeserializeObject<JsonSettings>(settings);
        newSettings.NullValueHandling = jsonSettings.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
        newSettings.DefaultValueHandling = jsonSettings.IgnoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include;
        newSettings.MaxDepth = jsonSettings.MaxDepth;
        newSettings.TypeNameHandling = jsonSettings.TypeNameHandling;
        newSettings.DateTimeZoneHandling = jsonSettings.DateTimeZoneHandling;

        if (!jsonSettings.SerializeEnumAsString)
        {
            newSettings.Converters.Remove(StringEnumConverter);
        }

        var namingStrategy = jsonSettings.NamingStrategy.ToPascalCase();

        var namingStrategyType = namingStrategy switch
        {
            "CamelCase" => typeof(CamelCaseNamingStrategy),
            "Default" => typeof(DefaultNamingStrategy),
            "KebabCase" => typeof(KebabCaseNamingStrategy),
            "SnakeCase" => typeof(SnakeCaseNamingStrategy),
            _ => throw new InvalidOperationException($"Invalid naming strategy: {namingStrategy}")
        };

        if (namingStrategy != "CamelCase")
        {
            newSettings.ContractResolver = new SpecificContractResolver
            {
                NamingStrategy = (NamingStrategy)Activator.CreateInstance(namingStrategyType),
                ExcludeTypes = ["Avro.Schema"]
            };
        }

        return newSettings;
    }

    public void Serialize(object obj, string settings, TextWriter textWriter)
    {
        var serializer = Newtonsoft.Json.JsonSerializer.CreateDefault(GetJsonSettings(settings));
        serializer.Serialize(textWriter, obj);
    }
}