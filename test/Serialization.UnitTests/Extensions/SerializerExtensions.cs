using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests.Extensions
{
    internal static class SerializerExtensions
    {
        internal static string Serialize(this ISerializer serializer, object obj, string settings)
        {
            using var stringWriter = new StringWriter();
            serializer.Serialize(obj, settings, stringWriter);
            return stringWriter.ToString();
        }
    }
}
