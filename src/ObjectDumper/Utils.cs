using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;


namespace ObjectDumper
{
    public static class Utils
    {
		public static IVsTextView GetTextView()
        {
            var dte2 = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE));
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte2;
            IVsTextManager textManager;
            using (ServiceProvider serviceProvider = new ServiceProvider(sp))
            {
                textManager = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
            }

            textManager.GetActiveView(1, null, out IVsTextView textViewCurrent);

            return textViewCurrent;
        }
    }
}
