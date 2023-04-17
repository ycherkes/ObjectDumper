using Newtonsoft.Json;
using System;
using System.Net;

namespace YellowFlavor.Serialization.Implementation.Json
{
    internal class IpAddressConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            return reader.Value == null ? null : IPAddress.Parse((string)reader.Value);
        }
    }
}
