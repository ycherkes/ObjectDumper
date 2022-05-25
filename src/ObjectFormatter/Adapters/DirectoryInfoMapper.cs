namespace ObjectFormatter.Adapters
{
    internal class DirectoryInfoMapper
    {
        public static DirectoryInfo Map(System.IO.DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                return null;
            }

            return new DirectoryInfo
            {
                Attributes = directoryInfo.Attributes,
                CreationTime = directoryInfo.CreationTime,
                CreationTimeUtc = directoryInfo.CreationTimeUtc,
                Exists = directoryInfo.Exists,
                LastAccessTime = directoryInfo.LastAccessTime,
                LastAccessTimeUtc = directoryInfo.LastAccessTimeUtc,
                LastWriteTime = directoryInfo.LastWriteTime,
                LastWriteTimeUtc = directoryInfo.LastWriteTimeUtc,
                Name = directoryInfo.Name,
                Extension = directoryInfo.Extension,
                FullName = directoryInfo.FullName,
                Parent = Map(directoryInfo.Parent),
                Root = directoryInfo.Root.FullName == directoryInfo.FullName ? null : Map(directoryInfo.Root)
            };
        }
    }
}
