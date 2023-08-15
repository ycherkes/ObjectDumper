using Newtonsoft.Json;
using System;
using System.Net;
using System.Xml;
using YAXLib;
using YAXLib.Enums;
using YAXLib.KnownTypes;
using YAXLib.Options;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation.Settings;
using YellowFlavor.Serialization.Implementation.Xml;

namespace YellowFlavor.Serialization.Implementation;

internal class XmlSerializer : ISerializer
{
    private static SerializerOptions SerializerOptions => new()
    {
        ExceptionHandlingPolicies = YAXExceptionHandlingPolicies.ThrowErrorsOnly,
        ExceptionBehavior = YAXExceptionTypes.Ignore,
        SerializationOptions = YAXSerializationOptions.SuppressMetadataAttributes | YAXSerializationOptions.DontSerializeNullObjects | YAXSerializationOptions.DoNotSerializeDefaultValues,
        MaxRecursion = 25,
        TypeInspector = new CustomTypeInspector()
    };

    private static readonly string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine;

    public string Serialize(object obj, string settings)
    {
        if (obj == null)
        {
            return XmlHeader;
        }

        var xmlSettings = settings != null ? JsonConvert.DeserializeObject<XmlSettings>(settings) : new XmlSettings();

        var objType = obj.GetType();

        if (objType == typeof(DateTime))
        {
            var doc = new XmlDocument();
            var elem = doc.CreateElement(null, XmlConvert.EncodeName(objType.Name), null);
            doc.AppendChild(elem);
            elem.InnerText = JsonConvert.ToString((DateTime)obj, DateFormatHandling.IsoDateFormat, xmlSettings.DateTimeZoneHandling).Trim('"');
            
            return XmlHeader + doc.InnerXml;
        }

        var yaxSettings = GetYaxSettings(xmlSettings);

        if (!WellKnownTypes.TryGetKnownType(typeof(IPAddress), out _))
        {
            WellKnownTypes.Add(new IpAddressKnownType());
            WellKnownTypes.Add(new DateTimeOffsetKnownType());
        }

        var serializer = new YAXSerializer(obj.GetType(), yaxSettings);
        var serializedValue = serializer.Serialize(obj);
        return XmlHeader + serializedValue;
    }

    private static SerializerOptions GetYaxSettings(XmlSettings xmlSettings)
    {
        var newSettings = SerializerOptions;
        if (xmlSettings == null) return newSettings;

        newSettings.MaxRecursion = xmlSettings.MaxDepth > 0 ? xmlSettings.MaxDepth : 1;

        newSettings.SerializationOptions |= xmlSettings.IgnoreNullValues
            ? YAXSerializationOptions.DontSerializeNullObjects
            : YAXSerializationOptions.SerializeNullObjects;

        newSettings.SerializationOptions ^= xmlSettings.IgnoreNullValues
            ? YAXSerializationOptions.SerializeNullObjects
            : YAXSerializationOptions.DontSerializeNullObjects;

        if (xmlSettings.IgnoreDefaultValues)
        {
            newSettings.SerializationOptions |= YAXSerializationOptions.DoNotSerializeDefaultValues;
        }
        else
        {
            newSettings.SerializationOptions ^= YAXSerializationOptions.DoNotSerializeDefaultValues;
        }

        var namingStrategy = xmlSettings.NamingStrategy.ToPascalCase();

        Func<string, string> namingStrategyType = namingStrategy switch
        {
            "Default"   => name => name,
            "CamelCase" => name => name.ToCamelCase(),
            "KebabCase" => name => name.ToKebabCase(),
            "SnakeCase" => name => name.ToSnakeCase(),
            _ => throw new InvalidOperationException($"Invalid naming strategy: {namingStrategy}")
        };

        newSettings.TypeInspector = new CustomTypeInspector
        {
            NamingPolicy = namingStrategyType,
            DateTimeZoneHandling = xmlSettings.DateTimeZoneHandling
        };

        return newSettings;
    }
}