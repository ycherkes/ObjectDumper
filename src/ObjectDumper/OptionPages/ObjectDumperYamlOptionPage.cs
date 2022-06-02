using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace ObjectDumper.OptionPages
{
    public class ObjectDumperYamlOptionPage : DialogPage, IBaseSettings
    {
        [Category("Yaml")]
        [DisplayName("Max Depth")]
        [Description("Max Depth")]
        public int MaxDepth { get; set; } = 100;

        public string ToJson()
        {
            return new BaseSettings(this).ToJson();
        }
    }
}
