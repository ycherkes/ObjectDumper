using System;
using System.Collections.Generic;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace YellowFlavor.Serialization
{
    public static class ObjectSerializer
    {
        private static readonly Dictionary<string, ISerializer> Serializers = new()
        {
            {"json", new JsonSerializer()},
            {"cs", new CSharpSerializer()},
            {"vb", new VisualBasicSerializer()},
            {"xml", new XmlSerializer()},
            {"yaml", new YamlSerializer()},
            {"txt", new TextSerializer()}
        };

        public static string Serialize(object obj, string format, string settings = null)
        {
            try
            {
                return Serializers.TryGetValue(format, out var serializer)
                    ? serializer.Serialize(obj, settings.FromBase64()).ToBase64()
                    : obj?.ToString().ToBase64();
            }
            catch (Exception e)
            {
                return e.ToString().ToBase64();
            }
        }
    }
}
