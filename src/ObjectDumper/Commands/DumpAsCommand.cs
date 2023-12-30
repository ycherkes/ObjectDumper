using Microsoft.VisualStudio.Shell;
using ObjectDumper.Options;
using System;
using System.ComponentModel.Design;
using EnvDTE80;

namespace ObjectDumper.Commands;

/// <summary>
/// Command handler
/// </summary>
internal sealed class DumpAsCommand(
    int commandId,
    DumpAsCommandHelper commandHelper,
    string commandFormat,
    Func<bool> commandConfigEnabledFunc,
    ObjectDumperOptionPage optionPage,
    DTE2 dte)
{
    private readonly CommandID _commandId = new(PackageConstants.CommandSet, commandId);
    private readonly DumpAsCommandHelper _commandHelper = commandHelper ?? throw new ArgumentNullException(nameof(commandHelper));
    private readonly string _commandFormat = commandFormat ?? throw new ArgumentNullException(nameof(commandFormat));
    private readonly Func<bool> _commandConfigEnabledFunc = commandConfigEnabledFunc ?? throw new ArgumentNullException(nameof(commandConfigEnabledFunc));
    private readonly ObjectDumperOptionPage _optionPage = optionPage ?? throw new ArgumentNullException(nameof(optionPage));

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

        var isAvailable = _commandConfigEnabledFunc() && _commandHelper.IsCommandAvailable();

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

        _commandHelper.ExecuteCommand(_commandFormat, _optionPage.GetDumpOutput(dte));
    }
}