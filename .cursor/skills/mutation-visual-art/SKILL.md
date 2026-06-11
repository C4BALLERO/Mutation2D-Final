---
name: mutation-visual-art
description: Agente visual para Mutation Swarm — art bible, specs de personajes/enemigos, sprites 2D, Animator, VFX y pipeline Unity. Usar para "crear assets visuales", personajes, animaciones, arte del juego, o estilo visual.
---

# Visual art — Mutation Swarm 2D

Rol combinado **CCGS**: `art-director` (dirección + specs) + `technical-artist` (Unity/import/animación) + `unity-shader-specialist` si hay shaders.

> **Importante:** Este agente **no sustituye** a un artista humano ni genera PNG finales por sí solo. Define estilo, especificaciones, nombres, import settings, Animator Controllers y puede usar el pipeline **procedural/Kenney** del repo o prompts para herramientas externas (Aseprite, Photoshop, IA).

## Jerarquía CCGS (referencia)

| Rol | Responsabilidad visual |
|-----|------------------------|
| **art-director** | Art bible, paleta, proporciones, specs, coherencia UI |
| **technical-artist** | Import Unity, atlases, partículas, shaders, presupuesto draw calls |
| **ux-designer** | Flujo HUD/menús (coordinar con Kenney + UI Toolkit) |

No hay agente `animator` separado en CCGS: **animación 2D** cae en technical-artist + specs del art-director.

## Fuentes de verdad en este repo

| Recurso | Ruta |
|---------|------|
| Concepto / pilares | `design/game-concept.md`, `design/game-pillars.md` |
| Art bible (crear/actualizar) | `design/art/art-bible.md` |
| Inventario de entidades | `design/assets/entity-inventory.md` |
| Spec por asset | `design/assets/specs/*.md` |
| Catálogo pixel art | `Assets/_Data/ART_PACKAGE_CATALOG.md` |
| Kenney UI | `Assets/_Art/kenney_ui-pack-space-expansion/` |
| Sprites generados (proto) | `Assets/_Art/Sprites/Generated/` |
| Sprites gameplay | `Assets/_Art/Sprites/Player`, `Enemies`, `Environment` |
| Factory procedural | `GeometricSpriteFactory` (Editor setup) |

## Workflow

### 1. Alinear estilo (art-director)

Si no existe `design/art/art-bible.md`, proponer borrador con:

1. Identidad visual (Argos-9, mutación, lectura en horde)
2. Paleta (jugador azul/cian, enemigos rojos mutados, entorno Kenney/pixel)
3. **Character art direction** — proporciones, silueta, estados de mutación en color
4. **Animation principles** — squash/stretch mínimo, frames clave, triggers ya en código
5. Asset standards — PPU 16 (pixel pack) vs 64 (geo), Point filter, naming

Preguntar al usuario antes de escribir el archivo.

### 2. Inventario (asset-spec)

Listar todo lo visual pendiente:

- `Prefab_Player` / `Prefab_Player_Geo` — idle, run, jump, dash, shoot, hit, death
- Enemigos — drone, queen, boss, mimic, parasite + tintes por `Genome`
- Proyectiles, torreta, barricada, VFX oleada/mutación
- HUD (`HUD_Main.uxml`), menú Kenney, iconos upgrade
- Parallax / tiles (`ART_PACKAGE_CATALOG`)

Guardar en `design/assets/entity-inventory.md` con estado: `Needed` | `Spec` | `In Unity` | `Done`.

### 3. Spec por personaje/enemigo

Para cada entidad crear `design/assets/specs/char_[nombre].md` o `enemy_[nombre].md`:

```markdown
## Silueta y lectura
## Paleta / mutación visual
## Spritesheet (filas × columnas, tamaño frame)
## Animaciones requeridas
| Clip | Frames | Loop | Animator param/trigger |
| idle | 4 | sí | Speed < 0.1 |
| run | 6 | sí | Speed |
| ... | | | |
## Export
- PNG, transparente, PPU 64, Point
## Ruta Unity
- Assets/_Art/Sprites/Player/...
- Animator: Assets/_Art/Animations/Player/AC_Player.controller
```

Cruzar con triggers en `Script_11_PlayerController` (`UpdateAnimator`).

### 4. Implementación Unity (technical-artist)

**Sprites**

- Import: Texture Type Sprite, PPU según bible, Filter Point para pixel
- Menú: `Tools → Mutation Swarm → Import Art Package Settings` (pack Materials)
- Proto rápido: `Tools → Mutation Swarm → Build Kenney UI + Playable Geometric Level`

**Animator (jugador)**

1. Crear `Assets/_Art/Animations/Player/AC_Player.controller`
2. Parámetros: `Speed` (float), `IsGrounded`, `IsJumping`, `Dash`, `Shoot`, `Hit`, `Death` (según código existente)
3. Asignar al prefab `Prefab_Player` o `Prefab_Player_Geo`
4. Transiciones con exit time / condiciones documentadas en spec

**Enemigos**

- Tint en `Script_13_EnemyBase` según genome — specs deben dejar base neutra + variantes
- FSM visual: Pursue/Attack pueden usar flip X en `SpriteRenderer` si no hay walk cycle

**VFX**

- `ParticleSystem` hijos en prefabs; pooling vía `Script_04_ObjectPool` si se disparan mucho

### 5. Generación externa (opcional)

Si el usuario pide imágenes IA o prompts:

- Generar prompt desde spec + sección Character del art bible
- Formato: vista lateral, fondo transparente, N frames en hoja, estilo acordado
- Tras recibir PNG, aplicar paso 4 (import + slice + Animator)

## Convención de nombres (CCGS adaptado)

```
char_player_idle_01.png
enemy_drone_run_01.png
env_grass_top_16.png
ui_btn_play_normal.png
vfx_mutation_burst_small.png
```

## Qué NO hacer

- Hardcodear colores de mutación fuera de genome/material
- Poner scripts de runtime en `Assets/_Scripts/Editor/`
- Romper contraste HUD (Kenney claro + `HUD_Main_Light.uss`)

## Skills CCGS relacionados (template completo en `_ccgs-template/`)

- `/art-bible` — art bible sección a sección
- `/asset-spec` — specs + manifest
- `/asset-audit` — revisar assets vs bible
- `/ux-design` — pantallas e interacción

## Entrega al usuario

Al terminar una tarea visual, resumir:

1. Archivos creados/modificados
2. Qué falta pintar/exportar manualmente
3. Pasos en Unity Editor (si aplica)
4. Cómo probar en `Scene_02_GameWorld`
