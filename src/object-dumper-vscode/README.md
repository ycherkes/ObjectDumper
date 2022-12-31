# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

<img src="https://yevhencherkes.gallerycdn.vsassets.io/extensions/yevhencherkes/yellowflavorobjectdumper/0.0.0.64/1665328424655/Microsoft.VisualStudio.Services.Icons.Default" width="100" height="100" />

# Object Dumper

[![marketplace](https://img.shields.io/visual-studio-marketplace/v/YevhenCherkes.object-dumper.svg?label=Marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper)
[![installs](https://img.shields.io/visual-studio-marketplace/i/YevhenCherkes.object-dumper?label=Installs&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper)
[![License: MIT](https://img.shields.io/github/license/ycherkes/ObjectDumper?style=for-the-badge)](https://github.com/ycherkes/ObjectDumper/blob/main/LICENSE.txt)

A Visual Studio Code extension for exporting in-memory objects during debugging to the **C# Object Initialization Code**, **JSON**, **Visual Basic Object Initialization Code**, **XML**, and **YAML** format.

Inspired by [ObjectExporter](https://github.com/OmarElabd/ObjectExporter).

"**Dump as**" commands are available via context menu in the **Code** window only.

The result is shown in a separate document window.

![Presentation](https://user-images.githubusercontent.com/13467759/201370888-c8aa6d18-a732-4538-8466-3251665bbfe6.gif)

# Configurable:
![image](https://user-images.githubusercontent.com/13467759/201370963-716fbb73-ed7a-47e1-a105-9ac9ee79a287.PNG)

### Known restrictions:
- C# and F# project languages are currently supported only.
- netstandard 2.0+, netcore3.1+, netframework is not supported yet.
- the debugging code mustn't be optimized so the Expression Evaluator can be run.
- local debugging only.
- it doesn't work for UWP applications, because [UAP doesn't support Assembly.LoadFrom](https://github.com/dotnet/runtime/issues/7543). You can bypass this restriction by referencing the .nestandard20 version of [Serialization lib](https://github.com/ycherkes/ObjectDumper/tree/main/src/Serialization) and calling: ```_ = ObjectSerializer.Serialize(null, "cs");``` for loading the serializer into executing assembly. [Example](https://github.com/ycherkes/ObjectDumper/blob/main/samples/uwp/TestUwp/App.xaml.cs#L26)

**Privacy Notice:** No personal data is collected at all.

# Powered By

| Repository  | License |
| ------------- | ------------- |
| [A heavily customized version](https://github.com/ycherkes/ObjectDumper/tree/main/src/Serialization/Embedded/CodeDom/ms) of [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom)  | [![MIT](https://img.shields.io/github/license/dotnet/runtime?style=flat-square)](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)  |
| [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)  | [![MIT](https://img.shields.io/github/license/JamesNK/Newtonsoft.Json?style=flat-square)](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)  |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet)  | [![MIT](https://img.shields.io/github/license/aaubry/YamlDotNet?style=flat-square)](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt)  |

This tool has been working well for my own personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/ObjectDumper/issues).

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+Object+Dumper+become+better%21)

Any donations during this time will be directed to local charities at my own discretion.

## Acknowledgments
Thanks to [JetBrains](https://www.jetbrains.com) for providing an [OSS development license](https://www.jetbrains.com/community/opensource/#support) of [All Products Pack](https://www.jetbrains.com/all/).

[<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" height="50" alt="All Products Pack logo.">](https://www.jetbrains.com/all/)
