# PowerShell setup script for Object Dumper Rider Plugin

Write-Host "Object Dumper Rider Plugin Setup" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Define paths
$riderPluginRoot = $PSScriptRoot
$objectDumperRoot = Split-Path -Parent $riderPluginRoot
$sourceLibsPath = Join-Path $objectDumperRoot "src\ObjectDumper\InjectableLibs"
$targetLibsPath = Join-Path $riderPluginRoot "src\main\resources\InjectableLibs"

# Check if source directory exists
if (-not (Test-Path $sourceLibsPath)) {
    Write-Host "ERROR: Source libraries not found at: $sourceLibsPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please build the main ObjectDumper project first:" -ForegroundColor Yellow
    Write-Host "  1. Open ObjectDumper.sln in Visual Studio" -ForegroundColor Yellow
    Write-Host "  2. Build the solution in Release mode" -ForegroundColor Yellow
    Write-Host "  3. Run this setup script again" -ForegroundColor Yellow
    exit 1
}

# Target frameworks
$frameworks = @("net45", "net6.0", "netcoreapp2.0", "netcoreapp3.1", "netstandard2.0")

Write-Host "Creating directory structure..." -ForegroundColor Green

# Create target directories
foreach ($framework in $frameworks) {
    $targetDir = Join-Path $targetLibsPath $framework
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        Write-Host "  Created: $targetDir" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Copying serialization libraries..." -ForegroundColor Green

# Copy DLLs
$copiedCount = 0
$failedCount = 0

foreach ($framework in $frameworks) {
    $sourceFile = Join-Path $sourceLibsPath "$framework\YellowFlavor.Serialization.dll"
    $targetFile = Join-Path $targetLibsPath "$framework\YellowFlavor.Serialization.dll"
    
    if (Test-Path $sourceFile) {
        Copy-Item -Path $sourceFile -Destination $targetFile -Force
        Write-Host "  ? Copied $framework\YellowFlavor.Serialization.dll" -ForegroundColor Green
        $copiedCount++
    } else {
        Write-Host "  ? Missing $framework\YellowFlavor.Serialization.dll" -ForegroundColor Red
        $failedCount++
    }
}

Write-Host ""
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Setup Summary:" -ForegroundColor Cyan
Write-Host "  Copied: $copiedCount file(s)" -ForegroundColor Green
if ($failedCount -gt 0) {
    Write-Host "  Failed: $failedCount file(s)" -ForegroundColor Red
}

Write-Host ""

if ($failedCount -eq 0) {
    Write-Host "? Setup completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Build the plugin: .\gradlew.bat buildPlugin" -ForegroundColor Yellow
    Write-Host "  2. Run in sandbox: .\gradlew.bat runIde" -ForegroundColor Yellow
} else {
    Write-Host "? Setup completed with errors. Please check the missing files." -ForegroundColor Yellow
    Write-Host "See SETUP.md for manual setup instructions." -ForegroundColor Yellow
}

Write-Host ""
