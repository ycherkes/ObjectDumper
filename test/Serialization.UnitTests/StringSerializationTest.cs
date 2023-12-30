using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class StringSerializationTest
    {
        [Fact]
        public void SerializeLongStringCSharp()
        {
            var stringVar = "C:\\temp\\postgresql-13.1-1-windows-x64-binaries\\pgsql\\pgAdmin 4\\docs\\en_US\\html\\_sources\\add_restore_point_dialog.rst.txt";

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(stringVar, null);

            Assert.DoesNotContain("+", result);
        }

        [Fact]
        public void SerializeLongStringVisualBasic()
        {
            var stringVar = "C:\\temp\\postgresql-13.1-1-windows-x64-binaries\\pgsql\\pgAdmin 4\\docs\\en_US\\html\\_sources\\add_restore_point_dialog.rst.txt";

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(stringVar, null);

            Assert.DoesNotContain("& _", result);
        }
    }
}
