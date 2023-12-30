using System.IO;

namespace YellowFlavor.Serialization.Implementation;

internal interface ISerializer
{
    void Serialize(object obj, string settings, TextWriter textWriter);
}