# Object Dumper for JetBrains Rider

[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://stand-with-ukraine.pp.ua)

## Terms of use[?](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://stand-with-ukraine.pp.ua). Glory to Ukraine! 🇺🇦

Reflection-based JetBrains Rider extension for exporting in-memory objects during debugging to **C# Object Initialization Code**, **JSON**, **Visual Basic Object Initialization Code**, **XML**, and **YAML** string.

This is the Rider version of the popular [Object Dumper extension](https://github.com/ycherkes/ObjectDumper) available for Visual Studio and Visual Studio Code.

## Features

- ?? **Dump objects during debugging** in multiple formats:
  - C# Object Initialization Code
  - JSON
  - XML
  - Visual Basic Object Initialization Code
  - YAML

- ?? **Highly configurable**:
  - Choose output destination (new editor tab, clipboard, or debug console)
  - Configure max depth and collection size limits
  - Customize DateTime formatting
  - Control collection layout (inline or one-per-line)
  - Select naming conventions (PascalCase, camelCase, snake_case)

- ?? **Easy to use**: Right-click on any variable in the Debug tool window and select "Dump as..."

- ? **Full Implementation**: Complete debugger integration with expression evaluation, assembly injection, and serialization

## Installation

### From JetBrains Marketplace (Coming Soon)

1. Open Rider
2. Go to `File` ? `Settings` ? `Plugins`
3. Search for "Object Dumper"
4. Click `Install`
5. Restart Rider

### From Source

1. Clone this repository
2. Navigate to the `ObjectDumper.Rider` directory
3. Run `./gradlew buildPlugin`
4. Install the generated plugin from `build/distributions/ObjectDumper.Rider-*.zip`

## Usage

1. Start debugging your .NET application in Rider
2. Set a breakpoint and pause execution
3. In the Debug tool window, right-click on any variable
4. Select `Dump As` ? choose your desired format (C#, JSON, XML, VB, or YAML)
5. The dumped object will appear based on your configured destination

![Usage Example](docs/usage-example.gif)

## Configuration

Access settings via `File` ? `Settings` ? `Tools` ? `Object Dumper`

### Available Options

- **Enabled Formats**: Toggle which formats appear in the context menu
- **Dump Destination**: 
  - New Tab (opens in editor)
  - Clipboard (copies to clipboard)
  - Debug Console (prints to debug output)
- **Serialization Options**:
  - Max Depth (default: 10)
  - Max Collection Size (default: 100)
  - Operation Timeout (default: 30 seconds)
- **DateTime Options**: Custom format string and kind (UTC/Local/Unspecified)
- **Collection Layout**: Inline or one-per-line
- **Naming Options**: Convention and strategy customization

## Requirements

- JetBrains Rider 2023.3 or later
- .NET Framework 4.5+ / .NET Core 2.0+ / .NET Standard 2.0+
- C#, F#, or Visual Basic projects

## Known Limitations

- **Supported Languages**: C#, F#, and Visual Basic only
- **Debug Mode**: Works best in Debug mode. Release mode may cause "optimized code" errors
- **Local Debugging**: Remote debugging is not currently supported
- **Target Frameworks**: .NET Framework 4.5+, .NET Core 2.0+, .NET Standard 2.0+

## Building from Source

Prerequisites:
- JDK 17 or later
- Gradle 8.5 (included via wrapper)

Build steps:
```bash
cd ObjectDumper.Rider
./gradlew buildPlugin
```

Run in sandbox:
```bash
./gradlew runIde
```

## Architecture

The Rider plugin integrates with the existing Object Dumper infrastructure:

1. **Plugin Layer** (Kotlin/Java): Provides the Rider UI integration
2. **Debugger Integration**: Interacts with Rider's debugger API to evaluate expressions
3. **Serialization Library** (Shared): Uses the same `YellowFlavor.Serialization.dll` as the Visual Studio extension
4. **Output Handlers**: Manages where the dumped output is sent

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Powered By

The same powerful serialization libraries used by the Visual Studio extension:

| Library  | Purpose | License |
| ------------- | ------------- | ------------- |
| [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) | JSON and XML serialization | [![MIT](https://img.shields.io/github/license/JamesNK/Newtonsoft.Json?style=flat-square)](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)  |
| [VarDump](https://github.com/ycherkes/VarDump)  | C# and VB serialization | [![Apache-2.0](https://img.shields.io/github/license/ycherkes/vardump?style=flat-square)](https://github.com/ycherkes/VarDump/blob/main/LICENSE)  |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet) | YAML serialization | [![MIT](https://img.shields.io/github/license/aaubry/YamlDotNet?style=flat-square)](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt)  |
| [ILRepack](https://github.com/gluck/il-repack) | Assembly merging | [![Apache-2.0](https://img.shields.io/github/license/gluck/il-repack?style=flat-square)](https://github.com/gluck/il-repack/blob/master/LICENSE)  |

## License

This project is licensed under the MIT License - see the [LICENSE.txt](../LICENSE.txt) file for details.

## Support

- ?? Report issues on [GitHub Issues](https://github.com/ycherkes/ObjectDumper/issues)
- ? Star the project on [GitHub](https://github.com/ycherkes/ObjectDumper)
- ?? Sponsor on [GitHub Sponsors](https://github.com/sponsors/ycherkes) or [PayPal](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ)

**Privacy Notice:** No personal data is collected at all.

A big thank you to [Yova Solutions](https://www.yovasolutions.com) for sponsoring this work!
