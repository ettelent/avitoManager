$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    & (Join-Path $PSScriptRoot 'Setup.ps1')
}
Push-Location $projectRoot
try { dotnet run --project .\src\AccountManager.Desktop -c Debug }
finally { Pop-Location }
