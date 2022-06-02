using Newtonsoft.Json;

namespace ObjectDumper
{
    internal class CommonSettings : ICommonSettings
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public CommonSettings(ICommonSettings commonSettings)
        {
            MaxDepth = commonSettings.MaxDepth;
            IgnoreDefaultValues = commonSettings.IgnoreDefaultValues;
            UseFullTypeName = commonSettings.UseFullTypeName;
            IgnoreNullValues = commonSettings.IgnoreNullValues;
        }

        public int MaxDepth { get; set; }

        public bool IgnoreNullValues { get; set; }
        public bool IgnoreDefaultValues { get; set; }
        public bool UseFullTypeName { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, SerializerSettings);
        }
    }

    internal class BaseSettings : IBaseSettings
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public BaseSettings()
        {
            
        }

        public BaseSettings(IBaseSettings baseSettings)
        {
            MaxDepth = baseSettings.MaxDepth;
        }

        public int MaxDepth { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, SerializerSettings);
        }
    }

    internal interface ICommonSettings : IBaseSettings
    {
        bool IgnoreNullValues { get; set; }

        bool IgnoreDefaultValues { get; set; }

        bool UseFullTypeName { get; set; }
    }

    internal interface IBaseSettings
    {
        int MaxDepth { get; set; }

        string ToJson();
    }
}