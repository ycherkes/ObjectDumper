using System;

namespace ObjectFormatter.Adapters
{
    internal class DirectoryInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DirectoryInfo Parent { get; set; }
        public bool Exists { get; set; }
        public DirectoryInfo Root { get; set; }
        public string Extension { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime CreationTimeUtc { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public System.IO.FileAttributes Attributes { get; set; }
    }
}
