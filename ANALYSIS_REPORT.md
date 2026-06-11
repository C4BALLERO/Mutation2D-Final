# ANALYSIS REPORT — Mutation Swarm 2D
**Fecha:** 2026-06-01  
**Engine:** Unity 6000.3.8f1 · Universal Render Pipeline 17.3.0  
**Analista:** Lead Game Developer / Technical Artist

---

## 1. ENGINE & STACK

| Campo | Valor |
|---|---|
| Engine | Unity 6000.3.8f1 |
| Render Pipeline | URP 17.3.0 |
| Input System | Unity Input System 1.18.0 |
| 2D Animation | com.unity.2d.animation 13.0.4 |
| Aseprite Import | com.unity.2d.aseprite 3.0.1 |
| Tilemap | com.unity.2d.tilemap + extras |
| IDE | Rider 3.0.39 / Visual Studio 2.0.26 |

---

## 2. ARQUITECTURA DE CARPETAS

```
Assets/
├── _Art/
│   ├── Animations/Player/          ← 8 clips + AC_Player.controller
│   ├── GunsPack/                   ← Spritesheets de armas (100+ sprites)
│   ├── Materials/                  ← Texturas de bloques, fondos, etc.
│   ├── Sprites/
│   │   ├── Enemies/                ← 5 PNGs estáticos (sin animación)
│   │   ├── Player/                 ← Spr_Player_ArgosArmor_sheet.png
│   │   ├── Generated/              ← Geo shapes (Circle, Square, Triangle)
│   │   └── Environment/
│   └── kenney_ui-pack-space-expansion/
├── _Audio/
├── _Data/
├── _Prefabs/
│   ├── Enemies/                    ← 6 prefabs (Drone, Boss, Geo, Mimic, Parasite, Queen)
│   ├── Player/                     ← 2 prefabs
│   ├── Projectiles/                ← 8 prefabs
│   └── Structures/                 ← 2 prefabs
├── _Scenes/                        ← 4 escenas (Boot, MainMenu, GameWorld, UpgradeMenu)
├── _Scripts/
│   ├── Core/                       ← 13 scripts (GameManager, WaveManager, EventBus, etc.)
│   ├── Entities/                   ← 8 scripts (Player + Enemy)
│   ├── Combat/                     ← 8 scripts (Weapons, Projectile, Damage, Status)
│   ├── Evolution/                  ← 4 scripts (Genome, Engine, Selection, Pressure)
│   ├── Building/                   ← 4 scripts (BuildManager, Structures)
│   ├── UI/                         ← 7 scripts (HUD, Menus, Upgrades)
│   ├── Meta/                       ← 2 scripts (Ecosystem, Analytics)
│   └── Editor/                     ← 11 scripts (Setup tools, builders)
└── _ScriptableObjects/             ← 27+ assets (Weapons, Upgrades, Pools, Waves)
```

**Total:** 64 C# scripts · 18 prefabs · 4 escenas · 27+ ScriptableObjects

---

## 3. SISTEMA DE ANIMACIONES

### Player (COMPLETO)
| Clip | Archivo | Estado |
|---|---|---|
| Idle | Player_Idle.anim | EXISTE |
| Walk | Player_Walk.anim | EXISTE |
| Jump | Player_Jump.anim | EXISTE |
| Fall | Player_Fall.anim | EXISTE |
| Dash | Player_Dash.anim | EXISTE |
| Attack | Player_Attack.anim | EXISTE |
| Hit | Player_Hit.anim | EXISTE |
| Die | Player_Die.anim | EXISTE |
| Animator Controller | AC_Player.controller | EXISTE |

**Parámetros del Animator Player:**
- `IsGrounded` (bool) — actualizado en `UpdateAnimator()`
- `IsRunning` (bool) — actualizado en `UpdateAnimator()`
- `IsFalling` (bool) — actualizado en `UpdateAnimator()`
- `Jump` (trigger) — en `DoGroundJump()`, `DoDoubleJump()`, `TryWallJump()`
- `Dash` (trigger) — en `DashRoutine()`
- `Attack` (trigger) — en `HandleCombatInput()`
- `Hit` (trigger) — en `ApplyDamage()`
- `Die` (trigger) — en `HandleDeath()`

### Enemigos (AUSENTE — CRÍTICO)
| Tipo | Sprites | Spritesheet | Clips | Controller |
|---|---|---|---|---|
| Drone | Spr_Enemy_Drone.png (estático) | NO | NO | NO |
| Boss | Spr_Enemy_Boss.png (estático) | NO | NO | NO |
| Queen | Spr_Enemy_Queen.png (estático) | NO | NO | NO |
| Mimic | Spr_Enemy_Mimic.png (estático) | NO | NO | NO |
| Parasite | Spr_Enemy_Parasite.png (estático) | NO | NO | NO |

**Conclusión:** Los enemigos no tienen sistema de animaciones implementado. Los sprites son PNGs estáticos sin frames de animación.

---

## 4. SISTEMA DE ENEMIGOS

### Script_13_EnemyBase.cs — FUNCIONAL
- Inicialización via Genome (stats, color, física)
- Movimiento: `MoveTowards()`, `MoveInDirection()`, `AddKnockback()`
- Detección: `GetNearestPlayer()`, `CanSeePlayer()`, `GetNearbyAllies()`, `GetAlliesCentroid()`
- Combate: `DealMeleeDamageTo()`, `TryApplyPoison()`, `ApplySpinesCounterDamage()`
- Ciclo de vida: `TakeDamage()`, `Die()`, `HealByRegen()`
- **Sin soporte Animator** — no hay referencia a `Animator` en la clase base

### Script_14_EnemyStateMachine.cs — FUNCIONAL (sin animaciones)
| Estado | Comportamiento |
|---|---|
| `IdleState` | Espera aleatoria 0.5–1.5s, detecta jugador por vision radius |
| `PursueState` | Persigue al jugador, considera centroide de aliados (ComportamientoGrupal) |
| `AttackState` | Daño cuerpo a cuerpo con cooldown, veneno, espinas, explosión suicida |
| `FleeState` | Huye cuando HP < 15% y Regeneracion > 0.4f, se cura |
| `SwarmState` | Flanqueo coordinado: 40% frontal, 60% lateral |
- **Sin triggers de animación** en ninguna transición de estado

### Subclases de Enemigos (INCOMPLETAS)
| Script | Estado | Implementación |
|---|---|---|
| Script_15_EnemyBoss.cs | Parcial | Solo `InitializeBoss(mergedGenome)` — sin fases |
| Script_16_EnemyQueen.cs | Vacía | Solo hereda base — sin spawning de drones |
| Script_17_EnemyMimic.cs | Vacía | Solo hereda base — sin copia de habilidades |
| Script_18_EnemyParasite.cs | Vacía | Solo hereda base — sin propagación de mutaciones |

---

## 5. STATE MACHINES EXISTENTES

### EnemyStateMachine (en Script_14_EnemyStateMachine.cs)
- Interfaz `IEnemyState` con `Enter/Execute/Exit`
- 5 estados concretos: `IdleState`, `PursueState`, `AttackState`, `FleeState`, `SwarmState`
- Transiciones basadas en: distancia al jugador, HP, aliados cercanos, traits del Genome

### No existe State Machine del Player
- El PlayerController usa lógica directa (flags booleanos + coroutines)
- El Animator del player actúa como una FSM visual implícita

---

## 6. SISTEMA DE EVOLUCIÓN

| Componente | Archivo | Estado |
|---|---|---|
| Genome (12 traits) | Script_08_Genome.cs | FUNCIONAL (traits, color, fitness) |
| Mutate() | Script_08_Genome.cs:88 | VACÍO — no implementado |
| Crossover() | Script_08_Genome.cs | CLONA en lugar de combinar genes |
| ProcessWave() | Script_07_EvolutionEngine.cs | PLACEHOLDER — retorna lista vacía |
| SelectionAlgorithm | Script_09_SelectionAlgorithm.cs | Desconocido (no leído completamente) |
| AdaptivePressure | Script_10_AdaptivePressure.cs | PLACEHOLDER — marcado PROMPT 02 |

---

## 7. SISTEMAS FUNCIONALES COMPLETOS

- **GameManager** — Singleton, estados de juego, co-op 1-4 jugadores
- **WaveManager** — Spawning por oleadas, escalado, fase de upgrades
- **EventBus** — Sistema de eventos desacoplado
- **ObjectPool** — Pooling de proyectiles (`ReturnAll()` vacío)
- **PlayerController** — Movimiento completo (dash, wall jump, double jump, coyote time)
- **PlayerStats** — HP, velocidad, salto, dash, habilidades booleanas
- **Weapon System** — WeaponBase, WeaponBasic, WeaponGun + 6 ScriptableObjects
- **Projectile** — Pool, lifetime, daño
- **DamageSystem** — Cálculo con resistencias
- **StatusEffects** — Veneno, quemado, etc.
- **BuildManager** — Colocación de estructuras (barricadas, torretas)
- **HUDController** — HUD en juego
- **MainMenu / KenneyUI** — Menús funcionales

---

## 8. PREFABS DETECTADOS

| Categoría | Prefab | Animator? |
|---|---|---|
| Player | Prefab_Player.prefab | Sí (AC_Player) |
| Player | Prefab_Player_Geo.prefab | Desconocido |
| Enemy | Prefab_Enemy_Drone.prefab | NO |
| Enemy | Prefab_Enemy_Boss.prefab | NO |
| Enemy | Prefab_Enemy_Queen.prefab | NO |
| Enemy | Prefab_Enemy_Mimic.prefab | NO |
| Enemy | Prefab_Enemy_Parasite.prefab | NO |
| Enemy | Prefab_Enemy_Geo.prefab | NO |
| Projectile | 8 prefabs de armas | N/A |
| Structure | Barricade, Turret | NO |

---

## 9. PACKAGES Y DEPENDENCIAS

Todas las dependencias están en `Packages/manifest.json`. No se detectaron packages faltantes o versiones incompatibles. El proyecto usa el stack oficial de Unity 6 para 2D.

---

## 10. RESUMEN EJECUTIVO

| Área | Estado |
|---|---|
| Arquitectura de código | Sólida y bien organizada |
| Sistema del Player | Completo y funcional |
| Animaciones del Player | Completas (8 clips + controller) |
| Sistema de Enemigos (lógica AI) | Funcional (base + state machine) |
| Animaciones de Enemigos | AUSENTE — máxima prioridad |
| Subclases de enemigos (Queen/Mimic/Parasite) | Vacías — sin comportamiento único |
| Sistema Boss (fases) | Sin implementar |
| Sistema de Evolución | Placeholders — no funcional en producción |
| Building System | Funcional pero limitado |
| UI / Menús | Funcionales |
| QA / Compilación | Pendiente de verificar |