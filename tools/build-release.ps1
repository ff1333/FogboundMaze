$ErrorActionPreference = "Stop"

$unity = "E:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe"
$project = Split-Path -Parent $PSScriptRoot
$logs = Join-Path $project "Builds"

function Invoke-UnityBuild([string]$method, [string]$logName, [string]$successPattern) {
    $log = Join-Path $logs $logName
    $process = Start-Process -FilePath $unity -ArgumentList @(
        "-batchmode",
        "-nographics",
        "-projectPath", $project,
        "-executeMethod", $method,
        "-quit",
        "-logFile", $log
    ) -PassThru -WindowStyle Hidden
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        Get-Content $log -Tail 160
        throw "$method failed with exit code $($process.ExitCode)."
    }

    $summary = Select-String -Path $log -Pattern $successPattern | Select-Object -Last 1
    if ($null -eq $summary) {
        throw "$method completed without a build summary."
    }
    $summary.Line
}

Invoke-UnityBuild "FogboundProjectBuilder.BuildAndValidate" "release-prepare.log" "FOGBOUND_VALIDATION_PASS"
Invoke-UnityBuild "FogboundPlatformBuilder.BuildWindowsRelease" "release-windows.log" "FOGBOUND_PLATFORM_BUILD"
Invoke-UnityBuild "FogboundPlatformBuilder.BuildWebGLRelease" "release-webgl.log" "FOGBOUND_PLATFORM_BUILD"
Invoke-UnityBuild "FogboundPlatformBuilder.BuildAndroidRelease" "release-android.log" "FOGBOUND_PLATFORM_BUILD"
