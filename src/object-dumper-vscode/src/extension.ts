import * as vscode from 'vscode';
import { ExpressionEvaluator } from './debuggee_interaction/expressionEvaluator';
import { CSharpExpressionProvider } from './debuggee_interaction/expressionProviders/csharpExpressionProvider';
import { InteractionService } from './debuggee_interaction/interactionService';
import * as tempUtils from './utils/TempFile'; 
import * as sanitazeUtil from 'sanitize-filename-ts';

export async function activate(context: vscode.ExtensionContext) {

	const expressionEvaluator = new ExpressionEvaluator();
	const expressionProvider = new CSharpExpressionProvider();
    const interactionService = new InteractionService(expressionEvaluator, context.extensionPath, expressionProvider);

	async function dumpAs(format: string): Promise<void>{
		const [result, success] = await interactionService.injectSerializer();

		if(!success){
			vscode.window.showInformationMessage(`error: ${result}`);
			return;
		}

        var editor = vscode.window.activeTextEditor;
		if (!editor) {
			vscode.window.showInformationMessage("No open text editor.");
			return; // No open text editor
		}

		const selection = editor.selection;
		var text: string = "";
		if(!selection.isEmpty){
			text = editor.document.getText(selection);
		}
        else{
			const cursorPosition = editor.selection.active;
			const range = editor.document.getWordRangeAtPosition(cursorPosition);
            text = editor.document.getText(range);
		}		

		if(text === "")
		{
			vscode.window.showInformationMessage("Empty selection.");
			return;
		}

		const serializedValue = await interactionService.getSerializedValue(text, format);
		const fileName = text.includes(" ") ? "expression" : sanitazeUtil.sanitize(text);
		const tempFile = tempUtils.TempFile.fromContents(serializedValue, fileName, "." + format);
        const tempDocument = new tempUtils.TempDocument(tempFile);
		const tempDocumentEditor = new tempUtils.TempEditor(tempDocument);

		await tempDocumentEditor.open();
	}

	context.subscriptions.push(vscode.commands.registerCommand('ObjectDumper.dumpAsCsharp', () => dumpAs("cs")));
	context.subscriptions.push(vscode.commands.registerCommand('ObjectDumper.dumpAsJson', () => dumpAs("json")));
	context.subscriptions.push(vscode.commands.registerCommand('ObjectDumper.dumpAsVb', () => dumpAs("vb")));
	context.subscriptions.push(vscode.commands.registerCommand('ObjectDumper.dumpAsXml', () => dumpAs("xml")));
	context.subscriptions.push(vscode.commands.registerCommand('ObjectDumper.dumpAsYaml', () => dumpAs("yaml")));
}

// This method is called when your extension is deactivated
export function deactivate() {}
