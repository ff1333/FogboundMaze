$ErrorActionPreference = "Stop"
$unity = "E:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe"
$project = Split-Path -Parent $PSScriptRoot
$log = Join-Path $project "Builds\windows-development.log"

$process = Start-Process -FilePath $unity -ArgumentList @(
    "-batchmode",
    "-nographics",
    "-projectPath", $project,
    "-executeMethod", "FogboundPlatformBuilder.BuildWindowsDevelopment",
    "-quit",
    "-logFile", $log
) -Wait -PassThru -WindowStyle Hidden

if ($process.ExitCode -ne 0) {
    Get-Content $log -Tail 120
    throw "Windows development build failed with exit code $($process.ExitCode)."
}

Select-String -Path $log -Pattern "FOGBOUND_PLATFORM_BUILD"
