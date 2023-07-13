using Newtonsoft.Json;
using System;
using System.Net;
using YAXLib;
using YAXLib.Enums;
using YAXLib.KnownTypes;
using YAXLib.Options;
using YellowFlavor.Serialization.Implementation.Settings;
using YellowFlavor.Serialization.Implementation.Xml;

namespace YellowFlavor.Serialization.Implementation
{
    internal class XmlSerializer : ISerializer
    {
        private static SerializerOptions SerializerOptions => new()
        {
            ExceptionHandlingPolicies = YAXExceptionHandlingPolicies.ThrowErrorsOnly,
            ExceptionBehavior = YAXExceptionTypes.Ignore,
            SerializationOptions = YAXSerializationOptions.SuppressMetadataAttributes | YAXSerializationOptions.DontSerializeNullObjects | YAXSerializationOptions.DontSerializeDefaultValues,
            MaxRecursion = 25,
            ExcludeTypes = { "Avro.Schema" }
        };

        private static readonly string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine;

        public string Serialize(object obj, string settings)
        {
            if (obj == null)
            {
                return XmlHeader;
            }

            if (!WellKnownTypes.TryGetKnownType(typeof(IPAddress), out _))
            {
                WellKnownTypes.Add(new IpAddressKnownType());
            }
            
            var xmlSettings = GetXmlSettings(settings);
            var serializer = new YAXSerializer(obj.GetType(), xmlSettings);
            var serializedValue = serializer.Serialize(obj);
            return XmlHeader + serializedValue;
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

            if (xmlSettings.IgnoreDefaultValues)
            {
                newSettings.SerializationOptions |= YAXSerializationOptions.DontSerializeDefaultValues;
            }
            else
            {
                newSettings.SerializationOptions ^= YAXSerializationOptions.DontSerializeDefaultValues;
            }

            return newSettings;
        }
    }
}
