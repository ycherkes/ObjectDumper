using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class ExceptionSerializationTest
    {
        [Fact]
        public void SerializeExceptionVisualBasic()
        {
            try
            {
                _ = new[] { "test" }[1];
            }
            catch (Exception e)
            {
                var serializer = new VisualBasicSerializer();

                var result = serializer.Serialize(e, "{ WritablePropertiesOnly: false }");

                Assert.Contains(".Message = \"Index was outside the bounds of the array.\"", result);
            }
        }
    }
}
