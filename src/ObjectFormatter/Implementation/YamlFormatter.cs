﻿using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Newtonsoft.Json.Embedded;
using ObjectFormatter.Implementation.Settings;
using ObjectFormatter.YamlDotNet.Embedded.Core;
using ObjectFormatter.YamlDotNet.Embedded.Serialization;

namespace ObjectFormatter.Implementation
{
    internal class YamlFormatter: IFormatter
    {
        private static YamlSettings YamlSettings => new()
        {
            MaxDepth = 100
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

        public string Format(object obj, string settings)
        {
            var yamlSerializer = GetYamlSerializer(settings);
            var stringBuilder = new StringBuilder();
            yamlSerializer.Serialize(new IndentedTextWriter(new StringWriter(stringBuilder)), obj);
            return stringBuilder.ToString();
        }
    }
}
