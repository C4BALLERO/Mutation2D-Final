---
name: ccgs-help
description: Índice de workflows Claude Code Game Studios aplicables a Mutation Swarm. Usar cuando pregunten qué skill o flujo usar del template CCGS.
---

# CCGS Help (adaptado a Cursor)

Template upstream: https://github.com/Donchitos/Claude-Code-Game-Studios

## Skills locales (este repo)

| Necesidad | Skill Cursor |
|-----------|----------------|
| Empezar | `mutation-start` |
| ¿Qué falta? | `mutation-project-stage` |
| Balance oleadas/genomas | `mutation-balance-check` |
| Compilar exe | `mutation-build` |
| Arte / personajes / animaciones | `mutation-visual-art` |

## Equivalentes CCGS (Claude Code `/comando`)

Útiles si usas Claude Code con el template completo en `_ccgs-template/`:

| Comando | Uso en Mutation Swarm |
|---------|------------------------|
| `/adopt` | Adoptar repo existente |
| `/setup-engine unity` | Referencia Unity |
| `/reverse-document` | Generar GDD desde código existente |
| `/dev-story` | Implementar historia de sprint |
| `/code-review` | Revisión antes de merge |
| `/qa-plan` | Plan de pruebas del loop de juego |
| `/perf-profile` | Profiler Unity / builds |
| `/team-combat` | Feature de combate multi-agente |
| `/gate-check` | Cambiar etapa en `production/stage.txt` |

## Agentes Unity (referencia)

En `_ccgs-template/.claude/agents/`:

- `unity-specialist.md`
- `unity-ui-specialist.md`
- `unity-shader-specialist.md`

## Documentación

- `docs/CCGS-INTEGRATION.md`
- `AGENTS.md`
