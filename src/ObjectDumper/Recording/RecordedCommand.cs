using System;
using ObjectDumper.DebuggeeInteraction;
using ObjectDumper.Output;

namespace ObjectDumper.Recording;

internal sealed class RecordedCommand
{
    public string Format { get; set; }
    public IDumpOutput DumpOutput { get; set; }
    public string SelectionText { get; set; }
    public InteractionService InteractionService { get; set; }

    public void Run(Action<int, int, string> statusCallback = null)
    {
        statusCallback?.Invoke(1, 4, "Injecting serialization library...");

        try
        {
            var (isInjected, injectionEvaluationResult) = InteractionService.InjectSerializer();

            string data;
            var isFileName = false;

            statusCallback?.Invoke(2, 4, "Run serializer...");

            if (isInjected)
            {
                (data, isFileName) = InteractionService.Serialize(SelectionText, Format);
            }
            else
            {
                data = injectionEvaluationResult;
            }

            statusCallback?.Invoke(3, 4, "Write result to output...");

            DumpOutput.Write(Format, SelectionText, data, isFileName);

            statusCallback?.Invoke(4, 4, "Serialization finished.");
        }
        catch
        {
            statusCallback?.Invoke(4, 4, "Error occurred.");
            throw;
        }
    }
}