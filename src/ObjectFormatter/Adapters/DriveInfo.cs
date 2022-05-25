using System.IO;

namespace ObjectFormatter.Adapters;

internal class DriveInfo
{
    public DirectoryInfo RootDirectory { get; set; }
    public string VolumeLabel { get; set; }
    public string DriveFormat { get; set; }
    public DriveType DriveType { get; set; }
    public long AvailableFreeSpace { get; set; }
    public bool IsReady { get; set; }
    public string Name { get; set; }
    public long TotalFreeSpace { get; set; }
    public long TotalSize { get; set; }
}