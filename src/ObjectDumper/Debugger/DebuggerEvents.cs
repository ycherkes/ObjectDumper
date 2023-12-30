using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;

namespace ObjectDumper.Debugger;


// src: https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/blob/master/src/toolkit/Community.VisualStudio.Toolkit.Shared/Debugger/DebuggerEvents.cs
/// <summary>
/// Events related to the debugger in Visual Studio.
/// </summary>
public class DebuggerEvents : IVsDebuggerEvents
{
    internal DebuggerEvents()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        IVsDebugger svc = GetRequiredService<IVsDebugger, IVsDebugger>();
        svc.AdviseDebuggerEvents(this, out _);
    }

    /// <summary>
    /// Gets a global service synchronously.
    /// </summary>
    /// <typeparam name="TService">The type identity of the service.</typeparam>
    /// <typeparam name="TInterface">The interface to cast the service to.</typeparam>
    /// <exception cref="Exception">Throws an exception when the service is not available.</exception>
    private static TInterface GetRequiredService<TService, TInterface>() where TService : class where TInterface : class
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return (TInterface)ServiceProvider.GlobalProvider.GetService(typeof(TService));
    }


    /// <summary>
    /// Fires when entering break mode.
    /// </summary>
    public event Action EnterBreakMode;

    /// <summary>
    /// Fired when the debugger enters run mode.
    /// </summary>
    public event Action EnterRunMode;

    /// <summary>
    /// Fired when leaving run mode or debug mode, and when the debugger establishes design mode after debugging.
    /// </summary>
    public event Action EnterDesignMode;

    /// <summary>
    /// Fires when entering Edit &amp; Continue mode.
    /// </summary>
    public event Action EnterEditAndContinueMode;

    int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
    {
        switch (dbgmodeNew)
        {
            case DBGMODE.DBGMODE_Design:
                EnterDesignMode?.Invoke();
                break;
            case DBGMODE.DBGMODE_Break:
                EnterBreakMode?.Invoke();
                break;
            case DBGMODE.DBGMODE_Run:
                EnterRunMode?.Invoke();
                break;
            case DBGMODE.DBGMODE_Enc:
                EnterEditAndContinueMode?.Invoke();
                break;
        }

        return VSConstants.S_OK;
    }
}