# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

# Object Dumper

[![marketplace](https://img.shields.io/visual-studio-marketplace/v/YevhenCherkes.YellowFlavorObjectDumper.svg?label=Marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)
[![downloads](https://img.shields.io/visual-studio-marketplace/d/YevhenCherkes.YellowFlavorObjectDumper?label=Downloads&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)


A Visual Studio 2022 extension. Dumps the objects from the debugging session in the next formats: C# Object Initialization Code, JSON, Visual Basic Object Initialization Code, XML and YAML.

It opens the dumped object in a separate document window.

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
- the debugging code mustn't be optimized so the Expression Evaluator can be run.
- local debugging only.
- it doesn't work for UWP applications, because [UAP doesn't support Assembly.LoadFrom](https://github.com/dotnet/runtime/issues/7543). You can bypass this restriction by referencing the .nestandard20 version of [Serialization lib](https://github.com/ycherkes/ObjectDumper/tree/main/src/Serialization) and calling: ```_ = ObjectSerializer.Serialize(null, "cs");``` for loading the serializer into executing assembly. [Example](https://github.com/ycherkes/ObjectDumper/blob/main/samples/uwp/TestUwp/App.xaml.cs#L26)

**Privacy Notice:** No personal data is collected at all.

# Powered By

* [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom)
* [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
* [YamlDotNet](https://github.com/aaubry/YamlDotNet)

This tool has been working well for my own personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/ObjectDumper/issues).

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+Object+Dumper+become+better%21)

## Acknowledgments
Thanks to [JetBrains](https://www.jetbrains.com) for providing an [OSS development license](https://www.jetbrains.com/community/opensource/#support) of [All Products Pack](https://www.jetbrains.com/all/).

[<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" height="50" alt="All Products Pack logo.">](https://www.jetbrains.com/all/)
