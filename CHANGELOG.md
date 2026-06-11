# CHANGELOG — Mutation Swarm 2D

## [Unreleased] — 2026-06-01

### Added

#### Auditoría y Herramientas
- `ANALYSIS_REPORT.md` — Auditoría completa del proyecto (engine, arquitectura, animaciones, enemigos, scripts, prefabs)
- `TASKS_DETECTED.md` — 60 tareas organizadas en 6 fases con prioridad y estado
- `RISKS.md` — 17 riesgos identificados con nivel de severidad (RSK-01 a RSK-17) y mitigación
- `GAME_STUDIOS_INTEGRATION.md` — Análisis del repositorio CCGS y componentes integrados
- `.claude/agents/unity-specialist.md` — Agente especializado Unity 6 integrado desde CCGS
- `.claude/agents/technical-artist.md` — Agente de arte técnico integrado desde CCGS
- `.claude/agents/gameplay-programmer.md` — Agente de gameplay integrado desde CCGS
- `.claude/agents/qa-lead.md` — Agente de QA integrado desde CCGS
- `.claude/agents/lead-programmer.md` — Agente de lead programador integrado desde CCGS
- `.claude/docs/engine-reference/unity/VERSION.md` — Referencia Unity 6.3 LTS
- `.claude/docs/engine-reference/unity/animation.md` — Referencia de Animation API para Unity 6
- `.claude/docs/coding-standards.md` — Estándares de código del estudio

#### Sistema de Animaciones de Enemigos (NUEVO)
- `Assets/_Scripts/Editor/EnemyAnimationSpriteFactory.cs` — Generador de frames pixel-art por estado
  - Genera 5 tipos de frames: Idle (bob), Move (waddle), Attack (squash/stretch), Hit (white flash), Die (fall/fade)
  - Transformaciones: `ShiftPixels`, `ScalePixels`, `BrightenPixels`, `AlphaPixels`, `WhiteFlashPixels`
  - Tabla de frame counts por enemigo × estado (Drone: 16, Boss: 20, Queen: 18, Mimic: 15, Parasite: 15)
- `Assets/_Scripts/Editor/MutationSwarmEnemyAnimationSetup.cs` — Pipeline completo de animación
  - MenuItem: `Tools > Mutation Swarm > Build Enemy Animations (Full Pipeline)`
  - Genera spritesheets PNG → configura slicing → crea AnimationClips → crea AnimatorControllers → wireless prefabs
  - Crea 5 Animator Controllers (AC_Enemy_Drone/Boss/Queen/Mimic/Parasite)
  - Parámetros: `IsMoving` (bool), `Attack/Hit/Die` (triggers)
  - Transiciones AnyState ordenadas por prioridad: Die → Hit → Attack

#### Integración Animator en Enemigos
- `Script_13_EnemyBase.cs` — Soporte completo de Animator
  - Campo `[SerializeField] private Animator _animator` con auto-detección en `Initialize()`
  - Campo `[SerializeField] private float _deathAnimDuration = 0.8f`
  - Métodos trigger: `TriggerIdle()`, `TriggerMove()`, `TriggerAttack()`, `TriggerHit()`, `TriggerDie()`
  - Coroutine `DestroyAfterDeath()` — espera la duración de la animación antes de destruir el GameObject
  - `OnAttackLand()` — Animation Event hook (hit detection desde clips)
  - `OnDeathComplete()` — Animation Event hook (destrucción al último frame de Die)
  - Cooldown de `TriggerHit()` (0.15s) para evitar spam del trigger con DoT
  - `Update()` ahora retorna early cuando `_hasDied = true`
  - `_rb.simulated = false` en `Die()` para detener física sin destruir inmediatamente
- `Script_14_EnemyStateMachine.cs` — Triggers de animación por estado
  - `IdleState.Enter()` → `TriggerIdle()`
  - `PursueState.Enter()` → `TriggerMove()`
  - `AttackState.Enter()` → `TriggerAttack()`
  - `FleeState.Enter()` → `TriggerMove()`
  - `SwarmState.Enter()` → `TriggerMove()`
- `MutationSwarmEnemyArtSetup.cs` — Agrega componente `Animator` a todos los enemy prefabs en `CreateEnemyGameObject()`

#### Subclases de Enemigos (IMPLEMENTADAS)
- `Script_15_EnemyBoss.cs` — Sistema de 3 fases
  - Fase 1 (100-66% HP): comportamiento estándar
  - Fase 2 (65-33% HP): Velocidad ×1.4, RangoVision ×1.3, spawna minions
  - Fase 3 (0-33% HP): Velocidad máxima, Espinas +0.4, spawna minions
  - `InitializeBoss(Genome mergedGenome)` — entrada para genomas boss fusionados
- `Script_16_EnemyQueen.cs` — Spawning de drones genéticos
  - Coroutine `SpawnRoutine()` — espera hasta que `Genome != null` antes de spawnear
  - Spawna drones con genoma clonado + mutación (0.15 intensity)
  - Intervalo escalado por `Regeneracion` (más regen = más fertilidad)
  - Máximo de `_maxSpawnedDrones` (4) activos simultáneamente
- `Script_17_EnemyMimic.cs` — Copia escala del jugador cercano
  - `LateUpdate()` (no sobreescribe `Update()` del base) — AI tick intacto
  - Actualiza escala visual basada en `PlayerStats.MoveSpeed` cada 2 segundos
  - Publica `MimicActivatedEvent` la primera vez que activa la copia
- `Script_18_EnemyParasite.cs` — Propaga mutaciones a aliados
  - Infecta hasta 3 aliados cercanos (radio 2.5u) cada 5 segundos
  - No infecta otros Parasites
  - Fusiona genoma via `Crossover(host.Genome, this.Genome)` + mutación 0.1
  - Publica `ParasiteInfectedEvent` por cada infección

#### Sistema de Evolución (COMPLETADO)
- `Script_08_Genome.cs` — Mutación y crossover implementados
  - `Mutate(float intensity)` — perturbación uniforme en cada gen: `delta = Random[-1,1] × intensity × (max-min)`
  - `Crossover(Genome parentA, Genome parentB)` — uniform crossover (cada gen se hereda aleatoriamente)
  - `Crossover(Genome other)` — método instancia de conveniencia
  - `OnMutationOccurred` event disparado cuando el delta supera el 5% del rango del gen
- `Script_09_SelectionAlgorithm.cs` — Implementado
  - `TournamentSelect()` — selección por torneo sin reemplazo
  - `ApplyElitism()` — clona los N mejores genomas por fitness
- `Script_07_EvolutionEngine.cs` — Pipeline completo
  - `ScorePopulation()` — fitness = genome modifier + damageDone×0.01 + timeAlive×0.005 + survived×0.5
  - `ProcessWave()` — elitismo (4) + torneo + crossover + mutación + presión adaptativa
  - `SeedGeneration()` — genera genomas aleatorios para la primera oleada
  - `OnEvolutionComplete` event con `EvolutionSummary`
- `Script_10_AdaptivePressure.cs` — Counter-estrategia implementada
  - Counter ranged → +Armadura +RangoVision
  - Counter melee → +Espinas +Velocidad
  - Player arriba → +Salto
  - Player usa torretas → +ComportamientoGrupal

#### Otros Sistemas
- `Script_04_ObjectPool.cs` — `ReturnAll(string poolKey)` implementado
  - Itera sobre hijos activos del pool, identifica por nombre del prefab
  - `ReturnAll()` sin argumentos — retorna todos los pools
- `Script_03_EventBus.cs` — Nuevos event structs
  - `BossPhaseChangedEvent { boss, phase }`
  - `MimicActivatedEvent { mimic, target }`
  - `ParasiteInfectedEvent { parasite, host }`

#### Documentación
- `walkthrough.md` — Arquitectura, Player, Enemigos, Animaciones, Compilación, Extensión
- `CHANGELOG.md` — Este archivo

---

### Changed

#### Script_13_EnemyBase.cs
- `using System.Collections` agregado para `IEnumerator`
- Header `[Header("Animación")]` con `_animator` y `_deathAnimDuration`
- `_hitFlashCooldown` y `HitFlashInterval` como campos privados
- `Initialize()` detecta Animator automáticamente como fallback
- `Update()` retorna early si `_hasDied`
- `TakeDamage()` llama `TriggerHit()` antes de verificar muerte
- `Die()` desactiva física, dispara animación y usa coroutine para destruir

#### Script_14_EnemyStateMachine.cs
- Todos los estados agregan llamadas a triggers en `Enter()`

#### Script_08_Genome.cs
- `Mutate()` reemplaza el cuerpo vacío con implementación real
- `Crossover()` reemplaza la clonación simple con uniform crossover real
- Agrega método instancia `Crossover(Genome other)`

#### Script_09_SelectionAlgorithm.cs
- `TournamentSelect()` reemplaza retorno vacío con selección real
- `ApplyElitism()` reemplaza retorno vacío con ordenación por fitness

#### Script_07_EvolutionEngine.cs
- `ProcessWave()` reemplaza placeholder con pipeline evolutivo completo

#### Script_10_AdaptivePressure.cs
- `Apply()` reemplaza placeholder con lógica de counter-estrategia

#### Script_04_ObjectPool.cs
- `ReturnAll(string)` reemplaza placeholder con implementación real

#### MutationSwarmEnemyArtSetup.cs
- `CreateEnemyGameObject()` agrega `Animator` component a todos los enemy prefabs

---

### Fixed

- `Script_16_EnemyQueen.cs`: Coroutine ahora espera `Genome != null` antes de spawnear (evitaba NullReferenceException)
- `Script_17_EnemyMimic.cs`: Eliminado `new void Update()` que bloqueaba el AI tick del base — reemplazado con `LateUpdate()`
- `Script_18_EnemyParasite.cs`: `host.Genome.Crossover(this.Genome)` en lugar de forma ambigua
- `Script_15_EnemyBoss.cs`: Usa literales numéricos (4.0f, 1.0f) en lugar de `Genome.VelocidadMax` (ambiguo con propiedad `Genome`)
- `MutationSwarmEnemyAnimationSetup.cs`: Transiciones AnyState agregadas en orden de prioridad (Die→Hit→Attack) eliminando la necesidad de reordenarlas (API sin setter)

---

### Removed
- `Script_07_EvolutionEngine.cs`: Comentario placeholder "Implementación completa en PROMPT 02"
- `Script_10_AdaptivePressure.cs`: Comentario placeholder "Implementación completa en PROMPT 02"
- `Script_08_Genome.cs`: Cuerpos vacíos de `Mutate()` y `Crossover()`
- `Script_04_ObjectPool.cs`: Comentario placeholder "Implementación completa en PROMPT 08"
