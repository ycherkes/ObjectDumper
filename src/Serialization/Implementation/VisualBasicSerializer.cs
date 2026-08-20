using System;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors.Specific;
using YellowFlavor.Serialization.Implementation.Dotnet;
using YellowFlavor.Serialization.Implementation.Settings;
using VarDump.Visitor.Format;

namespace YellowFlavor.Serialization.Implementation;

internal class VisualBasicSerializer : ISerializer
{
    private static DumpOptions VisualBasicDumpOptions => new()
    {
        ConfigureKnownObjects = (knownObjects, nextDepthVisitor, _, codeWriter) =>
        {
            knownObjects.Add(new ServiceDescriptorKnownObject(nextDepthVisitor, codeWriter));
        },
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
        GetBaseClassFields = false,
        GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
        IgnoreDefaultValues = true,
        IgnoreNullValues = true,
        IgnoreReadonlyProperties = true,
        IndentString = "    ",
        IntegralNumericFormat = "D",
        MaxCollectionSize = int.MaxValue,
        MaxDepth = 25,
        NewLineStyle = NewLineStyle.Auto,
        PrimitiveCollectionLayout = CollectionLayout.MultiLine,
        TypeNamePolicy = TypeNamingPolicy.ShortName,
        UseNamedArgumentsInConstructors = false,
        UsePredefinedConstants = true,
        UsePredefinedMethods = true
    };

    public void Serialize(object obj, string settings, TextWriter textWriter)
    {
        var dumpOptions = GetVbDumpOptions(settings);
        var dumper = new VisualBasicDumper(dumpOptions);
        dumper.Dump(obj, textWriter);
    }

    private static DumpOptions GetVbDumpOptions(string settings)
    {
        var newOptions = VisualBasicDumpOptions;
        if (settings == null) return newOptions;

        var deserializedSettings = JsonConvert.DeserializeObject<VbSettings>(settings);

        newOptions.DateKind = deserializedSettings.DateKind;
        newOptions.DateTimeInstantiation = deserializedSettings.DateTimeInstantiation;
        newOptions.GenerateVariableInitializer = deserializedSettings.GenerateVariableInitializer;
        newOptions.GetBaseClassFields = deserializedSettings.GetBaseClassFields;
        newOptions.GetFieldsBindingFlags = deserializedSettings.GetFieldsBindingFlags;
        newOptions.GetPropertiesBindingFlags = deserializedSettings.GetPropertiesBindingFlags;
        newOptions.IgnoreDefaultValues = deserializedSettings.IgnoreDefaultValues;
        newOptions.IgnoreNullValues = deserializedSettings.IgnoreNullValues;
        newOptions.IgnoreReadonlyProperties = deserializedSettings.IgnoreReadonlyProperties;
        newOptions.IndentString = deserializedSettings.IndentString;
        newOptions.IntegralNumericFormat = deserializedSettings.IntegralNumericFormat;
        newOptions.MaxCollectionSize = deserializedSettings.MaxCollectionSize;
        newOptions.MaxDepth = deserializedSettings.MaxDepth;
        newOptions.NewLineStyle = deserializedSettings.NewLineStyle;
        newOptions.PrimitiveCollectionLayout = deserializedSettings.PrimitiveCollectionLayout;
        newOptions.SortDirection = deserializedSettings.SortDirection;
        newOptions.UseNamedArgumentsInConstructors = deserializedSettings.UseNamedArgumentsInConstructors;
        newOptions.UsePredefinedConstants = deserializedSettings.UsePredefinedConstants;
        newOptions.UsePredefinedMethods = deserializedSettings.UsePredefinedMethods;
        newOptions.TypeNamePolicy = deserializedSettings.TypeNamePolicy;

        return newOptions;
    }
}
