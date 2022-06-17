namespace YellowFlavor.Serialization.Adapters
{
    internal static class DriveInfoMapper
    {
        public static DriveInfo Map(System.IO.DriveInfo driveInfo)
        {
            return new DriveInfo
            {
                RootDirectory = DirectoryInfoMapper.Map(driveInfo.RootDirectory),
                VolumeLabel = driveInfo.VolumeLabel,
                DriveFormat = driveInfo.DriveFormat,
                DriveType = driveInfo.DriveType,
                AvailableFreeSpace = driveInfo.AvailableFreeSpace,
                IsReady = driveInfo.IsReady,
                Name = driveInfo.Name,
                TotalFreeSpace = driveInfo.TotalFreeSpace,
                TotalSize = driveInfo.TotalSize
            };
        }
    }
}
