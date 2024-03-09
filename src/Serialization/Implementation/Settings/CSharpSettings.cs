using System.ComponentModel;
using System.Reflection;
using VarDump.Visitor;

namespace YellowFlavor.Serialization.Implementation.Settings;

internal class CSharpSettings
{
    public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.New;
    public DateKind DateKind { get; set; } = DateKind.ConvertToUtc;
    public bool IgnoreNullValues { get; set; } = true;
    public bool IgnoreDefaultValues { get; set; } = true;
    public int MaxDepth { get; set; } = 25;
    public int MaxCollectionSize { get; set; } = int.MaxValue;
    public bool UseFullTypeName { get; set; } = false;
    public bool UseNamedArgumentsInConstructors { get; set; } = false;
    public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public bool WritablePropertiesOnly { get; set; } = true;
    public ListSortDirection? SortDirection { get; set; }
    public BindingFlags? GetFieldsBindingFlags { get; set; }
    public bool GenerateVariableInitializer { get; set; } = true;
}