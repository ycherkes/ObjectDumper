using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ObjectDumper.Commands;
using ObjectDumper.Options;
using ObjectDumper.Recording;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ObjectDumper;

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
[InstalledProductRegistration("#110", "#112", PackageConstants.Version, IconResourceID = 400)] // Info on this package for Help/About
[Guid(PackageConstants.Id)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideOptionPage(typeof(ObjectDumperOptionPage), "Object Dumper", "General", 0, 0, true)]
public sealed class ObjectDumperPackage : AsyncPackage
{
    private List<IDisposable> _menuItems;
    private OleMenuCommandService _menuCommandService;
    private ObjectDumperOptionPage _optionsPage;
    private CommandRecorder _commandRecorder;

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
        _menuCommandService = (OleMenuCommandService)await GetServiceAsync(typeof(IMenuCommandService));
        Assumes.Present(_menuCommandService);
        _optionsPage = (ObjectDumperOptionPage)GetDialogPage(typeof(ObjectDumperOptionPage));
        var statusBar = (IVsStatusbar)await GetServiceAsync(typeof(SVsStatusbar));
        Assumes.Present(statusBar);
        _commandRecorder = new CommandRecorder(dte, statusBar);
        var commandHelper = new DumpAsCommandHelper(dte, this, _optionsPage, _commandRecorder, statusBar);

        _menuItems = new[]
        {
            new DumpAsCommand
            (
                commandId: 0x0100,
                commandHelper,
                commandFormat: "cs",
                commandConfigEnabledFunc: () => _optionsPage.CSharpEnabled,
                optionPage: _optionsPage,
                dte: dte
            ),
            new DumpAsCommand
            (
                commandId: 0x0200,
                commandHelper,
                commandFormat: "json",
                commandConfigEnabledFunc: () => _optionsPage.JsonEnabled,
                optionPage: _optionsPage,
                dte: dte
            ),
            new DumpAsCommand
            (
                commandId: 0x0300,
                commandHelper,
                commandFormat: "xml",
                commandConfigEnabledFunc: () => _optionsPage.XmlEnabled,
                optionPage: _optionsPage,
                dte: dte
            ),
            new DumpAsCommand
            (
                commandId: 0x0400,
                commandHelper,
                commandFormat: "vb",
                commandConfigEnabledFunc: () => _optionsPage.VisualBasicEnabled,
                optionPage: _optionsPage,
                dte: dte
            ),
            new DumpAsCommand
            (
                commandId: 0x0500,
                commandHelper,
                commandFormat: "yaml",
                commandConfigEnabledFunc: () => _optionsPage.YamlEnabled,
                optionPage: _optionsPage,
                dte: dte
            )
        }
        .Select(x => x.AddMenuCommand(_menuCommandService))
        .ToList();

        _menuItems.Add(new ReplayDumpAsCommand
        (
            commandId: 0x0600,
            _commandRecorder
        ).AddMenuCommand(_menuCommandService));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _menuCommandService.Dispose();
            _optionsPage.Dispose();

            foreach (var menuItem in _menuItems)
            {
                menuItem.Dispose();
            }
            _menuItems.Clear();
            _commandRecorder.Dispose();
        }
        base.Dispose(disposing);
    }
}