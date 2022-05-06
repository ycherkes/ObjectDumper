using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Document = Microsoft.CodeAnalysis.Document;


namespace ObjectDumper
{
    public static class Utils
    {
		public static async Task<ISymbol> FindSymbolAsync(Document document, int position, CancellationToken cancellationToken)
		{
			var workspace = document.Project.Solution.Workspace;

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var symbol = await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspace, cancellationToken: cancellationToken).ConfigureAwait(false);

            return symbol;
		}

        public static Document GetActiveDocument()
        {
            var dte = (DTE)Package.GetGlobalService(typeof(DTE));
            var activeDocument = dte?.ActiveDocument;
            if (activeDocument == null) return null;

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var workspace = (Workspace)componentModel.GetService<VisualStudioWorkspace>();

            var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(activeDocument.FullName).FirstOrDefault();
            if (documentId == null) return null;

            return workspace.CurrentSolution.GetDocument(documentId);
        }

        /// <summary>
        /// Returns an IVsTextView for the given file path, if the given file is open in Visual Studio.
        /// </summary>
        /// <param name="filePath">Full Path of the file you are looking for.</param>
        /// <returns>The IVsTextView for this file, if it is open, null otherwise.</returns>
        internal static IVsTextView GetIVsTextView(string filePath)
        {
            var dte2 = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE));
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte2;
            using (ServiceProvider serviceProvider = new ServiceProvider(sp))
            {
                Microsoft.VisualStudio.Shell.Interop.IVsUIHierarchy uiHierarchy;
                uint itemID;
                Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame windowFrame;
                if (VsShellUtilities.IsDocumentOpen(serviceProvider, filePath, Guid.Empty,
                        out uiHierarchy, out itemID, out windowFrame))
                {
                    // Get the IVsTextView from the windowFrame.
                    return VsShellUtilities.GetTextView(windowFrame);
                }
            }

            return null;
        }

        public static IWpfTextView GetWpfView()
        {
            var dte2 = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE));
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte2;
            IVsTextManager textManager;
            IComponentModel componentModel;
            using (ServiceProvider serviceProvider = new ServiceProvider(sp))
            {
                textManager = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
                componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            }

            var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            textManager.GetActiveView(1, null, out IVsTextView textViewCurrent);
            return editor.GetWpfTextView(textViewCurrent);
        }
    }
}
