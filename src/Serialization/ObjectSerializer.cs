using System;
using System.Collections.Generic;
using System.IO;
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
            {"yaml", new YamlSerializer()}
        };

        public static string Serialize(object obj, string format, string settings = null)
        {
            var content = SerializeInternal(obj, format, settings);
            return SaveToTemporaryFile(content);
        }

        private static string SerializeInternal(object obj, string format, string settings)
        {
            try
            {
                return Serializers.TryGetValue(format, out var serializer)
                    ? serializer.Serialize(obj, settings.FromBase64())
                    : obj?.ToString();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static string SaveToTemporaryFile(string content)
        {
            var fileName = Path.ChangeExtension(Guid.NewGuid().ToString("N"), "txt");
            var fullFilePath = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(fullFilePath, content);
            return fileName;
        }
    }
}
