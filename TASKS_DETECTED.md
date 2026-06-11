# TASKS DETECTED — Mutation Swarm 2D
**Fecha:** 2026-06-01  
**Origen:** Auditoría completa del proyecto

---

## LEYENDA
- 🔴 CRÍTICO — Bloquea compilación o funcionalidad core
- 🟠 ALTA — Afecta gameplay directamente
- 🟡 MEDIA — Mejora experiencia o completitud
- 🟢 BAJA — Pulido y optimización

---

## FASE 1 — PLAYER (ANIMACIONES)

| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| P-01 | Verificar que los 8 clips de animación del player tienen frames válidos | 🟠 | PENDIENTE |
| P-02 | Verificar que AC_Player.controller tiene todas las transiciones correctas | 🟠 | PENDIENTE |
| P-03 | Confirmar que Player_Jump, Player_Fall, Player_Dash, Player_Attack, Player_Hit, Player_Die tienen sprites | 🟠 | PENDIENTE |
| P-04 | Si faltan frames: generar spritesheet pixel art para animaciones faltantes | 🟡 | CONDICIONAL |
| P-05 | Configurar slicing en spritesheets del player si es necesario | 🟡 | CONDICIONAL |

**Nota:** Los clips existen como archivos .anim. Necesita verificación de que tienen frames asignados y no son clips vacíos.

---

## FASE 2 — ENEMIGOS (ANIMACIONES + ARTE)

### Enemy Drone
| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| D-01 | Crear spritesheet Drone_Idle (4 frames) | 🟠 | PENDIENTE |
| D-02 | Crear spritesheet Drone_Move (4 frames) | 🟠 | PENDIENTE |
| D-03 | Crear spritesheet Drone_Attack (3 frames) | 🟠 | PENDIENTE |
| D-04 | Crear spritesheet Drone_Die (3 frames) | 🟠 | PENDIENTE |
| D-05 | Configurar slicing en spritesheets del Drone | 🟠 | PENDIENTE |
| D-06 | Crear Animation Clips para Drone (4 clips .anim) | 🟠 | PENDIENTE |
| D-07 | Crear AC_Enemy_Drone.controller | 🟠 | PENDIENTE |
| D-08 | Integrar controller en Prefab_Enemy_Drone.prefab | 🟠 | PENDIENTE |

### Enemy Boss
| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| B-01 | Crear spritesheet Boss_Idle (4 frames) | 🟠 | PENDIENTE |
| B-02 | Crear spritesheet Boss_Move (6 frames) | 🟠 | PENDIENTE |
| B-03 | Crear spritesheet Boss_Attack (4 frames) | 🟠 | PENDIENTE |
| B-04 | Crear spritesheet Boss_Die (4 frames) | 🟠 | PENDIENTE |
| B-05 | Configurar slicing en spritesheets del Boss | 🟠 | PENDIENTE |
| B-06 | Crear Animation Clips para Boss (4 clips .anim) | 🟠 | PENDIENTE |
| B-07 | Crear AC_Enemy_Boss.controller | 🟠 | PENDIENTE |
| B-08 | Integrar controller en Prefab_Enemy_Boss.prefab | 🟠 | PENDIENTE |

### Enemy Queen
| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| Q-01 | Crear spritesheet Queen_Idle (4 frames) | 🟠 | PENDIENTE |
| Q-02 | Crear spritesheet Queen_Move (4 frames) | 🟠 | PENDIENTE |
| Q-03 | Crear spritesheet Queen_Attack (4 frames) | 🟠 | PENDIENTE |
| Q-04 | Crear spritesheet Queen_Die (4 frames) | 🟠 | PENDIENTE |
| Q-05 | Configurar slicing + clips + controller para Queen | 🟠 | PENDIENTE |
| Q-06 | Integrar controller en Prefab_Enemy_Queen.prefab | 🟠 | PENDIENTE |

### Enemy Mimic
| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| M-01 | Crear spritesheet Mimic_Idle (3 frames) | 🟠 | PENDIENTE |
| M-02 | Crear spritesheet Mimic_Move (4 frames) | 🟠 | PENDIENTE |
| M-03 | Crear spritesheet Mimic_Attack (3 frames) | 🟠 | PENDIENTE |
| M-04 | Crear spritesheet Mimic_Die (3 frames) | 🟠 | PENDIENTE |
| M-05 | Configurar slicing + clips + controller para Mimic | 🟠 | PENDIENTE |
| M-06 | Integrar controller en Prefab_Enemy_Mimic.prefab | 🟠 | PENDIENTE |

### Enemy Parasite
| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| PA-01 | Crear spritesheet Parasite_Idle (3 frames) | 🟠 | PENDIENTE |
| PA-02 | Crear spritesheet Parasite_Move (4 frames) | 🟠 | PENDIENTE |
| PA-03 | Crear spritesheet Parasite_Attack (3 frames) | 🟠 | PENDIENTE |
| PA-04 | Crear spritesheet Parasite_Die (3 frames) | 🟠 | PENDIENTE |
| PA-05 | Configurar slicing + clips + controller para Parasite | 🟠 | PENDIENTE |
| PA-06 | Integrar controller en Prefab_Enemy_Parasite.prefab | 🟠 | PENDIENTE |

---

## FASE 3 — INTEGRACIÓN DE CÓDIGO

| ID | Archivo | Tarea | Prioridad | Estado |
|---|---|---|---|---|
| C-01 | Script_13_EnemyBase.cs | Agregar campo `[SerializeField] Animator _animator` | 🔴 | PENDIENTE |
| C-02 | Script_13_EnemyBase.cs | Inicializar `_animator` en `Initialize()` o `Awake()` | 🔴 | PENDIENTE |
| C-03 | Script_13_EnemyBase.cs | Agregar métodos `TriggerIdle()`, `TriggerMove()`, `TriggerAttack()`, `TriggerHit()`, `TriggerDie()` | 🔴 | PENDIENTE |
| C-04 | Script_13_EnemyBase.cs | Agregar soporte a Animation Events (OnAttackLand, OnDeathComplete) | 🟠 | PENDIENTE |
| C-05 | Script_13_EnemyBase.cs | Llamar `TriggerDie()` en `Die()` antes de `Destroy()` | 🔴 | PENDIENTE |
| C-06 | Script_13_EnemyBase.cs | Agregar null safety en todas las referencias de Animator | 🟠 | PENDIENTE |
| C-07 | Script_13_EnemyBase.cs | Implementar `TriggerSuicideExplosion()` con daño en área real | 🟡 | PENDIENTE |
| C-08 | Script_14_EnemyStateMachine.cs | Llamar `enemy.TriggerIdle()` en `IdleState.Enter()` | 🔴 | PENDIENTE |
| C-09 | Script_14_EnemyStateMachine.cs | Llamar `enemy.TriggerMove()` en `PursueState.Enter()` y `SwarmState.Enter()` | 🔴 | PENDIENTE |
| C-10 | Script_14_EnemyStateMachine.cs | Llamar `enemy.TriggerAttack()` en `AttackState.Execute()` antes del daño | 🔴 | PENDIENTE |
| C-11 | Script_14_EnemyStateMachine.cs | Llamar `enemy.TriggerHit()` cuando el enemigo recibe daño | 🟠 | PENDIENTE |
| C-12 | Script_14_EnemyStateMachine.cs | Llamar `enemy.TriggerIdle()` en `FleeState.Enter()` o crear trigger de Flee separado | 🟡 | PENDIENTE |
| C-13 | Script_08_Genome.cs | Implementar `Mutate()` con variación aleatoria por trait | 🟡 | PENDIENTE |
| C-14 | Script_08_Genome.cs | Implementar `Crossover()` con combinación real de genes (no clonar) | 🟡 | PENDIENTE |
| C-15 | Script_07_EvolutionEngine.cs | Implementar `ProcessWave()` con selección y combinación de genomas | 🟡 | PENDIENTE |
| C-16 | Script_04_ObjectPool.cs | Implementar `ReturnAll()` para devolver todos los objetos al pool | 🟡 | PENDIENTE |
| C-17 | Script_15_EnemyBoss.cs | Implementar sistema de 3 fases basado en HP | 🟡 | PENDIENTE |
| C-18 | Script_16_EnemyQueen.cs | Implementar spawning de drones genéticos | 🟡 | PENDIENTE |
| C-19 | Script_17_EnemyMimic.cs | Implementar copia de habilidades del jugador más cercano | 🟡 | PENDIENTE |
| C-20 | Script_18_EnemyParasite.cs | Implementar propagación de mutaciones a aliados | 🟡 | PENDIENTE |

---

## FASE 4 — SISTEMA DE ANIMACIÓN (UNITY ASSETS)

| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| A-01 | Crear Assets/_Art/Animations/Enemies/ como carpeta destino | 🟠 | PENDIENTE |
| A-02 | Crear AC_Enemy_Drone.controller con parámetros: Idle(bool), Move(bool), Attack(trigger), Hit(trigger), Die(trigger) | 🔴 | PENDIENTE |
| A-03 | Crear AC_Enemy_Boss.controller (mismos parámetros) | 🔴 | PENDIENTE |
| A-04 | Crear AC_Enemy_Queen.controller (mismos parámetros) | 🔴 | PENDIENTE |
| A-05 | Crear AC_Enemy_Mimic.controller (mismos parámetros) | 🔴 | PENDIENTE |
| A-06 | Crear AC_Enemy_Parasite.controller (mismos parámetros) | 🔴 | PENDIENTE |
| A-07 | Configurar transiciones: Any State → Die (trigger Die) con exit time | 🟠 | PENDIENTE |
| A-08 | Configurar transiciones: Idle ↔ Move (bool IsMoving) | 🟠 | PENDIENTE |
| A-09 | Configurar transiciones: Any State → Attack (trigger Attack, no exit time) | 🟠 | PENDIENTE |
| A-10 | Agregar Animator component a los 5 prefabs de enemigos | 🔴 | PENDIENTE |

---

## FASE 5 — QA

| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| QA-01 | Compilar el proyecto y reportar todos los errores | 🔴 | PENDIENTE |
| QA-02 | Verificar que los 6 prefabs de enemigos tienen todos los componentes requeridos | 🔴 | PENDIENTE |
| QA-03 | Verificar referencias de scripts en prefabs (PlayerMask, EnemyMask, etc.) | 🟠 | PENDIENTE |
| QA-04 | Verificar que los ScriptableObjects SO_Weapon_*.asset tienen datos válidos | 🟡 | PENDIENTE |
| QA-05 | Verificar que SO_WaveConfig_Default.asset referencia prefabs de enemigos correctos | 🟠 | PENDIENTE |
| QA-06 | Buscar referencias rotas en escenas | 🟠 | PENDIENTE |
| QA-07 | Verificar que los animation clips de enemigos tienen frames asignados | 🟠 | PENDIENTE |
| QA-08 | Verificar que los triggers de Animator del Player coinciden con los definidos en AC_Player.controller | 🟡 | PENDIENTE |
| QA-09 | Corregir todos los errores/warnings detectados | 🔴 | PENDIENTE |

---

## FASE 6 — DOCUMENTACIÓN

| ID | Tarea | Prioridad | Estado |
|---|---|---|---|
| DOC-01 | Generar walkthrough.md con arquitectura completa | 🟡 | PENDIENTE |
| DOC-02 | Generar CHANGELOG.md con todas las modificaciones realizadas | 🟡 | PENDIENTE |
| DOC-03 | Documentar cómo agregar un nuevo tipo de enemigo | 🟡 | PENDIENTE |
| DOC-04 | Documentar cómo agregar nuevas animaciones al sistema de enemigos | 🟡 | PENDIENTE |
| DOC-05 | Documentar cómo compilar y ejecutar el juego | 🟡 | PENDIENTE |

---

## RESUMEN DE TOTALES

| Prioridad | Cantidad |
|---|---|
| 🔴 CRÍTICO | 14 |
| 🟠 ALTA | 28 |
| 🟡 MEDIA | 18 |
| 🟢 BAJA | 0 |
| **TOTAL** | **60** |

---

## ORDEN DE EJECUCIÓN RECOMENDADO

1. **C-01 a C-06** — Agregar Animator a EnemyBase (prerequisito de todo lo demás)
2. **A-01 a A-10** — Crear Animator Controllers para todos los enemigos
3. **D-01 a D-08** — Arte + clips + integración Enemy Drone (más simple, sirve como plantilla)
4. **B-01 a B-08, Q-01 a Q-06, M-01 a M-06, PA-01 a PA-06** — Arte resto de enemigos
5. **C-07 a C-12** — Conectar triggers en state machine
6. **C-13 a C-20** — Sistemas secundarios (evolución, boss, subclases)
7. **QA-01 a QA-09** — Verificación y corrección
8. **DOC-01 a DOC-05** — Documentación final