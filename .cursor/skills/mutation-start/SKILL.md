---
name: mutation-start
description: Onboarding de Mutation Swarm 2D — estado del proyecto, build, y próximos pasos. Usar al abrir el repo o cuando pregunten "por dónde empiezo".
---

# Mutation Swarm — Start

## 1. Leer contexto

- `AGENTS.md`, `README.md`
- `production/stage.txt` (etapa)
- `design/systems-index.md`

## 2. Comprobar artefactos clave

| Artefacto | Ruta esperada |
|-----------|----------------|
| Escenas | `Assets/_Scenes/Scene_00_Boot` … `Scene_03` |
| Build exe | `Builds/Windows/MutationSwarm.exe` (puede estar en .gitignore) |
| Prefabs geo | `Assets/_Prefabs/Player/Prefab_Player_Geo.prefab` |
| GDD evolución | `design/gdd/evolution-system.md` |

## 3. Si falta contenido en Unity

Ejecutar en Editor o batch:

```powershell
.\build.ps1
```

O en Unity: **Tools → Mutation Swarm → Build Kenney UI + Playable Geometric Level**, luego **Build Windows**.

## 4. Huecos conocidos (prioridad)

1. Cablear `WaveManager` → `BuildManager` + `Scene_03_UpgradeMenu` aditiva
2. Arte final vs geométrico/Kenney
3. `Script_28` / `Script_29` meta (stubs)

## 5. Preguntar al usuario

- ¿Trabajamos en gameplay, UI, evolución o build/release?
- ¿Modo review? (`production/review-mode.txt`: lean | full | solo)

No escribir archivos sin petición explícita en esta skill (solo orientación).
