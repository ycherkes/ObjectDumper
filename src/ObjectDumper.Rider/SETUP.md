# Setup Script for Object Dumper Rider Plugin

## Description
This script copies the YellowFlavor.Serialization.dll files from the main ObjectDumper project to the Rider plugin resources directory.

## Prerequisites
- The main ObjectDumper project must be built in Release mode
- The serialization DLLs should be available in: `../ObjectDumper/bin/Release/`

## Usage

### PowerShell (Windows)
```powershell
.\setup.ps1
```

### Bash (Linux/macOS)
```bash
chmod +x setup.sh
./setup.sh
```

## What it does
1. Creates the `src/main/resources/InjectableLibs` directory structure
2. Copies YellowFlavor.Serialization.dll for each target framework:
   - net45
   - net6.0
   - netcoreapp2.0
   - netcoreapp3.1
   - netstandard2.0

## Manual Setup
If the script doesn't work, manually copy the DLL files:

1. Build the main Serialization project in Release mode
2. Create these directories:
   ```
   ObjectDumper.Rider/src/main/resources/InjectableLibs/net45/
   ObjectDumper.Rider/src/main/resources/InjectableLibs/net6.0/
   ObjectDumper.Rider/src/main/resources/InjectableLibs/netcoreapp2.0/
   ObjectDumper.Rider/src/main/resources/InjectableLibs/netcoreapp3.1/
   ObjectDumper.Rider/src/main/resources/InjectableLibs/netstandard2.0/
   ```

3. Copy the corresponding DLL from each build output:
   ```
   From: ObjectDumper/InjectableLibs/{framework}/YellowFlavor.Serialization.dll
   To:   ObjectDumper.Rider/src/main/resources/InjectableLibs/{framework}/YellowFlavor.Serialization.dll
   ```

## Verification
After running the setup, verify that these files exist:
- `src/main/resources/InjectableLibs/net45/YellowFlavor.Serialization.dll`
- `src/main/resources/InjectableLibs/net6.0/YellowFlavor.Serialization.dll`
- `src/main/resources/InjectableLibs/netcoreapp2.0/YellowFlavor.Serialization.dll`
- `src/main/resources/InjectableLibs/netcoreapp3.1/YellowFlavor.Serialization.dll`
- `src/main/resources/InjectableLibs/netstandard2.0/YellowFlavor.Serialization.dll`

## Next Steps
After setup is complete:
1. Build the plugin: `./gradlew buildPlugin`
2. Run in sandbox: `./gradlew runIde`
3. Test the plugin with a .NET project
