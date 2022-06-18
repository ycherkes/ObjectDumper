using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.DebuggeeInteraction;
using ObjectDumper.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Constants = EnvDTE.Constants;

namespace ObjectDumper.Commands
{
    internal class DumpAsCommandHelper
    {
        private readonly DTE2 _dte;
        private readonly InteractionService _interactionService;
        private readonly AsyncPackage _package;

        public DumpAsCommandHelper(DTE2 dte, AsyncPackage package)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _interactionService = new InteractionService(dte, package);
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

            var (success, selectionText) = TextViewUtils.GetSelectedText(_package);

            if (!success)
            {
                return;
            }

            var (isInjected, evaluationResult) = _interactionService.InjectFormatter();

            var expression = selectionText;

            var fileName = SanitizeFileName(expression.Any(char.IsWhiteSpace) ? "expression" : expression);

            var (_, value) = isInjected ? _interactionService.GetFormattedValue(expression, format) : (false, evaluationResult);

            var formattedValue = value;

            var fileExtension = $".{format}";

            CreateNewFile($"{fileName}{fileExtension}", formattedValue);
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
