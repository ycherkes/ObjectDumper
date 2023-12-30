export class CSharpExpressionProvider{
    public getIsSerializerInjectedExpressionText(): string
    {
        return "nameof(YellowFlavor.Serialization.ObjectSerializer.Serialize)";
    }

    public getSerializedValueExpressionText(expression: string, format: string, filePath: string, options: string): string
    {
        return `YellowFlavor.Serialization.ObjectSerializer.SerializeToFile(${expression}, "${format}", @"${filePath}", "${options}")`;
    }

    public getLoadAssemblyExpressionText(serializerFileName: string): string
    {
        return `System.Reflection.Assembly.LoadFile(@\"${serializerFileName}\")`;
    }
}
