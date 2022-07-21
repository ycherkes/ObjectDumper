using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using ObjectDumper.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace ObjectDumper.Options
{
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
        [DefaultValue(DateTimeInstantiation.New)]
        public DateTimeInstantiation CSharpDateTimeInstantiation { get; set; } = DateTimeInstantiation.New;

        [Category("C#")]
        [DisplayName("DateTime Kind")]
        [Description("DateTime Kind")]
        [DefaultValue(DateKind.ConvertToUtc)]
        public DateKind CSharpDateKind { get; set; } = DateKind.ConvertToUtc;

        [Category("C#")]
        [DisplayName("Use Named Arguments For Reference Record Types")]
        [Description("Use Named Arguments For Reference Record Types")]
        [DefaultValue(false)]
        public bool CSharpUseNamedArgumentsForReferenceRecordTypes { get; set; } = false;

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
        [Description("Sort Properties, Fields and Enum values by Name")]
        [DefaultValue(null)]
        public ListSortDirection? CSharpSortDirection { get; set; }

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
        [DefaultValue(DateTimeInstantiation.New)]
        public DateTimeInstantiation VisualBasicDateTimeInstantiation { get; set; } = DateTimeInstantiation.New;

        [Category("Visual Basic")]
        [DisplayName("DateTime Kind")]
        [Description("DateTime Kind")]
        [DefaultValue(DateKind.ConvertToUtc)]
        public DateKind VisualBasicDateKind { get; set; } = DateKind.ConvertToUtc;

        [Category("Visual Basic")]
        [DisplayName("Use Named Arguments For Reference Record Types")]
        [Description("Use Named Arguments For Reference Record Types")]
        [DefaultValue(false)]
        public bool VisualBasicUseNamedArgumentsForReferenceRecordTypes { get; set; } = false;

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
        [Description("Sort Properties, Fields and Enum values by Name")]
        [DefaultValue(null)]
        public ListSortDirection? VisualBasicSortDirection { get; set; }

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
                        IgnoreDefaultValues = CSharpIgnoreDefaultValues,
                        IgnoreNullValues = CSharpIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        UseFullTypeName = CSharpUseFullTypeName,
                        DateTimeInstantiation = CSharpDateTimeInstantiation,
                        DateKind = CSharpDateKind,
                        UseNamedArgumentsForReferenceRecordTypes = CSharpUseNamedArgumentsForReferenceRecordTypes,
                        GetPropertiesBindingFlags = CSharpGetPropertiesBindingFlags,
                        WritablePropertiesOnly = CSharpWritablePropertiesOnly,
                        GetFieldsBindingFlags = CSharpGetFieldsBindingFlags,
                        SortDirection = CSharpSortDirection
                    }.ToJson();
                case "vb":
                    return new
                    {
                        IgnoreDefaultValues = VisualBasicIgnoreDefaultValues,
                        IgnoreNullValues = VisualBasicIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        UseFullTypeName = VisualBasicUseFullTypeName,
                        DateTimeInstantiation = VisualBasicDateTimeInstantiation,
                        DateKind = VisualBasicDateKind,
                        UseNamedArgumentsForReferenceRecordTypes = VisualBasicUseNamedArgumentsForReferenceRecordTypes,
                        GetPropertiesBindingFlags = VisualBasicGetPropertiesBindingFlags,
                        WritablePropertiesOnly = VisualBasicWritablePropertiesOnly,
                        GetFieldsBindingFlags = VisualBasicGetFieldsBindingFlags,
                        SortDirection = VisualBasicSortDirection
                    }.ToJson();
                case "json":
                    return new
                    {
                        IgnoreDefaultValues = JsonIgnoreDefaultValues,
                        IgnoreNullValues = JsonIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        TypeNameHandling = JsonTypeNameHandling,
                        NamingStrategy = JsonNamingStrategy,
                        SerializeEnumAsString = JsonSerializeEnumAsString
                    }.ToJson();
                case "xml":
                    return new
                    {
                        IgnoreDefaultValues = XmlIgnoreDefaultValues,
                        IgnoreNullValues = XmlIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        UseFullTypeName = XmlUseFullTypeName,
                        NamingStrategy = XmlNamingStrategy,
                        SerializeEnumAsString = XmlSerializeEnumAsString
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
    }

}
