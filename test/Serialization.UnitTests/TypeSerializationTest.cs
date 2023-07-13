using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class TypeSerializationTest
    {
        [Fact]
        public void SerializeTypeArrayXml()
        {
            var types = new[] { typeof(string), typeof(int), typeof(decimal) };

            var serializer = new XmlSerializer();

            var result = serializer.Serialize(types, null);

            Assert.Equal("""
                <?xml version="1.0" encoding="utf-8"?>
                <Array1OfType>
                  <Type>System.String</Type>
                  <Type>System.Int32</Type>
                  <Type>System.Decimal</Type>
                </Array1OfType>
                """, result);
        }
    }
}
