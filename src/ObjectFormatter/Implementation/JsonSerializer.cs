using System;
using Newtonsoft.Json.Embedded;
using Newtonsoft.Json.Embedded.Converters;
using Newtonsoft.Json.Embedded.Serialization;
using ObjectFormatter.Implementation.Settings;

namespace ObjectFormatter.Implementation
{
    internal class JsonSerializer: ISerializer
    {
        private static JsonSerializerSettings JsonSettings => new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new SpecificContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
                ExcludeTypes = new[] { "Avro.Schema" }
            },
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
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

            if (!jsonSettings.SerializeEnumAsString)
            {
                newSettings.Converters = null;
            }

            if (jsonSettings.NamingStrategy != "CamelCase")
            {
                newSettings.ContractResolver = new SpecificContractResolver
                {
                    NamingStrategy = (NamingStrategy)Activator.CreateInstance(Type.GetType($"Newtonsoft.Json.Embedded.Serialization.{jsonSettings.NamingStrategy}NamingStrategy")),
                    ExcludeTypes = new[] { "Avro.Schema" }
                };
            }

            return newSettings;
        }

        public string Serialize(object obj, string settings)
        {
            return JsonConvert.SerializeObject(obj, GetJsonSettings(settings));
        }
    }
}
