import * as vscode from 'vscode';

export class ExpressionEvaluator{
    
    public async evaluateExpression(expression: string): Promise<[value: string, isValidValue: boolean]>
    {
        const debugSession = vscode.debug.activeDebugSession;

        if(!debugSession)
		{
			vscode.window.showInformationMessage("No active debug session.");
			return ["", false];
		}

        const frameId = 1000;

		// Context: 'clipboard' will never truncate the response, see https://github.com/microsoft/vscode-js-debug/issues/689#issuecomment-669257847

		const args = {
			expression: expression,
			frameId: frameId,
			context: 'clipboard'
		  };

        var resultValue: string = "";
        var isValidValue: boolean = false;
		
		await debugSession.customRequest('evaluate', args).then((response) => {
				resultValue = response.result as string;
				isValidValue = !response.presentationHint?.attributes?.includes('failedEvaluation');
			  }, error => {
                resultValue = error.message;
			});

        return [resultValue, isValidValue];
    }
}
