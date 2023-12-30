using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class IgnoreIndexersTest
    {
        [Fact]
        public void IgnoreIndexersTestCsharp()
        {
            var index = new MyClassWithIndexer();

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(index, null);

            Assert.Equal("var myClassWithIndexer = new MyClassWithIndexer\r\n{\r\n    Caption = \"A Default caption\"\r\n};\r\n", result);
        }

        [Fact]
        public void IgnoreIndexersTestVb()
        {
            var index = new MyClassWithIndexer();

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(index, null);

            Assert.Equal("Dim myClassWithIndexerValue = New MyClassWithIndexer With {\r\n    .Caption = \"A Default caption\"\r\n}\r\n", result);
        }

        private class MyClassWithIndexer
        {
            public string Caption { get; set; } = "A Default caption";

            private readonly string[] _strings = { "abc", "def", "ghi", "jkl" };
            public string this[int index]
            {
                get => _strings[index];
                set => _strings[index] = value;
            }
        }
    }
}