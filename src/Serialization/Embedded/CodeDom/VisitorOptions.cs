using System.Collections.Generic;

namespace YellowFlavor.Serialization.Embedded.CodeDom
{
    internal class VisitorOptions
    {
        public bool ConvertDateTimeToUtc { get; set; } = true;
        public ICollection<string> ExcludeTypes { get; set; }
        public int MaxDepth { get; set; }
        public bool IgnoreNullValues { get; set; }
        public bool IgnoreDefaultValues { get; set; }
        public bool UseTypeFullName { get; set; }
    }
}
