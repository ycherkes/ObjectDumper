using Newtonsoft.Json;
using Serialization.UnitTests.TestModel;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class GroupingCollectionSerializationTest
    {
        [Fact]
        public void SerializeGroupingCollectionVisualBasic()
        {
            var grouping = new[]
            {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"}
            }.ToLookup(x => x.FirstName);

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(grouping, JsonConvert.SerializeObject(new
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseFullTypeName = false,
                DateTimeInstantiation = "New",
                DateKind = "ConvertToUtc"
            }));

            Assert.Equal(
@"Dim lookupOfGroupingOfPerson = {
    New With {
        .Key = ""Bob"",
        .Elements = New Person(){
            New Person With {
                .FirstName = ""Bob"",
                .Age = 32
            }
        }
    },
    New With {
        .Key = ""Alice"",
        .Elements = New Person(){
            New Person With {
                .FirstName = ""Alice"",
                .Age = 23
            }
        }
    }
}.ToLookup(Function (grp) grp.Key, Function (grp) grp.Elements)
", result);
        }

        [Fact]
        public void SerializeGroupingCollectionCSharp()
        {
            var grouping = new[]
             {
                new Person{ Age = 32, FirstName = "Bob"},
                new Person{ Age = 23, FirstName = "Alice"}
            }.GroupBy(x => x.FirstName).ToArray();

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(grouping, JsonConvert.SerializeObject(new
            {
                IgnoreDefaultValues = true,
                IgnoreNullValues = true,
                MaxDepth = 5,
                UseFullTypeName = false,
                DateTimeInstantiation = "New",
                DateKind = "ConvertToUtc"
            }));

            Assert.Equal(
@"var arrayOfGroupingOfPerson = new []
{
    new 
    {
        Key = ""Bob"",
        Elements = new Person[]
        {
            new Person
            {
                FirstName = ""Bob"",
                Age = 32
            }
        }
    },
    new 
    {
        Key = ""Alice"",
        Elements = new Person[]
        {
            new Person
            {
                FirstName = ""Alice"",
                Age = 23
            }
        }
    }
}.GroupBy(grp => grp.Key, grp => grp.Elements).ToArray();
", result);
        }
    }
}
