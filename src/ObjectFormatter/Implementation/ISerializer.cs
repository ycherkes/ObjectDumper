namespace ObjectFormatter.Implementation
{
    internal interface ISerializer
    {
        string Format(object obj, string settings);
    }
}
