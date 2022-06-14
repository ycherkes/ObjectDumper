using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using ObjectDumper.Extensions;

namespace ObjectDumper.Options
{
    public class ObjectDumperOptionPage : DialogPage
    {
        [Category(" Common Settings")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        [DefaultValue(25)]
        public int CommonMaxDepth { get; set; }

        [Category(" Common Settings")]
        [DisplayName("Operation Timeout (Seconds)")]
        [Description("Operation Timeout in Seconds. Negative value means no timeout.")]
        [DefaultValue(10)]
        public int CommonOperationTimeoutSeconds { get; set; }

        [Category("C#")]
        [DisplayName("Enabled")]
        [Description("Enabled")]
        [DefaultValue(true)]
        public bool CSharpEnabled { get; set; }

        [Category("C#")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        [DefaultValue(true)]
        public bool CSharpIgnoreNullValues { get; set; }


        [Category("C#")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        [DefaultValue(true)]
        public bool CSharpIgnoreDefaultValues { get; set; }
        
        [Category("C#")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        [DefaultValue(false)]
        public bool CSharpUseFullTypeName { get; set; }

        [Category("C#")]
        [DisplayName("Convert DateTime to UTC")]
        [Description("Convert DateTime to UTC")]
        [DefaultValue(true)]
        public bool CSharpConvertDateTimeToUtc { get; set; }

        [Category("Json")]
        [DisplayName("Enabled")]
        [Description("Enabled")]
        [DefaultValue(true)]
        public bool JsonEnabled { get; set; }

        [Category("Json")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        [DefaultValue(true)]
        public bool JsonIgnoreNullValues { get; set; }

        [Category("Json")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        [DefaultValue(true)]
        public bool JsonIgnoreDefaultValues { get; set; }

        [Category("Json")]
        [DisplayName("Naming Strategy")]
        [Description("Naming Strategy")]
        [DefaultValue(NamingStrategy.CamelCase)]
        public NamingStrategy JsonNamingStrategy { get; set; }

        [Category("Json")]
        [DisplayName("Serialize Enums As Strings")]
        [Description("Serialize Enums As Strings")]
        [DefaultValue(true)]
        public bool JsonSerializeEnumAsString { get; set; }

        [Category("Json")]
        [DisplayName("Type Name Handling")]
        [Description("Type Name Handling")]
        [DefaultValue(TypeNameHandling.None)]
        public TypeNameHandling JsonTypeNameHandling { get; set; }

        [Category("Visual Basic")]
        [DisplayName("Enabled")]
        [Description("Enabled")]
        [DefaultValue(true)]
        public bool VisualBasicEnabled { get; set; }

        [Category("Visual Basic")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        [DefaultValue(true)]
        public bool VisualBasicIgnoreNullValues { get; set; }

        [Category("Visual Basic")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        [DefaultValue(true)]
        public bool VisualBasicIgnoreDefaultValues { get; set; }

        [Category("Visual Basic")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        [DefaultValue(false)]
        public bool VisualBasicUseFullTypeName { get; set; }

        [Category("Visual Basic")]
        [DisplayName("Convert DateTime to UTC")]
        [Description("Convert DateTime to UTC")]
        [DefaultValue(true)]
        public bool VisualBasicConvertDateTimeToUtc { get; set; }

        [Category("Xml")]
        [DisplayName("Enabled")]
        [Description("Enabled")]
        [DefaultValue(true)]
        public bool XmlEnabled { get; set; }

        [Category("Xml")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        [DefaultValue(true)]
        public bool XmlIgnoreNullValues { get; set; }

        [Category("Xml")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        [DefaultValue(true)]
        public bool XmlIgnoreDefaultValues { get; set; }

        [Category("Xml")]
        [DisplayName("Naming Strategy")]
        [Description("Naming Strategy")]
        [DefaultValue(NamingStrategy.Default)]
        public NamingStrategy XmlNamingStrategy { get; set; }

        [Category("Xml")]
        [DisplayName("Serialize Enums As Strings")]
        [Description("Serialize Enums As Strings")]
        [DefaultValue(true)]
        public bool XmlSerializeEnumAsString { get; set; }

        [Category("Xml")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        [DefaultValue(false)]
        public bool XmlUseFullTypeName { get; set; }

        [Category("Yaml")]
        [DisplayName("Enabled")]
        [Description("Enabled")]
        [DefaultValue(true)]
        public bool YamlEnabled { get; set; }

        [Category("Yaml")]
        [DisplayName("Naming Convention")]
        [Description("Naming Convention")]
        [DefaultValue(NamingConvention.Null)]
        public NamingConvention YamlNamingConvention { get; set; }

        public string ToJson(string format)
        {
            switch(format)
            {
                case "cs":
                    return new
                    {
                        IgnoreDefaultValues = CSharpIgnoreDefaultValues,
                        IgnoreNullValues = CSharpIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        UseFullTypeName = CSharpUseFullTypeName,
                        ConvertDateTimeToUtc = CSharpConvertDateTimeToUtc
                    }.ToJson();
                case "vb":
                    return new
                    {
                        IgnoreDefaultValues = VisualBasicIgnoreDefaultValues,
                        IgnoreNullValues = VisualBasicIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        UseFullTypeName = VisualBasicUseFullTypeName,
                        ConvertDateTimeToUtc = VisualBasicConvertDateTimeToUtc
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
