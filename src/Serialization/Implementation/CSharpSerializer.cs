﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.KnownTypes;
using YellowFlavor.Serialization.Implementation.Dotnet;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation;

internal class CSharpSerializer : ISerializer
{
    private static DumpOptions CsharpDumpOptions => new()
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
        UseNamedArgumentsInConstructors = false,
        UseTypeFullName = false,
        WritablePropertiesOnly = true,
        ConfigureKnownTypes = (knownTypes, visitor, opts, codeGenerator) =>
        {
            var serviceDescriptorKnownObject = new ServiceDescriptorKnownObject(visitor, codeGenerator);
            knownTypes.Add(new KeyValuePair<string, IKnownObjectVisitor>(serviceDescriptorKnownObject.Id, serviceDescriptorKnownObject));
        }
    };

    public void Serialize(object obj, string settings, TextWriter textWriter)
    {
        var dumpOptions = GetCsharpDumpOptions(settings);
        var dumper = new CSharpDumper(dumpOptions);
        dumper.Dump(obj, textWriter);
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
        newSettings.MaxCollectionSize = csharpSettings.MaxCollectionSize;
        newSettings.DateTimeInstantiation = csharpSettings.DateTimeInstantiation;
        newSettings.DateKind = csharpSettings.DateKind;
        newSettings.UseNamedArgumentsInConstructors = csharpSettings.UseNamedArgumentsInConstructors;
        newSettings.GetPropertiesBindingFlags = csharpSettings.GetPropertiesBindingFlags;
        newSettings.WritablePropertiesOnly = csharpSettings.WritablePropertiesOnly;
        newSettings.GetFieldsBindingFlags = csharpSettings.GetFieldsBindingFlags;
        newSettings.SortDirection = csharpSettings.SortDirection;
        newSettings.GenerateVariableInitializer = csharpSettings.GenerateVariableInitializer;

        return newSettings;
    }
}