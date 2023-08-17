using Newtonsoft.Json;
using Dumpify;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation
{
    internal class TextSerializer : ISerializer
    {
        private static TextSettings TextSettings => new()
        {
            MaxDepth = 25
        };

        public string Serialize(object obj, string settings)
        {
            var textSettings = GetTextSettings(settings);
            return obj.DumpText(label: textSettings.Label, maxDepth: textSettings.MaxDepth);
        }

        private static TextSettings GetTextSettings(string settings)
        {
            return settings == null
                ? TextSettings
                : JsonConvert.DeserializeObject<TextSettings>(settings);
        }
    }
}
