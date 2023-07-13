using Serialization.UnitTests.TestModel;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class VariableNameTest
    {
        [Fact]
        public void Dictionary()
        {
            var dict = new[]
            {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"},
            }.ToDictionary(x => x.FirstName);

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(dict, null);

            Assert.StartsWith("var dictionaryOfPerson", result);
        }

        [Fact]
        public void DictionaryOfListOfPerson()
        {
            var dict = new[]
            {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"},
            }.ToDictionary(x => x.FirstName, x => new List<Person> { x });

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(dict, null);

            Assert.StartsWith("var dictionaryOfListOfPerson", result);
        }

        [Fact]
        public void HashSet()
        {
            var hashSet = new HashSet<Person>
            {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"},
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(hashSet, null);

            Assert.StartsWith("var hashSetOfPerson", result);
        }

        [Fact]
        public void String()
        {
            var stringValue = "Test string value";

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(stringValue, null);

            Assert.StartsWith("var stringValue", result);
        }

    }
}