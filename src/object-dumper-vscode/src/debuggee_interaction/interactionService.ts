import path = require("node:path");
import { ExpressionEvaluator } from './expressionEvaluator';
import { CSharpExpressionProvider } from './expressionProviders/csharpExpressionProvider';
import { OptionsProvider } from "./optionsProvider";

export class InteractionService    {

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
            let evaluationResult = await this.expressionEvaluator.evaluateExpression(loadAssemblyExpressionText, 5000);

            return evaluationResult;
        }

        public async getSerializedValue(expression: string, language: string): Promise<string>
        {
            const options = this.optionsProvider.getOptions(language);
            const optionsJson = JSON.stringify(options);
            const base64Options = Buffer.from(optionsJson, 'binary').toString('base64');
            const serializeExpressionText = this.expressionProvider.getSerializedValueExpressionText(expression, language, base64Options);
            const [value, isValidValue] = await this.expressionEvaluator.evaluateExpression(serializeExpressionText, 25000);
            var trimmedValue = value.replace(/^"(.*)"$/, '$1');

            if (isValidValue){
                return Buffer.from(trimmedValue, 'base64').toString('binary');
            }
            else{
                return trimmedValue;
            }
        }

        private async isSerializerInjected(): Promise<boolean>
        {
            var isSerializerInjectedExpressionText = this.expressionProvider.getIsSerializerInjectedExpressionText();
            const [_, isValidValue] = await this.expressionEvaluator.evaluateExpression(isSerializerInjectedExpressionText, 5000);
            return isValidValue;
        }        
    }