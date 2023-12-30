using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System;

namespace ObjectDumper.Notifications
{
    internal static class StatusBarHelper
    {
        /// <summary>
        /// Shows the progress indicator in the status bar. 
        /// Set <paramref name="currentStep"/> and <paramref name="numberOfSteps"/> 
        /// to the same value to stop the progress.
        /// </summary>
        /// <param name="statusBar"></param>
        /// <param name="text">The text to display in the status bar.</param>
        /// <param name="currentStep">The current step number starting at 1.</param>
        /// <param name="numberOfSteps">The total number of steps to completion.</param>
        public static void ShowProgress(this IVsStatusbar statusBar, string text, int currentStep, int numberOfSteps)
        {
            if (currentStep == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentStep), "currentStep must have a value of 1 or higher.");
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            statusBar.FreezeOutput(0);
            uint cookie = 0;

            // Start by resetting the status bar.
            if (currentStep == 1)
            {
                statusBar.Progress(ref cookie, 1, "", 0, 0);
            }

            // Then report progress.
            if (currentStep < numberOfSteps)
            {
                statusBar.Progress(ref cookie, 1, text, (uint)currentStep, (uint)numberOfSteps);
            }

            // And clear the status bar when done.
            else
            {
                statusBar.Progress(ref cookie, 1, "", 0, 0);
            }
        }

        
    }
}
