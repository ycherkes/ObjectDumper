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
    (""Bob"", New Person(){
            New Person With {
                .FirstName = ""Bob"",
                .Age = 32
            }
        }),
    (""Alice"", New Person(){
            New Person With {
                .FirstName = ""Alice"",
                .Age = 23
            }
        })
}.ToLookup(Function (tuple) tuple.Item1, Function (tuple) tuple.Item2)
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
    (""Bob"", new Person[]
        {
            new Person
            {
                FirstName = ""Bob"",
                Age = 32
            }
        }),
    (""Alice"", new Person[]
        {
            new Person
            {
                FirstName = ""Alice"",
                Age = 23
            }
        })
}.GroupBy(tuple => tuple.Item1, tuple => tuple.Item2).ToArray();
", result);
        }
    }
}
