﻿using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Embedded;
using Newtonsoft.Json.Embedded.Converters;
using Newtonsoft.Json.Embedded.Serialization;
using ObjectFormatter.Adapters;
using ObjectFormatter.ObjectDumper.NET.Embedded;
using ObjectFormatter.YamlDotNet.Embedded.Core;
using ObjectFormatter.YamlDotNet.Embedded.Serialization;
using DirectoryInfo = System.IO.DirectoryInfo;
using DriveInfo = System.IO.DriveInfo;
using FileInfo = System.IO.FileInfo;
using Formatting = Newtonsoft.Json.Embedded.Formatting;

namespace ObjectFormatter
{
    public static class Formatter
    {
        private static JsonSerializerSettings JsonSettings => new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new SpecificContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
                PropertyTypesToSkip = new []{ "Avro.Schema" }
            },
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 100
        };

        private static JsonSerializerSettings XmlSettings => new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new SpecificContractResolver
            {
                PropertyTypesToSkip = new[] { "Avro.Schema" }
            },
            Converters = { new StringEnumConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 100
        };

        private static DumpOptions CsharpDumpOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            IndentSize = 4,
            MaxLevel = 100,
            
        };

        private static DumpOptions VisualBasicDumpOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            IndentSize = 4,
            MaxLevel = 100
        };

        private static YamlSettings YamlSettings => new()
        {
            MaxDepth = 100
        };

        public static string Format(object obj, string formattingType, string settings = null)
        {
            //RefreshSettings();

            obj = obj switch
            {
                FileInfo info => FileInfoMapper.Map(info),
                DirectoryInfo directoryInfo => DirectoryInfoMapper.Map(directoryInfo),
                DriveInfo driveInfo => DriveInfoMapper.Map(driveInfo),
                _ => obj
            };

            try
            {
                return formattingType switch
                {
                    "json" => GetJson(obj, settings.FromBase64()).ToBase64(),
                    "cs" => GetCsharp(obj, settings.FromBase64()).ToBase64(),
                    "vb" => GetVisualBasic(obj, settings.FromBase64()).ToBase64(),
                    "xml" => GetXmlWithHeader(obj, settings.FromBase64()).ToBase64(),
                    "yaml" => GetYaml(obj, settings.FromBase64()).ToBase64(),
                    _ => obj?.ToString().ToBase64()
                };
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string GetVisualBasic(object obj, string settings)
        {
            return ObjectFormatterVisualBasic.Dump(obj, GetVbSettings(settings));
        }

        private static string GetCsharp(object obj, string settings)
        {
            return ObjectFormatterCSharp.Dump(obj, GetCsharpSettings(settings));
        }

        private static DumpOptions GetCsharpSettings(string settings)
        {
            var newSettings = CsharpDumpOptions;
            if (settings == null) return newSettings;

            var csharpSettings = JsonConvert.DeserializeObject<CSharpSettings>(settings);
            newSettings.IgnoreDefaultValues = csharpSettings.IgnoreDefaultValues;
            newSettings.IgnoreNullValues = csharpSettings.IgnoreNullValues;
            newSettings.UseTypeFullName = csharpSettings.UseFullTypeName;
            newSettings.MaxLevel = csharpSettings.MaxDepth;

            return newSettings;
        }

        private static DumpOptions GetVbSettings(string settings)
        {
            var newSettings = VisualBasicDumpOptions;
            if (settings == null) return newSettings;

            var vbSettings = JsonConvert.DeserializeObject<VbSettings>(settings);
            newSettings.IgnoreDefaultValues = vbSettings.IgnoreDefaultValues;
            newSettings.IgnoreNullValues = vbSettings.IgnoreNullValues;
            newSettings.UseTypeFullName = vbSettings.UseFullTypeName;
            newSettings.MaxLevel = vbSettings.MaxDepth;

            return newSettings;
        }

        private static string GetJson(object obj, string settings)
        {
            return JsonConvert.SerializeObject(obj, GetJsonSettings(settings));
        }

        private static JsonSerializerSettings GetJsonSettings(string settings)
        {
            var newSettings = JsonSettings;
            if (settings == null) return newSettings;

            var jsonSettings = JsonConvert.DeserializeObject<JsonSettings>(settings);
            newSettings.NullValueHandling = jsonSettings.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
            newSettings.DefaultValueHandling = jsonSettings.IgnoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include;
            newSettings.MaxDepth = jsonSettings.MaxDepth;
            newSettings.TypeNameHandling = jsonSettings.UseFullTypeName ? TypeNameHandling.All : TypeNameHandling.None;
            return newSettings;
        }

        private static JsonSerializerSettings GetXmlSettings(string settings)
        {
            var newSettings = XmlSettings;
            if (settings == null) return newSettings;

            var xmlSettings = JsonConvert.DeserializeObject<XmlSettings>(settings);
            newSettings.NullValueHandling = xmlSettings.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
            newSettings.DefaultValueHandling = xmlSettings.IgnoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include;
            newSettings.MaxDepth = xmlSettings.MaxDepth;
            newSettings.TypeNameHandling = xmlSettings.UseFullTypeName ? TypeNameHandling.All : TypeNameHandling.None;
            return newSettings;
        }

        private static string GetYaml(object obj, string settings)
        {
            var yamlSerializer = GetYamlSerializer(settings);
            var stringBuilder = new StringBuilder();
            yamlSerializer.Serialize(new IndentedTextWriter(new StringWriter(stringBuilder)), obj);
            return stringBuilder.ToString();
        }

        private static Serializer GetYamlSerializer(string settings)
        {
            var yamlSettings = GetYamlSettings(settings);

            return Serializer.FromValueSerializer(new SerializerBuilder().WithMaximumRecursion(yamlSettings.MaxDepth).BuildValueSerializer(), EmitterSettings.Default);
        }

        private static YamlSettings GetYamlSettings(string settings)
        {
            return settings == null 
                ? YamlSettings
                : JsonConvert.DeserializeObject<YamlSettings>(settings);
        }

        private static string GetXmlWithHeader(object obj, string settings)
        {
            var xmlSettings = GetXmlSettings(settings);
            return $"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}{GetXml(obj, xmlSettings)}";
        }

        private const int XmlIndentSize = 2;
        private static string GetXml(object obj, JsonSerializerSettings xmlSettings, int nestingLevel = 0)
        {
            string indent = new(' ', nestingLevel * XmlIndentSize);

            if (obj == null) return $"{indent}<!--NULL VALUE-->";

            var elementName = GetElementName(obj);

            if (obj is IEnumerable and not IDictionary and not string)
            {
                var xmlCollection = string.Join(Environment.NewLine, 
                    ((IEnumerable)obj)
                    .Cast<object>()
                    .Select(o => GetXml(o, xmlSettings, nestingLevel + 1)));

                var itemTypeName = obj.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    ?.GenericTypeArguments
                    .FirstOrDefault()
                    ?.Name;

                elementName = GetElementName(itemTypeName) ?? elementName;
                elementName = XmlConvert.EncodeName($"ArrayOf{elementName}");

                return $"<{elementName}>{Environment.NewLine}{xmlCollection}{Environment.NewLine}</{elementName}>";
            }

            var json = JsonConvert.SerializeObject(obj, xmlSettings);
            var objectType = obj.GetType();

            string xml;
            if (IsSimpleType(objectType))
            {
                elementName = GetElementName(objectType.Name);
                xml = $"<{elementName}>{json.Trim('"')}</{elementName}>";
            }
            else
                xml = JsonConvert.DeserializeXNode(json, elementName)?.ToString();

            if (nestingLevel == 0) return xml;

            return string.Join(Environment.NewLine,
                               xml?.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => $"{indent}{x}") ?? Enumerable.Empty<string>());
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                || type.IsValueType
                || type == typeof(string);
        }

        private static string GetElementName(object obj)
        {
            var typeName = obj.GetType().Name;
            return GetElementName(typeName);
        }

        private static string GetElementName(string typeName)
        {
            return typeName?.Contains("AnonymousType") == true
                ? "AnonymousType"
                : typeName;
        }
    }

    internal class JsonSettings
    {
        public bool IgnoreNullValues { get; set; } = true;

        public bool IgnoreDefaultValues { get; set; } = true;

        public int MaxDepth { get; set; } = 100;

        public bool UseFullTypeName { get; set; } = false;
    }

    internal class XmlSettings
    {
        public bool IgnoreNullValues { get; set; } = true;

        public bool IgnoreDefaultValues { get; set; } = true;

        public int MaxDepth { get; set; } = 100;

        public bool UseFullTypeName { get; set; } = false;
    }

    internal class CSharpSettings
    {
        public bool IgnoreNullValues { get; set; } = true;

        public bool IgnoreDefaultValues { get; set; } = true;

        public int MaxDepth { get; set; } = 100;

        public bool UseFullTypeName { get; set; } = false;
    }

    internal class VbSettings
    {
        public bool IgnoreNullValues { get; set; } = true;

        public bool IgnoreDefaultValues { get; set; } = true;

        public int MaxDepth { get; set; } = 100;

        public bool UseFullTypeName { get; set; } = false;
    }

    internal class YamlSettings
    {
        public int MaxDepth { get; set; } = 100;
    }
}
