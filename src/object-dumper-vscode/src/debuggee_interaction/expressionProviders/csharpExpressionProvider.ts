export class CSharpExpressionProvider{
    public getIsSerializerInjectedExpressionText(): string
    {
        return "nameof(YellowFlavor.Serialization.ObjectSerializer.SerializeToFile)";
    }

    public getSerializedValueExpressionText(expression: string, options: string): string
    {
        return `YellowFlavor.Serialization.ObjectSerializer.SerializeToFile(${expression}, "${options}")`;
    }

    public getLoadAssemblyExpressionText(serializerFileName: string): string
    {
        return `System.Reflection.Assembly.LoadFile(@\"${serializerFileName}\")`;
    }
}
