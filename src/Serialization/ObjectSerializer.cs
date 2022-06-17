using System;
using System.Collections.Generic;
using YellowFlavor.Serialization.Adapters;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation;
using DirectoryInfo = System.IO.DirectoryInfo;
using DriveInfo = System.IO.DriveInfo;
using FileInfo = System.IO.FileInfo;

namespace YellowFlavor.Serialization
{
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

        public static string Serialize(object obj, string formattingType, string settings = null)
        {
            obj = obj switch
            {
                FileInfo info => FileInfoMapper.Map(info),
                DirectoryInfo directoryInfo => DirectoryInfoMapper.Map(directoryInfo),
                DriveInfo driveInfo => DriveInfoMapper.Map(driveInfo),
                _ => obj
            };

            try
            {
                return Serializers.TryGetValue(formattingType, out var formatter)
                    ? formatter.Serialize(obj, settings.FromBase64()).ToBase64()
                    : obj?.ToString().ToBase64();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
