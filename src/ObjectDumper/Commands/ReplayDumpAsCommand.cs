using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using ObjectDumper.Recording;

namespace ObjectDumper.Commands;

/// <summary>
/// Command handler
/// </summary>
internal sealed class ReplayDumpAsCommand(
    int commandId,
    CommandRecorder commandRecorder)
{
    private readonly CommandID _commandId = new(PackageConstants.CommandSet, commandId);
    
    public IDisposable AddMenuCommand(MenuCommandService commandService)
    {
        var menuItem = new DisposableMenuCommand(Execute, BeforeQueryStatus, _commandId);
        commandService.AddCommand(menuItem);
        return menuItem;
    }

    private void BeforeQueryStatus(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // get the menu that fired the event
        if (!(sender is OleMenuCommand menuCommand)) return;

        var isAvailable = commandRecorder.IsCommandAvailable();

        menuCommand.Visible = isAvailable;
        menuCommand.Enabled = isAvailable;
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void Execute(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        commandRecorder.ExecuteRecordedCommand();
    }
}