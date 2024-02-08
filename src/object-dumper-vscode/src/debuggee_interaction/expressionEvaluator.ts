import * as vscode from 'vscode';

export class ExpressionEvaluator{
    
	private frameId: number = 1000;
	
	public initialize(){
		var onDidSendMessageMethod = this.onDidSendMessage;
		vscode.debug.registerDebugAdapterTrackerFactory("coreclr", {
			createDebugAdapterTracker(_session: vscode.DebugSession) {
			  return {
				onDidSendMessage: onDidSendMessageMethod
			  };
			},
		  });	
	}

	private onDidSendMessage(m: any){
		if (m.success && m.command === 'stackTrace') {
			if (m.body?.stackFrames && m.body.stackFrames.length > 0) {
				this.frameId = m.body.stackFrames[0].id;
			}		
	  }
	}

    public async evaluateExpression(expression: string): Promise<[value: string, isValidValue: boolean]>
    {
        const debugSession = vscode.debug.activeDebugSession;

        if(!debugSession)
		{
			vscode.window.showInformationMessage("No active debug session.");
			return ["", false];
		}

        // Context: 'clipboard' will never truncate the response, see https://github.com/microsoft/vscode-js-debug/issues/689#issuecomment-669257847

		const args = {
			expression: expression,
			frameId: this.frameId,
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
