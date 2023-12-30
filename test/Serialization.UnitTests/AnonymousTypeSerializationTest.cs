using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class AnonymousTypeSerializationTest
    {
        [Fact]
        public void SerializeAnonymousTypeCsharp()
        {
            var anonymous = new[]
            {
                new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
                new { Name = "Peter", Age = (int?)null, Reference = (string)null }
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(anonymous, null);

            Assert.Equal(
@"var arrayOfAnonymousType = new []
{
    new 
    {
        Name = ""Steeve"",
        Age = (int?)int.MaxValue,
        Reference = ""Test reference""
    },
    new 
    {
        Name = ""Peter"",
        Age = (int?)null,
        Reference = (string)null
    }
};
", result);
        }

        [Fact]
        public void SerializeAnonymousTypeVb()
        {
            var anonymous = new[]
            {
                new { Name = "Steeve", Age = (int?)int.MaxValue, Reference = "Test reference" },
                new { Name = "Peter", Age = (int?)null, Reference = (string)null }
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(anonymous, null);

            Assert.Equal(
@"Dim arrayOfAnonymousType = {
    New With {
        .Name = ""Steeve"",
        .Age = CType(Integer.MaxValue, Integer?),
        .Reference = ""Test reference""
    },
    New With {
        .Name = ""Peter"",
        .Age = CType(Nothing, Integer?),
        .Reference = CType(Nothing, String)
    }
}
", result);
        }
    }
}