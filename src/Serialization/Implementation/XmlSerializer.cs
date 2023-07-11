using Newtonsoft.Json;
using System;
using YAXLib;
using YAXLib.Enums;
using YAXLib.Options;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation
{
    internal class XmlSerializer : ISerializer
    {
        private static readonly SerializerOptions SerializerOptions = new SerializerOptions
        {
            ExceptionHandlingPolicies = YAXExceptionHandlingPolicies.ThrowErrorsOnly,
            ExceptionBehavior = YAXExceptionTypes.Ignore,
            SerializationOptions = YAXSerializationOptions.SuppressMetadataAttributes | YAXSerializationOptions.DontSerializeNullObjects,
            MaxRecursion = 25
        };

        private static readonly string XmlHeader = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}";

        public string Serialize(object obj, string settings)
        {
            if (obj == null)
            {
                return XmlHeader;
            }

            var xmlSettings = GetXmlSettings(settings);
            var serializer = new YAXSerializer(obj.GetType(), xmlSettings);

            return $"{XmlHeader}{serializer.Serialize(obj)}";
        }

        private static SerializerOptions GetXmlSettings(string settings)
        {
            var newSettings = SerializerOptions;
            if (settings == null) return newSettings;

            var xmlSettings = JsonConvert.DeserializeObject<XmlSettings>(settings);
            newSettings.MaxRecursion = xmlSettings.MaxDepth > 0 ? xmlSettings.MaxDepth : 1;

            newSettings.SerializationOptions |= xmlSettings.IgnoreNullValues
                ? YAXSerializationOptions.DontSerializeNullObjects
                : YAXSerializationOptions.SerializeNullObjects;

            newSettings.SerializationOptions ^= xmlSettings.IgnoreNullValues
                ? YAXSerializationOptions.SerializeNullObjects
                : YAXSerializationOptions.DontSerializeNullObjects;

            return newSettings;
        }
    }
}
