using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class InheritanceTest
    {

        [Fact]
        public void SerializeClassCsharp()
        {
            var person = new Person
            {
                FirstName = "Boris",
                LastName = "Johnson",
                BirthDate = DateTime.SpecifyKind(new DateTime(1964, 6, 19), DateTimeKind.Utc)
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(person, null);

            Assert.Equal(
@"var person = new Person
{
    FirstName = ""Boris"",
    LastName = ""Johnson"",
    BirthDate = DateTime.ParseExact(""1964-06-19T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
};
", result);
        }

        [Fact]
        public void SerializeClassVb()
        {
            var person = new Person
            {
                FirstName = "Boris",
                LastName = "Johnson",
                BirthDate = DateTime.SpecifyKind(new DateTime(1964, 6, 19), DateTimeKind.Utc)
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(person, null);

            Assert.Equal(
@"Dim personValue = New Person With {
    .FirstName = ""Boris"",
    .LastName = ""Johnson"",
    .BirthDate = Date.ParseExact(""1964-06-19T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
}
", result);
        }


        private class Human
        {
            public DateTime BirthDate { get; set; }
        }

        private class Person : Human
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}