using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace ObjectDumper.Utils
{
    public static class ShellUtils
    {
        public static IVsTextView GetTextView(IServiceProvider serviceProvider)
        {
            var textManager = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
            if (textManager?.GetActiveView(1, null, out var activeTextView) != VSConstants.S_OK)
            {
                return null;
            }
            return activeTextView;
        }
    }
}
