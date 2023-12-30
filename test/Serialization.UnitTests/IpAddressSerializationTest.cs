using Serialization.UnitTests.Extensions;
using System.Net;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class IpAddressSerializationTest
    {
        [Fact]
        public void SerializeIpAddressCs()
        {
            var ipAddress = IPAddress.Parse("142.250.74.110");

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(ipAddress, null);

            Assert.Equal("var iPAddress = IPAddress.Parse(\"142.250.74.110\");\r\n", result);
        }

        [Fact]
        public void SerializeIpAddressVb()
        {
            var ipAddress = IPAddress.Parse("142.250.74.110");

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(ipAddress, null);

            Assert.Equal("Dim iPAddressValue = IPAddress.Parse(\"142.250.74.110\")\r\n", result);
        }

        [Fact]
        public void SerializeIpAddressJson()
        {
            var ipAddress = IPAddress.Parse("142.250.74.110");

            var serializer = new JsonSerializer();

            var result = serializer.Serialize(ipAddress, null);

            Assert.Equal("\"142.250.74.110\"", result);
        }

        [Fact]
        public void SerializeIpAddressXml()
        {
            var ipAddress = IPAddress.Parse("142.250.74.110");

            var serializer = new XmlSerializer();

            var result = serializer.Serialize(ipAddress, null);

            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<IPAddress>142.250.74.110</IPAddress>", result);
        }

        [Fact]
        public void SerializeIpAddressYaml()
        {
            var ipAddress = IPAddress.Parse("142.250.74.110");

            var serializer = new YamlSerializer();

            var result = serializer.Serialize(ipAddress, null);

            Assert.Equal("142.250.74.110\r\n", result);
        }
    }
}