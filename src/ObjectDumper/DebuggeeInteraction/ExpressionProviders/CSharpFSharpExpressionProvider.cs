namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders
{
    internal class CSharpFSharpExpressionProvider : IExpressionProvider
    {
        public string GetTargetFrameworkExpressionText()
        {
            return "System.AppContext.TargetFrameworkName?.StartsWith(\".NETF\") != false ? (System.AppContext.TargetFrameworkName ??  ((System.Runtime.Versioning.TargetFrameworkAttribute)System.Reflection.Assembly.GetEntryAssembly()?.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), true)?[0]).FrameworkName) : System.AppContext.TargetFrameworkName?.Split(',')[0] + \",Version=v\" + System.Environment.Version";
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
