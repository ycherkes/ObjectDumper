using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Constants = EnvDTE.Constants;

namespace ObjectDumper
{
    internal class DumpAsCommandHelper
    {
        private readonly DTE2 _dte;
        private readonly LanguageService _languageService;

        public DumpAsCommandHelper(DTE2 dte, AsyncPackage package)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            _languageService = new LanguageService(dte, package);
        }

        public async Task<bool> IsCommandAvailableAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            return _dte.Debugger != null
                && _dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode
                && _dte.Debugger.CurrentStackFrame != null
                && (_dte.ActiveWindow.ObjectKind == Constants.vsDocumentKindText ||
                    _dte.ActiveWindow.ObjectKind == "{ECB7191A-597B-41F5-9843-03A4CF275DDE}");
            
        }

        public void ExecuteCommand(string format)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string selectionText = string.Empty;

            var textView = Utils.GetTextView();

            if (textView.GetSelection(out int piAnchorLine,
                out int piAnchorCol,
                out int piEndLine,
                out int piEndCol) != VSConstants.S_OK)
            {
                TextSpan[] textSpan = new TextSpan[1];

                if(textView.GetWordExtent(
                    piAnchorLine,
                    piAnchorCol,
                    (uint)WORDEXTFLAGS.WORDEXT_CURRENT,
                    textSpan) != VSConstants.S_OK)
                {
                    return;
                }

                var ts1 = textSpan[0];

                piAnchorLine = ts1.iStartLine;
                piEndLine = ts1.iEndLine;
                piAnchorCol = ts1.iStartIndex;
                piEndCol = ts1.iEndIndex;
            }

            if (piAnchorLine != piEndLine || piAnchorCol != piEndCol)
            {
                if(textView.GetBuffer(out var buffer) != VSConstants.S_OK)
                {
                    return;
                }

                var (startLine, endLine, startCol, endCol) = NormalizeSelection(piAnchorLine, piEndLine, piAnchorCol, piEndCol);

                if(buffer.GetLineText(startLine, startCol, endLine, endCol, out selectionText) != VSConstants.S_OK)
                {
                    return;
                }
            }

            var (isInjected, evaluationResult) = _languageService.InjectFormatter();

            var expression = selectionText;

            var fileName = SanitizeFileName(expression.Any(char.IsWhiteSpace) ? "expression" : expression);

            var (_, value) = isInjected ? _languageService.GetFormattedValue(expression, format) : (false, evaluationResult);

            var formattedValue = value;

            var fileExtension = $".{format}";

            CreateNewFile($"{fileName}{fileExtension}", formattedValue);
        }

        private static (int startLine, int endLine, int startCol, int endCol) NormalizeSelection(int startLine, int endLine, int startCol, int endCol)
        {
            var points = new (int x, int y)[]
            {
                (startLine, startCol),
                (endLine, endCol)
            }
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .ToArray();

            return (points[0].x, points[1].x, points[0].y, points[1].y);
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return invalids.Aggregate(fileName, (current, c) => current.Replace(c, '_'));
        }

        private void CreateNewFile(string fileName, string fileContents)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var newFileWindow = _dte.ItemOperations.NewFile(Name: fileName);
            var newDocument = newFileWindow.Document;

            if (!string.IsNullOrEmpty(fileContents))
            {
                var selection = (TextSelection)newDocument.Selection;
                selection?.Insert(fileContents);
                selection?.StartOfDocument();
            }

            newDocument.Save();
        }
    }
}
