# [Stand with the people of Ukraine: How to Help](https://dou.ua/lenta/articles/stand-with-ukraine/?hl=en)

# Object Dumper

[![marketplace](https://img.shields.io/visual-studio-marketplace/v/YevhenCherkes.YellowFlavorObjectDumper.svg?label=Marketplace&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)
[![installs](https://img.shields.io/visual-studio-marketplace/i/YevhenCherkes.YellowFlavorObjectDumper?label=Installs&style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=YevhenCherkes.YellowFlavorObjectDumper)


A Visual Studio 2022 extension. Dumps the objects from the debugging session in the next formats: CSharp, VB, JSON, YAML, and XML.

It opens the dumped object in a separate document window.

![HowTo3](https://user-images.githubusercontent.com/13467759/169960728-59afc54c-6458-49c9-adbb-043458240e9d.gif)

# Configurable:

![image](https://user-images.githubusercontent.com/13467759/173329406-50060b8a-4a67-4cfd-802f-8d8ed52b5cdf.png)

# Quick tips:
- How to compare two dumped objects:
  1. Enable option "Show Miscellaneous files in Solution Explorer":
  ![image](https://user-images.githubusercontent.com/13467759/173348566-e5768350-321a-4fbd-85fc-10e3a366a5ae.png)
  2. Install a diff extension - I verified the [Heku.VsDiff](https://marketplace.visualstudio.com/items?itemName=Heku.VsDiff)
  3. Select files in Solution Explorer under the Miscellaneous Files folder -> Right click -> Compare Selected Files:
  ![image](https://user-images.githubusercontent.com/13467759/173349566-518f89e1-9d21-4ab6-a4e1-da2dc86e3a78.png)


### Known restrictions: 
- the debugging code mustn't be optimized so the Expression Evaluator can be run.
- local debugging only.

# Powered By

* [System.CodeDom](https://github.com/kri-kan/Microsoft_projects/tree/master/src/System.CodeDom)
* [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
* [YamlDotNet](https://github.com/aaubry/YamlDotNet)

## Donate project

| Coin           | Address |
| -------------  |:-------------:|
| ETH            | [0xa3144c572eAB14bB404399fFE06757d5483ebD94](https://www.blockchain.com/en/eth/address/0xa3144c572eAB14bB404399fFE06757d5483ebD94) |
