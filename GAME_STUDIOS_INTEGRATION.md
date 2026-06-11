# GAME STUDIOS INTEGRATION — Mutation Swarm 2D
**Fecha:** 2026-06-01  
**Fuente:** https://github.com/Donchitos/Claude-Code-Game-Studios  
**Rama:** main (commit shallow clone)

---

## Descripción del Repositorio

**Claude Code Game Studios (CCGS)** es un framework de arquitectura de desarrollo de videojuegos indie que opera a través de 49 agentes Claude Code coordinados. Cada agente posee un dominio específico (Unity Specialist, Technical Artist, QA Lead, etc.), enforcing separación de responsabilidades y calidad.

---

## Estructura del Repositorio Analizada

```
Claude-Code-Game-Studios/
├── .claude/
│   ├── agents/          ← 50 definiciones de agentes especializados
│   ├── docs/            ← Estándares, reglas de coordinación, referencias
│   └── agent-memory/    ← Memoria persistente por agente
├── docs/
│   ├── engine-reference/
│   │   ├── unity/       ← Referencia Unity 6.3 LTS (Animation, Input, Physics, etc.)
│   │   ├── godot/       ← Referencia Godot 4
│   │   └── unreal/      ← Referencia Unreal Engine 5
│   ├── templates/       ← Plantillas de documentación
│   └── examples/        ← Ejemplos de workflows
├── CCGS Skill Testing Framework/
│   └── skills/          ← Skills organizadas en 9 categorías
└── design/              ← Registry de entidades y decisiones
```

---

## Componentes Integrados al Proyecto

### 1. Agentes Especializados

Copiados a: `c:\Game\Mutation2D\.claude\agents\`

| Agente | Archivo | Uso en este Proyecto |
|---|---|---|
| **Unity Specialist** | `unity-specialist.md` | Guía de APIs de Unity 6, mejores prácticas de Animator, ScriptableObjects, Input System |
| **Technical Artist** | `technical-artist.md` | Pipeline de arte, spritesheets, optimización de texturas, VFX |
| **Gameplay Programmer** | `gameplay-programmer.md` | Implementación de mecánicas de enemigos, sistema de combate, integración de animaciones |
| **QA Lead** | `qa-lead.md` | Plan de pruebas, triage de bugs, gates de calidad, smoke checks |
| **Lead Programmer** | `lead-programmer.md` | Supervisión arquitectural, revisión de código, estándares técnicos |

**Cómo usar los agentes:** En cualquier sesión futura de Claude Code, estos agentes están disponibles como subagentes especializados que pueden ser invocados para tareas específicas de su dominio.

---

### 2. Documentación de Referencia Unity 6.3 LTS

Copiada a: `c:\Game\Mutation2D\.claude\docs\engine-reference\unity\`

| Documento | Uso |
|---|---|
| `VERSION.md` | Referencia de versión Unity 6000.3.8f1, cambios post-2022 LTS |
| `animation.md` | API de Animator, Blend Trees, Animation Events, Animation Rigging |

**Por qué es crítico:** El LLM tiene conocimiento hasta ~2022 LTS. Unity 6 introdujo cambios significativos. Este documento sirve como referencia autoritativa para APIs actuales (e.g., `linearVelocity` en lugar de `velocity` para Rigidbody2D en Unity 6).

---

### 3. Estándares de Código

Copiado a: `c:\Game\Mutation2D\.claude\docs\coding-standards.md`

Estándares aplicables a este proyecto:
- Todos los valores de gameplay deben ser data-driven (ScriptableObjects) — **ya implementado**
- Public APIs deben tener doc comments — **aplicar en Fases 3-4**
- Conventional Commits: `feat:`, `fix:`, `chore:`, etc.
- Testing por tipo de historia (Logic → unit test, Visual → screenshot + sign-off)

---

## Catálogo de Skills Disponibles (No Copiadas, Disponibles como Referencia)

El repositorio incluye las siguientes categorías de skills:

### Skills de Análisis (`analysis/`)
| Skill | Uso Potencial |
|---|---|
| `asset-audit.md` | Auditar todos los assets de arte para detectar faltantes |
| `code-review.md` | Revisión formal de código modificado |
| `consistency-check.md` | Verificar coherencia artística entre enemy sprites |
| `tech-debt.md` | Identificar y cuantificar deuda técnica en scripts placeholders |

### Skills de Pipeline (`pipeline/`)
| Skill | Uso Potencial |
|---|---|
| `create-epics.md` | Convertir TASKS_DETECTED.md en épicas estructuradas |
| `create-stories.md` | Dividir las fases en historias de usuario con criterios de aceptación |
| `dev-story.md` | Template para historias de desarrollo técnico |

### Skills de QA (`utility/`)
| Skill | Uso Potencial |
|---|---|
| `qa-plan.md` | Generar plan de QA por sprint |
| `smoke-check.md` | Ejecutar smoke test antes de builds |
| `bug-report.md` | Template formal de bug reports |
| `regression-suite.md` | Suite de regresión para proteger funcionalidad existente |
| `playtest-report.md` | Reporte de playtesting |

### Skills de Documentación (`authoring/`)
| Skill | Uso Potencial |
|---|---|
| `create-architecture.md` | Documentar arquitectura del sistema de animaciones |
| `design-system.md` | Documentar el sistema de evolución genética |

---

## Referencia de Motor Unity 6.3 LTS (Resumen)

El módulo de animación de CCGS documenta las siguientes APIs críticas para este proyecto:

### Animator Controller (Mecanim)
```csharp
// Setup estándar en Unity 6
Animator _animator = GetComponent<Animator>();
_animator.SetTrigger("Attack");
_animator.SetBool("IsMoving", true);
_animator.CrossFade("Die", 0.1f);  // transición suave
```

### Animation Events (Recomendado para EnemyBase)
```csharp
// En Animation window: Add Animation Event en el frame de impacto
public void OnAttackLand() { DealDamageInArea(); }
public void OnDeathComplete() { Destroy(gameObject); }
```

### Performance — Culling Mode
```
Animator > Culling Mode: Cull Update Transforms (RECOMENDADO para enemigos)
```

---

## Workflows Relevantes Identificados

Del análisis del repositorio CCGS, los siguientes workflows son aplicables al proyecto:

### 1. Workflow de Adopción de Proyecto Existente (Brownfield)
Referencia: `docs/examples/session-adopt-brownfield.md`
- Auditoría completa antes de tocar código ✅ (ya ejecutado)
- Mapear sistemas existentes antes de agregar ✅ (documentado en ANALYSIS_REPORT.md)
- Reverse-document el código existente antes de modificarlo

### 2. Workflow de Implementación de Combate
Referencia: `docs/examples/session-implement-combat-damage.md`
- Definir contratos de interfaz antes de implementar
- Escribir tests unitarios para fórmulas de daño
- Usar Animation Events para timing de hit detection

### 3. Workflow de QA
Referencia: `docs/examples/session-gate-check-phase-transition.md`
- Smoke check antes de cada merge
- Gates de calidad antes de cambiar de fase

---

## Decisiones de Integración

### ¿Por qué solo 5 agentes?
Se priorizaron los agentes directamente relevantes para las 6 fases del proyecto:
- **Unity Specialist** y **Technical Artist**: Fases 1-2 (arte y animaciones)
- **Gameplay Programmer**: Fases 3-4 (integración de código)
- **QA Lead**: Fase 5 (verificación)
- **Lead Programmer**: Supervisión transversal

Los 45 agentes restantes (Network, Economy, Localization, etc.) no son relevantes para Mutation Swarm 2D en su estado actual.

### ¿Por qué solo los docs de Unity Animation?
El proyecto usa exclusivamente el Animator Controller (Mecanim) con sprites 2D. Los módulos de Audio, Physics y UI de Unity 6 ya están operativos; la referencia de animación es el único documento crítico para el trabajo de las Fases 1-4.

---

## Próximos Pasos con estas Herramientas

1. **Usar `unity-specialist` agent** para validar la estructura de los Animator Controllers de enemigos antes de crearlos
2. **Usar `technical-artist` agent** para definir especificaciones de pixel art (tamaño de frame, paleta) antes de generar spritesheets
3. **Usar `qa-lead` agent** para crear el plan de QA de la Fase 5
4. **Usar `animation.md`** como referencia autoritativa de APIs al modificar `Script_13_EnemyBase.cs`
