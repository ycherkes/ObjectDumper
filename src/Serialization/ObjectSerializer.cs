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

    public static string SerializeToTempFile(object obj, string format, string settings = null)
    {
        var tempFilePath = GenerateTempFilePath();
        using var textWriter = CreateFile(tempFilePath);
        SerializeInternal(obj, format, settings, textWriter);
        return tempFilePath;
    }

    public static void SerializeToFile(object obj, string format, string filePath, string settings = null)
    {
        using var textWriter = CreateFile(filePath);
        SerializeInternal(obj, format, settings, textWriter);
    }

    private static void SerializeInternal(object obj, string format, string settings, TextWriter textWriter)
    {
        try
        {
            if (Serializers.TryGetValue(format, out var serializer))
            {
                serializer.Serialize(obj, settings.FromBase64(), textWriter);
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

        var file = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);

        return new StreamWriter(file);
    }

    private static string GenerateTempFilePath()
    {
        var fileName = Path.ChangeExtension(Guid.NewGuid().ToString("N"), "txt");
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        return filePath;
    }
}