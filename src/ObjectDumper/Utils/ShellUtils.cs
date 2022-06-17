using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace ObjectDumper.Utils
{
    public static class ShellUtils
    {
        public static IVsTextView GetTextView(IServiceProvider serviceProvider)
        {
            IVsTextManager textManager = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
            IVsTextView textViewCurrent = null;
            textManager?.GetActiveView(1, null, out textViewCurrent);
            return textViewCurrent;
        }
    }
}
