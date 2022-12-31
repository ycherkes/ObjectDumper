using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation.Settings;

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

            var namingConvention = yamlSettings.NamingConvention.ToPascalCase();

            var namingConventionType = namingConvention switch
            {
                "CamelCase" => typeof(CamelCaseNamingConvention),
                "Hyphenated" => typeof(HyphenatedNamingConvention),
                "LowerCase" => typeof(LowerCaseNamingConvention),
                "Null" => typeof(NullNamingConvention),
                "PascalCase" => typeof(PascalCaseNamingConvention),
                "Underscored" => typeof(UnderscoredNamingConvention),
                _ => throw new InvalidOperationException($"Invalid naming convention: {namingConvention}")
            };

            var valueSerializer = new SerializerBuilder()
                .WithNamingConvention((INamingConvention)Activator.CreateInstance(namingConventionType))
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
