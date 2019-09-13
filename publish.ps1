param([string]$Version)

# PowerShell Script to automate publish and zipping
# Usage: .\publish [version]
# Example: .\publish 1.0.0

dotnet publish -c release -r win10-x64
dotnet publish -c release -r osx-x64
dotnet publish -c release -r linux-x64

if (-not (test-path "${env:ProgramFiles}\7-Zip\7z.exe")) {throw "${env:ProgramFiles}\7-Zip\7z.exe needed"} 
set-alias sz "${env:ProgramFiles}\7-Zip\7z.exe"  

$Source = ".\obj\release\netcoreapp2.2\win10-x64"
$Target = ".\obj\release\netcoreapp2.2\GetIgnore-v" + $Version + "-Windows.zip"

sz a -mx=9 $Target $Source

$Source = ".\obj\release\netcoreapp2.2\osx-x64"
$Target = ".\obj\release\netcoreapp2.2\GetIgnore-v" + $Version + "-MacOS.zip"

sz a -mx=9 $Target $Source

$Source = ".\obj\release\netcoreapp2.2\linux-x64"
$Target = ".\obj\release\netcoreapp2.2\GetIgnore-v" + $Version + "-Linux-x64.tar.gz"

sz a -mx=9 -ttar -so archive.tar $Source | sz a -si -tgzip $Target