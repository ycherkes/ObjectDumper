using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Linq;
using EnvDTE80;

namespace ObjectDumper.Output;

internal sealed class DumpToNewDocumentTab(DTE2 dte) : IDumpOutput
{
    public void Write(string format, string expression, string content, bool isFileName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var fileExtension = $".{format}";

        var fileName = SanitizeFileName(expression.Any(char.IsWhiteSpace) ? "expression" : expression);

        CreateNewFile($"{fileName}{fileExtension}", content, isFileName);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalids = Path.GetInvalidFileNameChars();
        return invalids.Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    }

    private void CreateNewFile(string fileName, string data, bool isFileName)
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var newFileWindow = dte.ItemOperations.NewFile(Name: fileName);
            var newDocument = newFileWindow.Document;

            if (!string.IsNullOrEmpty(data))
            {
                var selection = (TextSelection)newDocument.Selection;
                if (isFileName)
                {
                    selection.InsertFromFile(data);
                }
                else
                {
                    selection?.Insert(data);
                }

                selection?.StartOfDocument();
            }

            newDocument.Save();
        }
        finally
        {
            if (isFileName)
            {
                try
                {
                    File.Delete(data!);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}