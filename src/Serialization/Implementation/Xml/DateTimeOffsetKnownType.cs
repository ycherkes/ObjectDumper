using Newtonsoft.Json;
using System;
using System.Xml.Linq;
using YAXLib.Customization;
using YAXLib.KnownTypes;

namespace YellowFlavor.Serialization.Implementation.Xml
{
    internal class DateTimeOffsetKnownType : KnownTypeBase<DateTimeOffset>
    {
        public override void Serialize(DateTimeOffset dateTimeOffset, XElement elem, XNamespace overridingNamespace,
            ISerializationContext serializationContext)
        {
            elem.Add(JsonConvert.ToString(dateTimeOffset, DateFormatHandling.IsoDateFormat).Trim('"'));
        }

        public override DateTimeOffset Deserialize(XElement elem, XNamespace overridingNamespace, ISerializationContext serializationContext)
        {
            return DateTimeOffset.Parse(elem.Value);
        }
    }
}
