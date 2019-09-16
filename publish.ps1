param([string]$Version)

# PowerShell Script to automate publish and zipping
# Usage: .\publish [version]
# Example: .\publish v1.0.0

if($Version -match "[\d]+\.[\d]+\.[\d]+$")
{
    if(-not ($Version -match "^v"))
    {
        $Version = "v" + $Version
    }
} else
{
    Write-Output "Invalid Version Number. Example: v1.0.23"
    Exit
}

dotnet publish -c release -r win10-x64 --self-contained true
dotnet publish -c release -r osx-x64 --self-contained true
dotnet publish -c release -r linux-x64 --self-contained true

if (-not (test-path "${env:ProgramFiles}\7-Zip\7z.exe")) {throw "${env:ProgramFiles}\7-Zip\7z.exe needed"} 
set-alias sz "${env:ProgramFiles}\7-Zip\7z.exe"  

if(Test-Path $Version)
{
    mkdir $Version
}

##############
# Windows 10 #
##############
$Source = ".\bin\release\netcoreapp2.2\win10-x64"
$Target = ".\bin\release\netcoreapp2.2\" + $Version + "\GetIgnore-" + $Version + "-Windows.zip"

if (Test-Path $Target) 
{
  Remove-Item $Target
}
sz a -mx=9 $Target $Source


#############
#   MacOS   #
#############
$Source = ".\bin\release\netcoreapp2.2\osx-x64"
$Target = ".\bin\release\netcoreapp2.2\" + $Version + "\GetIgnore-" + $Version + "-MacOS.zip"

if (Test-Path $Target) 
{
  Remove-Item $Target
}
sz a -mx=9 $Target $Source


#############
#   Linux   #
#############
$Source = ".\bin\release\netcoreapp2.2\linux-x64"
$Target = ".\bin\release\netcoreapp2.2\" + $Version + "\GetIgnore-" + $Version + "-Linux-x64.tar.gz"

if (Test-Path $Target) 
{
  Remove-Item $Target
}
sz a -ttar archive.tar $Source
sz a -tgzip $Target archive.tar
Remove-Item archive.tar