namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders
{
    internal class VisualBasicExpressionProvider : IExpressionProvider
    {
        public string GetTargetFrameworkExpressionText()
        {
            return "If(System.AppContext.TargetFrameworkName?.StartsWith(\".NETF\") <> False, If(System.AppContext.TargetFrameworkName, DirectCast(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(GetType(System.Runtime.Versioning.TargetFrameworkAttribute), True)(0), System.Runtime.Versioning.TargetFrameworkAttribute).FrameworkName), System.AppContext.TargetFrameworkName?.Split(\",\")(0) & \",Version=v\" & System.Environment.Version.ToString)";
        }

        public string GetIsSerializerInjectedExpressionText()
        {
            return "NameOf(YellowFlavor.Serialization.ObjectSerializer.Serialize)";
        }

        public string GetSerializedValueExpressionText(string expression, string format, string settings)
        {
            return $@"YellowFlavor.Serialization.ObjectSerializer.Serialize({expression}, ""{format}"", ""{settings}"")";
        }

        public string GetLoadAssemblyExpressionText(string serializerFileName)
        {
            return $"System.Reflection.Assembly.LoadFile(\"{serializerFileName}\")";
        }
    }
}
