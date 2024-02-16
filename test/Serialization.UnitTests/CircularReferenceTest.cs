using Newtonsoft.Json;
using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class CircularReferenceTest
    {
        [Fact]
        public void SerializeAssemblyCSharp()
        {
            var assembly = new{ typeof(string).Assembly};

            var serializer = new CSharpSerializer();

            // Should not throw StackOverflowException or any other
            _ = serializer.Serialize(assembly, JsonConvert.SerializeObject(new
            {
                WritablePropertiesOnly = false
            }));
        }
    }
}
