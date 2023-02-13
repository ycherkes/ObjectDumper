using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace ObjectDumper.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DumpAsCommand
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        private static readonly Guid CommandSet = new Guid("1fa17746-fa3e-4970-bbc9-d8a37b866b73");

        private readonly CommandID _commandId;
        private readonly DumpAsCommandHelper _commandHelper;
        private readonly string _commandFormat;
        private readonly Func<bool> _commandConfigEnabledFunc;

        public DumpAsCommand(int commandId,
            DumpAsCommandHelper commandHelper,
            string commandFormat,
            Func<bool> commandConfigEnabledFunc)
        {
            _commandId = new CommandID(CommandSet, commandId);
            _commandHelper = commandHelper ?? throw new ArgumentNullException(nameof(commandHelper));
            _commandFormat = commandFormat ?? throw new ArgumentNullException(nameof(commandFormat));
            _commandConfigEnabledFunc = commandConfigEnabledFunc ?? throw new ArgumentNullException(nameof(commandConfigEnabledFunc));
        }

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

            _commandHelper.ExecuteCommand(_commandFormat);
        }

        private sealed class DisposableMenuCommand : OleMenuCommand, IDisposable
        {
            private readonly EventHandler _beforeQueryStatusHandler;
            public DisposableMenuCommand(EventHandler executeCommandHandler, EventHandler beforeQueryStatusHandler, CommandID command)
                : base(executeCommandHandler, command)
            {
                _beforeQueryStatusHandler = beforeQueryStatusHandler ?? throw new ArgumentNullException(nameof(beforeQueryStatusHandler));
                BeforeQueryStatus += beforeQueryStatusHandler;
            }

            public void Dispose()
            {
                BeforeQueryStatus -= _beforeQueryStatusHandler;
            }
        }
    }
}
