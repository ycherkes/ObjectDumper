using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ObjectDumper.OptionPages
{
    public class ObjectDumperJsonOptionPage : DialogPage, ICommonSettings
    {
        [Category("Json")]
        [DisplayName("Ignore Null Values")]
        [Description("Ignore Null Values")]
        public bool IgnoreNullValues{ get; set; } = true;

        [Category("Json")]
        [DisplayName("Ignore Default Values")]
        [Description("Ignore Default Values")]
        public bool IgnoreDefaultValues { get; set; } = true;
        
        [Category("Json")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int MaxDepth { get; set; } = 100;

        [Category("Json")]
        [DisplayName("Use Full Type Name")]
        [Description("Use Full Type Name")]
        public bool UseFullTypeName { get; set; } = false;

        public string ToJson()
        {
            return new CommonSettings(this).ToJson();
        }
    }
}
