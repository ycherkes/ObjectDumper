using Embedded.Newtonsoft.Json;
using YellowFlavor.Serialization.Implementation.Settings;
using YellowFlavor.Serialization.Embedded.YamlDotNet.Core;
using YellowFlavor.Serialization.Embedded.YamlDotNet.Serialization;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace YellowFlavor.Serialization.Implementation
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
                .WithNamingConvention((INamingConvention)Activator.CreateInstance(Type.GetType($"YellowFlavor.Serialization.Embedded.YamlDotNet.Serialization.NamingConventions.{yamlSettings.NamingConvention}NamingConvention")))
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
