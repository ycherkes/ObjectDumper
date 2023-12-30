using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ObjectDumper.Notifications;
using DebuggerEvents = ObjectDumper.Debugger.DebuggerEvents;

namespace ObjectDumper.Recording;

internal sealed class CommandRecorder : IDisposable
{
    private readonly DTE2 _dte;
    private readonly IVsStatusbar _statusBar;

    public CommandRecorder(DTE2 dte, IVsStatusbar statusBar)
    {
        _dte = dte ?? throw new ArgumentNullException(nameof(dte));
        _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
        _debuggerEvents = new DebuggerEvents();

        ThreadHelper.ThrowIfNotOnUIThread();
        _debuggerEvents.EnterDesignMode += DebuggerEvents_OnEnterDesignMode;
    }

    private void DebuggerEvents_OnEnterDesignMode()
    {
        _command = null;
    }

    private RecordedCommand _command;
    private readonly DebuggerEvents _debuggerEvents;

    public void SetCommand(RecordedCommand recordedCommand)
    {
        _command = recordedCommand ?? throw new ArgumentNullException(nameof(recordedCommand));
    }

    public bool IsCommandAvailable()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        return _dte.Debugger is { CurrentMode: dbgDebugMode.dbgBreakMode, CurrentStackFrame: not null }
               && _command != null;

    }

    public void ExecuteRecordedCommand()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _command?.Run((currentStep, numberOfSteps,statusText) => _statusBar.ShowProgress(statusText, currentStep, numberOfSteps));
    }

    public void Dispose()
    {
        _debuggerEvents.EnterDesignMode -= DebuggerEvents_OnEnterDesignMode;
    }
}