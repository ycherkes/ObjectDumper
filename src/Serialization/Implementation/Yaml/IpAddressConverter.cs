using System;
using System.Net;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YellowFlavor.Serialization.Implementation.Yaml
{
    internal sealed class IpAddressConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(IPAddress);

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var ipAddress = (IPAddress)value;
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, ipAddress.ToString(), ScalarStyle.Any, true, false));
        }
    }
}
