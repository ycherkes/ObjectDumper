using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class EnumerableRangeSerializationTest
    {
        [Fact]
        public void SerializeAnonymousTypeCsharp()
        {
            var range = Enumerable.Range(5, 2).Select((x, i) => new { i, x });

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(range, null);

            Assert.Equal(
@"var selectIteratorOfAnonymousType = new []
{
    new 
    {
        i = 0,
        x = 5
    },
    new 
    {
        i = 1,
        x = 6
    }
};
", result);
        }

        [Fact]
        public void SerializeAnonymousTypeVb()
        {
            var range = Enumerable.Range(5, 2).Select((i, x) => new { i, x });

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(range, null);

            Assert.Equal(
@"Dim selectIteratorOfAnonymousType = {
    New With {
        .i = 5,
        .x = 0
    },
    New With {
        .i = 6,
        .x = 1
    }
}
", result);
        }
    }
}