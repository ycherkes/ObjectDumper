using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
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

            if (_dte.Debugger == null || _dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode ||
                _dte.Debugger.CurrentStackFrame == null)
            {
                return false;
            }

            if (!(_dte.ActiveDocument.Selection is TextSelection ts))
                return false;

            var selectionText = ts.Text;

            if (!string.IsNullOrEmpty(selectionText))
                return true;//_dte.Debugger.GetExpression(selectionText).IsValidValue;

            var document = Utils.GetActiveDocument();

            if (document == null) return false;

            var textView = Utils.GetWpfView();

            if (textView == null) return false;

            var caretPos = textView.Caret.Position.BufferPosition.Position;

            ISymbol symbol = Utils.FindSymbolAsync(document, caretPos, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            
            if (symbol != null)
            {
               return _dte.Debugger.GetExpression(symbol.Name).IsValidValue;
            }

            return false;
        }

        public void ExecuteCommand(string format)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(_dte.ActiveDocument.Selection is TextSelection ts))
                return;

            var selectionText = ts.Text;

            if (string.IsNullOrEmpty(selectionText))
            {
                var document = Utils.GetActiveDocument();

                if (document == null) return;

                var textView = Utils.GetWpfView();

                if (textView == null) return;

                var caretPos = textView.Caret.Position.BufferPosition.Position;

                ISymbol symbol = Utils.FindSymbolAsync(document, caretPos, CancellationToken.None).ConfigureAwait(false)
                    .GetAwaiter().GetResult();
                if (symbol != null)
                {
                    selectionText = symbol.Name;
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
            var fileName = expression.Any(char.IsWhiteSpace) ? "expression" : expression;

            var runFormatterExpression = _dte.Debugger.GetExpression($@"ObjectFormatter.Formatter.Format({expression}, ""{format}"")");

            string formattedValue = Regex.Unescape(runFormatterExpression.Value).Trim('"');

            switch (format)
            {
                case "csharp":
                    CreateNewFile(@"General\Visual C# Class", $"{fileName}.cs", formattedValue);
                    break;
                case "xml":
                    CreateNewFile(@"General\XML File", $"{fileName}.xml", formattedValue);
                    break;
                case "json":
                    CreateNewFile(@"Web\JSON File", $"{fileName}.json", formattedValue);
                    break;
            }
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
                ? _dte.Solution.Projects.OfType<Project>()
                    .SingleOrDefault(x => x.Name == entryAssemblyValue)
                : _dte.ActiveDocument.ProjectItem.ContainingProject;

            return project;
        }

        internal void CreateNewFile(string fileType, string title, string fileContents)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var newDocument = _dte.ItemOperations.NewFile(fileType, title).Document;
            if (!string.IsNullOrEmpty(fileContents))
            {
                var selection = (TextSelection)newDocument.Selection;
                selection?.SelectAll();
                selection?.Delete();
                selection?.Insert(fileContents);
            }
            newDocument.Saved = true;
        }
    }
}
