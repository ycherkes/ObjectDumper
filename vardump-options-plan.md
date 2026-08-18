# VarDump 2.x Options Rollout Plan

Date: 2026-08-18

Status: Implemented and verified.

## Objective

Expose the formatting and traversal options added by VarDump 2.x in the Visual Studio, VS Code, and Rider extensions with one consistent settings contract.

## Option surface

| Option | C# | Visual Basic | Default |
| --- | --- | --- | --- |
| Type naming policy (`ShortName`, `NestedQualified`, `FullName`) | Yes | Yes | `ShortName` |
| Newline style (`Auto`, `Unix`, `Windows`) | Yes | Yes | `Auto` |
| Include base-class fields | Yes | Yes | `false` |
| String literal style (`Auto`, `Escaped`, `Verbatim`, `Raw`) | Yes | No | `Auto` |
| Collection literal style (`Initializer`, `Expression`) | Yes | No | `Initializer` |

String and collection literal styles remain C#-only because those VarDump options control C# syntax. Raw strings and collection expressions can require newer C# language versions, so plugin descriptions will state that constraint.

## Settings strategy

1. Add non-null `TypeNamePolicy` fields to the shared C# and VB settings contracts.
2. Use `ShortName` as the default in the serializer and all three hosts.
3. Remove the obsolete Boolean type-name setting rather than maintaining a migration layer; plugin settings may reset after this update.
4. Preserve legacy-equivalent defaults for the other options, so generated output changes only when a user selects a new value.

## Implementation steps

1. Extend `CSharpSettings` and `VbSettings`, then map the new values onto `DumpOptions` in both serializers.
2. Add local option enums and properties to the Visual Studio option page and include them in its JSON payload.
3. Add configuration schema entries to VS Code and forward them through `OptionsProvider`.
4. Add persistent settings, combo boxes/check boxes, and JSON fields to Rider.
5. Add serializer regression tests for every supported value and the new default type-name policy.
6. Compile the shared serializer, Visual Studio extension, VS Code extension, and Rider plugin; rebuild and propagate the merged serializer DLLs if the shared assembly changes.

## Acceptance criteria

- Every applicable option is visible and persisted in all three hosts.
- C# and VB payloads use the same JSON property names and enum values.
- Type naming is controlled exclusively by the three-value `TypeNamePolicy` setting.
- The defaults remain output-compatible with the pre-options release.
- Tests demonstrate newline, type naming, inherited fields, C# string style, and C# collection style behavior.
- All locally available host builds and package inspections pass, with any pre-existing harness limitations documented.

## Verification result

- Shared serializer Release build passed for all five target frameworks, including ILRepack.
- The focused VarDump upgrade/options suite passed all 20 cases.
- The remaining runtime-stable C# suite passed 80 tests with 2 existing skips; the F# suite passed its test.
- Visual Studio Release build and VSIX creation passed.
- VS Code TypeScript compilation, lint, schema parsing, and VSIX creation passed.
- Rider Kotlin compilation and plugin packaging passed on JDK 17 with the pre-existing `instrumentCode` task excluded as documented in the main upgrade plan.
- Package inspection found five serializer assemblies in the Visual Studio VSIX, one in the VS Code VSIX, and five in the Rider ZIP.
- All checked-in serializer copies report assembly version `0.0.0.104`; Rider and VS Code copies hash-match their Visual Studio framework source.
- `git diff --check` passed.
