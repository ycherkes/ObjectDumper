using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace ObjectDumper
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DumpAsJsonCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("cb7b52b9-a74d-4534-b5ec-b110625425cc");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly ObjectDumperPackage _package;

        private readonly DumpAsCommandHelper _dumpAsCommandHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpAsJsonCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DumpAsJsonCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = (ObjectDumperPackage)package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            _dumpAsCommandHelper = new DumpAsCommandHelper(_package.Dte);
        }

        private async void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            //ThreadHelper.ThrowIfNotOnUIThread();
            // get the menu that fired the event
            if (!(sender is OleMenuCommand menuCommand)) return;


            // start by assuming that the menu will not be shown
            menuCommand.Visible = false;
            menuCommand.Enabled = false;

            var isAvailable = await _dumpAsCommandHelper.IsCommandAvailableAsync();

            menuCommand.Visible = isAvailable;
            menuCommand.Enabled = isAvailable;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DumpAsJsonCommand Instance
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
            // Switch to the main thread - the call to AddCommand in DumpAsJsonCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new DumpAsJsonCommand(package, commandService);
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

            _dumpAsCommandHelper.ExecuteCommand("json");
        }
    }
}
