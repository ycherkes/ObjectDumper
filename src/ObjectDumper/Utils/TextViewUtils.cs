using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Linq;

namespace ObjectDumper.Utils
{
    internal static class TextViewUtils
    {
        public static (bool success, string text) GetSelectedText(IServiceProvider serviceProvider)
        {
            string selectionText = string.Empty;

            var textView = ShellUtils.GetTextView(serviceProvider);

            if (textView == null)
            {
                return (false, selectionText);
            }

            if (textView.GetSelection(out int piAnchorLine,
                out int piAnchorCol,
                out int piEndLine,
                out int piEndCol) != VSConstants.S_OK)
            {
                TextSpan[] textSpan = new TextSpan[1];

                if (textView.GetWordExtent(
                    piAnchorLine,
                    piAnchorCol,
                    (uint)WORDEXTFLAGS.WORDEXT_CURRENT,
                    textSpan) != VSConstants.S_OK)
                {
                    return (false, selectionText);
                }

                var ts1 = textSpan[0];

                piAnchorLine = ts1.iStartLine;
                piEndLine = ts1.iEndLine;
                piAnchorCol = ts1.iStartIndex;
                piEndCol = ts1.iEndIndex;
            }

            if (piAnchorLine != piEndLine || piAnchorCol != piEndCol)
            {
                if (textView.GetBuffer(out var buffer) != VSConstants.S_OK)
                {
                    return (false, selectionText);
                }

                var (startLine, endLine, startCol, endCol) = NormalizeSelection(piAnchorLine, piEndLine, piAnchorCol, piEndCol);

                if (buffer.GetLineText(startLine, startCol, endLine, endCol, out selectionText) != VSConstants.S_OK)
                {
                    return (false, selectionText);
                }
            }

            return (true, selectionText);
        }

        private static (int startLine, int endLine, int startCol, int endCol) NormalizeSelection(int startLine, int endLine, int startCol, int endCol)
        {
            var points = new (int x, int y)[]
            {
                (startLine, startCol),
                (endLine, endCol)
            }
            .OrderBy(p => p.x)
            .ThenBy(p => p.y)
            .ToArray();

            return (points[0].x, points[1].x, points[0].y, points[1].y);
        }
    }
}
