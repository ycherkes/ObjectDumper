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
- Removed all Json.Net and DotNetYaml embedded code. Replaced with ILRepack msbuild task.
- Fix Xml System.Type serialization. 
- Fix Xml naming issues.
- .Net6+ assemblies use .Net6 version of serialization library. It adds for example DateOnly & TimeOnly structs support.

## [0.0.8]
- Fix NamingStrategy Json Serialization setting behavior. 

## [0.0.9]
- Extract CSharpDumper and VisualBasicDumper to the external package https://www.nuget.org/packages/VarDump.

## [0.0.10]
- Update DotNetYaml to v 13.0.0.
- Rename internalized classes to avoid conflicts with user's code.

## [0.0.11]
- Update DotNetYaml to v 13.0.1.
- Add JsonDateTimeZoneHandling & XmlDateTimeZoneHandling options.