import path = require("node:path");
import { ExpressionEvaluator } from './expressionEvaluator';
import { CSharpExpressionProvider } from './expressionProviders/csharpExpressionProvider';
import { OptionsProvider } from "./optionsProvider";

export class InteractionService {

    constructor(
        private readonly expressionEvaluator: ExpressionEvaluator,
        private readonly optionsProvider: OptionsProvider,
        private readonly extensionLocation: string,
        private readonly expressionProvider: CSharpExpressionProvider
    ) {}

        public async injectSerializer(): Promise<[evaluationResult: string, success: boolean]>
        {
            const isSerializerInjected = await this.isSerializerInjected();

            if (isSerializerInjected)
            {
                return ["", true];
            }

            var serializerFileName = path.join(this.extensionLocation,
                "injectable_libraries",
                "netstandard2.0",
                "YellowFlavor.Serialization.dll");

            let loadAssemblyExpressionText = this.expressionProvider.getLoadAssemblyExpressionText(serializerFileName);
            let evaluationResult = await this.expressionEvaluator.evaluateExpression(loadAssemblyExpressionText);

            return evaluationResult;
        }

        public async getSerializedValue(expression: string, language: string, filePath: string): Promise<[isValid: boolean, data: string]>
        {
            const options = this.optionsProvider.getOptions(language);
            const optionsJson = JSON.stringify(options);
            const settings = `${language};${filePath};${optionsJson}`;
            const base64Settings = Buffer.from(settings, 'binary').toString('base64');
            const serializeExpressionText = this.expressionProvider.getSerializedValueExpressionText(expression, base64Settings);
            const [value, isValidValue] = await this.expressionEvaluator.evaluateExpression(serializeExpressionText);
            var trimmedValue = value.replace(/^"(.*)"$/, '$1');
            return [isValidValue, trimmedValue];
        }

        private async isSerializerInjected(): Promise<boolean>
        {
            var isSerializerInjectedExpressionText = this.expressionProvider.getIsSerializerInjectedExpressionText();
            const [_, isValidValue] = await this.expressionEvaluator.evaluateExpression(isSerializerInjectedExpressionText);
            return isValidValue;
        }        
    }