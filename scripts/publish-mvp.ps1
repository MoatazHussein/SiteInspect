param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$releaseName = 'mvp-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
$releasePath = Join-Path $repoRoot ('artifacts/' + $releaseName)

Push-Location $repoRoot
try {
    dotnet publish src/SiteInspect.Api/SiteInspect.Api.csproj -c Release -o $releasePath
    if ($LASTEXITCODE -ne 0) { throw 'Publishing failed.' }

    $requiredFiles = @(
        'SiteInspect.Api.dll', 'web.config', 'appsettings.Production.json',
        'wwwroot/index.html', 'wwwroot/ngsw.json', 'wwwroot/ngsw-worker.js',
        'wwwroot/manifest.webmanifest'
    )
    foreach ($relativeFile in $requiredFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $releasePath $relativeFile))) {
            throw "Published release is missing $relativeFile"
        }
    }
    if (Test-Path -LiteralPath (Join-Path $releasePath 'appsettings.Development.json')) {
        throw 'Development configuration must not be included in a public release.'
    }
    Write-Output "Release ready: $releasePath"
    Write-Output 'Configure server settings and initialize the database before starting IIS.'
}
finally {
    Pop-Location
}
