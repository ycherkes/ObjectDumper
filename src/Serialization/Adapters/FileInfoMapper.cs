namespace YellowFlavor.Serialization.Adapters
{
    internal static class FileInfoMapper
    {
        public static FileInfo Map(System.IO.FileInfo fileInfo)
        {
            return new FileInfo
            {
                Attributes = fileInfo.Attributes,
                CreationTime = fileInfo.CreationTime,
                CreationTimeUtc = fileInfo.CreationTimeUtc,
                Directory = DirectoryInfoMapper.Map(fileInfo.Directory),
                DirectoryName = fileInfo.DirectoryName,
                Exists = fileInfo.Exists,
                Extension = fileInfo.Extension,
                FullName = fileInfo.FullName,
                IsReadOnly = fileInfo.IsReadOnly,
                LastAccessTime = fileInfo.LastAccessTime,
                LastAccessTimeUtc = fileInfo.LastAccessTimeUtc,
                LastWriteTime = fileInfo.LastWriteTime,
                LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                Length = fileInfo.Exists ? fileInfo.Length : -1,
                Name = fileInfo.Name
            };
        }
    }
}
