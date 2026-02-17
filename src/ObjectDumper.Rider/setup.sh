#!/bin/bash

# Bash setup script for Object Dumper Rider Plugin

echo "Object Dumper Rider Plugin Setup"
echo "================================="
echo ""

# Define paths
RIDER_PLUGIN_ROOT="$(cd "$(dirname "$0")" && pwd)"
OBJECT_DUMPER_ROOT="$(dirname "$RIDER_PLUGIN_ROOT")"
SOURCE_LIBS_PATH="$OBJECT_DUMPER_ROOT/src/ObjectDumper/InjectableLibs"
TARGET_LIBS_PATH="$RIDER_PLUGIN_ROOT/src/main/resources/InjectableLibs"

# Check if source directory exists
if [ ! -d "$SOURCE_LIBS_PATH" ]; then
    echo "ERROR: Source libraries not found at: $SOURCE_LIBS_PATH"
    echo ""
    echo "Please build the main ObjectDumper project first:"
    echo "  1. Open ObjectDumper.sln in Visual Studio"
    echo "  2. Build the solution in Release mode"
    echo "  3. Run this setup script again"
    exit 1
fi

# Target frameworks
frameworks=("net45" "net6.0" "netcoreapp2.0" "netcoreapp3.1" "netstandard2.0")

echo "Creating directory structure..."

# Create target directories
for framework in "${frameworks[@]}"; do
    target_dir="$TARGET_LIBS_PATH/$framework"
    if [ ! -d "$target_dir" ]; then
        mkdir -p "$target_dir"
        echo "  Created: $target_dir"
    fi
done

echo ""
echo "Copying serialization libraries..."

# Copy DLLs
copied_count=0
failed_count=0

for framework in "${frameworks[@]}"; do
    source_file="$SOURCE_LIBS_PATH/$framework/YellowFlavor.Serialization.dll"
    target_file="$TARGET_LIBS_PATH/$framework/YellowFlavor.Serialization.dll"
    
    if [ -f "$source_file" ]; then
        cp "$source_file" "$target_file"
        echo "  ? Copied $framework/YellowFlavor.Serialization.dll"
        ((copied_count++))
    else
        echo "  ? Missing $framework/YellowFlavor.Serialization.dll"
        ((failed_count++))
    fi
done

echo ""
echo "================================="
echo "Setup Summary:"
echo "  Copied: $copied_count file(s)"
if [ $failed_count -gt 0 ]; then
    echo "  Failed: $failed_count file(s)"
fi

echo ""

if [ $failed_count -eq 0 ]; then
    echo "? Setup completed successfully!"
    echo ""
    echo "Next steps:"
    echo "  1. Build the plugin: ./gradlew buildPlugin"
    echo "  2. Run in sandbox: ./gradlew runIde"
else
    echo "? Setup completed with errors. Please check the missing files."
    echo "See SETUP.md for manual setup instructions."
fi

echo ""
