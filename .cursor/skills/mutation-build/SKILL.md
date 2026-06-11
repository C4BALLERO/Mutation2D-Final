---
name: mutation-build
description: Genera contenido Kenney/geométrico y build Windows de Mutation Swarm. Usar cuando pidan "haz el build", exe, o compilar el juego.
---

# Build — Mutation Swarm

## Requisitos

- Unity Hub: **6000.3.8f1** (ruta típica `C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe`)
- Cerrar Unity Editor antes del batch (evita lockfile)

## Comando recomendado

Desde la raíz del repo:

```powershell
.\build.ps1
```

Pasos internos:

1. `MutationSwarmBatchBuild.RunContentOnly` — prefabs geo, menú Kenney, `Scene_02`
2. `MutationSwarmBatchBuild.RunWindowsOnly` — `Builds/Windows/MutationSwarm.exe`

## Solo contenido (sin exe)

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe" -batchmode -quit -nographics `
  -projectPath "C:\Game\Mutation2D" `
  -executeMethod MutationSwarm.Editor.MutationSwarmBatchBuild.RunContentOnly `
  -logFile "Logs\batch-content.log"
```

## Verificación

- `Test-Path Builds\Windows\MutationSwarm.exe`
- Log: `[BuildPipeline] Build OK` en `Logs\batch-windows.log`

## Errores frecuentes

- **UnityLockfile**: cerrar Editor
- **Scripts en Editor/**: componentes runtime deben estar fuera de `Assets/_Scripts/Editor/`
- **Compile errors**: revisar últimas 40 líneas de `Logs/batch-content.log`

Si el build falla, arreglar compilación antes de reintentar.
