using System;
using System.Collections.Generic;
using ObjectFormatter.Adapters;
using ObjectFormatter.Implementation;
using DirectoryInfo = System.IO.DirectoryInfo;
using DriveInfo = System.IO.DriveInfo;
using FileInfo = System.IO.FileInfo;

namespace ObjectFormatter
{
    public static class Formatter
    {
        private static readonly Dictionary<string, IFormatter> Formatters = new()
        {
            {"json", new JsonFormatter()},
            {"cs", new CSharpFormatter()},
            {"vb", new VisualBasicFormatter()},
            {"xml", new XmlFormatter()},
            {"yaml", new YamlFormatter()}
        };

        public static string Format(object obj, string formattingType, string settings = null)
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
                return Formatters.TryGetValue(formattingType, out var formatter) 
                    ? formatter.Format(obj, settings.FromBase64()).ToBase64() 
                    : obj?.ToString().ToBase64();
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
