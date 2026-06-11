# GAME FLOW — Mutation Swarm 2D
**Fecha:** 2026-06-02

---

## Flujo Completo

```
Unity Play
    │
    ▼
Scene_00_Boot
    ├── GameManager.Awake()   → singleton
    ├── SaveManager.Awake()   → carga save JSON
    ├── AudioManager.Awake()  → singleton
    ├── Script_34_BootSplashUI → barra 0→100% (2.8s)
    └── Script_00_BootLoader.LoadMainMenu()
                │
                ▼
        Scene_01_MainMenu
            ├── Script_30_MainMenuController
            ├── Script_35_MainMenuAnimator
            │
            ├── [BtnPlay] → GameManager.StartGameSession(playerCount)
            │                    └── Script_36_SceneLoader.LoadFirstRoom()
            │                                │
            │                                ▼
            │                           Room_01
            │
            └── [BtnQuit] → Application.Quit()
```

---

## Flujo de Sala (Room_XX)

```
Room cargada
    │
    ├── _Managers/GameManager  (persiste DontDestroyOnLoad)
    ├── _Managers/WaveManager  (spawna enemigos)
    ├── _RoomBootstrap
    │       ├── Start() → SpawnPlayer() → Instantiate(Prefab_Player, p1.position)
    │       └── Invoke(StartWave, 1.2s)
    │
    ├── _HUD (UIDocument/HUD_Main.uxml)
    │       └── Script_25_HUDController ← eventos EventBus
    │
    ├── _Environment/Platforms + Spawns
    │
    └── LevelExit (prefab en escena)
                ├── _requireWaveClear = false (Room_01) / true (Room_02+)
                └── Espera WaveEndedEvent → se abre
```

---

## Ciclo de Oleada

```
WaveManager.StartWave()
    │
    ├── Spawna N enemigos (uno por spawn point, en _spawnInterval segundos)
    │       └── EnemyBase.Initialize(genome) → AI activa
    │
    ├── Publica WaveStartedEvent
    │       └── HUDController actualiza "Oleada X"
    │
    ├── Enemigos mueren → EnemyDiedEvent → EnemiesAlive--
    │       └── HUDController actualiza contador
    │
    └── EnemiesAlive == 0 → Publica WaveEndedEvent
                └── LevelExit.OnWaveEnded() → exit se abre (verde)
```

---

## Transición Entre Salas

```
Player entra al LevelExit
    │
    ├── LevelExit.OnTriggerEnter2D()
    │       └── StartCoroutine(LoadNext) → WaitForSeconds(0.8s)
    │               └── SceneManager.LoadScene(_nextScene)
    │
    └── Room siguiente carga:
            Room_01 → Room_02 → Room_03 → Room_04 → Room_05 → Room_Boss
                                                                    │
                                                                    └── Script_36_SceneLoader.LoadMainMenu()
```

---

## Game Over

```
Player HP = 0
    │
    ├── Script_12_PlayerStats.TakeDamage()
    │       └── OnDeath?.Invoke()
    │
    ├── Script_11_PlayerController.HandleDeath()
    │       ├── Animator.SetTrigger("Die")
    │       └── StartCoroutine(ReviveWindowRoutine) → 10s
    │
    └── (si sin revive) → GameManager.OnPlayerDiedHandler(index)
                ├── _playersAlive[index] = false
                └── (si todos muertos) → OnAllPlayersDead?.Invoke()
                                └── Script_37_RoomBootstrap.OnGameOver()
                                            └── Invoke(ReturnToMenu, 2s)
                                                        └── Script_36_SceneLoader.LoadMainMenu()
```

---

## Upgrade Menu (entre oleadas o desde MainMenu)

```
UpgradeMenu (accesible via MainMenu > "Mejoras" — futuro)

Script_26_UpgradeMenuUI:
    ├── Muestra 3 tarjetas aleatorias del pool (8 SO_UpgradeData)
    ├── Timer 15s → auto-selecciona si no hay input
    ├── Q/E → navegar tarjetas
    └── SPACE/A → confirmar → aplica upgrade al jugador
```

---

## Checkpoints

```
Player entra en CheckpointController trigger
    │
    ├── CheckpointController.Activate()
    │       └── CheckpointController.LastCheckpoint = transform.position
    └── Publica CheckpointActivatedEvent
```

*Nota: El sistema de respawn desde checkpoint no está integrado aún en RoomBootstrap.
Para activarlo, `Script_37_RoomBootstrap.ResolveSpawnPosition()` puede ser extendido
para usar `CheckpointController.LastCheckpoint` si no es Vector2.zero.*
