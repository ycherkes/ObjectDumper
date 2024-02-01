using System;
using System.Collections.Generic;
using System.IO;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace YellowFlavor.Serialization;

public static class ObjectSerializer
{
    private static readonly Dictionary<string, ISerializer> Serializers = new()
    {
        {"json", new JsonSerializer()},
        {"cs", new CSharpSerializer()},
        {"vb", new VisualBasicSerializer()},
        {"xml", new XmlSerializer()},
        {"yaml", new YamlSerializer()}
    };

    /// <summary>
    /// Just a stub - used in UWP only as a workaround - see https://github.com/ycherkes/ObjectDumper/blob/main/samples/uwp/TestUwp/App.xaml.cs#L26
    /// </summary>
    public static void WarmUp()
    {
    }
    
    public static void SerializeToFile(object obj, string settings)
    {
        var settingsArray = settings.FromBase64().Split(';');
        if (settingsArray.Length != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Settings must contain a semicolon separated array with 3 items.");
        }

        var format = settingsArray[0];
        var fileName = settingsArray[1];
        var dumpOptions = settingsArray[2];
        
        using var textWriter = CreateFile(fileName);
        SerializeInternal(obj, format, dumpOptions, textWriter);
    }

    private static void SerializeInternal(object obj, string format, string settings, TextWriter textWriter)
    {
        try
        {
            if (Serializers.TryGetValue(format, out var serializer))
            {
                serializer.Serialize(obj, settings, textWriter);
            }
            else
            {
                textWriter.Write(obj?.ToString());
            }
        }
        catch (Exception e)
        {
            textWriter.Write(e.ToString());
        }
    }

    private static TextWriter CreateFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        var file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

        return new StreamWriter(file);
    }
}