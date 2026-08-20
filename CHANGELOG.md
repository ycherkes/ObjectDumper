# Changelog

All notable changes to Object Dumper across Visual Studio, Visual Studio Code, and JetBrains Rider are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Package versions

| Host | Version |
|---|---|
| Visual Studio | `0.0.0.104` |
| Visual Studio Code | `0.0.33` |
| JetBrains Rider | `0.0.3` |

### Changed

- Updated the bundled [VarDump](https://github.com/ycherkes/VarDump#readme) library from `1.0.4.11` to `2.0.9`.
- Added type-naming policy, newline-style, and inherited-field options for C# and Visual Basic in all three IDE integrations.
- Added C# string-literal and collection-literal style options.
- Rebuilt and synchronized the merged serializer assemblies for .NET Framework 4.5, .NET Standard 2.0, .NET Core 2.0, .NET Core 3.1, and .NET 6.

### Improved

- Improved enum serialization, including unnamed and negative values.
- Improved `IQueryable` serialization and lazy collection traversal.
- Improved default-value and readonly-property handling.
- Improved circular-reference detection for objects, collections, and dictionaries without treating shared sibling references as cycles.
- Reduced allocations while writing collections and traversing object graphs.

### Migration notes

- Replaced the C# and Visual Basic Boolean full-type-name setting with **Type Naming Policy**: `ShortName`, `NestedQualified`, or `FullName`.
- Existing C# and Visual Basic preferences may reset because the previous setting format is not migrated.
- XML's independent **Use Full Type Name** setting is unchanged.
- Raw C# string literals and collection expressions require a compatible C# language version in the code consuming the generated output.
