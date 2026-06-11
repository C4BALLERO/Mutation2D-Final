# SCENE ARCHITECTURE — Mutation Swarm 2D
**Versión:** 1.0 · **Fecha:** 2026-06-02

---

## Flujo de Escenas

```
Scene_00_Boot ──► Scene_01_MainMenu ──► Scene_02_GameWorld ──► Scene_03_UpgradeMenu
                         │                      │                        │
                         │                 Room_01..05              (vuelve a
                         │                 Room_Boss                GameWorld)
                         │
                    [Quit = Application.Quit()]
```

**Persistencia:** `Script_01_GameManager` y `Script_05_SaveManager` usan `DontDestroyOnLoad` — sobreviven entre escenas.

---

## Scene_00_Boot — Splash Screen

**Archivo:** `Assets/_Scenes/Scene_00_Boot.unity`  
**Herramienta:** `Tools > Mutation Swarm > Setup Scene_00_Boot`

### Jerarquía de GameObjects

```
Scene_00_Boot
├── Main Camera         ← ortho size 5.4, bg #04050A
├── _Managers
│   ├── GameManager     ← Script_01_GameManager (DontDestroyOnLoad)
│   ├── SaveManager     ← Script_05_SaveManager (DontDestroyOnLoad)
│   └── AudioManager    ← Script_AudioManager (DontDestroyOnLoad)
├── _BootUI             ← UIDocument (BootSplash.uxml + BootSplash.uss)
│                          Script_34_BootSplashUI (anima la barra)
└── _Boot               ← Script_00_BootLoader (carga MainMenu al terminar)
```

### Flujo de ejecución

1. `Awake()` → inicializa GameManager + SaveManager singletons
2. `Start()` → activa `Script_34_BootSplashUI`
3. Splash anima barra 0→100% en 2.8 segundos con mensajes cíclicos
4. `OnSplashComplete` → `SceneManager.LoadScene("Scene_01_MainMenu")`

### UXML/USS
- `BootSplash.uxml` — Título, barra de carga, corner labels
- `BootSplash.uss` — Tema oscuro biopunk, cyan glow

---

## Scene_01_MainMenu — Menú Principal

**Archivo:** `Assets/_Scenes/Scene_01_MainMenu.unity`  
**Herramienta:** `Tools > Mutation Swarm > Setup Scene_01_MainMenu`

### Jerarquía de GameObjects

```
Scene_01_MainMenu
├── Main Camera         ← ortho size 5.4, bg #04050A
├── _Managers
│   ├── GameManager     ← (reusa singleton si ya existe)
│   ├── SaveManager     ← (reusa singleton si ya existe)
│   └── AudioManager    ← (reusa singleton si ya existe)
├── _Background         ← (placeholder para fondo animado futuro)
└── _MainMenuUI         ← UIDocument (MainMenu.uxml + MainMenu.uss)
                           Script_30_MainMenuController (botones, player count)
                           Script_35_MainMenuAnimator (pulse título, record)
```

### Funcionalidades
- **Selector de jugadores:** 1-4 vía botones `−/+` → `Script_30_MainMenuController`
- **Iniciar misión:** `BtnPlay` → `Script_01_GameManager.StartGameSession(playerCount)`
- **Opciones:** `BtnOptions` → placeholder (futuro)
- **Salir:** `BtnQuit` → `Application.Quit()` (o `EditorApplication.isPlaying=false`)
- **Animación título:** Pulse suave blanco↔cian vía `Script_35_MainMenuAnimator`
- **Record de oleada:** Leído desde `SaveManager.CurrentSave.maxWaveReached`

### UXML/USS
- `MainMenu.uxml` — Panel centrado con título, status bar, selector, botones, record
- `MainMenu.uss` — Biopunk completo con hover transitions

---

## Scene_02_GameWorld — Gameplay Principal

**Archivo:** `Assets/_Scenes/Scene_02_GameWorld.unity`  
**Herramienta:** `Tools > Mutation Swarm > Build Art Level (Scene_02)` o `Setup Scene_02_GameWorld`

### Jerarquía de GameObjects

```
Scene_02_GameWorld
├── Main Camera         ← ortho size 5.4
├── _Managers
│   ├── GameManager
│   ├── WaveManager     ← Script_02_WaveManager (spawn points, enemy prefab)
│   ├── EvolutionEngine ← Script_07_EvolutionEngine
│   ├── ObjectPool      ← Script_04_ObjectPool (8 SO_Pool configs)
│   ├── BuildManager    ← Script_23_BuildManager
│   └── AudioManager
├── _Environment
│   ├── Parallax        ← 5 capas de fondo (Script_31_ParallaxLayer)
│   ├── Platforms       ← 9 plataformas tile-by-tile + paredes/techo
│   └── Decoration      ← árboles, rocas, pilares
├── _SpawnPoints        ← 12 puntos (8 enemigos, 4 jugadores)
│   └── Script_SpawnPointGizmos
├── _Players            ← contenedor de jugadores activos
├── _Enemies            ← contenedor de enemigos activos
├── _Structures         ← contenedor de estructuras construidas
├── _UI
│   ├── UIDocument      ← HUD_Main.uxml + HUD_Main.uss
│   ├── Script_25_HUDController
│   └── Script_27_EvolutionDisplayUI
└── _WeaponShop         ← UIDocument (WeaponShop.uxml + WeaponShop.uss)
```

### Sistemas activos
- **Wave System:** `WaveManager` spawna enemigos con genomas evolutivos
- **Evolution:** `EvolutionEngine.ProcessWave()` genera la siguiente generación
- **HUD:** Barras de HP (4 jugadores), oleada, conteo de enemigos, materiales, arma
- **Build System:** Construcción de torretas y barricadas en tiempo real
- **Weapon Shop:** Modal post-oleada para comprar/equipar armas

### UXML/USS
- `HUD_Main.uxml` — HP bars, wave info, evolution panel, resources, dash ring
- `HUD_Main.uss` — Biopunk HUD completo
- `WeaponShop.uxml` — Modal de tienda
- `BuildRadialMenu.uxml` — Menú radial de construcción

---

## Scene_03_UpgradeMenu — Menú de Mejoras

**Archivo:** `Assets/_Scenes/Scene_03_UpgradeMenu.unity`  
**Herramienta:** `Tools > Mutation Swarm > Setup Scene_03_UpgradeMenu`

### Jerarquía de GameObjects

```
Scene_03_UpgradeMenu
├── Main Camera         ← ortho size 5.4, bg #04050A
├── _Managers
│   ├── GameManager     ← (reusa singleton)
│   └── AudioManager
└── _UpgradeUI          ← UIDocument (UpgradeMenu.uxml + UpgradeMenu.uss)
                           Script_26_UpgradeMenuUI (8 SO_UpgradeData en pool)
```

### Funcionalidades
- **3 tarjetas de upgrade** aleatorias del pool de 8 upgrades
- **Timer de 15s** — auto-selecciona si no hay decisión
- **Controles:** Q/E o ◄/► para navegar · SPACE/A para confirmar
- **Soporte co-op:** Cada jugador selecciona independientemente
- **Integración con save:** Los upgrades equipados persisten vía `Script_05_SaveManager`

### UXML/USS
- `UpgradeMenu.uxml` — Header, timer, 3 tarjetas con hotkeys
- `UpgradeMenu.uss` — Tarjetas biopunk con accent bar, hover effects

---

## Room Scenes (Assets/_Scenes/Rooms/)

Ver [ROOMS_DESIGN.md](ROOMS_DESIGN.md) para layouts detallados.

| Escena | Tipo | Dificultad | Próxima escena |
|---|---|---|---|
| Room_01.unity | Platform Easy | ⭐ | Room_02 |
| Room_02.unity | Combat Medium | ⭐⭐⭐ | Room_03 |
| Room_03.unity | Secret Medium | ⭐⭐⭐ | Room_04 |
| Room_04.unity | Mixed Hard | ⭐⭐⭐⭐ | Room_05 |
| Room_05.unity | Platform Hard | ⭐⭐⭐⭐⭐ | Room_Boss |
| Room_Boss.unity | Boss Arena | ⭐⭐⭐⭐⭐ | Scene_01_MainMenu |

**Herramienta:** `Tools > Mutation Swarm > Build All Rooms`

---

## Manager Dependencies

```
Script_01_GameManager  (DontDestroyOnLoad)
    ├── Inicia sesión de juego → carga Scene_02_GameWorld
    ├── Rastrea jugadores vivos (1-4)
    └── Publica OnGameStateChanged, OnAllPlayersDead

Script_05_SaveManager  (DontDestroyOnLoad)
    ├── JSON persistente: highScore, maxWaveReached, unlockedUpgrades
    └── PlayerPrefs con prefijo "MS_" para settings

Script_AudioManager    (DontDestroyOnLoad)
    └── Gestiona reproducción de audio global

Script_03_EventBus     (static)
    ├── WaveStartedEvent, WaveEndedEvent
    ├── EvolutionPhaseEvent, EnemyCountChangedEvent
    ├── BuildMaterialsChangedEvent, MutationToastEvent
    ├── WeaponEquippedEvent, WeaponShopOpenedEvent/ClosedEvent/PurchasedEvent
    ├── BossPhaseChangedEvent, MimicActivatedEvent, ParasiteInfectedEvent
    ├── CheckpointActivatedEvent, ChestOpenedEvent, LevelExitReachedEvent
    └── (más en Room system)
```

---

## Build Settings Order

```
0. Scene_00_Boot           ← Entry point
1. Scene_01_MainMenu
2. Scene_02_GameWorld
3. Scene_03_UpgradeMenu
4. Rooms/Room_01
5. Rooms/Room_02
6. Rooms/Room_03
7. Rooms/Room_04
8. Rooms/Room_05
9. Rooms/Room_Boss
```
