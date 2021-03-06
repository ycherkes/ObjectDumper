using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class NumberSpecialValuesSerializationTest
    {
        [Fact]
        public void SerializeMaxValueFloatCsharp()
        {
            const float max = float.MaxValue;

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(max, null);

            Assert.Equal("var singleValue = float.MaxValue;\r\n", result);
        }

        [Fact]
        public void SerializeMinValueFloatCsharp()
        {
            const float min = float.MinValue;

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(min, null);

            Assert.Equal("var singleValue = float.MinValue;\r\n", result);
        }

        [Fact]
        public void SerializeNaNValueFloatCsharp()
        {
            const float nan = float.NaN;

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(nan, null);

            Assert.Equal("var singleValue = float.NaN;\r\n", result);
        }

        [Fact]
        public void SerializeMaxValueSingleVb()
        {
            const float max = float.MaxValue;

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(max, null);

            Assert.Equal("Dim singleValue = Single.MaxValue\r\n", result);
        }
    }
}