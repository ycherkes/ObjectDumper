using System;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using ObjectDumper.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EnvDTE80;
using ObjectDumper.Output;

namespace ObjectDumper.Options;

public class ObjectDumperOptionPage : DialogPage
{
    [Category(" Common Settings")]
    [DisplayName("Max Depth")]
    [Description("Max Depth")]
    [DefaultValue(25)]
    public int CommonMaxDepth { get; set; } = 25;

    [Category(" Common Settings")]
    [DisplayName("Operation Timeout (Seconds)")]
    [Description("Operation Timeout in Seconds. Negative value means no timeout.")]
    [DefaultValue(10)]
    public int CommonOperationTimeoutSeconds { get; set; } = 10;

    [Category(" Common Settings")]
    [DisplayName("Dump To")]
    [Description("Outputs the dump result to the selected destination.")]
    [DefaultValue(DumpDestination.NewDocumentTab)]
    public DumpDestination DumpTo { get; set; } = DumpDestination.NewDocumentTab;

    [Category("C#")]
    [DisplayName("Enabled")]
    [Description("Enabled")]
    [DefaultValue(true)]
    public bool CSharpEnabled { get; set; } = true;

    [Category("C#")]
    [DisplayName("Ignore Null Values")]
    [Description("Ignore Null Values")]
    [DefaultValue(true)]
    public bool CSharpIgnoreNullValues { get; set; } = true;


    [Category("C#")]
    [DisplayName("Ignore Default Values")]
    [Description("Ignore Default Values")]
    [DefaultValue(true)]
    public bool CSharpIgnoreDefaultValues { get; set; } = true;

    [Category("C#")]
    [DisplayName("Use Full Type Name")]
    [Description("Use Full Type Name")]
    [DefaultValue(false)]
    public bool CSharpUseFullTypeName { get; set; }

    [Category("C#")]
    [DisplayName("DateTime Instantiation")]
    [Description("Configures how to DateTime, DateTimeOffset, TimeSpan will be instantiated")]
    [DefaultValue(DateTimeInstantiation.Parse)]
    public DateTimeInstantiation CSharpDateTimeInstantiation { get; set; } = DateTimeInstantiation.Parse;

    [Category("C#")]
    [DisplayName("DateTime Kind")]
    [Description("DateTime Kind")]
    [DefaultValue(DateKind.Original)]
    public DateKind CSharpDateKind { get; set; } = DateKind.Original;

    [Category("C#")]
    [DisplayName("Max Collection Size")]
    [Description("Max Collection Size [1, int.MaxValue]")]
    [DefaultValue(int.MaxValue)]
    [Range(1, int.MaxValue)]
    public int CSharpMaxCollectionSize { get; set; } = int.MaxValue;

    [Category("C#")]
    [DisplayName("Use Named Arguments In Constructors")]
    [Description("Use Named Arguments In Constructors")]
    [DefaultValue(false)]
    public bool CSharpUseNamedArgumentsInConstructors { get; set; } = false;

    [Category("C#")]
    [DisplayName("Get Properties Binding Flags")]
    [Description("Get Properties Binding Flags")]
    [DefaultValue(BindingFlags.Public | BindingFlags.Instance)]
    public BindingFlags CSharpGetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;

    [Category("C#")]
    [DisplayName("Writable Properties Only")]
    [Description("Writable Properties Only")]
    [DefaultValue(true)]
    public bool CSharpWritablePropertiesOnly { get; set; } = true;

    [Category("C#")]
    [DisplayName("Get Fields Binding Flags")]
    [Description("Get Fields Binding Flags")]
    [DefaultValue(null)]
    public BindingFlags? CSharpGetFieldsBindingFlags { get; set; }

    [Category("C#")]
    [DisplayName("Sort Direction")]
    [Description("Sort Properties and Fields by Name")]
    [DefaultValue(null)]
    public ListSortDirection? CSharpSortDirection { get; set; }

    [Category("C#")]
    [DisplayName("Generate Variable Initializer")]
    [Description("Generate Variable Initializer")]
    [DefaultValue(true)]
    public bool CSharpGenerateVariableInitializer { get; set; } = true;

    [Category("C#")]
    [DisplayName("Primitive collection layout")]
    [Description("Primitive collection layout")]
    [DefaultValue(CollectionLayout.MultiLine)]
    public CollectionLayout CSharpPrimitiveCollectionLayout { get; set; } = CollectionLayout.MultiLine;

    [Category("C#")]
    [DisplayName("Integral numeric format")]
    [Description("Available formats: [dDbBxX]\\d+_\\d+ example: 0 formatted as \"X8_4\" will output 0X0000_0000")]
    [DefaultValue("D")]
    public string CSharpIntegralNumericFormat { get; set; } = "D";

    [Category("Json")]
    [DisplayName("Enabled")]
    [Description("Enabled")]
    [DefaultValue(true)]
    public bool JsonEnabled { get; set; } = true;

    [Category("Json")]
    [DisplayName("Ignore Null Values")]
    [Description("Ignore Null Values")]
    [DefaultValue(true)]
    public bool JsonIgnoreNullValues { get; set; } = true;

    [Category("Json")]
    [DisplayName("Ignore Default Values")]
    [Description("Ignore Default Values")]
    [DefaultValue(true)]
    public bool JsonIgnoreDefaultValues { get; set; } = true;

    [Category("Json")]
    [DisplayName("Naming Strategy")]
    [Description("Naming Strategy")]
    [DefaultValue(NamingStrategy.CamelCase)]
    public NamingStrategy JsonNamingStrategy { get; set; } = NamingStrategy.CamelCase;

    [Category("Json")]
    [DisplayName("Serialize Enums As Strings")]
    [Description("Serialize Enums As Strings")]
    [DefaultValue(true)]
    public bool JsonSerializeEnumAsString { get; set; } = true;

    [Category("Json")]
    [DisplayName("Type Name Handling")]
    [Description("Type Name Handling")]
    [DefaultValue(TypeNameHandling.None)]
    public TypeNameHandling JsonTypeNameHandling { get; set; } = TypeNameHandling.None;

    [Category("Json")]
    [DisplayName("DateTime Zone Handling")]
    [Description("DateTime Zone Handling")]
    [DefaultValue(DateTimeZoneHandling.RoundtripKind)]
    public DateTimeZoneHandling JsonDateTimeZoneHandling { get; set; } = DateTimeZoneHandling.RoundtripKind;

    [Category("Visual Basic")]
    [DisplayName("Enabled")]
    [Description("Enabled")]
    [DefaultValue(true)]
    public bool VisualBasicEnabled { get; set; } = true;

    [Category("Visual Basic")]
    [DisplayName("Ignore Null Values")]
    [Description("Ignore Null Values")]
    [DefaultValue(true)]
    public bool VisualBasicIgnoreNullValues { get; set; } = true;

    [Category("Visual Basic")]
    [DisplayName("Ignore Default Values")]
    [Description("Ignore Default Values")]
    [DefaultValue(true)]
    public bool VisualBasicIgnoreDefaultValues { get; set; } = true;

    [Category("Visual Basic")]
    [DisplayName("Use Full Type Name")]
    [Description("Use Full Type Name")]
    [DefaultValue(false)]
    public bool VisualBasicUseFullTypeName { get; set; }

    [Category("Visual Basic")]
    [DisplayName("DateTime Instantiation")]
    [Description("Configures how to DateTime, DateTimeOffset, TimeSpan will be instantiated")]
    [DefaultValue(DateTimeInstantiation.Parse)]
    public DateTimeInstantiation VisualBasicDateTimeInstantiation { get; set; } = DateTimeInstantiation.Parse;

    [Category("Visual Basic")]
    [DisplayName("DateTime Kind")]
    [Description("DateTime Kind")]
    [DefaultValue(DateKind.Original)]
    public DateKind VisualBasicDateKind { get; set; } = DateKind.Original;

    [Category("Visual Basic")]
    [DisplayName("Max Collection Size")]
    [Description("Max Collection Size [1, int.MaxValue]")]
    [DefaultValue(int.MaxValue)]
    [Range(1, int.MaxValue)]
    public int VisualBasicMaxCollectionSize { get; set; } = int.MaxValue;

    [Category("Visual Basic")]
    [DisplayName("Use Named Arguments In Constructors")]
    [Description("Use Named Arguments In Constructors")]
    [DefaultValue(false)]
    public bool VisualBasicUseNamedArgumentsInConstructors { get; set; } = false;

    [Category("Visual Basic")]
    [DisplayName("Get Properties Binding Flags")]
    [Description("Get Properties Binding Flags")]
    [DefaultValue(BindingFlags.Public | BindingFlags.Instance)]
    public BindingFlags VisualBasicGetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;

    [Category("Visual Basic")]
    [DisplayName("Writable Properties Only")]
    [Description("Writable Properties Only")]
    [DefaultValue(true)]
    public bool VisualBasicWritablePropertiesOnly { get; set; } = true;

    [Category("Visual Basic")]
    [DisplayName("Get Fields Binding Flags")]
    [Description("Get Fields Binding Flags")]
    [DefaultValue(null)]
    public BindingFlags? VisualBasicGetFieldsBindingFlags { get; set; }

    [Category("Visual Basic")]
    [DisplayName("Sort Direction")]
    [Description("Sort Properties and Fields by Name")]
    [DefaultValue(null)]
    public ListSortDirection? VisualBasicSortDirection { get; set; }

    [Category("Visual Basic")]
    [DisplayName("Generate Variable Initializer")]
    [Description("Generate Variable Initializer")]
    [DefaultValue(true)]
    public bool VisualBasicGenerateVariableInitializer { get; set; } = true;

    [Category("Visual Basic")]
    [DisplayName("Primitive collection layout")]
    [Description("Primitive collection layout")]
    [DefaultValue(CollectionLayout.MultiLine)]
    public CollectionLayout VisualBasicPrimitiveCollectionLayout { get; set; } = CollectionLayout.MultiLine;

    [Category("Visual Basic")]
    [DisplayName("Integral numeric format")]
    [Description("Available formats: [dDbBxX]\\d+_\\d+ example: 0 formatted as \"X8_4\" will output &H0000_0000")]
    [DefaultValue("D")]
    public string VisualBasicIntegralNumericFormat { get; set; } = "D";

    [Category("Xml")]
    [DisplayName("Enabled")]
    [Description("Enabled")]
    [DefaultValue(true)]
    public bool XmlEnabled { get; set; } = true;

    [Category("Xml")]
    [DisplayName("Ignore Null Values")]
    [Description("Ignore Null Values")]
    [DefaultValue(true)]
    public bool XmlIgnoreNullValues { get; set; } = true;

    [Category("Xml")]
    [DisplayName("Ignore Default Values")]
    [Description("Ignore Default Values")]
    [DefaultValue(true)]
    public bool XmlIgnoreDefaultValues { get; set; } = true;

    [Category("Xml")]
    [DisplayName("Naming Strategy")]
    [Description("Naming Strategy")]
    [DefaultValue(NamingStrategy.Default)]
    public NamingStrategy XmlNamingStrategy { get; set; } = NamingStrategy.Default;

    [Category("Xml")]
    [DisplayName("Serialize Enums As Strings")]
    [Description("Serialize Enums As Strings")]
    [DefaultValue(true)]
    public bool XmlSerializeEnumAsString { get; set; } = true;

    [Category("Xml")]
    [DisplayName("Use Full Type Name")]
    [Description("Use Full Type Name")]
    [DefaultValue(false)]
    public bool XmlUseFullTypeName { get; set; }

    [Category("Xml")]
    [DisplayName("DateTime Zone Handling")]
    [Description("DateTime Zone Handling")]
    [DefaultValue(DateTimeZoneHandling.RoundtripKind)]
    public DateTimeZoneHandling XmlDateTimeZoneHandling { get; set; } = DateTimeZoneHandling.RoundtripKind;

    [Category("Yaml")]
    [DisplayName("Enabled")]
    [Description("Enabled")]
    [DefaultValue(true)]
    public bool YamlEnabled { get; set; } = true;

    [Category("Yaml")]
    [DisplayName("Naming Convention")]
    [Description("Naming Convention")]
    [DefaultValue(NamingConvention.Null)]
    public NamingConvention YamlNamingConvention { get; set; } = NamingConvention.Null;

    public string ToJson(string format)
    {
        switch (format)
        {
            case "cs":
                return new
                {
                    DateKind = CSharpDateKind,
                    DateTimeInstantiation = CSharpDateTimeInstantiation,
                    GenerateVariableInitializer = CSharpGenerateVariableInitializer,
                    GetFieldsBindingFlags = CSharpGetFieldsBindingFlags,
                    GetPropertiesBindingFlags = CSharpGetPropertiesBindingFlags,
                    IgnoreDefaultValues = CSharpIgnoreDefaultValues,
                    IgnoreNullValues = CSharpIgnoreNullValues,
                    MaxCollectionSize = CSharpMaxCollectionSize,
                    MaxDepth = CommonMaxDepth,
                    SortDirection = CSharpSortDirection,
                    UseFullTypeName = CSharpUseFullTypeName,
                    UseNamedArgumentsInConstructors = CSharpUseNamedArgumentsInConstructors,
                    WritablePropertiesOnly = CSharpWritablePropertiesOnly,
                    PrimitiveCollectionLayout = CSharpPrimitiveCollectionLayout,
                    IntegralNumericFormat = CSharpIntegralNumericFormat
                }.ToJson();
            case "vb":
                return new
                {
                    DateKind = VisualBasicDateKind,
                    DateTimeInstantiation = VisualBasicDateTimeInstantiation,
                    GenerateVariableInitializer = VisualBasicGenerateVariableInitializer,
                    GetFieldsBindingFlags = VisualBasicGetFieldsBindingFlags,
                    GetPropertiesBindingFlags = VisualBasicGetPropertiesBindingFlags,
                    IgnoreDefaultValues = VisualBasicIgnoreDefaultValues,
                    IgnoreNullValues = VisualBasicIgnoreNullValues,
                    MaxCollectionSize = VisualBasicMaxCollectionSize,
                    MaxDepth = CommonMaxDepth,
                    SortDirection = VisualBasicSortDirection,
                    UseFullTypeName = VisualBasicUseFullTypeName,
                    UseNamedArgumentsInConstructors = VisualBasicUseNamedArgumentsInConstructors,
                    WritablePropertiesOnly = VisualBasicWritablePropertiesOnly,
                    PrimitiveCollectionLayout = VisualBasicPrimitiveCollectionLayout,
                    IntegralNumericFormat = VisualBasicIntegralNumericFormat
                }.ToJson();
            case "json":
                return new
                {
                    DateTimeZoneHandling = JsonDateTimeZoneHandling,
                    IgnoreDefaultValues = JsonIgnoreDefaultValues,
                    IgnoreNullValues = JsonIgnoreNullValues,
                    MaxDepth = CommonMaxDepth,
                    NamingStrategy = JsonNamingStrategy,
                    SerializeEnumAsString = JsonSerializeEnumAsString,
                    TypeNameHandling = JsonTypeNameHandling
                }.ToJson();
            case "xml":
                return new
                {
                    DateTimeZoneHandling = XmlDateTimeZoneHandling,
                    IgnoreDefaultValues = XmlIgnoreDefaultValues,
                    IgnoreNullValues = XmlIgnoreNullValues,
                    MaxDepth = CommonMaxDepth,
                    NamingStrategy = XmlNamingStrategy,
                    SerializeEnumAsString = XmlSerializeEnumAsString,
                    UseFullTypeName = XmlUseFullTypeName,
                }.ToJson();
            case "yaml":
                return new
                {
                    MaxDepth = CommonMaxDepth,
                    NamingConvention = YamlNamingConvention
                }.ToJson();
            default:
                return new object().ToJson();
        }
    }

    protected override void OnApply(PageApplyEventArgs e)
    {
        if (CSharpMaxCollectionSize < 1 || VisualBasicMaxCollectionSize < 1)
        {
            e.ApplyBehavior = ApplyKind.CancelNoNavigate;
            LoadSettingsFromStorage();
        }

        base.OnApply(e);
    }
    public IDumpOutput GetDumpOutput(DTE2 dte)
    {
        return DumpTo switch
        {
            DumpDestination.NewDocumentTab => new DumpToNewDocumentTab(dte),
            DumpDestination.Clipboard => new DumpToClipboard(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}