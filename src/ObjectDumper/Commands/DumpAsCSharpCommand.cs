﻿using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.Options;
using Task = System.Threading.Tasks.Task;

namespace ObjectDumper.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DumpAsCSharpCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("1fa17746-fa3e-4970-bbc9-d8a37b866b73");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly ObjectDumperPackage _package;

        private readonly DumpAsCommandHelper _dumpAsCommandHelper;
        private readonly ObjectDumperOptionPage _optionsPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpAsCSharpCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DumpAsCSharpCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = (ObjectDumperPackage)package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _optionsPage = (ObjectDumperOptionPage)package.GetDialogPage(typeof(ObjectDumperOptionPage));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            _dumpAsCommandHelper = new DumpAsCommandHelper(_package.Dte, package);
        }

        private async void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!(sender is OleMenuCommand menuCommand)) return;

            // start by assuming that the menu will not be shown
            menuCommand.Visible = false;
            menuCommand.Enabled = false;

            var isAvailable = _optionsPage.CSharpEnabled && await _dumpAsCommandHelper.IsCommandAvailableAsync();

            menuCommand.Visible = isAvailable;
            menuCommand.Enabled = isAvailable;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DumpAsCSharpCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DumpAsCSharpCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new DumpAsCSharpCommand(package, commandService);
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

            _dumpAsCommandHelper.ExecuteCommand("cs");
        }
    }
}
