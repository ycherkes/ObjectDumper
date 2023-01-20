using Newtonsoft.Json;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation
{
    internal class CSharpSerializer : ISerializer
    {
        private static DumpOptions CsharpDumpOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 25,
            ExcludeTypes = new[] { "Avro.Schema" },
            UseTypeFullName = false,
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc,
            UseNamedArgumentsForReferenceRecordTypes = false,
            GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
            WritablePropertiesOnly = true
        };

        public string Serialize(object obj, string settings)
        {
            var dumpOptions = GetCsharpDumpOptions(settings);
            var dumper = new CSharpDumper();
            var result = dumper.Dump(obj, dumpOptions);
            return result;
        }

        private static DumpOptions GetCsharpDumpOptions(string settings)
        {
            var newSettings = CsharpDumpOptions;
            if (settings == null) return newSettings;

            var csharpSettings = JsonConvert.DeserializeObject<CSharpSettings>(settings);
            newSettings.IgnoreDefaultValues = csharpSettings.IgnoreDefaultValues;
            newSettings.IgnoreNullValues = csharpSettings.IgnoreNullValues;
            newSettings.UseTypeFullName = csharpSettings.UseFullTypeName;
            newSettings.MaxDepth = csharpSettings.MaxDepth;
            newSettings.DateTimeInstantiation = csharpSettings.DateTimeInstantiation;
            newSettings.DateKind = csharpSettings.DateKind;
            newSettings.UseNamedArgumentsForReferenceRecordTypes = csharpSettings.UseNamedArgumentsForReferenceRecordTypes;
            newSettings.GetPropertiesBindingFlags = csharpSettings.GetPropertiesBindingFlags;
            newSettings.WritablePropertiesOnly = csharpSettings.WritablePropertiesOnly;
            newSettings.GetFieldsBindingFlags = csharpSettings.GetFieldsBindingFlags;
            newSettings.SortDirection = csharpSettings.SortDirection;

            return newSettings;
        }
    }
}
