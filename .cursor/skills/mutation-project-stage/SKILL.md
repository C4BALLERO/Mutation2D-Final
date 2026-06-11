---
name: mutation-project-stage
description: Audita etapa de desarrollo de Mutation Swarm — compara design/, código Unity y production/. Usar con "¿en qué fase estamos?" o "auditoría del proyecto".
---

# Project stage — Mutation Swarm

## 1. Override manual

Si existe `production/stage.txt`, usar su valor como etapa.

## 2. Escanear

**Diseño** (`design/`):

- `game-concept.md`, `game-pillars.md`, `systems-index.md`
- GDDs en `design/gdd/*.md`

**Código** (`Assets/_Scripts/`):

- Contar `.cs` por carpeta (Core, Evolution, Entities, Combat, Building, UI, Editor)
- Listar scripts stub o TODOs críticos

**Producción** (`production/`):

- `stage.txt`, `review-mode.txt`
- Sprint plans si existen

**Build**:

- ¿Existe `Builds/Windows/MutationSwarm.exe`?
- Revisar `Logs/batch-*.log` si hay fallos recientes

## 3. Clasificar etapa

| Etapa | Indicadores en este repo |
|-------|---------------------------|
| Pre-Production | Pocas escenas, sin WaveManager funcional |
| **Production** | 30+ scripts, oleadas + evolución + menú Kenney |
| Polish | Build estable, pendiente solo arte/audio/meta |
| Release | Checklist release, Steam, etc. |

## 4. Informe (formato)

```markdown
## Etapa: [X]
## Fortalezas
- ...
## Huecos (preguntar, no asumir)
- ...
## Siguiente skill sugerida
- mutation-balance-check | mutation-build | (tarea concreta)
```

Colaborativo: preguntar si huecos son intencionales (prototipo geométrico).
