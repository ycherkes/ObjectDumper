using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                    "xml" => GetXml(obj),
                    _ => obj?.ToString()
                };
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string GetXml(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, XmlSettings);

            var typeName = obj.GetType().Name;
            var elementName = GetElementName(typeName);

            if (obj is IEnumerable and not IDictionary)
            {
                var itemTypeName = obj.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    ?.GenericTypeArguments
                    .FirstOrDefault()
                    ?.Name;

                elementName = GetElementName(itemTypeName) ?? elementName;

                json = "{\"" + elementName + "\":" + json + "}";
                
                elementName = "ArrayOf" + elementName;
            }

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" + JsonConvert.DeserializeXNode(json, elementName);
        }

        private static string GetElementName(string typeName)
        {
            return typeName?.Contains("AnonymousType") == true
                ? "AnonymousType"
                : typeName;
        }
    }
}
