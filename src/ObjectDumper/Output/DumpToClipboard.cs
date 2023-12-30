using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Windows.Forms;

namespace ObjectDumper.Output;

internal sealed class DumpToClipboard : IDumpOutput
{
    public void Write(string format, string expression, string content, bool isFileName)
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!string.IsNullOrEmpty(content))
            {
                Clipboard.SetText(isFileName ? File.ReadAllText(content) : content);
            }
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
}