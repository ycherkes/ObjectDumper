using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class AnonymousTypeSerializationTest
    {
        [Fact]
        public void SerializeAnonymousTypeCsharp()
        {
            var anonymous = new { Name = "Peter" };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(anonymous, null);

            Assert.Equal(
@"var anonymousType = new 
{
    Name = ""Peter""
};
", result);
        }

        [Fact]
        public void SerializeAnonymousTypeVb()
        {
            var anonymous = new { Name = "Peter" };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(anonymous, null);

            Assert.Equal(
@"Dim anonymousType = New With {
    .Name = ""Peter""
}
", result);
        }
    }
}