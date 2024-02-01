namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders;

internal class VisualBasicExpressionProvider : IExpressionProvider
{
    public string GetTargetFrameworkExpressionText()
    {
        return "CType(System.Attribute.GetCustomAttribute(If(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.Assembly.GetExecutingAssembly()), GetType(System.Runtime.Versioning.TargetFrameworkAttribute)), System.Runtime.Versioning.TargetFrameworkAttribute)?.FrameworkName & \";\" & CType(System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), GetType(System.Runtime.Versioning.TargetFrameworkAttribute)), System.Runtime.Versioning.TargetFrameworkAttribute)?.FrameworkName";
    }

    public string GetIsSerializerInjectedExpressionText()
    {
        return "NameOf(YellowFlavor.Serialization.ObjectSerializer.SerializeToFile)";
    }

    public string GetSerializedValueExpressionText(string expression, string settings)
    {
        return $"""YellowFlavor.Serialization.ObjectSerializer.SerializeToFile({expression}, "{settings}")""";
    }

    public string GetLoadAssemblyExpressionText(string serializerFileName)
    {
        return $"System.Reflection.Assembly.LoadFile(\"{serializerFileName}\")";
    }
}