using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ObjectDumper.Commands;
using ObjectDumper.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(ObjectDumperOptionPage), "Object Dumper", "General", 0, 0, true)]
    public sealed class ObjectDumperPackage : AsyncPackage
    {
        private List<IDisposable> _menuItems;

        /// <summary>
        /// ObjectDumperPackage GUID string.
        /// </summary>
        private const string PackageGuidString = "75562b3a-ff38-4ad7-94f8-dc7f08140914";

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

            var dte = (DTE2)GetGlobalService(typeof(DTE));
            var menuCommandService = (OleMenuCommandService)await GetServiceAsync(typeof(IMenuCommandService));
            Assumes.Present(menuCommandService);
            var optionsPage = (ObjectDumperOptionPage)GetDialogPage(typeof(ObjectDumperOptionPage));
            var commandHelper = new DumpAsCommandHelper(dte, this, optionsPage);

            _menuItems = new[]
            {
                new DumpAsCommand
                (
                    commandId: 0x0100,
                    commandHelper,
                    commandFormat: "cs",
                    commandConfigEnabledFunc: () => optionsPage.CSharpEnabled
                ),
                new DumpAsCommand
                (
                    commandId: 0x0200,
                    commandHelper,
                    commandFormat: "json",
                    commandConfigEnabledFunc: () => optionsPage.JsonEnabled
                ),
                new DumpAsCommand
                (
                    commandId: 0x0300,
                    commandHelper,
                    commandFormat: "xml",
                    commandConfigEnabledFunc: () => optionsPage.XmlEnabled
                ),
                new DumpAsCommand
                (
                    commandId: 0x0400,
                    commandHelper,
                    commandFormat: "vb",
                    commandConfigEnabledFunc: () => optionsPage.VisualBasicEnabled
                ),
                new DumpAsCommand
                (
                    commandId: 0x0500,
                    commandHelper,
                    commandFormat: "yaml",
                    commandConfigEnabledFunc: () => optionsPage.YamlEnabled
                )
            }
            .Select(x => x.AddMenuCommand(menuCommandService))
            .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var menuItem in _menuItems)
                {
                    menuItem.Dispose();
                }
                _menuItems.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
