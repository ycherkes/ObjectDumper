export class CSharpExpressionProvider{
    public getStringTypeAssemblyLocationExpressionText(): string
    {
        return "typeof(System.String).Assembly.Location";
    }

    public getIsSerializerInjectedExpressionText(): string
    {
        return "nameof(YellowFlavor.Serialization.ObjectSerializer.Serialize)";
    }

    public getSerializedValueExpressionText(expression: string, format: string, options: string): string
    {
        return `YellowFlavor.Serialization.ObjectSerializer.Serialize(${expression}, "${format}", "${options}")`;
    }

    public getLoadAssemblyExpressionText(serializerFileName: string): string
    {
        return `System.Reflection.Assembly.LoadFile(@\"${serializerFileName}\")`;
    }
}
