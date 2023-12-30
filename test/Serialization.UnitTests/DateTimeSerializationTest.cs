using Newtonsoft.Json;
using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class DateTimeSerializationTest
    {
        [Fact]
        public void SerializeDateOnlyCsharp()
        {
            var anonymous = new
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"var anonymousType = new 
{
    DateOnly = DateOnly.ParseExact(""2022-12-10"", ""O"")
};
", result);
        }

        [Fact]
        public void SerializeTimeOnlyCsharp()
        {
            var anonymous = new
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"var anonymousType = new 
{
    TimeOnly = TimeOnly.ParseExact(""22:55:33.1220000"", ""O"")
};
", result);
        }

        [Fact]
        public void SerializeDateOnlyVb()
        {
            var anonymous = new
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"Dim anonymousType = New With {
    .DateOnly = DateOnly.ParseExact(""2022-12-10"", ""O"")
}
", result);
        }

        [Fact]
        public void SerializeTimeOnlyVb()
        {
            var anonymous = new
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"Dim anonymousType = New With {
    .TimeOnly = TimeOnly.ParseExact(""22:55:33.1220000"", ""O"")
}
", result);
        }
    }
}