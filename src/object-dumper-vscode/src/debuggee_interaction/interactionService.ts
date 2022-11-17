import path = require("node:path");
import { ExpressionEvaluator } from './expressionEvaluator';
import { IExpressionProvider } from "./expressionProviders/iExpressionProvider";
import { OptionsProvider } from "./optionsProvider";
import * as vscode from 'vscode';

export class InteractionService {

    constructor(
        private readonly expressionEvaluator: ExpressionEvaluator,
        private readonly optionsProvider: OptionsProvider,
        private readonly extensionLocation: string,
        private readonly expressionProviders: Map<string, IExpressionProvider>
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

            let loadAssemblyExpressionText = this.getExpressionProvider().getLoadAssemblyExpressionText(serializerFileName);
            let evaluationResult = await this.expressionEvaluator.evaluateExpression(loadAssemblyExpressionText);

            return evaluationResult;
        }

        public async getSerializedValue(expression: string, language: string): Promise<string>
        {
            const options = this.optionsProvider.getOptions(language);
            const optionsJson = JSON.stringify(options);
            const base64Options = Buffer.from(optionsJson, 'binary').toString('base64');
            const serializeExpressionText = this.getExpressionProvider().getSerializedValueExpressionText(expression, language, base64Options);
            const [value, isValidValue] = await this.expressionEvaluator.evaluateExpression(serializeExpressionText);
            var trimmedValue = value.replace(/^"(.*)"$/, '$1');

            return isValidValue
                ? Buffer.from(trimmedValue, 'base64').toString('binary')
                : trimmedValue;
        }

        private async isSerializerInjected(): Promise<boolean>
        {
            var isSerializerInjectedExpressionText = this.getExpressionProvider().getIsSerializerInjectedExpressionText();
            const [_, isValidValue] = await this.expressionEvaluator.evaluateExpression(isSerializerInjectedExpressionText);
            return isValidValue;
        }

        private getExpressionProvider(): IExpressionProvider{
            const editor = vscode.window.activeTextEditor;
            const [firstValue] = this.expressionProviders.values();

            return editor 
            ? this.expressionProviders.get(editor.document.languageId) ?? firstValue 
            : firstValue;            
        }
    }