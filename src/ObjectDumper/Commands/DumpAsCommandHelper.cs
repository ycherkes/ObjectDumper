using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.DebuggeeInteraction;
using ObjectDumper.Options;
using ObjectDumper.Output;
using ObjectDumper.Utils;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using ObjectDumper.Recording;
using Constants = EnvDTE.Constants;
using ObjectDumper.Notifications;

namespace ObjectDumper.Commands;

internal class DumpAsCommandHelper(
    DTE2 dte,
    IServiceProvider serviceProvider,
    ObjectDumperOptionPage optionPage,
    CommandRecorder commandRecorder,
    IVsStatusbar statusBar)
{
    private const string ImmediateWindowObjectKind = "{ECB7191A-597B-41F5-9843-03A4CF275DDE}";
    private readonly DTE2 _dte = dte ?? throw new ArgumentNullException(nameof(dte));
    private readonly InteractionService _interactionService = new(dte.Debugger, optionPage);
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IVsStatusbar _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));

    public bool IsCommandAvailable()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        return _dte.Debugger is { CurrentMode: dbgDebugMode.dbgBreakMode, CurrentStackFrame: not null }
               && _dte.ActiveWindow.ObjectKind is Constants.vsDocumentKindText or ImmediateWindowObjectKind;

    }

    public void ExecuteCommand(string format, IDumpOutput dumpOutput)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var (success, selectionText) = TextViewUtils.GetSelectedText(_serviceProvider);

        if (!success)
        {
            return;
        }

        var recordedCommand = new RecordedCommand
        {
            Format = format,
            SelectionText = selectionText,
            DumpOutput = dumpOutput,
            InteractionService = _interactionService
        };

        commandRecorder.SetCommand(recordedCommand);

        recordedCommand.Run((currentStep, numberOfSteps, statusText) => _statusBar.ShowProgress(statusText, currentStep, numberOfSteps));
    }
}