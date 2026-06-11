# Art Bible — Mutation Swarm 2D (borrador)

> Generado para el flujo CCGS / skill `mutation-visual-art`. Completar con el equipo.

## 1. Identidad visual

- **Fantasía:** laboratorio Argos-9, enjambre sintético que muta en tiempo real.
- **Lectura:** el jugador debe identificar en <1 s oleada, mutación enemiga y peligro.
- **Tono:** sci-fi contenido, no horror; UI clara (Kenney space, variantes Blue/Green/Yellow).

## 2. Paleta

| Rol | Color referencia | Uso |
|-----|------------------|-----|
| Jugador | Cian/azul `#33BFFF` | Cuadrado/sprite héroe, amigable |
| Enemigo base | Rojo `#FF5959` | Círculo/forma simple, legible |
| Mutación fuerte | Púrpura + shift en `Script_13` | Genome alto |
| Entorno | Pack `Assets/_Art/Materials` + Kenney amarillo | Plataformas y UI |
| HUD | `HUD_Main_Light.uss` | Fondo claro, alto contraste |

## 3. Personajes y enemigos

- **Proporción:** cabeza/cuerpo ~40/60 en humanoide; enemigos redondos = hitbox circular.
- **Mutación:** no redibujar todo el sprite por oleada — **tinte + escala + VFX** salvo bosses.
- **Siluetas distintas:** Queen > Boss > Drone; Mimic copia silueta jugador con detalle “incorrecto”.

## 4. Animación (Unity Animator)

Parámetros alineados con `Script_11_PlayerController`:

- Movimiento: `Speed`, `IsGrounded`, `IsJumping`
- Acciones: `Dash`, `Shoot`, `Hit`, `Death`

Enemigos sin hoja completa: **flip X** + squash opcional en impacto; bosses con clips dedicados.

## 5. Assets y PPU

| Tipo | PPU | Filter |
|------|-----|--------|
| Pixel pack Materials | 16 | Point |
| Sprites generados / geo | 64 | Point |
| Kenney SVG UI | 100 | Bilinear |

## 6. Prohibiciones de estilo

- No mezclar pixel 16px con vector Kenney en el **mismo personaje**.
- No UI oscura Grey/Red del pack Kenney en menú principal (usar Blue/Green/Yellow/Extra).
- No texto de gameplay hardcodeado en sprites.

## 7. Pipeline

1. Spec en `design/assets/specs/`
2. Arte en `Assets/_Art/Sprites/...`
3. Animator en `Assets/_Art/Animations/...`
4. Prefab actualizado en `Assets/_Prefabs/`
