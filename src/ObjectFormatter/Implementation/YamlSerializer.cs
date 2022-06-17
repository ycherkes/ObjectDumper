using Newtonsoft.Json.Embedded;
using ObjectFormatter.Implementation.Settings;
using ObjectFormatter.YamlDotNet.Embedded.Core;
using ObjectFormatter.YamlDotNet.Embedded.Serialization;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace ObjectFormatter.Implementation
{
    internal class YamlSerializer : ISerializer
    {
        private static YamlSettings YamlSettings => new()
        {
            MaxDepth = 25
        };

        private static Serializer GetYamlSerializer(string settings)
        {
            var yamlSettings = GetYamlSettings(settings);

            var valueSerializer = new SerializerBuilder()
                .WithNamingConvention((INamingConvention)Activator.CreateInstance(Type.GetType($"ObjectFormatter.YamlDotNet.Embedded.Serialization.NamingConventions.{yamlSettings.NamingConvention}NamingConvention")))
                .WithMaximumRecursion(yamlSettings.MaxDepth)
                .BuildValueSerializer();

            return Serializer.FromValueSerializer(valueSerializer, EmitterSettings.Default);
        }

        private static YamlSettings GetYamlSettings(string settings)
        {
            return settings == null
                ? YamlSettings
                : JsonConvert.DeserializeObject<YamlSettings>(settings);
        }

        public string Serialize(object obj, string settings)
        {
            var yamlSerializer = GetYamlSerializer(settings);
            var stringBuilder = new StringBuilder();
            using var stringWriter = new StringWriter(stringBuilder);
            using var textWriter = new IndentedTextWriter(stringWriter);
            yamlSerializer.Serialize(textWriter, obj);

            return stringBuilder.ToString();
        }
    }
}
