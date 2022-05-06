using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ObjectDumper
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CSharpProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(ObjectDumperPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ObjectDumperPackage : AsyncPackage
    {
        /// <summary>
        /// ObjectDumperPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "75562b3a-ff38-4ad7-94f8-dc7f08140914";

        public DTE2 Dte { get; set; }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            Dte = (DTE2)GetGlobalService(typeof(DTE));

            //Dte.Events.DebuggerEvents.OnEnterBreakMode += DebuggerEvents_OnEnterBreakMode;
            await DumpAsCSharpCommand.InitializeAsync(this);
            await DumpAsJsonCommand.InitializeAsync(this);
            await DumpAsXmlCommand.InitializeAsync(this);
        }

        private void DebuggerEvents_OnEnterBreakMode(dbgEventReason reason, ref dbgExecutionAction executionAction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);
            string entryAssembly = "System.Reflection.Assembly.GetEntryAssembly().GetName().Name";
            var entryAssemblyExpression = Dte.Debugger.GetExpression(entryAssembly);
            if(!entryAssemblyExpression.IsValidValue)
            {
                return;
            }

            var project = Dte.Solution.Projects.OfType<Project>().SingleOrDefault(x => x.Name == entryAssemblyExpression.Value.Trim('"'));

            if(project == null)
            {
                return;
            }

            var xProject = XDocument.Parse(File.ReadAllText(project.FileName));
            //var targetFramework = xProject.Root.DescendantNodes().OfType<XElement>().FirstOrDefault(x => x.Name.LocalName == "TargetFramework");
            var targetFrameworkVersion = xProject.Root.DescendantNodes().OfType<XElement>().FirstOrDefault(x => x.Name.LocalName == "TargetFrameworkVersion");
            var formatterFileName = Path.Combine(targetFrameworkVersion != null 
                ? $@"{dllLocation}\Formatter\net45\" 
                : $@"{dllLocation}\Formatter\netcoreapp3.1\", 
                "ObjectFormatter.dll");

            string loadAssembly = $"System.Reflection.Assembly.LoadFile(\"{formatterFileName}\")";
            var loadAssemblyExpression = Dte.Debugger.GetExpression(loadAssembly);
            if(!loadAssemblyExpression.IsValidValue)
            {
                return;
            }
            var format = "json";
            var runFormatterExpression = Dte.Debugger.GetExpression($@"ObjectFormatter.Formatter.Format(someProp, ""{format}"")");
            
            
            if (runFormatterExpression.IsValidValue)
            {
                string formattedValue = System.Text.RegularExpressions.Regex.Unescape(runFormatterExpression.Value).Trim('"');

                switch (format)
                {
                    case "csharp":
                        CreateNewFile(@"General\Visual C# Class", "someProp.cs", formattedValue);
                        break;
                    case "xml":
                        CreateNewFile(@"General\XML File", "someProp.xml", formattedValue);
                        break;
                    case "json":
                        CreateNewFile(@"Web\JSON File", "someProp.json", formattedValue);
                        break;
                }
            }
        }

        internal void CreateNewFile(string fileType, string title, string fileContents)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var newDocument = Dte.ItemOperations.NewFile(fileType, title).Document;
            if (!string.IsNullOrEmpty(fileContents))
            {
                var selection = (TextSelection)newDocument.Selection;
                selection?.SelectAll();
                selection?.Delete();
                selection?.Insert(fileContents);
            }
            newDocument.Saved = true;
        }

        #endregion
    }
}
