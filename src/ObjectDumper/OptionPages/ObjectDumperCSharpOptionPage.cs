using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ObjectDumper.OptionPages
{
    public class ObjectDumperCSharpOptionPage : DialogPage, ICommonSettings
    {
        [Category("C#")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool IgnoreNullValues{ get; set; } = true;

        [Category("C#")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool IgnoreDefaultValues { get; set; } = true;
        
        [Category("C#")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int MaxDepth { get; set; } = 100;

        [Category("C#")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool UseFullTypeName { get; set; } = false;

        public string ToJson()
        {
            return new CommonSettings(this).ToJson();
        }
    }

}
