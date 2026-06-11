param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "TankBattleOnline.csproj"
$dist = Join-Path $root "dist"
$packageRoot = Join-Path $root "obj\package"
$stage = Join-Path $packageRoot ("TankBattleOnline-Release-" + (Get-Date -Format "yyyyMMddHHmmss"))
$releaseFolder = Join-Path $dist "TankBattleOnline-Release"
$zip = Join-Path $dist "TankBattleOnline-Release.zip"
$output = Join-Path $root "bin\$Configuration"

dotnet build $project -c $Configuration

New-Item -ItemType Directory -Force $stage | Out-Null
Copy-Item (Join-Path $output "TankBattleOnline.exe") $stage -Force

$configFile = Join-Path $output "TankBattleOnline.exe.config"

if (Test-Path $configFile) {
    Copy-Item $configFile $stage -Force
}

Copy-Item (Join-Path $root "README.md") $stage -Force
Copy-Item (Join-Path $root "assets\LICENSE") (Join-Path $stage "ASSETS-LICENSE.txt") -Force

if (Test-Path $zip) {
    Remove-Item $zip -Force
}

Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zip

try {
    if (Test-Path $releaseFolder) {
        Remove-Item $releaseFolder -Recurse -Force
    }

    Copy-Item $stage $releaseFolder -Recurse -Force
}
catch {
    Write-Warning "Release folder was not refreshed, probably because an old exe is running. Zip package is still up to date."
}

Write-Host "Release package created: $zip"
