using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class RecordReferenceTypeSerializationTest
    {
        [Fact]
        public void SerializeRecordCsharp()
        {
            var recordVar = new Person("Jeremy", "Clarkson");

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(recordVar, null);

            Assert.Equal("var person = new Person(\"Jeremy\", \"Clarkson\");\r\n", result);
        }

        [Fact]
        public void SerializeFlagsVb()
        {
            var flagsVar = new Person("Jeremy", "Clarkson");

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(flagsVar, null);

            Assert.Equal("Dim person = New Person(\"Jeremy\", \"Clarkson\")\r\n", result);
        }

        private record Person(string FirstName, string LastName);
    }
}