using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YellowFlavor.Serialization.Implementation.Json
{
    public class SpecificContractResolver : DefaultContractResolver
    {
        public string[] ExcludeTypes { get; set; }
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = (List<JsonProperty>)base.CreateProperties(type, memberSerialization);

            // Do not serialize properties
            return properties.FindAll(p => !ExcludeTypes.Any(pt => string.Equals(p.PropertyType?.FullName, pt, StringComparison.Ordinal)));
        }
    }
}
