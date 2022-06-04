using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectDumper.Extensions
{
    internal static class JsonExtensions
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        internal static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, SerializerSettings);
        }
    }
}
