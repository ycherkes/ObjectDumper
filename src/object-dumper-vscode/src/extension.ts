import * as vscode from 'vscode';
import { ExpressionEvaluator } from './debuggee_interaction/expressionEvaluator';
import { CSharpExpressionProvider } from './debuggee_interaction/expressionProviders/csharpExpressionProvider';
import { InteractionService } from './debuggee_interaction/interactionService';
import * as tempUtils from './utils/TempFile'; 
import { OptionsProvider } from './debuggee_interaction/optionsProvider';
import { getTempFilePath } from './utils/getTempFilePath';
import { sanitizeFileName } from './utils/sanitizeFileName';
import * as fs from 'fs';

export async function activate(context: vscode.ExtensionContext) {

	const expressionEvaluator = new ExpressionEvaluator();
	const expressionProvider = new CSharpExpressionProvider();
	const optionsProvider = new OptionsProvider();
    const interactionService = new InteractionService(expressionEvaluator, optionsProvider, context.extensionPath, expressionProvider);

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

		const baseFileName = sanitizeFileName(text.includes(" ") ? "expression" : text);
		const fileExtension = "." + format;
		const filePath = getTempFilePath(baseFileName, fileExtension);
		const [isValid, data] = await interactionService.getSerializedValue(text, format, filePath);
		
		var tempFile: tempUtils.TempFile;

		if(isValid){
			tempFile = tempUtils.TempFile.fromExistingFile(filePath, true);
		}
		else{			
			try {
				if(fs.existsSync(filePath)){
					fs.unlinkSync(filePath);
				}				
			} 
			catch{}
						
			tempFile = tempUtils.TempFile.fromContents(data, baseFileName, fileExtension);
		}

        const tempDocument = new tempUtils.TempDocument(tempFile);
		const tempDocumentEditor = new tempUtils.TempEditor(tempDocument);

		await tempDocumentEditor.open();
		context.subscriptions.push(tempDocumentEditor);
	}

	context.subscriptions.push(vscode.commands.registerCommand('objectDumper.dumpAsCsharp', () => dumpAs("cs")));
	context.subscriptions.push(vscode.commands.registerCommand('objectDumper.dumpAsJson', () => dumpAs("json")));
	context.subscriptions.push(vscode.commands.registerCommand('objectDumper.dumpAsVb', () => dumpAs("vb")));
	context.subscriptions.push(vscode.commands.registerCommand('objectDumper.dumpAsXml', () => dumpAs("xml")));
	context.subscriptions.push(vscode.commands.registerCommand('objectDumper.dumpAsYaml', () => dumpAs("yaml")));

}

// This method is called when your extension is deactivated
export function deactivate() {}
