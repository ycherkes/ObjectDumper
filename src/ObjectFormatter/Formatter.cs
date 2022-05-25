using System;
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
using FileInfo = System.IO.FileInfo;
using Formatting = Newtonsoft.Json.Embedded.Formatting;

namespace ObjectFormatter
{
    public static class Formatter
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
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

        private static readonly JsonSerializerSettings XmlSettings = new()
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

        private static readonly DumpOptions CsharpDumpOptions = new()
        {
            IgnoreDefaultValues = true,
            IndentSize = 4,
            MaxLevel = 100
        };

        private static readonly DumpOptions VisualBasicDumpOptions = new()
        {
            IgnoreDefaultValues = true,
            IndentSize = 4,
            MaxLevel = 100
        };

        private static readonly Serializer YamlSerializer = Serializer.FromValueSerializer(new SerializerBuilder().WithMaximumRecursion(100).BuildValueSerializer(), EmitterSettings.Default);

        public static string Format(object obj, string formattingType)
        {
            obj = obj switch
            {
                FileInfo info => FileInfoMapper.Map(info),
                DirectoryInfo directoryInfo => DirectoryInfoMapper.Map(directoryInfo),
                _ => obj
            };

            try
            {
                return formattingType switch
                {
                    "json" => JsonConvert.SerializeObject(obj, JsonSettings).ToBase64(),
                    "csharp" => ObjectFormatterCSharp.Dump(obj, CsharpDumpOptions).ToBase64(),
                    "vb" => ObjectFormatterVisualBasic.Dump(obj, VisualBasicDumpOptions).ToBase64(),
                    "xml" => GetXmlWithHeader(obj).ToBase64(),
                    "yaml" => GetYaml(obj).ToBase64(),
                    _ => obj?.ToString().ToBase64()
                };
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string GetYaml(object o)
        {
            var stringBuilder = new StringBuilder();
            YamlSerializer.Serialize(new IndentedTextWriter(new StringWriter(stringBuilder)), o);
            return stringBuilder.ToString();
        }

        private static string GetXmlWithHeader(object obj)
        {
            return $"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}{GetXml(obj)}";
        }

        private const int XmlIndentSize = 2;
        private static string GetXml(object obj, int nestingLevel = 0)
        {
            string indent = new(' ', nestingLevel * XmlIndentSize);

            if (obj == null) return $"{indent}<!--NULL VALUE-->";

            var elementName = GetElementName(obj);

            if (obj is IEnumerable and not IDictionary and not string)
            {
                var xmlCollection = string.Join(Environment.NewLine, 
                    ((IEnumerable)obj)
                    .Cast<object>()
                    .Select(o => GetXml(o, nestingLevel + 1)));

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

            var json = JsonConvert.SerializeObject(obj, XmlSettings);
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
}
