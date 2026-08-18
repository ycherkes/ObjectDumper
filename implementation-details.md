# VarDump 2.0.8.0 Implementation Details

Date: 2026-08-18

## Outcome

ObjectDumper now uses VarDump `2.0.8.0`, and the VarDump 2.x formatting and traversal options are exposed consistently across the Visual Studio, VS Code, and Rider plugins.

Host versions:

- Visual Studio: `0.0.0.104`
- VS Code: `0.0.33`
- Rider: `0.0.3`
- Shared serializer assembly: `0.0.0.104`

## Exposed options

The following options are available for C# and Visual Basic:

- Type naming policy: `ShortName`, `NestedQualified`, or `FullName`
- Newline style: `Auto`, `Unix`, or `Windows`
- Include base-class fields

The following syntax options are available for C# only:

- String literal style: `Auto`, `Escaped`, `Verbatim`, or `Raw`
- Collection literal style: `Initializer` or `Expression`

Raw string literals and collection expressions require a compatible C# language version in code that consumes the generated output.

## Settings reset behavior

The obsolete C# and Visual Basic Boolean type-name option has been removed from the shared contract and all three plugins. Type naming is controlled exclusively by `TypeNamePolicy`, whose default is `ShortName`. Persisted plugin settings may reset during the update; no migration layer is maintained.

XML's separate `UseFullTypeName` option remains available because it belongs to the XML serializer rather than VarDump's C#/Visual Basic type naming.

## Shared serializer changes

- Updated the VarDump package reference from `1.0.4.11` to `2.0.8.0`.
- Replaced the removed `DumpOptions.UseTypeFullName` API with direct `TypeNamePolicy` configuration.
- Added the new fields to the C# and Visual Basic JSON settings contracts.
- Mapped the applicable fields to `DumpOptions` before serialization.
- Kept `StringLiteralStyle` and `CollectionLiteralStyle` exclusively in the C# serializer; the Visual Basic defaults no longer set these ineffective C#-only options.
- Set explicit output-compatible defaults for type naming, newlines, base fields, string literals, and collection literals.
- Rebuilt and ILRepacked the serializer for `net45`, `netstandard2.0`, `netcoreapp2.0`, `netcoreapp3.1`, and `net6.0`.

## Visual Studio changes

- Added local enums corresponding to the new VarDump option values.
- Added C# and Visual Basic properties to the Visual Studio options page.
- Added type-name policy properties with `ShortName` defaults.
- Added all new option values to the serialized debugger payload.
- Rebuilt the Release VSIX with five framework-specific serializer assemblies.

Package: `src/ObjectDumper/bin/Release/ObjectDumper.vsix`

## VS Code changes

- Added configuration-schema entries for all applicable C# and Visual Basic options.
- Replaced the old Boolean configuration with the required `typeNamePolicy` setting.
- Added the new values to `OptionsProvider` and the debugger payload.
- Updated the changelog and rebuilt the VSIX.

Package: `src/object-dumper-vscode/object-dumper-0.0.33.vsix`

## Rider changes

- Added persistent enums and settings for all applicable options.
- Replaced the C# and Visual Basic full-type-name checkboxes with type-name policy selectors.
- Added newline, inherited-field, C# string-style, and C# collection-style controls.
- Persisted the three-value type-name policy directly.
- Added all new values to the debugger JSON payload.
- Corrected the serializer-library copy paths and declared the copy task as a resource-processing dependency.
- Installed Microsoft OpenJDK 17 side-by-side with the existing Android JDK 21.
- Rebuilt the Rider plugin using JDK 17.

Package: `src/ObjectDumper.Rider/build/distributions/ObjectDumper-0.0.3.zip`

The old IntelliJ Platform Gradle Plugin `2.0.0` still fails its `instrumentCode` task on this Windows JDK because it looks for a nonexistent `Packages` directory. Plugin packaging succeeds with `instrumentCode` excluded. The plugin is Kotlin-only and has no Java GUI forms to instrument.

## Binary propagation

The rebuilt serializer assemblies were copied to:

- `src/ObjectDumper/InjectableLibs/<framework>/`
- `src/ObjectDumper.Rider/src/main/resources/InjectableLibs/<framework>/`
- `src/object-dumper-vscode/injectable_libraries/netstandard2.0/`
- `samples/uwp/TestUwp/ThirdParty/`

All checked-in copies report assembly version `0.0.0.104`. Rider and VS Code copies hash-match the corresponding Visual Studio source assemblies.

## Verification

- Shared serializer Release build passed for all five target frameworks, including ILRepack.
- Focused VarDump upgrade and option tests: 20 passed.
- Runtime-stable C# suite: 80 passed and 2 existing tests skipped.
- F# suite: 1 passed.
- Visual Studio Release solution build and VSIX creation passed.
- VS Code TypeScript compilation, lint, configuration-schema parsing, and VSIX creation passed.
- Rider Kotlin compilation and plugin packaging passed on JDK 17 with `instrumentCode` excluded.
- Package inspection found five serializer assemblies in the Visual Studio VSIX, one in the VS Code VSIX, and five in the Rider ZIP.
- `git diff --check` passed.

Two existing C# test classes remain excluded from the runtime-stable verification because they fail identically with the old VarDump package when .NET 6 tests are rolled forward to .NET 8: one hard-codes a core-library version and the other asserts a runtime-sensitive exception message.

## Related plans

- `vardump-upgrade-plan.md`
- `vardump-options-plan.md`
