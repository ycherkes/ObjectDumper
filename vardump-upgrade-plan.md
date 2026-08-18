# VarDump 2.0.8.0 Upgrade Plan

Date: 2026-08-18

Status: Implemented and verified, except for the host smoke tests and pre-existing harness issues noted below.

## Implementation result

- Upgraded the shared serializer to VarDump `2.0.8.0` and versioned it as `0.0.0.104`.
- Replaced the old C# and Visual Basic Boolean type-name setting with `TypeNamingPolicy` and explicitly retained legacy-equivalent defaults for all other new VarDump options.
- Added focused C# and VB regression coverage for the changed API and behavior.
- Rebuilt and synchronized all five framework-specific serializer assemblies across Visual Studio and Rider, plus the VS Code and UWP copies.
- Versioned the Visual Studio extension as `0.0.0.104`, VS Code as `0.0.33`, and Rider as `0.0.3`, with corresponding release notes.
- Corrected the pre-existing Rider library-copy paths so both the setup script and Gradle copy task resolve the shared Visual Studio library directory, and declared the copy task as a dependency of resource processing.

## Resolution

Upgrade ObjectDumper from VarDump `1.0.4.11` to `2.0.8.0`, expose the supported VarDump 2.x options in every host, and replace the obsolete C# and Visual Basic Boolean type-name preference with the three-value `TypeNamePolicy`. Do not retain a migration layer for the old preference; persisted plugin settings may reset after the update. Keep output-compatible defaults for the remaining options.

The upgrade is considered moderate risk because VarDump crossed a major-version boundary and changed output behavior in several edge cases. The required ObjectDumper source migration is small, package target-framework compatibility is retained, and a compatibility probe successfully built every Serialization target.

## Baseline and target

- ObjectDumper currently references VarDump `1.0.4.11` in `src/Serialization/Serialization.csproj`.
- The target is the tagged VarDump `2.0.8.0` commit `301d9a223fdf465a5426ef7f9f289ab1cb4e9042` in `C:\Play\VarDump`.
- The baseline VarDump tag is `version_1.0.4.11`, commit `7263c8e68161c62dc2559479a5b8409e3c82949a`.
- The range contains 43 commits and changes 95 files, with 3,618 insertions and 1,038 deletions.
- VarDump continues to target `net45` and `netstandard2.0`; ObjectDumper's supported target frameworks therefore remain compatible.
- The VarDump worktree has no tracked local modifications. Its untracked planning, profiling, and documentation files are not part of the package upgrade.

## Relevant VarDump changes

### Breaking API changes

- `DumpOptions.UseTypeFullName` was removed and replaced by `DumpOptions.TypeNamePolicy`.
- `TypeNamePolicy` supports `ShortName`, `NestedQualified`, and `FullName`.
- `ICodeWriter` was substantially refactored for streaming output. Several methods were removed or replaced with item-oriented methods.
- `PropertyDescription` and `FieldDescription` constructors changed as part of lazy reflection and descriptor improvements.

ObjectDumper directly encounters only the `UseTypeFullName` removal. Its `ServiceDescriptorKnownObject` uses `ICodeWriter` methods that remain available, and its descriptor middleware does not construct `PropertyDescription` or `FieldDescription` directly.

### Behavior and correctness changes

- Correct rendering of zero, negative, and unnamed enum values.
- Correct handling of `IQueryable`, including `.AsQueryable()` output.
- Optional inclusion of base-class fields through `GetBaseClassFields`.
- Improved handling of read-only properties, private setters, default values, and `DefaultValueAttribute`.
- `TimeSpan` default-value and construction improvements.
- Correct circular-reference detection for objects, collections, dictionaries, and tuples without treating repeated sibling references as cycles.
- Collection-expression and indentation fixes.
- Streaming collection and dictionary output, reduced deferred-action allocation, cached property descriptors, and other performance improvements.

### New options

- `NewLineStyle`
- `StringLiteralStyle` for C#
- `CollectionLiteralStyle` for C#
- `TypeNamePolicy`
- `GetBaseClassFields`

The new options are exposed across Visual Studio, VS Code, and Rider. Their defaults preserve the previous output behavior, except that the removed Boolean type-name preference is not migrated.

## Implementation plan

### 1. Update the dependency and migrate the API

1. Change the VarDump package reference in `src/Serialization/Serialization.csproj` from `1.0.4.11` to `2.0.8.0`.
2. Replace the four uses of `DumpOptions.UseTypeFullName` in:
   - `src/Serialization/Implementation/CSharpSerializer.cs`
   - `src/Serialization/Implementation/VisualBasicSerializer.cs`
3. Replace the old Boolean setting in `CSharpSettings`, `VbSettings`, Visual Studio options, VS Code configuration, and Rider settings with `TypeNamePolicy`.
4. Map the selected policy directly when constructing `DumpOptions`:

   ```csharp
   newOptions.TypeNamePolicy = deserializedSettings.TypeNamePolicy;
   ```

5. Set the default serializer options to `TypeNamingPolicy.ShortName`.
6. For an auditable compatibility boundary, explicitly retain these legacy-equivalent values in the serializer defaults:
   - `CollectionLiteralStyle.Initializer`
   - `StringLiteralStyle.Auto`
   - `NewLineStyle.Auto`
   - `GetBaseClassFields = false`

### 2. Add regression tests

Add focused C# and VB coverage in `test/Serialization.UnitTests` for:

- Short, nested-qualified, and fully qualified type names.
- Nested types under all three policies.
- Zero and negative unnamed enum values.
- `IQueryable` serialization.
- Self-referencing objects, collections, and dictionaries.
- A shared object referenced by two sibling properties, which must not be reported as circular.
- `TimeSpan.Zero`, `DefaultValueAttribute`, private setters, and read-only properties with the existing ignore settings.
- A single-use or lazy enumerable to ensure streaming does not enumerate it more than once.
- `MaxCollectionSize` truncation and its generated comment.
- Long and multiline strings under the default `StringLiteralStyle.Auto` behavior.

Retain and run the existing C# and VB service-descriptor tests because they exercise ObjectDumper's custom known-object visitor against the refactored `ICodeWriter` interface.

### 3. Version and rebuild the shared serializer

1. Increment `AssemblyVersion` and `FileVersion` in `src/Serialization/Serialization.csproj` from `0.0.0.103` to the selected release version, expected to be `0.0.0.104` if no intervening release changes it.
2. Build `src/Serialization/Serialization.csproj` in Release for:
   - `net45`
   - `netstandard2.0`
   - `netcoreapp2.0`
   - `netcoreapp3.1`
   - `net6.0`
3. Confirm ILRepack succeeds for every target and that the merged assembly does not require a separately deployed VarDump DLL.
4. Verify the merged assemblies contain the VarDump `2.0.8.0` implementation.

### 4. Propagate the rebuilt assemblies

Replace all checked-in copies of `YellowFlavor.Serialization.dll`:

- `src/ObjectDumper/InjectableLibs/<framework>/`
- `src/object-dumper-vscode/injectable_libraries/netstandard2.0/`
- `src/ObjectDumper.Rider/src/main/resources/InjectableLibs/<framework>/`
- `samples/uwp/TestUwp/ThirdParty/`

Use `src/ObjectDumper.Rider/setup.ps1` or the Rider Gradle copy task to synchronize the Rider copies from the Visual Studio injectable-library directory.

After copying, compare hashes where the same target-framework assembly is expected to be identical. Ensure no stale `0.0.0.103` serializer remains in a package input directory.

### 5. Update host versions and release notes

- Visual Studio:
  - Increment `src/ObjectDumper/PackageConstants.cs`.
  - Increment `src/ObjectDumper/source.extension.vsixmanifest` to the same version.
- VS Code:
  - Increment `src/object-dumper-vscode/package.json`, expected from `0.0.32` to `0.0.33`.
  - Add a VarDump 2.0.8.0 entry to `src/object-dumper-vscode/CHANGELOG.md`.
- Rider:
  - Increment `src/ObjectDumper.Rider/build.gradle.kts`, expected from `0.0.2` to `0.0.3`.
  - Update `src/ObjectDumper.Rider/CHANGELOG.md` and plugin change notes as appropriate.

Release notes should call out corrected enum, queryable, default-value, and circular-reference output together with the newly exposed formatting options.

## Verification plan

### Automated verification

1. Run the VarDump test suite for its supported test targets.
2. Run `Serialization.UnitTests` and `Serialization.UnitTests.FSharp` with a genuine .NET 6 runtime in CI.
3. Build the shared Serialization project in Release for all five target frameworks.
4. Build the Visual Studio extension, VS Code extension package, and Rider plugin.
5. Inspect each produced package and confirm the expected serializer DLLs and versions are present.

### Host smoke tests

For Visual Studio, VS Code, and Rider, debug representative applications targeting .NET Framework and modern .NET and verify:

- C# and VB output succeeds.
- Short and full type-name settings are honored.
- A custom service descriptor is serialized correctly.
- A circular graph terminates with a circular-reference comment.
- A large or lazy collection is responsive and respects `MaxCollectionSize`.
- JSON, XML, and YAML output remains unaffected.

## Probe evidence

A temporary detached ObjectDumper worktree was used to test the package update without modifying the main worktree.

- Updating only the package produced four compile errors, all caused by the removed `UseTypeFullName` property.
- Mapping those uses to `TypeNamePolicy` removed all compile errors.
- The Release build succeeded for all five Serialization target frameworks, including ILRepack.
- ObjectDumper C# tests with the migrated package produced 66 passes, 2 skips, and 2 failures.
- The same two failures occur on the baseline package when the net6 test host is rolled forward to .NET 8:
  - An XML assertion embeds the expected .NET 6 core-library version.
  - An exception-message assertion is runtime-sensitive.
- The F# test passed.
- VarDump's own tests passed on `net10.0` and `net472` with only the repository's existing skipped tests and package-resolution warnings.

These two ObjectDumper failures are not VarDump 2.0.8.0 regressions. CI should nevertheless use the intended .NET 6 runtime or make those tests runtime-independent in a separate cleanup.

## Acceptance criteria

- ObjectDumper references VarDump `2.0.8.0` with no direct use of removed VarDump APIs.
- All three hosts expose the same three-value type-name policy.
- All supported Serialization targets compile and merge successfully.
- New and existing serialization tests pass in the intended runtime environment.
- Every packaged host contains the rebuilt serializer; no stale serializer DLL remains.
- VS, VS Code, and Rider smoke tests succeed for C# and VB output.
- Release notes accurately describe correctness and performance changes without prematurely exposing new configuration features.

## Follow-up work (completed in the options rollout)

Handle VarDump 2.x feature exposure separately:

- Three-value type naming is exposed across all hosts without retaining the obsolete Boolean preference.
- Expose newline selection.
- Expose C# escaped, verbatim, and raw string styles.
- Expose C# collection expressions with appropriate language-version guidance.
- Expose inherited-field inclusion.

These options are now implemented across Visual Studio, VS Code, and Rider under the implementation and verification plan in `vardump-options-plan.md`.

## Verification result

- Shared Serialization Release build: passed for `net45`, `netstandard2.0`, `netcoreapp2.0`, `netcoreapp3.1`, and `net6.0`, including ILRepack.
- VarDump regression and options suite: 20 passed.
- Runtime-stable C# suite excluding the two established runtime-sensitive test classes: 80 passed and 2 skipped.
- Existing F# suite: 1 passed.
- VarDump suite: `net10.0` had 219 passed and 4 skipped; `net472` had 211 passed and 5 skipped.
- Visual Studio Release build and VSIX packaging: passed; the VSIX contains all five serializer assemblies.
- VS Code compile, lint, and VSIX packaging: passed; the VSIX contains the `netstandard2.0` serializer assembly.
- Checked-in duplicate assemblies were hash-compared and match their framework source copies; all report assembly version `0.0.0.104`.
- Rider `buildPlugin`: passed on Microsoft OpenJDK 17 with the pre-existing, unnecessary bytecode-instrumentation task excluded; the produced ZIP contains all five serializer assemblies.
- `git diff --check`: passed.

The full C# run still has the same two failures observed on the old VarDump package when the .NET 6 tests are rolled forward to .NET 8: a hard-coded core-library version and a runtime-sensitive exception message. VS Code's integration harness cannot launch the downloaded app package because its old `@vscode/test-electron` version expects `Code.exe` in a layout no longer supplied by the download. The Rider project's standard `build` still fails in the old IntelliJ Platform Gradle Plugin 2.0.0 `instrumentCode` task because it looks for a nonexistent `Packages` directory under the Windows JDK; `buildPlugin -x instrumentCode` succeeds, and this Kotlin-only plugin has no Java forms to instrument. These are environment or pre-existing harness issues, not VarDump upgrade regressions. Manual host smoke tests remain release-validation work.
