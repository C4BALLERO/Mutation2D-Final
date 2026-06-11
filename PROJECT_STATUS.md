# PROJECT STATUS — Mutation Swarm 2D
**Fecha:** 2026-06-02 · **Estado:** En desarrollo — Jugable con setup

---

## Estado General

| Sistema | Estado | Notas |
|---|---|---|
| Boot Screen | ✅ FUNCIONAL | Splash 2.8s → MainMenu |
| Main Menu | ✅ FUNCIONAL | Selección jugadores, Nueva Partida, Salir |
| Player Controller | ✅ FUNCIONAL | Movimiento, salto, dash, ataque |
| Player Animations | ⚠️ REQUIERE SETUP | AC_Player.controller existe, necesita asignarse con `Full Game Setup` |
| Enemy AI | ✅ FUNCIONAL | 5 estados: Idle/Pursue/Attack/Flee/Swarm |
| Enemy Animations | ⚠️ REQUIERE SETUP | Run `Build Enemy Animations` primero, luego `Full Game Setup` |
| Wave System | ✅ FUNCIONAL | Spawn con genomas evolutivos, escalado por oleada |
| Evolution System | ✅ FUNCIONAL | Mutate(), Crossover(), EvolutionEngine completo |
| Room System | ✅ FUNCIONAL | 6 salas, plataformas, spawners, salidas |
| Room Bootstrap | ✅ NUEVO | Script_37_RoomBootstrap spawna player + inicia oleada |
| Scene Loader | ✅ NUEVO | Script_36_SceneLoader gestiona cadena Room_01→Boss |
| HUD | ✅ FUNCIONAL | HP, oleada, enemigos, materiales, arma |
| Upgrade Menu | ✅ FUNCIONAL | 3 tarjetas, timer, 8 upgrades disponibles |
| Save System | ✅ FUNCIONAL | JSON + PlayerPrefs, record de oleadas |
| Build Manager | ✅ FUNCIONAL | Torretas, barricadas |
| Weapon System | ✅ FUNCIONAL | 6 armas con ScriptableObjects |
| Boss Phases | ✅ FUNCIONAL | 3 fases (66%/33% HP), spawn de minions |

---

## Cómo Hacer el Juego Jugable (3 pasos)

### Paso 1 — Arte de enemigos (si no se ha hecho)
```
Tools > Mutation Swarm > Import Art Package Settings
Tools > Mutation Swarm > Build Enemy Animations (Full Pipeline)
```

### Paso 2 — Setup completo del juego
```
Tools > Mutation Swarm > Full Game Setup (Playable)
```
Esto hace automáticamente:
- ✓ Asigna AC_Player.controller al Player prefab
- ✓ Asigna controllers de animación a enemy prefabs
- ✓ Añade RoomBootstrap a cada room (spawna player + inicia oleada)
- ✓ Añade HUD a cada room
- ✓ Desactiva upgrade menu mid-room en WaveManager
- ✓ Asigna SO_WaveConfig_Default al WaveManager
- ✓ Configura Build Settings completo

### Paso 3 — Presionar Play
Abrir `Scene_00_Boot.unity` y presionar Play.

---

## Issues Conocidos

| Issue | Severidad | Solución |
|---|---|---|
| AnimatorController null en prefabs | Alto | Ejecutar `Full Game Setup` |
| Rooms sin Bootstrap | Alto | Ejecutar `Full Game Setup` |
| Rooms sin HUD | Medio | Ejecutar `Full Game Setup` |
| SO_WaveConfig null en WaveManager | Medio | Ejecutar `Full Game Setup` |
| UpgradeMenu se cargaba mid-room | Resuelto | FullSetup pone `_upgradeSceneName = ""` |

---

## Archivos Clave

| Archivo | Propósito |
|---|---|
| `Assets/_Prefabs/Player/Prefab_Player.prefab` | Prefab del jugador |
| `Assets/_Prefabs/Enemies/Prefab_Enemy_*.prefab` | Prefabs de enemigos |
| `Assets/_Art/Animations/Player/AC_Player.controller` | Controller de animación del player |
| `Assets/_Art/Animations/Enemies/AC_Enemy_*.controller` | Controllers de enemigos |
| `Assets/_ScriptableObjects/Waves/SO_WaveConfig_Default.asset` | Configuración de oleadas |
| `Assets/_Scenes/Rooms/Room_0*.unity` | Salas de juego |
| `Assets/_Scripts/Core/Script_36_SceneLoader.cs` | Navegación entre salas |
| `Assets/_Scripts/Core/Script_37_RoomBootstrap.cs` | Bootstrap por sala |
| `Assets/_Scripts/Editor/MutationSwarmFullSetup.cs` | Tool de setup completo |
