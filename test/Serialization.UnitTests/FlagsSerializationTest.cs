using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class FlagsSerializationTest
    {
        [Fact]
        public void SerializeFlagsCsharp()
        {
            var flagsVar = TestEnum.First | TestEnum.Third;

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(flagsVar, null);

            Assert.Equal("var testEnum = TestEnum.First | TestEnum.Third;\r\n", result);
        }

        [Fact]
        public void SerializeFlagsVb()
        {
            var flagsVar = TestEnum.Second | TestEnum.Third;

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(flagsVar, null);

            Assert.Equal("Dim testEnum = TestEnum.Second Or TestEnum.Third\r\n", result);
        }

        [Flags]
        private enum TestEnum
        {
            First = 1,
            Second = 2,
            Third = 4
        }
    }
}