namespace Serialization.UnitTests
{
#if net461
    public class RecordReferenceTypeSerializationTest
    {
        [Fact]
        public void SerializeRecordWithConstructorCsharp()
        {
            var person = new Person("Boris", "Johnson");

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(person, null);

            Assert.Equal("var person = new Person(\"Boris\", \"Johnson\");\r\n", result);
        }

        [Fact]
        public void SerializeRecordWithConstructorVb()
        {
            var person = new Person("Boris", "Johnson");

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(person, null);

            Assert.Equal("Dim personValue = New Person(\"Boris\", \"Johnson\")\r\n", result);
        }

        [Fact]
        public void SerializeRecordWithoutConstructorCsharp()
        {
            var person1 = new Person1
            {
                FirstName = "Boris",
                LastName = "Johnson"
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(person1, null);

            Assert.Equal(
@"var person1 = new Person1
{
    FirstName = ""Boris"",
    LastName = ""Johnson""
};
", result);
        }

        [Fact]
        public void SerializeRecordWithoutConstructorVb()
        {
            var person1 = new Person1
            {
                FirstName = "Boris",
                LastName = "Johnson"
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(person1, null);

            Assert.Equal(
@"Dim person1Value = New Person1 With {
    .FirstName = ""Boris"",
    .LastName = ""Johnson""
}
", result);
        }

        private record Person(string FirstName, string LastName)
        {
            public string FullName => $"{FirstName} {LastName}";
        }

        public record Person2(string FirstName, string LastName, string Id)
        {
            internal string Id { get; init; } = Id;
        }


        private record Person1
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
#endif
}