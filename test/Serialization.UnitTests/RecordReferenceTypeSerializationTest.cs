using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class RecordReferenceTypeSerializationTest
    {
        [Fact]
        public void SerializeRecordCsharp()
        {
            var recordVar = new Person("Boris", "Johnson");

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(recordVar, null);

            Assert.Equal("var person = new Person(\"Boris\", \"Johnson\");\r\n", result);
        }

        [Fact]
        public void SerializeRecordVb()
        {
            var recordVar = new Person("Boris", "Johnson");

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(recordVar, null);

            Assert.Equal("Dim person = New Person(\"Boris\", \"Johnson\")\r\n", result);
        }

        private record Person(string FirstName, string LastName);
    }
}