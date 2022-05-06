using System;
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
            Converters =
            {
                new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
            },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static readonly JsonSerializerSettings XmlSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static string Format(object obj, string formattingType)
        {
            try
            {
                switch (formattingType)
                {
                    case "json":
                        return JsonConvert.SerializeObject(obj, JsonSettings);
                    case "csharp":
                        return ObjectFormatterCSharp.Dump(obj, new DumpOptions
                        {
                            IgnoreDefaultValues = true,
                            IndentSize = 4
                        });
                    case "xml":
                        return GetXml(obj);
                    default:
                        return obj?.ToString();
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string GetXml(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, XmlSettings);
            var name = obj.GetType().Name;
            var element = name.Contains("AnonymousType")
                ? "AnonymousType"
                : name;

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" + JsonConvert.DeserializeXNode(json, element);
        }
    }
}
