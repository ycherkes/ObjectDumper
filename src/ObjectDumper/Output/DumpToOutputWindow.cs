using System;
using Microsoft.VisualStudio.Shell;
using System.IO;
using EnvDTE80;
using EnvDTE;

namespace ObjectDumper.Output;

internal sealed class DumpToOutputWindow(DTE2 dte2) : IDumpOutput
{
    private OutputWindowPane _pane;
    private readonly object _syncRoot = new();

    public void Write(string format, string expression, string content, bool isFileName)
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrEmpty(content) || !EnsurePane())
            {
                return;
            }

            var outputString = isFileName ? File.ReadAllText(content) : content;
            outputString += Environment.NewLine;

            _pane.OutputString(outputString);
        }
        finally
        {
            if (isFileName)
            {
                try
                {
                    File.Delete(content!);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    private bool EnsurePane()
    {
        if (_pane != null) 
            return true;


        lock (_syncRoot)
        {
            _pane ??= dte2.ToolWindows.OutputWindow.OutputWindowPanes.Add("Object Dumper");
        }

        return _pane != null;
    }
}