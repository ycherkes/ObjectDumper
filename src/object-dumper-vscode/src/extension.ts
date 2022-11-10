// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import * as debug from '@vscode/debugprotocol';
import { ExpressionEvaluator } from './debuggee_interaction/expressionEvaluator';
import { CSharpExpressionProvider } from './debuggee_interaction/expressionProviders/csharpExpressionProvider';
import { InteractionService } from './debuggee_interaction/interactionService';
import * as tempUtils from './utils/TempFile'; 
import * as sanitazeUtil from 'sanitize-filename-ts';
import { stringify } from 'querystring';

// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext) {

	// Use the console to output diagnostic information (console.log) and errors (console.error)
	// This line of code will only be executed once when your extension is activated
	console.log('Congratulations, your extension "ObjectDumper" is now active!');

	// vscode.debug.onDidChangeActiveDebugSession(c => {
	// 	var b = vscode.debug.breakpoints[0];
	// });

	// let disposable1 = vscode.debug.registerDebugAdapterTrackerFactory('*', {
	// 	createDebugAdapterTracker(session: vscode.DebugSession) {
	// 	  return {
	// 		onWillReceiveMessage: handleDAPMessage,//m => vscode.window.showInformationMessage(`result: ${JSON.stringify(m, undefined, 2)}`),
	// 		onDidSendMessage: handleDAPMessage,
	// 	  };
	// 	}
	//   });

	//  function handleDAPMessage(baseMsg: debug.DebugProtocol.ProtocolMessage) {
    //     if (baseMsg.type === 'event') {
    //         const event = ((baseMsg as unknown) as debug.DebugProtocol.Event).event;
    //         if (event === 'stopped') {
    //             const msg = (baseMsg as unknown) as debug.DebugProtocol.StoppedEvent;
	// 			vscode.window.showInformationMessage(`thread id: ${JSON.stringify(msg.body.threadId, undefined, 2)}`);
    //         } else {
    //             // For now there aren't any other events we care about.
    //         }
	// 		} else if (baseMsg.type === 'request') {
	// 			const request = ((baseMsg as unknown) as debug.DebugProtocol.Request);
	// 			if(request.command === 'stackTrace')
	// 			{
	// 				vscode.window.showInformationMessage(`thread id: ${JSON.stringify(request.arguments.threadId, undefined, 2)}`);
	// 			}
	// 			// For now there aren't any requests we care about.
	// 		} else if (baseMsg.type === 'response') {
	// 			const response = ((baseMsg as unknown) as debug.DebugProtocol.Response);
	// 			if(response.command === 'stackTrace')
	// 			{
	// 				vscode.window.showInformationMessage(`thread id: ${JSON.stringify(response, undefined, 2)}`);
	// 			}
	// 		} else {
	// 			// This shouldn't happen but for now we don't worry about it.
	// 		}
    // }

	//   let disposable2 =   vscode.debug.onDidReceiveDebugSessionCustomEvent(event => {
	// 	//if(event.event === 'stopped') {
	// 		// ...
	// 		vscode.window.showInformationMessage(`result: ${JSON.stringify(event, undefined, 2)}`);
	// 	//}
	// });

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

		//vscode.window.showInformationMessage(serializedValue);
        var fileName = text.includes(" ") ? "expression" : sanitazeUtil.sanitize(text);

		const tempFile = tempUtils.TempFile.fromContents(serializedValue, fileName, "." + format);
        const tempDocument = new tempUtils.TempDocument(tempFile);
		const tempDocumentEditor = new tempUtils.TempEditor(tempDocument);
		await tempDocumentEditor.open();
	}

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with registerCommand
	// The commandId parameter must match the command field in package.json
	// let dumpAsCsharpCommand = vscode.commands.registerCommand('ObjectDumper.dumpAsCsharp', async() => {
	// 	// The code you place here will be executed every time your command is executed
	// 	// Display a message box to the user
	
	// 	await dumpAs("cs");
	// 	//vscode.commands.executeCommand('vscode.newFile', serializedValue);
    //     // await vscode.workspace.openTextDocument(newUri).then(
	// 	// 	async document => {      
	// 	// 	  await vscode.window.showTextDocument(document);
	// 	// 	  // if you are using a predefined snippet
	// 	// 	  await vscode.commands.executeCommand('editor.action.insertSnippet', { 'name': 'My Custom Snippet Label Here'});
	// 	// 	});
	// 	// let threads: debug.DebugProtocol.Thread[] = (await session.customRequest('threads')).threads;
	// 	// let someThread = threads.filter(thr => thr.name === ".NET ThreadPool Gate")[0];
    //     // let stackTraceResponse = await session.customRequest('stackTrace', { threadId: someThread.id, startFrame: 0, levels: 20 });
	// 	// let frameId = 1000;// stackTraceResponse.stackFrames[0].id;
	// 	// const args = {
	// 	// 	expression: "(5d+2)/2",
	// 	// 	frameId: frameId,
	// 	// 	context: 'watch'
	// 	//   };
	// 	// await session.customRequest('evaluate', args).then(({result}) => {
	// 	// 		vscode.window.showInformationMessage(result);
	// 	// 	  }, error => {
	// 	// 		vscode.window.showInformationMessage(`error: ${error.message}`);
	// 	// 	});
	// 	//vscode.window.showInformationMessage(response);
    //     // let stackTraceResponse = await session.customRequest('stackTrace', { threadId: 1 });
	// 	// let frameId = stackTraceResponse.stackFrames[0].id;
    //     // let response = await session.customRequest('evaluate', { expression: 'i', frameId: frameId });
	// 	// const args = {
	// 	// 	expression: "i",
	// 	// 	//frameId: frameId,
	// 	// 	context: 'repl'
	// 	//   };
	// 	//   session.customRequest('evaluate', args).then(({result}) => {
	// 	// 	vscode.window.showInformationMessage(result);
	// 	//   }, error => {
	// 	// 	vscode.window.showInformationMessage(`error: ${error.message}`);
	// 	// });

	// 	// session.customRequest("evaluate", { "expression": "10+15", context: 'repl' }).then(reply => {
	// 	// 	vscode.window.showInformationMessage(`result: ${reply.result}`);
	// 	// }, error => {
	// 	// 	vscode.window.showInformationMessage(`error: ${error.message}`);
	// 	// });
	// });

	let dumpAsCsharpCommand = vscode.commands.registerCommand('ObjectDumper.dumpAsCsharp', async() => {
		await dumpAs("cs");
	});

	let dumpAsJsonCommand = vscode.commands.registerCommand('ObjectDumper.dumpAsJson', async() => {
		await dumpAs("json");
	});

	let dumpAsVbCommand = vscode.commands.registerCommand('ObjectDumper.dumpAsVb', async() => {
		await dumpAs("vb");
	});

	let dumpAsXmlCommand = vscode.commands.registerCommand('ObjectDumper.dumpAsXml', async() => {
		await dumpAs("xml");
	});

	let dumpAsYamlCommand = vscode.commands.registerCommand('ObjectDumper.dumpAsYaml', async() => {
		await dumpAs("yaml");
	});

	context.subscriptions.push(dumpAsCsharpCommand);
	context.subscriptions.push(dumpAsJsonCommand);
	context.subscriptions.push(dumpAsVbCommand);
	context.subscriptions.push(dumpAsXmlCommand);
	context.subscriptions.push(dumpAsYamlCommand);
	// context.subscriptions.push(disposable1);
	// context.subscriptions.push(disposable2);
}

// This method is called when your extension is deactivated
export function deactivate() {}
