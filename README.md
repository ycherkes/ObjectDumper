[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://stand-with-ukraine.pp.ua)

## Terms of use[?](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://stand-with-ukraine.pp.ua). Glory to Ukraine! 🇺🇦

<img src="https://yevhencherkes.gallerycdn.vsassets.io/extensions/yevhencherkes/yellowflavorobjectdumper/0.0.0.64/1665328424655/Microsoft.VisualStudio.Services.Icons.Default" width="100" height="100" />

# Object Dumper for Visual Studio

[![VS Marketplace](https://vsmarketplacebadges.dev/version-short/YevhenCherkes.YellowFlavorObjectDumper.svg?label=VS%20Marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)
[![VS Installs](https://vsmarketplacebadges.dev/installs-short/YevhenCherkes.YellowFlavorObjectDumper.svg?label=VS%20Installs&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)
[![License: MIT](https://img.shields.io/badge/LICENSE-MIT-44bb00?style=for-the-badge)](https://github.com/ycherkes/ObjectDumper/blob/main/LICENSE.txt)

**Object Dumper** is a Visual Studio extension for exporting live .NET objects during debugging into reusable **C#**, **Visual Basic**, **JSON**, **XML**, or **YAML** representations.

Instead of manually expanding a large object graph and copying debugger values one by one, pause at a breakpoint, select an expression in the **Code** or **Immediate** window, choose **Dump As**, and Object Dumper sends the generated result to a new document, the Object Dumper output pane, or the clipboard.

Object Dumper is also available for:

- [Visual Studio Code](src/object-dumper-vscode/README.md)
- [JetBrains Rider](src/ObjectDumper.Rider/README.md)

Inspired by [ObjectExporter](https://github.com/OmarElabd/ObjectExporter).

![Presentation](https://user-images.githubusercontent.com/13467759/175763360-6d714f96-8b90-48a9-bff0-8bceac4c2502.gif)

## Features

### Dump debugger objects in multiple formats

- C# object initialization code
- Visual Basic object initialization code
- JSON
- XML
- YAML

### Generate reusable code from runtime state

C# and Visual Basic output is powered by [VarDump](https://github.com/ycherkes/VarDump).

Typical use cases include:

- creating unit-test fixtures from real runtime objects
- capturing complex DTOs or domain objects for bug reproduction
- generating C# or Visual Basic initialization code
- exporting debugger state to JSON or YAML
- inspecting large object graphs without manually expanding every debugger node
- comparing two runtime objects by dumping them into separate documents and using Visual Studio's file comparison tools

### Fine-grained C# and Visual Basic formatting

Object Dumper exposes VarDump 2.x formatting and traversal options through:

**Tools → Options → Object Dumper**

For both C# and Visual Basic you can configure:

- **Type naming policy**
  - `ShortName`
  - `NestedQualified`
  - `FullName`
- **Newline style**
  - `Auto`
  - `Unix`
  - `Windows`
- inclusion of **base-class fields**
- null and default-value handling
- maximum collection size
- property binding flags
- field binding flags
- readonly-property handling
- member sorting
- indentation
- generated variable initializers
- primitive collection layout
- integral numeric formatting
- named constructor arguments
- predefined constants and helper methods
- `DateTime` construction and kind handling

C# additionally supports:

- **String literal style**
  - `Auto`
  - `Escaped`
  - `Verbatim`
  - `Raw`
- **Collection literal style**
  - `Initializer`
  - `Expression`

> [!NOTE]
> Raw string literals and collection expressions require a compatible C# language version in the project where the generated code is used.

### Format-specific settings

JSON supports:

- null/default-value handling
- naming strategy
- enums as strings
- type-name handling
- `DateTime` zone handling

XML supports:

- null/default-value handling
- naming strategy
- enums as strings
- full type names
- `DateTime` zone handling

YAML supports configurable naming conventions.

### Multiple output destinations

Use **Dump To** to choose where generated content is written:

- **New Document**
- **Output Window → Object Dumper**
- **Clipboard**

## Installation

Install Object Dumper from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper).

After installation, restart Visual Studio if prompted.

## Usage

1. Start debugging your .NET application.
2. Stop at a breakpoint.
3. Select an expression in the **Code** window, or enter/select an expression in the **Immediate** window.
4. Right-click and open **Dump As**.
5. Choose C#, Visual Basic, JSON, XML, or YAML.
6. The generated output is sent to the destination configured by **Dump To**.

## Configuration

Open:

**Tools → Options → Object Dumper**

### Common defaults

| Option | Default |
|---|---|
| Max Depth | `25` |
| Operation Timeout | `10 seconds` |
| Dump To | `New Document` |

### C# defaults

| Option | Default |
|---|---|
| Enabled | `true` |
| Ignore Null Values | `true` |
| Ignore Default Values | `true` |
| Type Naming Policy | `ShortName` |
| Newline Style | `Auto` |
| Include Base-Class Fields | `false` |
| String Literal Style | `Auto` |
| Collection Literal Style | `Initializer` |
| DateTime Instantiation | `Parse` |
| DateTime Kind | `Original` |
| Max Collection Size | `int.MaxValue` |
| Use Named Arguments In Constructors | `false` |
| Use Predefined Constants | `true` |
| Use Predefined Methods | `true` |
| Get Properties Binding Flags | `Public, Instance` |
| Ignore Readonly Properties | `true` |
| Generate Variable Initializer | `true` |
| Primitive Collection Layout | `MultiLine` |
| Integral Numeric Format | `D` |

Visual Basic exposes equivalent defaults for the options that apply to it.

## Changelog

See the [changelog](CHANGELOG.md) for release details and migration guidance.

## IDE support

| IDE | Documentation |
|---|---|
| Visual Studio | This page |
| Visual Studio Code | [VS Code README](src/object-dumper-vscode/README.md) |
| JetBrains Rider | [Rider README](src/ObjectDumper.Rider/README.md) |

## Quick tip: compare two dumped objects

Visual Studio can be used to compare two dumped object documents:

1. Enable **Show Miscellaneous Files in Solution Explorer**.
2. Dump both objects to separate documents.
3. In Visual Studio 17.7 or later, select both files under **Miscellaneous Files**, right-click, and use the built-in comparison command.
4. On older versions, install a diff extension such as [Heku.VsDiff](https://marketplace.visualstudio.com/items?itemName=Heku.VsDiff2022).

![Show Miscellaneous Files](https://github.com/ycherkes/ObjectDumper/assets/13467759/2cd2d786-1e30-4425-83ab-664277068ad6)

![Compare Selected Files](https://user-images.githubusercontent.com/13467759/173349566-518f89e1-9d21-4ab6-a4e1-da2dc86e3a78.png)

## Requirements

Supported Visual Studio versions:

- Visual Studio 2019
- Visual Studio 2022

Supported project languages:

- C#
- F#
- Visual Basic

Supported serializer targets include:

- .NET Framework 4.5+
- .NET Standard 2.0+
- .NET Core 2.0+
- .NET 5+

Local debugging is currently required.

## Known limitations and troubleshooting

### Optimized code / Release builds

When debugging optimized code, external DLLs, or some NuGet package code, Visual Studio may report errors such as:

```text
Cannot evaluate expression because the code of the current method is optimized
```

or:

```text
error CS0103: The name 'YellowFlavor' does not exist in the current context
```

Use a Debug build when possible.

Alternatively enable:

**Tools → Options → Debugging → General → Suppress JIT optimization on module load**

See the [Visual Studio documentation](https://learn.microsoft.com/en-us/visualstudio/debugger/jit-optimization-and-debugging?view=vs-2022#the-suppress-jit-optimization-on-module-load-managed-only-option).

### Remote debugging

Remote debugging is not currently supported.

### Encoding issues

If generated documents contain encoding-related issues, enable:

**Tools → Options → Environment → Documents → Save documents as Unicode when data cannot be saved in codepage**

### UWP applications

UWP does not support `Assembly.LoadFrom`, which prevents Object Dumper from injecting the serializer assembly automatically.

A workaround is to reference the .NET Standard 2.0 version of the [Serialization library](src/Serialization) directly and preload it:

```csharp
YellowFlavor.Serialization.ObjectSerializer.WarmUp();
```

See the [UWP sample](samples/uwp/TestUwp/App.xaml.cs).

### IIS-hosted ASP.NET MVC

When debugging an IIS-hosted ASP.NET MVC backend, the debugger process may require additional filesystem access.

Grant **YourComputerName\IIS_IUSRS**:

- **Read/Write** access to `%userprofile%\AppData\Local\Temp`
- **Read** access to `%userprofile%\AppData\Local\Microsoft\VisualStudio`

Otherwise Object Dumper may fail with `UnauthorizedAccessException`.

See [issue #90](https://github.com/ycherkes/ObjectDumper/issues/90).

## Architecture

Object Dumper separates Visual Studio integration from its serializer layer:

1. **Visual Studio extension layer:** Provides commands, menus, options, output destinations, and debugger integration.

2. **Debugger interaction:** Evaluates the selected expression and loads the appropriate serializer into the debuggee.

3. **Shared serialization layer:** Uses framework-specific `YellowFlavor.Serialization.dll` assemblies.

4. **Serialization libraries**
   - VarDump for C# and Visual Basic
   - Json.NET for JSON and XML
   - YamlDotNet for YAML

5. **ILRepack:** Bundles serializer dependencies into merged assemblies to reduce dependency conflicts with the debuggee.

## Privacy

**Object Dumper does not collect personal data.**

Debugger interaction and serialization happen locally as part of your Visual Studio debugging session.

## Powered by

| Library | Purpose | License |
|---|---|---|
| [VarDump](https://github.com/ycherkes/VarDump) | C# and Visual Basic serialization | [Apache-2.0](https://github.com/ycherkes/VarDump/blob/main/LICENSE) |
| [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) | JSON and XML serialization | [MIT](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md) |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet) | YAML serialization | [MIT](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt) |
| [ILRepack](https://github.com/gluck/il-repack) | Serializer dependency merging | [Apache-2.0](https://github.com/gluck/il-repack/blob/master/LICENSE) |

## Contributing

Contributions, bug reports, and feature requests are welcome.

- [Star the repository](https://github.com/ycherkes/ObjectDumper)
- [Open an issue](https://github.com/ycherkes/ObjectDumper/issues/new/choose)
- [Submit a pull request](https://github.com/ycherkes/ObjectDumper/compare)
- [Review the Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper&ssr=false#review-details)

## Support the project

If Object Dumper saves you time, you can support its continued development through:

- [GitHub Sponsors](https://github.com/sponsors/ycherkes)
- [PayPal](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+Object+Dumper+become+better%21)

## License

Object Dumper is licensed under the [MIT License](LICENSE.txt).
