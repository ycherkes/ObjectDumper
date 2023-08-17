using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class TextSerializerTest
    {
        [Fact]
        public void SerializeArrayOfArraysText()
        {
            int[][] array = { new[] { 1 } };

            var serializer = new TextSerializer();

            var result = serializer.Serialize(array, """{maxDepth: 5, label: "array"}""");

            Assert.Equal(
           """
            ╭───┬────────────────╮
            │ # │ Int32[][1]     │
            ├───┼────────────────┤
            │ 0 │ ╭───┬────────╮ │
            │   │ │ # │ int[1] │ │
            │   │ ├───┼────────┤ │
            │   │ │ 0 │ 1      │ │
            │   │ ╰───┴────────╯ │
            ╰───┴────────────────╯
                    array
            """, result);
        }
    }
}