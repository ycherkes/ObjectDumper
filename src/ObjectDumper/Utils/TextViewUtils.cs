using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace ObjectDumper.Utils;

internal static class TextViewUtils
{
    public static (bool success, string text) GetSelectedText(IServiceProvider serviceProvider)
    {
        var selectionText = string.Empty;

        var textView = ShellUtils.GetTextView(serviceProvider);

        if (textView == null)
        {
            return (false, selectionText);
        }

        if (textView.GetSelection(out var piAnchorLine,
                out var piAnchorCol,
                out var piEndLine,
                out var piEndCol) != VSConstants.S_OK)
        {
            var textSpans = new TextSpan[1];

            if (textView.GetWordExtent(
                    piAnchorLine,
                    piAnchorCol,
                    (uint)WORDEXTFLAGS.WORDEXT_CURRENT,
                    textSpans) != VSConstants.S_OK)
            {
                return (false, selectionText);
            }

            var textSpan = textSpans[0];

            piAnchorLine = textSpan.iStartLine;
            piEndLine = textSpan.iEndLine;
            piAnchorCol = textSpan.iStartIndex;
            piEndCol = textSpan.iEndIndex;
        }

        if (piAnchorLine == piEndLine && piAnchorCol == piEndCol)
            return (true, selectionText);

        if (textView.GetBuffer(out var buffer) != VSConstants.S_OK)
        {
            return (false, selectionText);
        }

        var (startLine, endLine, startCol, endCol) = NormalizeSelection(piAnchorLine, piEndLine, piAnchorCol, piEndCol);

        var success = buffer.GetLineText(startLine, startCol, endLine, endCol, out selectionText) == VSConstants.S_OK;

        return (success, selectionText);
    }

    private static (int startLine, int endLine, int startCol, int endCol) NormalizeSelection(int startLine, int endLine, int startCol, int endCol)
    {
        if (startLine > endLine || (startLine == endLine && startCol > endCol))
        {
            return (endLine, startLine, endCol, startCol);
        }

        return (startLine, endLine, startCol, endCol);
    }
}