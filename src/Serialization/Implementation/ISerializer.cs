namespace YellowFlavor.Serialization.Implementation
{
    internal interface ISerializer
    {
        string Serialize(object obj, string settings);
    }
}
