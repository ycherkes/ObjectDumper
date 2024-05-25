# Change Log

All notable changes to the "ObjectDumper" extension will be documented in this file.

Check [Keep a Changelog](http://keepachangelog.com/) for recommendations on how to structure this file.

## [0.0.5]

### C# & Visual Basic

### Improved Array serialization:
- Multi dimensional arrays
- Arrays of arrays

### Improved variable name generation

## [0.0.6]
- Fixed VB & C# variable naming when the generated name is the same as type name.
- Excludes indexers from being exported.

## [0.0.7]
- Removed all Json.Net and YamlDotNet embedded code. Replaced with ILRepack msbuild task.
- Fix Xml System.Type serialization. 
- Fix Xml naming issues.
- .Net6+ assemblies use .Net6 version of serialization library. It adds for example DateOnly & TimeOnly structs support.

## [0.0.8]
- Fix NamingStrategy Json Serialization setting behavior. 

## [0.0.9]
- Extract CSharpDumper and VisualBasicDumper to the external package https://www.nuget.org/packages/VarDump.

## [0.0.10]
- Update YamlDotNet to v 13.0.0.
- Rename internalized classes to avoid conflicts with user's code.

## [0.0.11]
- Update YamlDotNet to v 13.0.1.
- Add JsonDateTimeZoneHandling & XmlDateTimeZoneHandling options.

## [0.0.12]
- Fix the Grouping collection serialization to C# & VB (Update VarDump NuGet).

## [0.0.13]
- Update YamlDotNet to v 13.0.2.
- Update Newtonsoft.Json to v 13.0.3.
- Fix F# anonymous type detection (update VarDump NuGet to v 0.0.8)

## [0.0.14]
- Update YamlDotNet to v 13.1.0.
- Support for dumping NATS objects (IPAddress serializers, ignore delegates serialization) - read more about NATS: http://thinkmicroservices.com/blog/2021/jetstream/nats-jetstream.html

## [0.0.15]
- Update YamlDotNet to v 13.1.1.
- Update VarDump to v 0.2.0 (date-time serialization fix and custom collection fix)

## [0.0.16]
- Changed default settings for c# & vb date time: "dateTimeInstantiation": "parse", "dateKind": "original"

## [0.0.17]
- update VarDump to v 0.2.2 [use array initializer for non-public collection](https://github.com/ycherkes/VarDump/pull/11)

## [0.0.18]
- Fix minor dotnet version parsing
- Fix Yaml serializer's camelCase naming convention instantiation

## [0.0.19]
- Update VarDump to v 0.2.4 (Version support + generateVariableInitializer option)
- Add generateVariableInitializer option to vb & c# settings

## [0.0.20]
- Update YamlDotNet to v 13.3.1.
- Update VarDump to VarDumpExtended v 0.2.6 (improved DateTime serialization, added DependencyInjection.ServiceDescriptor serialization).

## [0.0.21]
- Update YamlDotNet to v 13.7.1.
- Update ILRepack to v 2.0.21.
- Update VarDump to v 0.2.11 (removed VarDumpExtended, added dump to textWriter feature).

## [0.0.22]
- Fixed. Previously when expression timed out, the temporary file created for output was not deleted.
- Update ILRepack to v 2.0.25.
- Update VarDump to v 0.2.12 (fixed readonly collection serialization).

## [0.0.23]
- Update VarDump to v 0.2.14 (laziness + MaxCollectionSize).

## [0.0.24]
- Update VarDump to v 0.2.15 (Fix the non-public collection serialization (regression .net 8.0)).

## [0.0.25]
- Update YellowFlavor.Serialization lib to v 0.0.0.96 (pass all the parameters as a single base64 encoded string).

## [0.0.26]
- Update VarDump to v 0.2.16 (Fix the cicrular reference detection - remove lazy behavior, regression in VarDump 0.2.13).

## [0.0.27]
- Update VarDump to v 1.0.0 (see https://github.com/ycherkes/VarDump/releases/tag/version_1.0.0).
- Renamed setting UseNamedArgumentsForReferenceRecordTypes to UseNamedArgumentsInConstructors.

## [0.0.29]
- Update VarDump to v 1.0.2 (see https://github.com/ycherkes/VarDump/releases/tag/version_1.0.2).

## [0.0.30]
- Update VarDump to v 1.0.4.6-alpha (see https://github.com/ycherkes/VarDump/pull).
