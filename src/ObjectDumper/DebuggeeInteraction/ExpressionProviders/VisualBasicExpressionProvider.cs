namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders
{
    internal class VisualBasicExpressionProvider : IExpressionProvider
    {
        public string GetTargetFrameworkExpressionText()
        {
            return "CType(System.Attribute.GetCustomAttribute(If(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.Assembly.GetExecutingAssembly()), GetType(System.Runtime.Versioning.TargetFrameworkAttribute)), System.Runtime.Versioning.TargetFrameworkAttribute)?.FrameworkName & \";\" & CType(System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), GetType(System.Runtime.Versioning.TargetFrameworkAttribute)), System.Runtime.Versioning.TargetFrameworkAttribute)?.FrameworkName";
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
