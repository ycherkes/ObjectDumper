using System.ComponentModel;
using System.Reflection;
using VarDump.Visitor;
using VarDump.Visitor.Format;

namespace YellowFlavor.Serialization.Implementation.Settings;

internal class VbSettings
{
    public bool GenerateVariableInitializer { get; set; } = true;
    public BindingFlags? GetFieldsBindingFlags { get; set; }
    public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public bool IgnoreDefaultValues { get; set; } = true;
    public bool IgnoreNullValues { get; set; } = true;
    public bool IgnoreReadonlyProperties { get; set; } = true;
    public string IndentString { get; set; } = "    ";
    public string IntegralNumericFormat { get; set; } = "D";
    public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.New;
    public DateKind DateKind { get; set; } = DateKind.ConvertToUtc;
    public int MaxCollectionSize { get; set; } = int.MaxValue;
    public int MaxDepth { get; set; } = 25;
    public CollectionLayout PrimitiveCollectionLayout { get; set; } = CollectionLayout.MultiLine;
    public ListSortDirection? SortDirection { get; set; }
    public bool UseFullTypeName { get; set; } = false;
    public bool UseNamedArgumentsInConstructors { get; set; } = false;
    public bool UsePredefinedConstants { get; set; } = true;
    public bool UsePredefinedMethods { get; set; } = true;
}