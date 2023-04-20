# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

<img src="https://yevhencherkes.gallerycdn.vsassets.io/extensions/yevhencherkes/yellowflavorobjectdumper/0.0.0.64/1665328424655/Microsoft.VisualStudio.Services.Icons.Default" width="100" height="100" />

# Object Dumper

[![VS marketplace](https://img.shields.io/visual-studio-marketplace/v/YevhenCherkes.YellowFlavorObjectDumper.svg?label=VS%20marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)
[![VS installs](https://img.shields.io/visual-studio-marketplace/i/YevhenCherkes.YellowFlavorObjectDumper?label=VS%20installs&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)
[![VS Code marketplace](https://img.shields.io/visual-studio-marketplace/v/YevhenCherkes.object-dumper.svg?label=VS%20Code%20marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper)
[![VS Code installs](https://img.shields.io/visual-studio-marketplace/i/YevhenCherkes.object-dumper?label=VS%20Code%20installs&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.object-dumper)

[![License: MIT](https://img.shields.io/github/license/ycherkes/ObjectDumper?style=for-the-badge)](https://github.com/ycherkes/ObjectDumper/blob/main/LICENSE.txt)

A Visual Studio and [Visual Studio Code](https://github.com/ycherkes/ObjectDumper/blob/main/src/object-dumper-vscode/README.md) extension for exporting in-memory objects during debugging to **C# Object Initialization Code**, **JSON**, **Visual Basic Object Initialization Code**, **XML**, and **YAML** string.

Inspired by [ObjectExporter](https://github.com/OmarElabd/ObjectExporter).

Another alternative is [OzCode export](https://github.com/oz-code/OzCodeDemo/tree/master/OzCodeDemo/12.Export) functionality.

"**Dump as**" commands are available via context menu in the **Code** and **Immediate** windows.

The result is shown in a separate document window.

![Presentation](https://user-images.githubusercontent.com/13467759/175763360-6d714f96-8b90-48a9-bff0-8bceac4c2502.gif)

# Configurable:

![image](https://user-images.githubusercontent.com/13467759/180246198-dfbb4809-c2cb-4a3f-9ea4-f9c52c67c47a.png)

# Quick tips:
- How to compare two dumped objects:
  1. Enable option "Show Miscellaneous files in Solution Explorer":
  ![image](https://user-images.githubusercontent.com/13467759/173348566-e5768350-321a-4fbd-85fc-10e3a366a5ae.png)
  2. Install a diff extension - I verified the [Heku.VsDiff](https://marketplace.visualstudio.com/items?itemName=Heku.VsDiff2022)
  3. Select files in Solution Explorer under the Miscellaneous Files folder -> Right click -> Compare Selected Files:
  ![image](https://user-images.githubusercontent.com/13467759/173349566-518f89e1-9d21-4ab6-a4e1-da2dc86e3a78.png)


### Known restrictions:
- [C#, F# and VisualBasic](https://github.com/ycherkes/ObjectDumper/blob/main/src/ObjectDumper/DebuggeeInteraction/InteractionService.cs#L25-L30) project languages are currently supported only.
- netstandard 2.0+, netcore2.0+, netframework 4.5+
- if you are debugging the solution in **Release mode** or debuging DLLs from another source, such as a nuget package, you'll get an error message: "Cannot evaluate expression because the code of the current method is optimized" or "error CS0103: The name 'YellowFlavor' does not exist in the current context". Solution: switch to **Debug mode** or turn the [Tools > Options > Debugging > General > Suppress JIT optimization on module load](https://learn.microsoft.com/en-us/visualstudio/debugger/jit-optimization-and-debugging?view=vs-2022#the-suppress-jit-optimization-on-module-load-managed-only-option) option on.
- local debugging only.
- if you see any encoding-related issues, please select the option: **Tools > Options > Environment > Documents > Save documents as Unicode when data cannot be saved in codepage**.
- it doesn't work for UWP applications, because [UAP doesn't support Assembly.LoadFrom](https://github.com/dotnet/runtime/issues/7543). You can bypass this restriction by referencing the .nestandard20 version of [Serialization lib](https://github.com/ycherkes/ObjectDumper/tree/main/src/Serialization) and calling: ```_ = ObjectSerializer.Serialize(null, "cs");``` for loading the serializer into executing assembly. [Example](https://github.com/ycherkes/ObjectDumper/blob/main/samples/uwp/TestUwp/App.xaml.cs#L26)

**Privacy Notice:** No personal data is collected at all.

# Powered By

| Repository  | License |
| ------------- | ------------- |
| [ILRepack](https://github.com/gluck/il-repack)  | [![Apache-2.0](https://img.shields.io/github/license/gluck/il-repack?style=flat-square)](https://github.com/gluck/il-repack/blob/master/LICENSE)  |
| [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)  | [![MIT](https://img.shields.io/github/license/JamesNK/Newtonsoft.Json?style=flat-square)](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)  |
| [VarDump](https://github.com/ycherkes/VarDump)  | [![Apache-2.0](https://img.shields.io/github/license/ycherkes/vardump?style=flat-square)](https://github.com/ycherkes/VarDump/blob/main/LICENSE)  |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet)  | [![MIT](https://img.shields.io/github/license/aaubry/YamlDotNet?style=flat-square)](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt)  |


This tool has been working well for my own personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/ObjectDumper/issues).

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+Object+Dumper+become+better%21)

Any donations during this time will be directed to local charities at my own discretion.

## Acknowledgments
Thanks to [JetBrains](https://www.jetbrains.com) for providing an [OSS development license](https://www.jetbrains.com/community/opensource/#support) of [All Products Pack](https://www.jetbrains.com/all/).

[<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" height="50" alt="All Products Pack logo.">](https://www.jetbrains.com/all/)
