import * as assert from 'assert';

// You can import and use all API from the 'vscode' module
// as well as import your extension to test it
import * as vscode from 'vscode';
import { sanitizeFileName } from '../../utils/sanitizeFileName';

suite('Extension Test Suite', () => {
	vscode.window.showInformationMessage('Start all tests.');

	test('Sample test', () => {
		assert.strictEqual(-1, [1, 2, 3].indexOf(5));
		assert.strictEqual(-1, [1, 2, 3].indexOf(0));
	});

	test('Object Dumper activates successfully', async () => {
		const extension = vscode.extensions.getExtension('YevhenCherkes.object-dumper');

		assert.ok(extension);
		await extension.activate();
		assert.strictEqual(extension.isActive, true);
	});

	test('Sanitizes expression text for use in a temporary file name', () => {
		assert.strictEqual(sanitizeFileName('customer:address?'), 'customeraddress');
		assert.strictEqual(sanitizeFileName('CON'), 'expression');
		assert.strictEqual(sanitizeFileName('...'), 'expression');
	});
});
