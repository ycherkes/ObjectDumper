namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders
{
    internal class CSharpFSharpExpressionProvider : IExpressionProvider
    {
        public string GetTargetFrameworkExpressionText()
        {
            return "System.AppContext.TargetFrameworkName";
        }

        public string GetIsSerializerInjectedExpressionText()
        {
            return "nameof(YellowFlavor.Serialization.ObjectSerializer.Serialize)";
        }

        public string GetSerializedValueExpressionText(string expression, string format, string settings)
        {
            return $@"YellowFlavor.Serialization.ObjectSerializer.Serialize({expression}, ""{format}"", ""{settings}"")";
        }

        public string GetLoadAssemblyExpressionText(string serializerFileName)
        {
            return $"System.Reflection.Assembly.LoadFile(@\"{serializerFileName}\")";
        }
    }
}
