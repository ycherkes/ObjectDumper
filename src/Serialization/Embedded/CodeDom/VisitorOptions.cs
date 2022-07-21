using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace YellowFlavor.Serialization.Embedded.CodeDom
{
    internal class VisitorOptions
    {
        public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.New;
        public DateKind DateKind { get; set; } = DateKind.ConvertToUtc;
        public ICollection<string> ExcludeTypes { get; set; }
        public int MaxDepth { get; set; }
        public bool IgnoreNullValues { get; set; }
        public bool IgnoreDefaultValues { get; set; }
        public bool UseTypeFullName { get; set; }
        public bool UseNamedArgumentsForReferenceRecordTypes { get; set; }
        public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
        public bool WritablePropertiesOnly { get; set; } = true;
        public ListSortDirection? SortDirection { get; set; }
        public BindingFlags? GetFieldsBindingFlags { get; set; }
    }
}
