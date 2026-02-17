# Quick Start Guide - Object Dumper for Rider

Welcome! This guide will help you get started with the Object Dumper Rider plugin in just a few minutes.

## Installation

### Option 1: From JetBrains Marketplace (Recommended - Coming Soon)

1. Open JetBrains Rider
2. Press `Ctrl+Alt+S` (Windows/Linux) or `Cmd+,` (macOS) to open Settings
3. Navigate to **Plugins**
4. Click on the **Marketplace** tab
5. Search for **"Object Dumper"**
6. Click **Install**
7. Click **OK** and restart Rider when prompted

### Option 2: From ZIP File

1. Download the latest release from [GitHub Releases](https://github.com/ycherkes/ObjectDumper/releases)
2. Open Rider
3. Press `Ctrl+Alt+S` (Windows/Linux) or `Cmd+,` (macOS) to open Settings
4. Navigate to **Plugins**
5. Click the gear icon (?) ? **Install Plugin from Disk...**
6. Select the downloaded ZIP file
7. Click **OK** and restart Rider when prompted

## First Use

### 1. Start Debugging

1. Open your .NET project in Rider
2. Set a breakpoint by clicking in the left gutter next to a line of code
3. Start debugging by pressing `Shift+F9` or clicking the Debug button

### 2. Dump an Object

Once the debugger hits your breakpoint:

1. Look at the **Debug** tool window (usually at the bottom of Rider)
2. Find the **Variables** tab
3. Right-click on any variable you want to inspect
4. Select **Dump As** ? choose your format:
   - **C# Object Initialization** - Get C# code to recreate the object
   - **JSON** - Get JSON representation
   - **XML** - Get XML representation  
   - **Visual Basic Object Initialization** - Get VB code
   - **YAML** - Get YAML representation

### 3. View the Output

Depending on your settings, the dumped object will appear in:

- **New Editor Tab** (default) - Opens as a temporary file
- **Clipboard** - Copied and ready to paste
- **Debug Console** - Printed in the debug output

## Examples

### Example 1: Dump a Simple Object

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

// In your code:
var person = new Person 
{ 
    FirstName = "John", 
    LastName = "Doe", 
    Age = 30 
};
// Set breakpoint here ? right-click 'person' ? Dump As ? C#
```

**Result (C#):**
```csharp
new Person
{
    FirstName = "John",
    LastName = "Doe",
    Age = 30
}
```

**Result (JSON):**
```json
{
  "FirstName": "John",
  "LastName": "Doe",
  "Age": 30
}
```

### Example 2: Dump a Collection

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };
// Set breakpoint ? right-click 'numbers' ? Dump As ? JSON
```

**Result (JSON):**
```json
[1, 2, 3, 4, 5]
```

### Example 3: Dump Complex Objects

```csharp
public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
    public List<OrderItem> Items { get; set; }
}

var order = GetOrder(); // Some complex order object
// Set breakpoint ? right-click 'order' ? Dump As ? C#
```

The plugin will serialize the entire object graph including nested objects!

## Configuration

### Access Settings

1. Press `Ctrl+Alt+S` (Windows/Linux) or `Cmd+,` (macOS)
2. Navigate to **Tools** ? **Object Dumper**

### Common Settings

#### Change Output Destination

- **New Tab** - Opens dumped content in a new editor tab (default)
- **Clipboard** - Copies to clipboard for easy pasting
- **Debug Console** - Prints to debug output

#### Adjust Depth and Collection Limits

- **Max Depth** - How deep to serialize nested objects (default: 10)
- **Max Collection Size** - Maximum items in arrays/lists (default: 100)

#### Customize DateTime Format

- **DateTime Format** - Use standard .NET format strings (e.g., "yyyy-MM-dd HH:mm:ss")
- **DateTime Kind** - Choose UTC, Local, or Unspecified

#### Enable/Disable Formats

Toggle which formats appear in the "Dump As" menu:
- ? C# Enabled
- ? JSON Enabled
- ? XML Enabled
- ? Visual Basic Enabled
- ? YAML Enabled

## Tips & Tricks

### Tip 1: Quick Compare Objects
1. Dump two objects to new tabs
2. Select both tabs in the editor
3. Right-click ? **Compare Files** (or `Ctrl+D`)
4. See differences side-by-side!

### Tip 2: Copy Large Objects
If an object is too large to view comfortably:
1. Change **Dump Destination** to **Clipboard**
2. Dump the object
3. Paste into your favorite text editor

### Tip 3: Chain Serialization Formats
1. Dump as JSON
2. Copy the JSON
3. Use online tools to convert to other formats

### Tip 4: Customize for Your Team
Set up consistent formats:
- Choose naming conventions (PascalCase, camelCase, snake_case)
- Set collection layouts (inline or one-per-line)
- Configure DateTime formats to match your team's standards

## Troubleshooting

### "Cannot evaluate expression" Error

**Cause:** Debugging in Release mode with optimizations enabled.

**Solution:**
1. Switch your project to Debug mode, or
2. In Rider: **File** ? **Settings** ? **Build, Execution, Deployment** ? **Debugger** ? Enable **Suppress JIT optimization on module load**

### "Unsupported language" Error

**Cause:** Trying to dump from a language other than C#, F#, or VB.

**Solution:** Object Dumper currently only supports C#, F#, and Visual Basic projects.

### "Operation timed out" Error

**Cause:** Object is very large or has circular references.

**Solution:**
1. Go to Settings ? Tools ? Object Dumper
2. Reduce **Max Depth** and **Max Collection Size**
3. Increase **Operation Timeout**

### Dumps Show Placeholder Text

**Cause:** This is the initial release foundation - full debugger API integration is in progress.

**What's happening:** The current version establishes the plugin architecture and UI. Full expression evaluation will be added in upcoming releases.

**Current status:** You'll see placeholder text indicating where the actual dumped data will appear.

## What's Next?

- ? **Star the project** on [GitHub](https://github.com/ycherkes/ObjectDumper)
- ?? **Read the full documentation** in [README.md](README.md)
- ?? **Report issues** on [GitHub Issues](https://github.com/ycherkes/ObjectDumper/issues)
- ?? **Request features** or contribute ideas
- ? **Support development** via [GitHub Sponsors](https://github.com/sponsors/ycherkes) or [PayPal](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ)

## Getting Help

- **Documentation:** [README.md](README.md)
- **Developer Guide:** [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md)
- **Issues:** [GitHub Issues](https://github.com/ycherkes/ObjectDumper/issues)
- **Discussions:** [GitHub Discussions](https://github.com/ycherkes/ObjectDumper/discussions)

---

**Happy Debugging! ????**
