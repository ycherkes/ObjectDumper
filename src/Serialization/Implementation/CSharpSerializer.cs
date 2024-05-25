using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors.Specific;
using YellowFlavor.Serialization.Implementation.Dotnet;
using YellowFlavor.Serialization.Implementation.Settings;
using System;
using VarDump.Visitor.Format;

namespace YellowFlavor.Serialization.Implementation;

internal class CSharpSerializer : ISerializer
{
    private static DumpOptions CsharpDumpOptions => new()
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
        GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
        IgnoreDefaultValues = true,
        IgnoreNullValues = true,
        IgnoreReadonlyProperties = true,
        IndentString = "    ",
        IntegralNumericFormat = "D",
        MaxCollectionSize = int.MaxValue,
        MaxDepth = 25,
        PrimitiveCollectionLayout = CollectionLayout.MultiLine,
        UseNamedArgumentsInConstructors = false,
        UsePredefinedConstants = true,
        UsePredefinedMethods = true,
        UseTypeFullName = false
    };

    public void Serialize(object obj, string settings, TextWriter textWriter)
    {
        var dumpOptions = GetCsharpDumpOptions(settings);
        var dumper = new CSharpDumper(dumpOptions);
        dumper.Dump(obj, textWriter);
    }

    private static DumpOptions GetCsharpDumpOptions(string settings)
    {
        var newOptions = CsharpDumpOptions;
        if (settings == null) return newOptions;

        var deserializedSettings = JsonConvert.DeserializeObject<CSharpSettings>(settings);

        newOptions.DateKind = deserializedSettings.DateKind;
        newOptions.DateTimeInstantiation = deserializedSettings.DateTimeInstantiation;
        newOptions.GenerateVariableInitializer = deserializedSettings.GenerateVariableInitializer;
        newOptions.GetFieldsBindingFlags = deserializedSettings.GetFieldsBindingFlags;
        newOptions.GetPropertiesBindingFlags = deserializedSettings.GetPropertiesBindingFlags;
        newOptions.IgnoreDefaultValues = deserializedSettings.IgnoreDefaultValues;
        newOptions.IgnoreNullValues = deserializedSettings.IgnoreNullValues;
        newOptions.IgnoreReadonlyProperties = deserializedSettings.IgnoreReadonlyProperties;
        newOptions.IndentString = deserializedSettings.IndentString;
        newOptions.IntegralNumericFormat = deserializedSettings.IntegralNumericFormat;
        newOptions.MaxCollectionSize = deserializedSettings.MaxCollectionSize;
        newOptions.MaxDepth = deserializedSettings.MaxDepth;
        newOptions.PrimitiveCollectionLayout = deserializedSettings.PrimitiveCollectionLayout;
        newOptions.SortDirection = deserializedSettings.SortDirection;
        newOptions.UseNamedArgumentsInConstructors = deserializedSettings.UseNamedArgumentsInConstructors;
        newOptions.UsePredefinedConstants = deserializedSettings.UsePredefinedConstants;
        newOptions.UsePredefinedMethods = deserializedSettings.UsePredefinedMethods;
        newOptions.UseTypeFullName = deserializedSettings.UseFullTypeName;

        return newOptions;
    }
}