# Object Dumper Rider Plugin - Developer Guide

## Project Structure

```
ObjectDumper.Rider/
??? src/
?   ??? main/
?       ??? kotlin/
?       ?   ??? com/yellowflavor/objectdumper/
?       ?       ??? actions/
?       ?       ?   ??? DumpAsAction.kt              # Base action class
?       ?       ?   ??? DumpAsCSharpAction.kt        # C# dump action
?       ?       ?   ??? DumpAsJsonAction.kt          # JSON dump action
?       ?       ?   ??? DumpAsXmlAction.kt           # XML dump action
?       ?       ?   ??? DumpAsVisualBasicAction.kt   # VB dump action
?       ?       ?   ??? DumpAsYamlAction.kt          # YAML dump action
?       ?       ??? debugger/
?       ?       ?   ??? DebuggerInteractionService.kt # Debugger API integration
?       ?       ??? output/
?       ?       ?   ??? OutputHandler.kt             # Output destination handling
?       ?       ??? settings/
?       ?           ??? ObjectDumperSettings.kt      # Persistent settings
?       ?           ??? ObjectDumperConfigurable.kt  # Settings UI
?       ??? resources/
?           ??? META-INF/
?           ?   ??? plugin.xml                       # Plugin descriptor
?           ?   ??? notifications.xml                # Notification groups
?           ??? InjectableLibs/                      # Serialization libraries (copied from main project)
?               ??? net45/
?               ?   ??? YellowFlavor.Serialization.dll
?               ??? net6.0/
?               ?   ??? YellowFlavor.Serialization.dll
?               ??? netcoreapp2.0/
?               ?   ??? YellowFlavor.Serialization.dll
?               ??? netcoreapp3.1/
?               ?   ??? YellowFlavor.Serialization.dll
?               ??? netstandard2.0/
?                   ??? YellowFlavor.Serialization.dll
??? build.gradle.kts                                 # Gradle build configuration
??? settings.gradle.kts                              # Gradle settings
??? gradle.properties                                # Gradle properties
??? CHANGELOG.md                                     # Version history
??? README.md                                        # Plugin documentation
```

## Development Setup

### Prerequisites

1. **JDK 17 or later**
   - Download from [Adoptium](https://adoptium.net/) or [Oracle](https://www.oracle.com/java/technologies/downloads/)
   - Verify: `java -version`

2. **JetBrains Rider 2023.3+**
   - Download from [JetBrains website](https://www.jetbrains.com/rider/)

3. **IntelliJ IDEA** (recommended for plugin development)
   - Download Community or Ultimate edition
   - Install the following plugins:
     - Kotlin
     - Gradle

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/ycherkes/ObjectDumper.git
   cd ObjectDumper/ObjectDumper.Rider
   ```

2. **Build the plugin**
   ```bash
   ./gradlew buildPlugin
   ```
   On Windows:
   ```cmd
   gradlew.bat buildPlugin
   ```

3. **Run in Rider sandbox**
   ```bash
   ./gradlew runIde
   ```
   This will launch a sandboxed Rider instance with the plugin installed.

4. **Test the plugin**
   - Open a .NET project in the sandbox Rider instance
   - Start debugging
   - Set a breakpoint
   - Right-click on a variable in the Debug tool window
   - Select "Dump As" ? choose a format

## Building for Distribution

### Create Plugin ZIP

```bash
./gradlew buildPlugin
```

The plugin ZIP will be created in: `build/distributions/ObjectDumper.Rider-{version}.zip`

### Install Locally

1. Build the plugin (see above)
2. In Rider, go to `File` ? `Settings` ? `Plugins`
3. Click the gear icon ? `Install Plugin from Disk...`
4. Select the ZIP file from `build/distributions/`
5. Restart Rider

## Publishing to JetBrains Marketplace

### Prerequisites

1. Create a JetBrains account at [JetBrains Hub](https://account.jetbrains.com/)
2. Get a permanent token from [JetBrains Hub](https://plugins.jetbrains.com/author/me/tokens)
3. Set the token as an environment variable:
   ```bash
   export PUBLISH_TOKEN=your-token-here
   ```

### Publish

```bash
./gradlew publishPlugin
```

## Key Integration Points

### 1. Debugger Integration

The `DebuggerInteractionService` class handles interaction with Rider's debugger:

- Getting the current debug session
- Evaluating expressions in the debugged process
- Injecting the serialization library
- Retrieving serialized object data

**TODO**: Full implementation requires deep integration with Rider's debugger API, specifically:
- `com.jetbrains.rider.debugger` package
- Expression evaluation APIs
- Assembly loading in the debugged process

### 2. Action Registration

Actions are registered in `plugin.xml`:

```xml
<action id="ObjectDumper.DumpAsCSharp"
        class="com.yellowflavor.objectdumper.actions.DumpAsCSharpAction"
        text="C# Object Initialization"
        description="Dump object as C# initialization code">
</action>
```

They appear in the Debug tool window's context menu via:
```xml
<add-to-group group-id="XDebugger.ValueGroup" anchor="last"/>
```

### 3. Settings Persistence

Settings are persisted using IntelliJ Platform's `PersistentStateComponent`:

```kotlin
@State(
    name = "ObjectDumperSettings",
    storages = [Storage("ObjectDumperSettings.xml")]
)
class ObjectDumperSettings : PersistentStateComponent<ObjectDumperSettings>
```

Settings are stored in: `~/.config/JetBrains/Rider{version}/options/ObjectDumperSettings.xml`

### 4. Output Handling

The `OutputHandler` supports three destinations:

1. **New Tab**: Creates a light virtual file in the editor
2. **Clipboard**: Uses `CopyPasteManager`
3. **Debug Console**: Prints to the debug session's console view

## Testing

### Manual Testing

1. Run the plugin in sandbox: `./gradlew runIde`
2. Open a test .NET project
3. Start debugging
4. Test each dump format
5. Verify output destinations
6. Check settings persistence

### Automated Testing

TODO: Implement automated tests using IntelliJ Platform Test Framework

```kotlin
class ObjectDumperTest : BasePlatformTestCase() {
    fun testDumpAction() {
        // Test implementation
    }
}
```

## Troubleshooting

### Common Issues

1. **Gradle build fails**
   - Ensure Java 17+ is installed and in PATH
   - Run `./gradlew clean` and try again

2. **Plugin doesn't appear in Rider**
   - Check that the plugin.xml is valid
   - Ensure the plugin is compatible with your Rider version
   - Check the `sinceBuild` and `untilBuild` values

3. **Debugger integration doesn't work**
   - Verify you're debugging a supported .NET project
   - Check that the serialization DLLs are included in the plugin
   - Review the Rider logs: `Help` ? `Show Log in Explorer/Finder`

## Resources

- [IntelliJ Platform SDK Documentation](https://plugins.jetbrains.com/docs/intellij/)
- [Rider Plugin Development](https://plugins.jetbrains.com/docs/intellij/rider.html)
- [Kotlin for Plugin Developers](https://plugins.jetbrains.com/docs/intellij/kotlin.html)
- [IntelliJ Platform UI Guidelines](https://plugins.jetbrains.com/docs/intellij/ui-guidelines.html)

## Contributing

See the main [CONTRIBUTING.md](../CONTRIBUTING.md) file for contribution guidelines.

## License

MIT License - see [LICENSE.txt](../LICENSE.txt) for details.
