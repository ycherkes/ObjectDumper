using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.KnownTypes;
using YellowFlavor.Serialization.Implementation.Dotnet;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation;

internal class VisualBasicSerializer : ISerializer
{
    private static DumpOptions VisualBasicDumpOptions => new()
    {
        DateKind = DateKind.Original,
        DateTimeInstantiation = DateTimeInstantiation.Parse,
        Descriptors =
        {
            new DelegateMiddleware(),
            new MemberInfoMiddleware(),
            new FileSystemInfoMiddleware()
        },
        ExcludeTypes = new[] { "Avro.Schema" },
        GenerateVariableInitializer = true,
        GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
        IgnoreDefaultValues = true,
        IgnoreNullValues = true,
        MaxDepth = 25,
        MaxCollectionSize = int.MaxValue,
        UseNamedArgumentsForReferenceRecordTypes = false,
        UseTypeFullName = false,
        WritablePropertiesOnly = true,
        ConfigureKnownTypes = (knownTypes, visitor, opts) =>
        {
            var serviceDescriptorKnownObject = new ServiceDescriptorKnownObject(visitor, opts);
            knownTypes.Add(new KeyValuePair<string, IKnownObjectVisitor>(serviceDescriptorKnownObject.Id, serviceDescriptorKnownObject));
        }
    };

    public void Serialize(object obj, string settings, TextWriter textWriter)
    {
        var dumpOptions = GetVbDumpOptions(settings);
        var dumper = new VisualBasicDumper(dumpOptions);
        dumper.Dump(obj, textWriter);
    }

    private static DumpOptions GetVbDumpOptions(string settings)
    {
        var newSettings = VisualBasicDumpOptions;
        if (settings == null) return newSettings;

        var vbSettings = JsonConvert.DeserializeObject<VbSettings>(settings);
        newSettings.IgnoreDefaultValues = vbSettings.IgnoreDefaultValues;
        newSettings.IgnoreNullValues = vbSettings.IgnoreNullValues;
        newSettings.UseTypeFullName = vbSettings.UseFullTypeName;
        newSettings.MaxDepth = vbSettings.MaxDepth;
        newSettings.MaxCollectionSize = vbSettings.MaxCollectionSize;
        newSettings.DateTimeInstantiation = vbSettings.DateTimeInstantiation;
        newSettings.DateKind = vbSettings.DateKind;
        newSettings.UseNamedArgumentsForReferenceRecordTypes = vbSettings.UseNamedArgumentsForReferenceRecordTypes;
        newSettings.GetPropertiesBindingFlags = vbSettings.GetPropertiesBindingFlags;
        newSettings.WritablePropertiesOnly = vbSettings.WritablePropertiesOnly;
        newSettings.GetFieldsBindingFlags = vbSettings.GetFieldsBindingFlags;
        newSettings.SortDirection = vbSettings.SortDirection;
        newSettings.GenerateVariableInitializer = vbSettings.GenerateVariableInitializer;

        return newSettings;
    }
}