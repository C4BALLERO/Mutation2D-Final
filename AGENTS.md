# Mutation Swarm 2D — AI Studio (CCGS + Cursor)

Este proyecto usa una adaptación de [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) para **Cursor**: reglas por carpeta, skills de workflow y documentación de diseño.

## Stack

| Campo | Valor |
|-------|--------|
| Motor | Unity 6000.3.x (2D URP) |
| Lenguaje | C# |
| UI | UI Toolkit (HUD) + uGUI Kenney (menú) |
| Repo | `Illanes09/Mutation2D-Final` |
| Build | `build.ps1` → `Builds/Windows/MutationSwarm.exe` |

## Estructura del código

```
Assets/_Scripts/
  Core/       Script_01–06, pools, boot, audio
  Evolution/  Genome, selección, presión adaptativa
  Entities/   Player, enemigos, FSM
  Combat/     Armas, daño, estados
  Building/   Construcción entre oleadas
  UI/         HUD, menús, Kenney uGUI
  Editor/     Setup escenas, build pipeline (solo Editor)
Assets/_Scenes/     Scene_00_Boot … Scene_03_UpgradeMenu
Assets/_Prefabs/    Player, Enemies, Projectiles, Structures
Assets/_ScriptableObjects/
design/             GDDs, pilares, balance (CCGS)
production/         Etapa, sprints, review-mode
```

## Convenciones (obligatorias)

- Scripts: `Script_XX_Nombre.cs` con namespace `MutationSwarm.*`
- Escenas: `Scene_XX_Nombre.unity`
- Comunicación gameplay ↔ UI: `Script_03_EventBus`, no `FindObjectOfType`
- Valores de balance: `ScriptableObject` o datos en `Assets/_ScriptableObjects/`, no magic numbers en gameplay
- Singletons de sesión: `Script_01_GameManager`, `DontDestroyOnLoad` solo donde ya está definido

## Jerarquía de agentes (referencia CCGS)

Usa el rol mental adecuado según la tarea:

| Rol | Cuándo |
|-----|--------|
| **creative-director** | Visión, pilares, coherencia narrativa Argos-9 |
| **game-designer** | Oleadas, evolución, economía de materiales |
| **unity-specialist** | API Unity, prefabs, capas, build batch |
| **gameplay-programmer** | Controller, FSM, combate, pooling |
| **qa-tester** | Regresiones, build, smoke del loop oleada→upgrade |
| **art-director** + **technical-artist** | Art bible, specs, sprites, Animator, VFX (skill `mutation-visual-art`) |

## Protocolo de colaboración

1. Preguntar si falta contexto de diseño
2. Proponer opciones con pros/contras en cambios grandes
3. Cambios mínimos y alineados con código existente
4. No commitear salvo petición explícita

## Skills de Cursor (`.cursor/skills/`)

| Skill | Uso |
|-------|-----|
| `mutation-start` | Onboarding del proyecto y próximos pasos |
| `mutation-project-stage` | Auditoría de etapa y huecos (GDD vs código) |
| `mutation-balance-check` | Revisar oleadas, genome, daño |
| `mutation-build` | Generar contenido + exe Windows |
| `mutation-visual-art` | Personajes, sprites, animaciones, art bible, pipeline visual |
| `ccgs-help` | Índice de workflows CCGS aplicables |

## Documentación

- Diseño: `design/`
- Integración CCGS: `docs/CCGS-INTEGRATION.md`
- Catálogo arte: `Assets/_Data/ART_PACKAGE_CATALOG.md`
