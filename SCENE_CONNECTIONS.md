# SCENE CONNECTIONS — Mutation Swarm 2D
**Fecha:** 2026-06-02

---

## Mapa de Escenas

```
┌─────────────────────────────────────────────────────────────────┐
│                     CADENA DE ESCENAS                           │
│                                                                 │
│  Scene_00_Boot ──► Scene_01_MainMenu ──► Room_01               │
│                          │                  │                   │
│                    [Continuar]         Room_02                  │
│                    [Salir → Quit]          │                    │
│                                       Room_03                   │
│                                           │                     │
│                                       Room_04                   │
│                                           │                     │
│                                       Room_05                   │
│                                           │                     │
│                                       Room_Boss                 │
│                                           │                     │
│                          ◄────── Game Over / Completado ◄──────┘│
└─────────────────────────────────────────────────────────────────┘
```

---

## Build Settings Order

| Index | Escena | Archivo |
|---|---|---|
| 0 | Scene_00_Boot | `Assets/_Scenes/Scene_00_Boot.unity` |
| 1 | Scene_01_MainMenu | `Assets/_Scenes/Scene_01_MainMenu.unity` |
| 2 | Scene_02_GameWorld | `Assets/_Scenes/Scene_02_GameWorld.unity` |
| 3 | Scene_03_UpgradeMenu | `Assets/_Scenes/Scene_03_UpgradeMenu.unity` |
| 4 | Room_01 | `Assets/_Scenes/Rooms/Room_01.unity` |
| 5 | Room_02 | `Assets/_Scenes/Rooms/Room_02.unity` |
| 6 | Room_03 | `Assets/_Scenes/Rooms/Room_03.unity` |
| 7 | Room_04 | `Assets/_Scenes/Rooms/Room_04.unity` |
| 8 | Room_05 | `Assets/_Scenes/Rooms/Room_05.unity` |
| 9 | Room_Boss | `Assets/_Scenes/Rooms/Room_Boss.unity` |

---

## Quién Carga Qué

| Escena origen | Carga | Código responsable |
|---|---|---|
| Scene_00_Boot | Scene_01_MainMenu | `Script_00_BootLoader.LoadMainMenu()` |
| Scene_01_MainMenu | Room_01 | `GameManager.StartGameSession()` → `SceneLoader.LoadFirstRoom()` |
| Room_01 | Room_02 | `LevelExit._nextScene = "Room_02"` |
| Room_02 | Room_03 | `LevelExit._nextScene = "Room_03"` |
| Room_03 | Room_04 | `LevelExit._nextScene = "Room_04"` |
| Room_04 | Room_05 | `LevelExit._nextScene = "Room_05"` |
| Room_05 | Room_Boss | `LevelExit._nextScene = "Room_Boss"` |
| Room_Boss | Scene_01_MainMenu | `LevelExit._nextScene = "Scene_01_MainMenu"` |
| Cualquier Room | Scene_01_MainMenu | `SceneLoader.LoadMainMenu()` (Game Over) |
| Cualquier Room | Misma Room | `SceneLoader.RestartRoom()` (Respawn) |

---

## Persistent Singletons (DontDestroyOnLoad)

| Script | Nacido en | Persiste entre |
|---|---|---|
| `Script_01_GameManager` | Scene_00_Boot | Todas las escenas |
| `Script_05_SaveManager` | Scene_00_Boot | Todas las escenas |
| `Script_AudioManager` | Scene_00_Boot | Todas las escenas |

---

## Componentes por Escena

### Scene_00_Boot
- `Script_00_BootLoader` → inicia la cadena
- `Script_34_BootSplashUI` → anima la barra de carga
- `Script_01_GameManager` (singleton)
- `Script_05_SaveManager` (singleton)
- `Script_AudioManager` (singleton)

### Scene_01_MainMenu
- `Script_30_MainMenuController` → botones, selección de jugadores
- `Script_35_MainMenuAnimator` → animación del título, record de oleada
- `Script_01_GameManager` (reusa singleton existente)

### Room_01..Room_Boss (estructura por sala)
- `Script_37_RoomBootstrap` → spawna player + inicia oleada
- `Script_02_WaveManager` → spawna enemigos evolutivos
- `Script_25_HUDController` → muestra HP, oleada, recursos
- `MutationSwarm.Rooms.LevelExit` → carga siguiente sala
- `MutationSwarm.Rooms.CheckpointController` → guarda posición de respawn
- `Script_01_GameManager` (reusa singleton existente)

### Scene_03_UpgradeMenu
- `Script_26_UpgradeMenuUI` → 3 tarjetas de mejora con timer

---

## Cómo Agregar una Escena Nueva

1. Crear la escena en `Assets/_Scenes/`
2. Agregar manualmente a Build Settings (o correr `Full Game Setup`)
3. Si es una sala: agregar entrada en `Script_36_SceneLoader.RoomChain[]`
4. Si es una sala: agregar el nombre a `MutationSwarmFullSetup.RoomNames[]`
