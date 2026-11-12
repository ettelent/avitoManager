$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$localDotnet = Join-Path $env:LOCALAPPDATA 'AccountManager\dotnet'
$env:Path = "$localDotnet;$env:Path"

function Get-DotnetCommand {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    $candidate = Join-Path $localDotnet 'dotnet.exe'
    if (Test-Path -LiteralPath $candidate) { return $candidate }
    return $null
}

$dotnet = Get-DotnetCommand
if (-not $dotnet) {
    Write-Host '.NET SDK не найден. Устанавливаю локальную копию .NET 10 SDK...'
    New-Item -ItemType Directory -Path $localDotnet -Force | Out-Null
    $installer = Join-Path $env:TEMP 'dotnet-install-account-manager.ps1'
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
    powershell.exe -ExecutionPolicy Bypass -File $installer -Channel 10.0 -InstallDir $localDotnet
    $dotnet = Join-Path $localDotnet 'dotnet.exe'
}

Push-Location $projectRoot
try {
    & $dotnet restore AccountManager.sln
    & $dotnet build AccountManager.sln -c Debug --nologo
    $playwright = Join-Path $projectRoot 'src\AccountManager.Agent\bin\Debug\net10.0\playwright.ps1'
    if (-not (Test-Path -LiteralPath $playwright)) { throw "Не найден скрипт Playwright: $playwright" }
    powershell.exe -ExecutionPolicy Bypass -File $playwright install chromium
    Write-Host 'Установка завершена.' -ForegroundColor Green
}
finally { Pop-Location }
