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
    BirthDate = new DateTime(1964, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc)
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
    .BirthDate = New Date(1964, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc)
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