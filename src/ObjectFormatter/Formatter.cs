using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json.Embedded;
using Newtonsoft.Json.Embedded.Converters;
using Newtonsoft.Json.Embedded.Serialization;
using Formatting = Newtonsoft.Json.Embedded.Formatting;

namespace ObjectFormatter
{
    public static class Formatter
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly JsonSerializerSettings XmlSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = { new StringEnumConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly DumpOptions CsharpDumpOptions = new()
        {
            IgnoreDefaultValues = true,
            IndentSize = 4
        };

        public static string Format(object obj, string formattingType)
        {
            try
            {
                return formattingType switch
                {
                    "json" => JsonConvert.SerializeObject(obj, JsonSettings),
                    "csharp" => ObjectFormatterCSharp.Dump(obj, CsharpDumpOptions),
                    "xml" => GetXmlWithHeader(obj),
                    _ => obj?.ToString()
                };
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string GetXmlWithHeader(object obj)
        {
            return $"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}{GetXml(obj)}";
        }

        private static string GetXml(object obj)
        {
            if (obj == null) return "<!--NULL VALUE-->";

            var elementName = GetElementName(obj);

            if (obj is IEnumerable and not IDictionary)
            {
                var xmlCollection = string.Join(Environment.NewLine, 
                    ((IEnumerable)obj)
                    .Cast<object>()
                    .Select(GetXml));

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

            return JsonConvert.DeserializeXNode(json, elementName)?.ToString();
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
