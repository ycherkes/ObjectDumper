using System.Net;
using System.Xml.Linq;
using YAXLib.Customization;
using YAXLib.KnownTypes;

namespace YellowFlavor.Serialization.Implementation.Xml
{
    internal class IpAddressKnownType: KnownTypeBase<IPAddress>
    {
        public override void Serialize(IPAddress ipAddress, XElement elem, XNamespace overridingNamespace,
            ISerializationContext serializationContext)
        {
            elem.Add(ipAddress.ToString());
        }

        public override IPAddress Deserialize(XElement elem, XNamespace overridingNamespace, ISerializationContext serializationContext)
        {
            return IPAddress.Parse(elem.Value);
        }
    }
}
