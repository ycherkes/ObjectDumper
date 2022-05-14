using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Constants = EnvDTE.Constants;
using Project = EnvDTE.Project;

namespace ObjectDumper
{
    internal class DumpAsCommandHelper
    {
        private readonly DTE2 _dte;

        public DumpAsCommandHelper(DTE2 dte)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
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
                    (uint)(WORDEXTFLAGS.WORDEXT_CURRENT),
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
                var startLine = Math.Min(piAnchorLine, piEndLine);
                var endLine = Math.Max(piAnchorLine, piEndLine);
                var startCol = Math.Min(piAnchorCol, piEndCol);
                var endCol = Math.Max(piAnchorCol, piEndCol);

                if(buffer.GetLineText(startLine, startCol, endLine, endCol, out selectionText) != VSConstants.S_OK)
                {
                    return;
                }
            }

            var isFormatterInjected = "nameof(ObjectFormatter.Formatter.Format)";

            var formatterInjectedExpression = _dte.Debugger.GetExpression(isFormatterInjected);

            if (!formatterInjectedExpression.IsValidValue)
            {
                var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);
                var project = GetEntryProject();

                if (project == null)
                {
                    return;
                }

                var xProject = XDocument.Parse(File.ReadAllText(project.FileName));
                
                var targetFrameworkVersion = xProject.Root.DescendantNodes().OfType<XElement>()
                    .FirstOrDefault(x => x.Name.LocalName == "TargetFrameworkVersion");

                var formatterFileName = Path.Combine(dllLocation,
                    "Formatter",
                    targetFrameworkVersion != null
                        ? "net45"
                        : "netcoreapp3.1",
                    "ObjectFormatter.dll");

                string loadAssembly = $"System.Reflection.Assembly.LoadFile(@\"{formatterFileName}\")";
                var loadAssemblyExpression = _dte.Debugger.GetExpression(loadAssembly);
                if (!loadAssemblyExpression.IsValidValue)
                {
                    return;
                }
            }

            var expression = selectionText;

            //var expressionName = _dte.Debugger.GetExpression(expression).Name;

            var fileName = expression.Any(char.IsWhiteSpace) ? "expression" : expression;

            var runFormatterExpression = _dte.Debugger.GetExpression($@"ObjectFormatter.Formatter.Format({expression}, ""{format}"")");

            string formattedValue = Regex.Unescape(runFormatterExpression.Value).Trim('"');

            var fileExtension = GetFileExtension(format);

            CreateNewFile($"{fileName}{fileExtension}", formattedValue);
        }

        private static string GetFileExtension(string format)
        {
            return format == "csharp" ? ".cs" : $".{format}";
        }

        private Project GetEntryProject()
        {
            string entryAssembly = "System.Reflection.Assembly.GetEntryAssembly().GetName().Name";
            var entryAssemblyExpression = _dte.Debugger.GetExpression(entryAssembly);
            if (!entryAssemblyExpression.IsValidValue)
            {
                entryAssembly = "System.Reflection.Assembly.GetCallingAssembly().GetName().Name";

                entryAssemblyExpression = _dte.Debugger.GetExpression(entryAssembly);
                if (!entryAssemblyExpression.IsValidValue)
                {
                    return null;
                }
            }

            var entryAssemblyValue = entryAssemblyExpression.Value.Trim('"');

           var project = entryAssemblyValue != "testhost"
                      && entryAssemblyValue != "ReSharperTestRunner"
                      && entryAssemblyValue != "Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter"
                ? _dte.Solution.GetProjectsRecursively().FirstOrDefault(x => x.Name == entryAssemblyValue)
                : _dte.ActiveDocument.ProjectItem.ContainingProject;

            return project;
        }

        internal void CreateNewFile(string fileName, string fileContents)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var newFileWindow = _dte.ItemOperations.NewFile(Name: fileName);
            var newDocument = newFileWindow.Document;

            if (!string.IsNullOrEmpty(fileContents))
            {
                var selection = (TextSelection)newDocument.Selection;
                selection?.SelectAll();
                selection?.Delete();
                selection?.Insert(fileContents);
                selection?.StartOfDocument();
            }
            newDocument.Saved = true;
        }
    }
}
