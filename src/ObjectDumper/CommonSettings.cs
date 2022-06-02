using Newtonsoft.Json;

namespace ObjectDumper
{
    internal class CommonSettings : ICommonSettings
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public int MaxDepth { get; set; }

        public bool IgnoreNullValues { get; set; }
        public bool IgnoreDefaultValues { get; set; }
        public bool UseFullTypeName { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, SerializerSettings);
        }
    }

   
    internal interface ICommonSettings 
    {
        bool IgnoreNullValues { get; set; }

        bool IgnoreDefaultValues { get; set; }

        bool UseFullTypeName { get; set; }

        int MaxDepth { get; set; }

        string ToJson();
    }
}