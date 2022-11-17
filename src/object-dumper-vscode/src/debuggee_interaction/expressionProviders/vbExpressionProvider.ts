import { IExpressionProvider } from "./iExpressionProvider";

export class VbExpressionProvider implements IExpressionProvider{
    public getStringTypeAssemblyLocationExpressionText(): string
    {
        return "GetType(System.String).Assembly.Location";
    }

    public getIsSerializerInjectedExpressionText(): string
    {
        return "NameOf(YellowFlavor.Serialization.ObjectSerializer.Serialize)";
    }

    public getSerializedValueExpressionText(expression: string, format: string, options: string): string
    {
        return `YellowFlavor.Serialization.ObjectSerializer.Serialize(${expression}, "${format}", "${options}")`;
    }

    public getLoadAssemblyExpressionText(serializerFileName: string): string
    {
        return `System.Reflection.Assembly.LoadFile(\"${serializerFileName}\")`;
    }
}