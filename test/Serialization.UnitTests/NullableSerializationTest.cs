using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class NullableSerializationTest
    {
        [Fact]
        public void SerializeNullableEnumCSharp()
        {
            MyEnum? value = MyEnum.TestValue;

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(value, null);

            Assert.Equal(
@"var myEnum = MyEnum.TestValue;
", result);
        }

        [Fact]
        public void SerializeNullableEnumVb()
        {
            MyEnum? value = MyEnum.TestValue;

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(value, null);

            Assert.Equal(
@"Dim myEnum = MyEnum.TestValue
", result);
        }

        enum MyEnum
        {
            TestValue
        }

    }
}