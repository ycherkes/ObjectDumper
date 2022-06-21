namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders
{
    internal interface IExpressionProvider
    {
        string GetStringTypeAssemblyLocationExpressionText();
        string GetIsSerializerInjectedExpressionText();
        string GetSerializedValueExpressionText(string expression, string format, string settings);
        string GetLoadAssemblyExpressionText(string serializerFileName);
    }
}