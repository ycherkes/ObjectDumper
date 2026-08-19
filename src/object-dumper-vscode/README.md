# Object Dumper for Visual Studio Code

[![Marketplace](https://vsmarketplacebadges.dev/version/YevhenCherkes.object-dumper.svg?label=VS%20Marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper)
[![Installs](https://vsmarketplacebadges.dev/installs/YevhenCherkes.object-dumper.svg?label=VS%20Marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper)
[![License: MIT](https://img.shields.io/github/license/ycherkes/ObjectDumper?style=for-the-badge)](https://github.com/ycherkes/ObjectDumper/blob/main/LICENSE.txt)

[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua)

**Object Dumper** is a Visual Studio Code extension for exporting live .NET objects during debugging into reusable **C#**, **Visual Basic**, **JSON**, **XML**, or **YAML** representations.

Instead of manually expanding a complex object graph and copying debugger values one by one, select an expression in the editor while paused at a breakpoint, choose **Dump As**, and Object Dumper opens the generated representation in a separate editor document.

The extension also works in **GitHub Codespaces**.

Object Dumper is also available for [Visual Studio](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper) and [JetBrains Rider](https://plugins.jetbrains.com/plugin/30257-object-dumper).

Inspired by [ObjectExporter](https://github.com/OmarElabd/ObjectExporter).

![Presentation](https://user-images.githubusercontent.com/13467759/201370888-c8aa6d18-a732-4538-8466-3251665bbfe6.gif)

## Features

### Dump objects in multiple formats

- C# object initialization code
- Visual Basic object initialization code
- JSON
- XML
- YAML

### Generate reusable code from runtime objects

C# and Visual Basic output is powered by [VarDump](https://github.com/ycherkes/VarDump).

Typical use cases include:

- creating unit-test fixtures from real runtime objects
- capturing DTOs and domain objects for bug reproduction
- turning debugger state into C# or Visual Basic initialization code
- copying runtime objects as JSON or YAML for API testing
- inspecting large object graphs without manually expanding every debugger node

### Fine-grained C# and Visual Basic formatting

Object Dumper exposes VarDump 2.x formatting and traversal options directly through VS Code settings.

For both C# and Visual Basic you can configure:

- **Type naming policy**
  - `shortName`
  - `nestedQualified`
  - `fullName`
- **Newline style**
  - `auto`
  - `unix`
  - `windows`
- inclusion of **base-class fields**
- null and default-value handling
- maximum collection size
- property visibility
- field visibility
- instance/static member selection
- readonly-property handling
- member sorting
- generated variable initializers
- primitive collection layout
- integral numeric formatting
- named constructor arguments
- predefined constants and helper methods
- `DateTime` construction and kind handling

C# additionally supports:

- **String literal style**
  - `auto`
  - `escaped`
  - `verbatim`
  - `raw`
- **Collection literal style**
  - `initializer`
  - `expression`

> [!NOTE]
> Raw string literals and collection expressions require a compatible C# language version in the project where the generated code is used.

### Improved serialization with VarDump 2.x

Version `0.0.33` updates the bundled VarDump library from `1.0.4.11` to `2.0.8.0`.

The new serializer improves:

- enum serialization
- `IQueryable` handling
- default-value handling
- circular-reference detection
- allocation behavior while traversing collections and object graphs

### Format-specific settings

JSON supports configuration for:

- null/default-value handling
- naming strategy
- enums as strings
- type-name handling
- `DateTime` zone handling

XML supports:

- null/default-value handling
- naming strategy
- enums as strings
- `DateTime` zone handling

YAML supports configurable naming conventions.

## Installation

Install **Object Dumper** from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper), or install it from VS Code:

1. Open **Extensions**
2. Search for **Object Dumper**
3. Select the extension published by **YevhenCherkes**
4. Click **Install**

## Usage

Object Dumper commands are available while debugging a **C#** or **F#** project.

1. Start a debugging session.
2. Stop at a breakpoint.
3. Select or place the cursor on the expression you want to export.
4. Right-click in the editor.
5. Open **Dump As**.
6. Choose the desired format.
7. The generated output opens in a separate editor document.

### Keyboard shortcuts

| Format | Windows / Linux | macOS |
|---|---|---|
| C# | `Ctrl+K D` | `Cmd+K D` |
| JSON | `Ctrl+K J` | `Cmd+K J` |
| Visual Basic | `Ctrl+K V` | `Cmd+K V` |
| XML | `Ctrl+K X` | `Cmd+K X` |
| YAML | `Ctrl+K Y` | `Cmd+K Y` |

The commands are only enabled during a supported debugging session.

## Configuration

Open **Settings** and search for:

```text
Object Dumper
```

or edit your `settings.json` directly.

### Common settings

```json
{
  "objectDumper.common.maxDepth": 25
}
```

### C# example

```json
{
  "objectDumper.csharp.typeNamePolicy": "shortName",
  "objectDumper.csharp.newLineStyle": "auto",
  "objectDumper.csharp.getBaseClassFields": false,
  "objectDumper.csharp.stringLiteralStyle": "auto",
  "objectDumper.csharp.collectionLiteralStyle": "initializer",
  "objectDumper.csharp.generateVariableInitializer": true,
  "objectDumper.csharp.primitiveCollectionLayout": "multiLine"
}
```

### Visual Basic example

```json
{
  "objectDumper.vb.typeNamePolicy": "shortName",
  "objectDumper.vb.newLineStyle": "auto",
  "objectDumper.vb.getBaseClassFields": false,
  "objectDumper.vb.generateVariableInitializer": true,
  "objectDumper.vb.primitiveCollectionLayout": "multiLine"
}
```

### Type naming

The `typeNamePolicy` setting controls how generated C# and Visual Basic type names are written:

| Value | Behavior |
|---|---|
| `shortName` | Uses the shortest practical type name |
| `nestedQualified` | Includes containing type names for nested types |
| `fullName` | Uses fully qualified type names |

### C# string literal style

| Value | Behavior |
|---|---|
| `auto` | VarDump chooses an appropriate representation |
| `escaped` | Standard escaped strings |
| `verbatim` | Verbatim `@"..."` strings where applicable |
| `raw` | C# raw string literals |

### C# collection literal style

| Value | Behavior |
|---|---|
| `initializer` | Traditional collection/array initializer syntax |
| `expression` | C# collection-expression syntax |

## Upgrading from earlier versions

Starting with Object Dumper `0.0.33`, C# and Visual Basic type naming is controlled by `typeNamePolicy`.

The previous Boolean full-type-name setting has been removed.

The available replacements are:

- `shortName`
- `nestedQualified`
- `fullName`

Persisted extension settings using the old option may need to be updated manually.

## Requirements and known restrictions

- Visual Studio Code `1.73.0` or later
- C# and F# source projects are currently supported
- local debugging only
- .NET Standard 2.0+ and .NET Core 2.0+ debugging scenarios are supported
- .NET Framework debugging is not currently supported

Object Dumper commands are exposed from the **editor context menu**, not from the Variables/Watch context menu.

## GitHub Codespaces

Object Dumper can be used in GitHub Codespaces as long as the active debugging scenario satisfies the extension's supported .NET requirements.

## Privacy

**Object Dumper does not collect personal data.**

Debugger interaction and serialization happen as part of your local or Codespaces debugging session.

## Powered by

| Library | Purpose | License |
|---|---|---|
| [VarDump](https://github.com/ycherkes/VarDump) | C# and Visual Basic serialization | [Apache-2.0](https://github.com/ycherkes/VarDump/blob/main/LICENSE) |
| [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) | JSON and XML serialization | [MIT](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md) |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet) | YAML serialization | [MIT](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt) |
| [ILRepack](https://github.com/gluck/il-repack) | Serializer assembly merging | [Apache-2.0](https://github.com/gluck/il-repack/blob/master/LICENSE) |

## Contributing

Contributions, bug reports, and feature requests are welcome.

- [Star the repository](https://github.com/ycherkes/ObjectDumper)
- [Open an issue](https://github.com/ycherkes/ObjectDumper/issues)
- Submit a pull request
- [Review the VS Code extension](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper&ssr=false#review-details)

## Support the project

If Object Dumper saves you time, you can support its continued development through:

- [GitHub Sponsors](https://github.com/sponsors/ycherkes)
- [PayPal](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+Object+Dumper+become+better%21)

## License

Object Dumper is licensed under the [MIT License](../../LICENSE.txt).
