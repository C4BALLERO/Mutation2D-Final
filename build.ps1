# Mutation Swarm — build automático (cierra Unity, genera contenido y .exe)
$ErrorActionPreference = "Stop"
$unity = "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe"
$project = $PSScriptRoot

Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 4

New-Item -ItemType Directory -Force -Path "$project\Logs" | Out-Null

Write-Host "=== Generando contenido (Kenney + escenas) ==="
& $unity -batchmode -quit -nographics -projectPath $project `
    -executeMethod MutationSwarm.Editor.MutationSwarmBatchBuild.RunContentOnly `
    -logFile "$project\Logs\batch-content.log"

if ($LASTEXITCODE -ne 0) {
    Get-Content "$project\Logs\batch-content.log" -Tail 30
    throw "Fallo generación de contenido. Revisa Logs\batch-content.log"
}

Write-Host "=== Build Windows (puede tardar varios minutos) ==="
& $unity -batchmode -quit -nographics -projectPath $project `
    -executeMethod MutationSwarm.Editor.MutationSwarmBatchBuild.RunWindowsOnly `
    -logFile "$project\Logs\batch-windows.log"

if ($LASTEXITCODE -ne 0) {
    Get-Content "$project\Logs\batch-windows.log" -Tail 30
    throw "Fallo build Windows. Revisa Logs\batch-windows.log"
}

Write-Host "LISTO: $project\Builds\Windows\MutationSwarm.exe"
