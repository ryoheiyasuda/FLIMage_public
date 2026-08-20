param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [ValidateSet("x64")]
    [string]$Platform = "x64",

    [ValidateSet("Auto", "2022", "18")]
    [string]$VisualStudioVersion = "Auto",

    [string]$MSBuildPath
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionPath = Join-Path $repoRoot "FLIMage\FLIMageWin10.sln"

if (-not (Test-Path $solutionPath)) {
    throw "Solution not found: $solutionPath"
}

if ($MSBuildPath) {
    if (-not (Test-Path $MSBuildPath)) {
        throw "MSBuild.exe was not found at: $MSBuildPath"
    }
}
else {
    $candidateSets = @{
        "2022" = @(
            "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
            "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
        )
        "18" = @(
            "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe",
            "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "C:\Program Files\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
        )
    }

    $searchOrder = if ($VisualStudioVersion -eq "Auto") {
        @("2022", "18")
    }
    else {
        @($VisualStudioVersion)
    }

    $msbuildCandidates = foreach ($version in $searchOrder) {
        $candidateSets[$version]
    }

    $MSBuildPath = $msbuildCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not $MSBuildPath) {
    throw "MSBuild.exe was not found in the requested Visual Studio locations."
}

Write-Host "Using MSBuild: $MSBuildPath"
Write-Host "Building: $solutionPath"
Write-Host "Configuration: $Configuration"
Write-Host "Platform: $Platform"

& $MSBuildPath $solutionPath /t:Build "/p:Configuration=$Configuration" "/p:Platform=$Platform"

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
