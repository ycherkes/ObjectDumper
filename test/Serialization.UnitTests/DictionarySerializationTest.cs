using Newtonsoft.Json;
using Serialization.UnitTests.TestModel;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class DictionarySerializationTest
    {
        [Fact]
        public void SerializeDictionaryVisualBasic()
        {
            var dict = new[]
            {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"},
            }.ToDictionary(x => x.FirstName);

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(dict, JsonConvert.SerializeObject(new
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseFullTypeName = false,
                DateTimeInstantiation = "New",
                DateKind = "ConvertToUtc"
            }));

            Assert.Equal(@"Dim dictionaryOfPerson = New Dictionary(Of String, Person) From {
    {
        ""Bob"",
        New Person With {
            .FirstName = ""Bob"",
            .Age = 32
        }
    },
    {
        ""Alice"",
        New Person With {
            .FirstName = ""Alice"",
            .Age = 23
        }
    }
}
", result);
        }

        [Fact]
        public void SerializeDictionaryCSharp()
        {
            var dict = new[]
            {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"},
            }.ToDictionary(x => x.FirstName);

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(dict, JsonConvert.SerializeObject(new
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseFullTypeName = false,
                DateTimeInstantiation = "New",
                DateKind = "ConvertToUtc"
            }));

            Assert.Equal(@"var dictionaryOfPerson = new Dictionary<string, Person>
{
    {
        ""Bob"",
        new Person
        {
            FirstName = ""Bob"",
            Age = 32
        }
    },
    {
        ""Alice"",
        new Person
        {
            FirstName = ""Alice"",
            Age = 23
        }
    }
};
", result);
        }

        [Fact]
        public void SerializeDictionaryOfTypeArrayCSharp()
        {
            var dict = new Dictionary<string, Type[]>
            {
                {"First",  new[]{ typeof(Person) } },
                {"Second", new[]{ typeof(string) } }
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(dict, JsonConvert.SerializeObject(new
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseFullTypeName = false,
                DateTimeInstantiation = "New",
                DateKind = "ConvertToUtc"
            }));

            Assert.Equal(@"var dictionaryOfArrayOfType = new Dictionary<string, Type[]>
{
    {
        ""First"",
        new Type[]
        {
            typeof(Person)
        }
    },
    {
        ""Second"",
        new Type[]
        {
            typeof(string)
        }
    }
};
", result);
        }
    }
}
