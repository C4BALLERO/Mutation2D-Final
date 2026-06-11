# Walkthrough — Mutation Swarm 2D
**Engine:** Unity 6000.3.8f1 · URP 17.3.0  
**Última actualización:** 2026-06-01

---

## Arquitectura del Proyecto

```
Assets/
├── _Art/
│   ├── Animations/
│   │   ├── Player/          ← AC_Player.controller + 8 animation clips
│   │   └── Enemies/         ← 5 AC_Enemy_*.controller + clips (generados por editor tool)
│   ├── Sprites/
│   │   ├── Player/          ← Spr_Player_ArgosArmor_sheet.png
│   │   └── Enemies/         ← Spritesheets por tipo y estado
│   └── GunsPack/            ← Spritesheets de armas (AK47, Glock, etc.)
├── _Prefabs/
│   ├── Player/              ← Prefab_Player.prefab (con Animator)
│   └── Enemies/             ← 5 prefabs con Animator + controladores
├── _Scenes/
│   ├── Scene_00_Boot        ← BootLoader, inicialización de managers
│   ├── Scene_01_MainMenu    ← Kenney UI, menú principal
│   ├── Scene_02_GameWorld   ← Gameplay principal
│   └── Scene_03_UpgradeMenu ← Fase de upgrades entre oleadas
├── _Scripts/
│   ├── Core/                ← GameManager, WaveManager, EventBus, ObjectPool, etc.
│   ├── Entities/            ← PlayerController, PlayerStats, EnemyBase, StateMachine, subclases
│   ├── Combat/              ← Weapons, Projectile, DamageSystem, StatusEffects
│   ├── Evolution/           ← Genome, EvolutionEngine, SelectionAlgorithm, AdaptivePressure
│   ├── Building/            ← BuildManager, Barricade, Turret
│   ├── UI/                  ← HUD, Menus, UpgradeUI, WeaponShop
│   ├── Meta/                ← Analytics, GlobalEcosystem
│   └── Editor/              ← Setup tools (MenuItem)
└── _ScriptableObjects/
    ├── Weapons/             ← SO_Weapon_*.asset
    ├── Upgrades/            ← SO_Upgrade_*.asset
    ├── Pools/               ← SO_Pool_*.asset
    └── Waves/               ← SO_WaveConfig_Default.asset
```

---

## Sistema del Player

**Script principal:** [Script_11_PlayerController.cs](Assets/_Scripts/Entities/Script_11_PlayerController.cs)

### Movimiento
| Feature | Implementación |
|---|---|
| Movimiento horizontal | `HandleHorizontalMovement()` con suavizado |
| Salto | Coyote time (0.15s) + jump buffer (0.1s) |
| Double Jump | Habilitado via `PlayerStats.HasDoubleJump` |
| Wall Jump | Raycast lateral + `HasWallJump` |
| Wall Slide | Gravedad reducida al tocar pared |
| Dash | Invulnerabilidad durante `_dashDuration` |
| Variable jump height | Corta el impulso al soltar el botón |

### Controles (Keyboard/Gamepad)
| Acción | Keyboard | Gamepad |
|---|---|---|
| Mover | WASD / Flechas | Left Stick |
| Saltar | Space / W / Up | A / B |
| Dash | Shift | R Shoulder |
| Fuego primario | Mouse izquierdo | R Trigger |
| Fuego secundario | Mouse derecho | L Trigger |
| Swap arma | Q / E | Y |

### Animator (AC_Player.controller)
| Parámetro | Tipo | Usado en |
|---|---|---|
| `IsGrounded` | bool | `UpdateAnimator()` |
| `IsRunning` | bool | `UpdateAnimator()` |
| `IsFalling` | bool | `UpdateAnimator()` |
| `Jump` | trigger | saltos |
| `Dash` | trigger | `DashRoutine()` |
| `Attack` | trigger | `HandleCombatInput()` |
| `Hit` | trigger | `ApplyDamage()` |
| `Die` | trigger | `HandleDeath()` |

---

## Sistema de Enemigos

### Jerarquía de clases
```
Script_13_EnemyBase (MonoBehaviour)
├── Script_15_EnemyBoss   — 3 fases (HP thresholds: 66%, 33%)
├── Script_16_EnemyQueen  — Spawna drones genéticos cada N segundos
├── Script_17_EnemyMimic  — Copia escala/velocidad del jugador cercano
├── Script_18_EnemyParasite — Infecta aliados con mutaciones combinadas
└── (Drone = instancia directa de EnemyBase)
```

### EnemyBase — Componentes requeridos
```
Rigidbody2D + SpriteRenderer + CircleCollider2D
+ Script_13_EnemyBase + Script_22_StatusEffects + Animator
```

### Flujo de inicialización
1. `WaveManager` instancia el prefab
2. Llama `enemy.Initialize(genome)` con un `Genome` del pool evolutivo
3. `Initialize()` configura stats, colores, física y el `Animator`
4. La `StateMachine` arranca en `IdleState`

### Animator de Enemigos (AC_Enemy_*.controller)
| Parámetro | Tipo | Disparado desde |
|---|---|---|
| `IsMoving` | bool | `TriggerMove()` / `TriggerIdle()` |
| `Attack` | trigger | `TriggerAttack()` en `AttackState.Enter()` |
| `Hit` | trigger | `TriggerHit()` en `TakeDamage()` |
| `Die` | trigger | `TriggerDie()` en `Die()` |

Transiciones AnyState (por prioridad):
1. **Die** (máxima — no interrumpible)
2. **Hit** (intermedia)
3. **Attack** (mínima)

---

## State Machine de Enemigos

**Script:** [Script_14_EnemyStateMachine.cs](Assets/_Scripts/Entities/Script_14_EnemyStateMachine.cs)

```
IdleState
  ├── → PursueState (player visible + wait timer)
  └── → FleeState (HP < 15% y Regeneracion > 0.4f)

PursueState
  ├── → AttackState (distancia ≤ AttackRange)
  ├── → SwarmState (ComportamientoGrupal > 0.6 y ≥3 aliados cercanos)
  └── → FleeState (HP < 15% y Regeneracion > 0.4f)

AttackState
  └── → PursueState (después del ataque)

FleeState
  └── → PursueState (HP > 50%)

SwarmState
  ├── → AttackState (distancia ≤ AttackRange)
  └── → PursueState (< 2 aliados cercanos)
```

---

## Sistema de Animaciones de Enemigos

### Generación de assets (Editor Tool)

**Herramienta:** `Tools > Mutation Swarm > Build Enemy Animations (Full Pipeline)`

Este editor tool ejecuta el pipeline completo en un solo clic:
1. Genera pixel-art spritesheets para cada tipo × estado via `EnemyAnimationSpriteFactory`
2. Guarda PNGs en `Assets/_Art/Sprites/Enemies/`
3. Configura slicing múltiple en cada PNG
4. Crea Animation Clips `.anim` en `Assets/_Art/Animations/Enemies/`
5. Crea Animator Controllers `.controller` en `Assets/_Art/Animations/Enemies/`
6. Asigna los controllers a los prefabs de enemigos

### Frames por enemigo
| Enemigo | Idle | Move | Attack | Hit | Die | Total |
|---|---|---|---|---|---|---|
| Drone | 4 | 4 | 3 | 2 | 3 | 16 |
| Boss | 4 | 6 | 4 | 2 | 4 | 20 |
| Queen | 4 | 4 | 4 | 2 | 4 | 18 |
| Mimic | 3 | 4 | 3 | 2 | 3 | 15 |
| Parasite | 3 | 4 | 3 | 2 | 3 | 15 |

### Prerrequisito
Antes de correr el animation pipeline, ejecutar primero:
`Tools > Mutation Swarm > Build Enemy Sprites (Art Bible)`
para que existan los prefabs base.

---

## Sistema de Evolución Genética

**Genome:** 12 traits (Velocidad, Tamaño, Salto, Armadura, Veneno, ExplosionAlMorir, Regeneracion, RangoVision, ComportamientoGrupal, ResistenciaFuego, ResistenciaElectrica, Espinas)

**Pipeline por oleada:**
1. `WaveManager` recolecta `EnemyCombatData` por cada enemigo muerto
2. Al terminar la oleada, llama `EvolutionEngine.ProcessWave()`
3. `ProcessWave()` ejecuta: scoring → elitismo → selección torneo → crossover → mutación → presión adaptativa
4. Retorna el nuevo pool de genomas para la siguiente oleada
5. `WaveManager` usa el pool para spawnear la siguiente oleada

**AdaptivePressure:** Contra-estrategia automática.
- Jugador usa ranged → enemigos ganan más Armadura y RangoVision
- Jugador usa melee → enemigos ganan más Espinas y Velocidad
- Jugador está arriba → enemigos ganan Salto
- Jugador usa torretas → enemigos ganan ComportamientoGrupal

---

## Cómo Compilar

1. Abrir proyecto en Unity 6000.3.8f1
2. Esperar que Unity compile todos los scripts
3. Si hay errores de compilación, revisar CHANGELOG.md para cambios recientes
4. Ir a `File > Build Settings > PC, Mac & Linux Standalone`
5. Seleccionar plataforma Windows
6. Asegurarse que las 4 escenas estén en la build list (en orden: Boot, MainMenu, GameWorld, UpgradeMenu)
7. Click `Build`

---

## Cómo Ejecutar (Editor)

1. Abrir `Scene_00_Boot.unity`
2. Press Play
3. El BootLoader carga el GameManager singleton y abre el MainMenu

---

## Cómo Agregar un Nuevo Tipo de Enemigo

1. Crear clase `Script_XX_EnemyNuevo : Script_13_EnemyBase` en `Assets/_Scripts/Entities/`
2. Implementar comportamiento único en `LateUpdate()` o `Start()` (NO sobreescribir `Update()`)
3. En `EnemyAnimationSpriteFactory.cs`:
   - Agregar al enum `Archetype` en `EnemySpriteFactory`
   - Agregar método `DrawNuevo(Color[] px, int s)` en `EnemySpriteFactory`
   - Agregar fila en `FrameCounts[,]` en `EnemyAnimationSpriteFactory`
4. En `MutationSwarmEnemyArtSetup.cs`: agregar llamada a `BuildArchetype()` en `BuildAll()`
5. En `MutationSwarmEnemyAnimationSetup.cs`: agregar entrada en el array `Enemies`
6. Ejecutar `Tools > Mutation Swarm > Build Enemy Sprites (Art Bible)`
7. Ejecutar `Tools > Mutation Swarm > Build Enemy Animations (Full Pipeline)`

---

## Cómo Agregar Nuevas Animaciones

1. Crear nuevos frames en `EnemyAnimationSpriteFactory`:
   - Agregar estado al enum `AnimState`
   - Agregar método `BuildXXXFrames()` en el switch de `GenerateFrames()`
   - Agregar fila en `FrameCounts[,]`
2. En `MutationSwarmEnemyAnimationSetup.cs`:
   - Agregar clip via `BuildStateClip()` con el nuevo estado
   - Agregar el clip como nuevo estado en `BuildAnimatorController()`
3. En `Script_13_EnemyBase.cs`: agregar método `TriggerXXX()`
4. Llamar el trigger desde la `StateMachine` en el estado apropiado
5. Ejecutar `Tools > Mutation Swarm > Build Enemy Animations (Full Pipeline)`
