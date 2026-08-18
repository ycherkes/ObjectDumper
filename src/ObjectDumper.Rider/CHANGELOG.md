# Changelog

All notable changes to the Object Dumper Rider plugin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.3] - 2026-08-18

### Changed
- Updated the bundled VarDump library to 2.0.8.0.
- Improved enum, queryable, default-value, and circular-reference serialization.
- Reduced allocations while writing collections and object graphs.
- Added type naming, newline, and inherited-field options for C# and Visual Basic.
- Added C# string-literal and collection-literal style options.

## [0.0.1] - 2024-01-XX

### Added
- Initial release of Object Dumper for JetBrains Rider
- Support for dumping objects as C# Object Initialization Code
- Support for dumping objects as JSON
- Support for dumping objects as XML
- Support for dumping objects as Visual Basic Object Initialization Code
- Support for dumping objects as YAML
- Configurable output destinations (New Tab, Clipboard, Debug Console)
- Comprehensive settings panel with serialization options
- DateTime formatting options
- Collection layout options
- Naming convention options
- Integration with Rider's debug tool window
- Support for .NET Framework 4.5+, .NET Core 2.0+, .NET Standard 2.0+
- Support for C#, F#, and Visual Basic projects

### Known Issues
- Full debugger API integration pending (current implementation is a foundation)
- Remote debugging not yet supported

[Unreleased]: https://github.com/ycherkes/ObjectDumper/compare/v0.0.3...HEAD
[0.0.3]: https://github.com/ycherkes/ObjectDumper/compare/v0.0.1...v0.0.3
[0.0.1]: https://github.com/ycherkes/ObjectDumper/releases/tag/v0.0.1
