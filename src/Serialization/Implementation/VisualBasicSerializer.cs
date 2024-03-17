using System;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors.Specific;
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
            new ObjectMembersFilter
            {
                Condition = member => !string.Equals(member.Type.FullName, "Avro.Schema", StringComparison.InvariantCulture)
            },
            new DelegateMiddleware(),
            new MemberInfoMiddleware(),
            new FileSystemInfoMiddleware()
        },
        GenerateVariableInitializer = true,
        GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
        IgnoreDefaultValues = true,
        IgnoreNullValues = true,
        MaxDepth = 25,
        MaxCollectionSize = int.MaxValue,
        UseNamedArgumentsInConstructors = false,
        UseTypeFullName = false,
        WritablePropertiesOnly = true,
        ConfigureKnownObjects = (knownObjects, nextDepthVisitor, _, codeWriter) =>
        {
            knownObjects.Add(new ServiceDescriptorKnownObject(nextDepthVisitor, codeWriter));
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
        newSettings.UseNamedArgumentsInConstructors = vbSettings.UseNamedArgumentsInConstructors;
        newSettings.GetPropertiesBindingFlags = vbSettings.GetPropertiesBindingFlags;
        newSettings.WritablePropertiesOnly = vbSettings.WritablePropertiesOnly;
        newSettings.GetFieldsBindingFlags = vbSettings.GetFieldsBindingFlags;
        newSettings.SortDirection = vbSettings.SortDirection;
        newSettings.GenerateVariableInitializer = vbSettings.GenerateVariableInitializer;

        return newSettings;
    }
}