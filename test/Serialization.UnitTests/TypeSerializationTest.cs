using Serialization.UnitTests.Extensions;
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

            Assert.Equal(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfType>
  <RuntimeType>System.String, System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e</RuntimeType>
  <RuntimeType>System.Int32, System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e</RuntimeType>
  <RuntimeType>System.Decimal, System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e</RuntimeType>
</ArrayOfType>", result);
        }
    }
}
