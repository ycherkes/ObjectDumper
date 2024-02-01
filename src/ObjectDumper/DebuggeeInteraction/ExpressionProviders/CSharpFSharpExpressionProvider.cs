namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders;

internal class CSharpFSharpExpressionProvider : IExpressionProvider
{
    public string GetTargetFrameworkExpressionText()
    {
        return "((System.Runtime.Versioning.TargetFrameworkAttribute)System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Runtime.Versioning.TargetFrameworkAttribute)))?.FrameworkName + \";\" + ((System.Runtime.Versioning.TargetFrameworkAttribute)System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Runtime.Versioning.TargetFrameworkAttribute)))?.FrameworkName";
    }

    public string GetIsSerializerInjectedExpressionText()
    {
        return "nameof(YellowFlavor.Serialization.ObjectSerializer.SerializeToFile)";
    }

    public string GetSerializedValueExpressionText(string expression, string settings)
    {
        return $"""YellowFlavor.Serialization.ObjectSerializer.SerializeToFile({expression}, "{settings}")""";
    }

    public string GetLoadAssemblyExpressionText(string serializerFileName)
    {
        return $"System.Reflection.Assembly.LoadFile(@\"{serializerFileName}\")";
    }
}