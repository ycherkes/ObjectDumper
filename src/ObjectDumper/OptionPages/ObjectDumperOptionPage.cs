using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ObjectDumper.OptionPages
{
    public class ObjectDumperOptionPage : DialogPage
    {
        [Category("C#")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool CSharpIgnoreNullValues { get; set; } = true;

        [Category("C#")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool CSharpIgnoreDefaultValues { get; set; } = true;
        
        [Category("C#")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int CSharpMaxDepth { get; set; } = 100;

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
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int JsonMaxDepth { get; set; } = 100;

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

        [Category("Visual Basic")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int VisualBasicMaxDepth { get; set; } = 100;

        [Category("Visual Basic")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool VisualBasicUseFullTypeName { get; set; } = false;

        [Category("Xml")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool XmlIgnoreNullValues { get; set; } = true;

        [Category("Xml")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool XmlIgnoreDefaultValues { get; set; } = true;

        [Category("Xml")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int XmlMaxDepth { get; set; } = 100;

        [Category("Xml")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool XmlUseFullTypeName { get; set; } = false;

        [Category("Yaml")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int YamlMaxDepth { get; set; } = 100;

        public string ToJson(string format)
        {
            switch(format)
            {
                case "cs":
                    return new CommonSettings
                    {
                        IgnoreDefaultValues = CSharpIgnoreDefaultValues,
                        IgnoreNullValues = CSharpIgnoreNullValues,
                        MaxDepth = CSharpMaxDepth,
                        UseFullTypeName = CSharpUseFullTypeName
                    }.ToJson();
                case "vb":
                    return new CommonSettings
                    {
                        IgnoreDefaultValues = VisualBasicIgnoreDefaultValues,
                        IgnoreNullValues = VisualBasicIgnoreNullValues,
                        MaxDepth = VisualBasicMaxDepth,
                        UseFullTypeName = VisualBasicUseFullTypeName
                    }.ToJson();
                case "json":
                    return new CommonSettings
                    {
                        IgnoreDefaultValues = JsonIgnoreDefaultValues,
                        IgnoreNullValues = JsonIgnoreNullValues,
                        MaxDepth = JsonMaxDepth,
                        UseFullTypeName = JsonUseFullTypeName
                    }.ToJson();
                case "xml":
                    return new CommonSettings
                    {
                        IgnoreDefaultValues = XmlIgnoreDefaultValues,
                        IgnoreNullValues = XmlIgnoreNullValues,
                        MaxDepth = XmlMaxDepth,
                        UseFullTypeName = XmlUseFullTypeName
                    }.ToJson();
                case "yaml":
                    return new CommonSettings
                    {
                        MaxDepth = YamlMaxDepth
                    }.ToJson();
                default:
                    return new CommonSettings().ToJson();
            }
        }
    }

}
