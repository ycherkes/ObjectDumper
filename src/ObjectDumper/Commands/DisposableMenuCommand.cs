using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace ObjectDumper.Commands;

internal sealed class DisposableMenuCommand : OleMenuCommand, IDisposable
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