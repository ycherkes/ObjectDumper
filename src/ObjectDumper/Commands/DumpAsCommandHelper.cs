using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.DebuggeeInteraction;
using ObjectDumper.Options;
using ObjectDumper.Utils;
using System;
using System.IO;
using System.Linq;
using Constants = EnvDTE.Constants;

namespace ObjectDumper.Commands
{
    internal class DumpAsCommandHelper
    {
        private const string ImmediateWindowObjectKind = "{ECB7191A-597B-41F5-9843-03A4CF275DDE}";
        private readonly DTE2 _dte;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;

        public DumpAsCommandHelper(DTE2 dte, IServiceProvider serviceProvider, ObjectDumperOptionPage optionPage)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _interactionService = new InteractionService(dte, dte.Debugger, optionPage);
        }

        public bool IsCommandAvailable()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return _dte.Debugger != null
                && _dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode
                && _dte.Debugger.CurrentStackFrame != null
                && (_dte.ActiveWindow.ObjectKind == Constants.vsDocumentKindText ||
                    _dte.ActiveWindow.ObjectKind == ImmediateWindowObjectKind);

        }

        public void ExecuteCommand(string format)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var (success, selectionText) = TextViewUtils.GetSelectedText(_serviceProvider);

            if (!success)
            {
                return;
            }

            var (isInjected, injectionEvaluationResult) = _interactionService.InjectSerializer();

            var fileName = SanitizeFileName(selectionText.Any(char.IsWhiteSpace) ? "expression" : selectionText);

            var fileContent = isInjected
                ? _interactionService.GetSerializedValue(selectionText, format)
                : injectionEvaluationResult;

            var fileExtension = $".{format}";

            CreateNewFile($"{fileName}{fileExtension}", fileContent);
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return invalids.Aggregate(fileName, (current, c) => current.Replace(c, '_'));
        }

        private void CreateNewFile(string fileName, string fileContent)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var newFileWindow = _dte.ItemOperations.NewFile(Name: fileName);
            var newDocument = newFileWindow.Document;

            if (!string.IsNullOrEmpty(fileContent))
            {
                var selection = (TextSelection)newDocument.Selection;
                selection?.Insert(fileContent);
                selection?.StartOfDocument();
            }

            newDocument.Save();
        }
    }
}
