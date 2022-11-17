export interface IExpressionProvider {
    getStringTypeAssemblyLocationExpressionText(): string;
    getIsSerializerInjectedExpressionText(): string;
    getSerializedValueExpressionText(expression: string, format: string, options: string): string;
    getLoadAssemblyExpressionText(serializerFileName: string): string;
  }