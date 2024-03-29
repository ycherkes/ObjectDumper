﻿using Newtonsoft.Json;
using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation.Settings;
using YellowFlavor.Serialization.Implementation.Yaml;

namespace YellowFlavor.Serialization.Implementation;

internal class YamlSerializer : ISerializer
{
    private static YamlSettings YamlSettings => new()
    {
        MaxDepth = 25
    };

    private static Serializer GetYamlSerializer(string settings)
    {
        var yamlSettings = GetYamlSettings(settings);

        var namingConvention = yamlSettings.NamingConvention.ToPascalCase();

        var namingConventionInstance = namingConvention switch
        {
            "CamelCase" => CamelCaseNamingConvention.Instance,
            "Hyphenated" => HyphenatedNamingConvention.Instance,
            "LowerCase" => LowerCaseNamingConvention.Instance,
            "Null" => NullNamingConvention.Instance,
            "PascalCase" => PascalCaseNamingConvention.Instance,
            "Underscored" => UnderscoredNamingConvention.Instance,
            _ => throw new InvalidOperationException($"Invalid naming convention: {namingConvention}")
        };

        var valueSerializer = new SerializerBuilder()
            .WithNamingConvention(namingConventionInstance)
            .WithMaximumRecursion(yamlSettings.MaxDepth)
            .WithTypeConverter(new IpAddressConverter())
            .WithTypeInspector(ti => new IgnoreDelegatesTypeInspector(ti))
            .BuildValueSerializer();

        return Serializer.FromValueSerializer(valueSerializer, EmitterSettings.Default);
    }

    private static YamlSettings GetYamlSettings(string settings)
    {
        return settings == null
            ? YamlSettings
            : JsonConvert.DeserializeObject<YamlSettings>(settings);
    }

    public void Serialize(object obj, string settings, TextWriter textWriter)
    {
        var yamlSerializer = GetYamlSerializer(settings);
        yamlSerializer.Serialize(textWriter, obj);
    }
}