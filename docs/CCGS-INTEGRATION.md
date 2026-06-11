# Integración Claude Code Game Studios

Basado en el template MIT [Donchitos/Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) (49 agentes, 73 skills), adaptado para **Cursor** y **Mutation Swarm 2D**.

## Qué se importó

| Original (Claude Code) | En este repo (Cursor) |
|------------------------|------------------------|
| `CLAUDE.md` | `AGENTS.md` |
| `.claude/rules/*.md` | `.cursor/rules/*.mdc` (paths Unity) |
| `.claude/skills/*` (subset) | `.cursor/skills/mutation-*` |
| `design/`, `production/` | Carpetas creadas con contenido del juego |

## Qué no se copió entero

- **Hooks** (`.claude/hooks/*.sh`): pensados para Claude Code; en Cursor usa reglas + revisión manual.
- **73 skills completos**: solo los más útiles para este proyecto; el resto está documentado en `ccgs-help`.
- **Agentes `.md`**: la jerarquía se resume en `AGENTS.md`; agentes Unity completos en `_ccgs-template/.claude/agents/` (referencia local).

## Carpeta `_ccgs-template/`

Clon superficial del upstream para consulta. **No subir a git** (está en `.gitignore`). Para actualizar:

```powershell
Remove-Item -Recurse -Force _ccgs-template
git clone --depth 1 https://github.com/Donchitos/Claude-Code-Game-Studios.git _ccgs-template
```

## Usar el template completo en Claude Code

Si además usas Claude Code en la misma máquina:

```bash
git clone https://github.com/Donchitos/Claude-Code-Game-Studios.git
cd Claude-Code-Game-Studios
claude
# /start → /setup-engine unity
# /adopt  (para proyectos existentes)
```

## Mapeo de paths CCGS → Mutation2D

| CCGS | Mutation2D |
|------|------------|
| `src/gameplay/**` | `Assets/_Scripts/{Entities,Combat,Evolution}/**` |
| `src/core/**` | `Assets/_Scripts/Core/**` |
| `src/ui/**` | `Assets/_Scripts/UI/**` |
| `assets/data/**` | `Assets/_ScriptableObjects/**` |
| `design/gdd/**` | `design/gdd/**` |

## Créditos

Template por [Donchitos](https://github.com/Donchitos) — MIT License.
