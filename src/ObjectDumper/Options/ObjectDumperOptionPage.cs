using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.Extensions;

namespace ObjectDumper.Options
{
    public class ObjectDumperOptionPage : DialogPage
    {
        [Category(" Common Settings")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int CommonMaxDepth { get; set; } = 100;

        [Category("C#")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool CSharpIgnoreNullValues { get; set; } = true;

        [Category("C#")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool CSharpIgnoreDefaultValues { get; set; } = true;
        
        [Category("C#")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool CSharpUseFullTypeName { get; set; } = false;

        [Category("Json")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool JsonIgnoreNullValues { get; set; } = true;

        [Category("Json")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool JsonIgnoreDefaultValues { get; set; } = true;

        [Category("Json")]
        [DisplayName("Naming Strategy")]
        [Description("Naming Strategy")]
        public NamingStrategy JsonNamingStrategy { get; set; } = NamingStrategy.CamelCase;

        [Category("Json")]
        [DisplayName("Serialize Enums As Strings")]
        [Description("Serialize Enums As Strings")]
        public bool JsonSerializeEnumAsString { get; set; } = true;

        [Category("Json")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool JsonUseFullTypeName { get; set; } = false;

        [Category("Visual Basic")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool VisualBasicIgnoreNullValues { get; set; } = true;

        [Category("Visual Basic")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool VisualBasicIgnoreDefaultValues { get; set; } = true;

        //[Category("Visual Basic")]
        //[DisplayName("Use Full Type Name")]
        //[Description("Use Full Type Name")]
        //public bool VisualBasicUseFullTypeName { get; set; } = false;

        [Category("Xml")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool XmlIgnoreNullValues { get; set; } = true;

        [Category("Xml")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool XmlIgnoreDefaultValues { get; set; } = true;

        [Category("Xml")]
        [DisplayName("Naming Strategy")]
        [Description("Naming Strategy")]
        public NamingStrategy XmlNamingStrategy { get; set; } = NamingStrategy.Default;

        [Category("Xml")]
        [DisplayName("Serialize Enums As Strings")]
        [Description("Serialize Enums As Strings")]
        public bool XmlSerializeEnumAsString { get; set; } = true;

        [Category("Xml")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool XmlUseFullTypeName { get; set; } = false;

        [Category("Yaml")]
        [DisplayName("Naming Convention")]
        [Description("Naming Convention")]
        public NamingConvention YamlNamingConvention { get; set; } = NamingConvention.Null;

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
                        UseFullTypeName = CSharpUseFullTypeName
                    }.ToJson();
                case "vb":
                    return new
                    {
                        IgnoreDefaultValues = VisualBasicIgnoreDefaultValues,
                        IgnoreNullValues = VisualBasicIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        //UseFullTypeName = VisualBasicUseFullTypeName
                    }.ToJson();
                case "json":
                    return new
                    {
                        IgnoreDefaultValues = JsonIgnoreDefaultValues,
                        IgnoreNullValues = JsonIgnoreNullValues,
                        MaxDepth = CommonMaxDepth,
                        UseFullTypeName = JsonUseFullTypeName,
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
