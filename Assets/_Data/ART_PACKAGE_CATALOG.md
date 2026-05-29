# Catálogo — Paquete Pixel Art (`Assets/_Art/Materials`)

Paquete estilo **plataformas 2D** (césped/tierra, zona púrpura mutada, bloques galaxia, fondos parallax y decoración).  
Configuración recomendada en Unity: **Sprite Mode = Single**, **PPU = 16**, **Filter = Point**, **Compression = None**.

---

## Background (9 archivos) — Parallax, sin colisión

| Archivo | Uso recomendado | Notas |
|---------|-----------------|-------|
| `Starry_Night_Big.png` | Fondo lejano (cielo estrellado) | Imagen grande; ideal como capa 0, sorting -200 |
| `GalaxyBackground.png` | Fondo alternativo espacial | Tono morado/azul, temática mutación |
| `Blue_Background_LINEAR.png` | Gradiente vertical azul | Relleno de cielo o transición |
| `Blue_Background_LINEAR_Simple.png` | Variante más simple del gradiente | Menos detalle, mejor rendimiento |
| `LayerBackground.png` | Montañas/siluetas lejanas | Parallax lento |
| `LayerBackground2.png` | Segunda capa de siluetas | Combinar con LayerBackground |
| `Layer1.png` | Colinas + árboles azules (silueta) | Parallax medio, sorting -120 |
| `Layer2.png` | Detalle medio parallax | sorting -110 |
| `Layer3.png` | Detalle cercano parallax | sorting -100 |

---

## Blocks (18 archivos) — Plataformas con colisión (`Platform` + `BuildSurface`)

### Set césped orgánico (principal del nivel)
| Archivo | Rol en tileset |
|---------|----------------|
| `Grassy_Top.png` | Superficie caminable (césped) |
| `Grassy_Fill.png` | Relleno interior de plataforma |
| `Grassy_Side.png` | Borde lateral / terminación |
| `Grassy_Edge.png` | Esquina y bordes irregulares |

### Set bloque 16×16 (estilo Minecraft-lite)
| Archivo | Rol |
|---------|-----|
| `Grass_Block_Top.png` | Cara superior |
| `Grass_Block_Side.png` | Cara lateral |
| `Grass_Block_Bottom.png` | Cara inferior |

### Set tierra púrpura (zona mutada)
| Archivo | Rol |
|---------|-----|
| `Dirt_Purple_Top` → usar `Dirt_Purple_Fill` + bordes | Zona de mutación |
| `Dirt_Purple_Fill.png` | Interior púrpura |
| `Dirt_Purple_Side.png` | Lateral |
| `Dirt_Purple_Corner.png` | Esquina |
| `Dirt_Purple_Bottom.png` | Parte inferior colgante |

### Set galaxia (plataformas flotantes especiales)
| Archivo | Rol |
|---------|-----|
| `Galaxy_Block_1.png` | Bloque espacial base |
| `Galaxy_Block_2.png` | Variante con más brillo |
| `Galaxy_Block_3.png` | Variante estándar |
| `Galaxy_Block_3_Purple.png` | Variante mutada |

### Otros
| Archivo | Rol |
|---------|-----|
| `Brick_Purple.png` | Muro decorativo / estructura |

---

## Tiles (4 archivos) — Texturas repetibles (suelo amplio)

| Archivo | Uso |
|---------|-----|
| `Grassy_Pattern.png` | Patrón grande de césped para suelos extensos |
| `Grass_Block_PATTERN.png` | Patrón de bloques de hierba |
| `Grass_Block_PATTERN+.png` | Variante con más detalle |
| `Purple_Ground_Pattern.png` | Suelo de zona púrpura |

---

## Grass (2 archivos) — Detalle decorativo

| Archivo | Uso |
|---------|-----|
| `Grass.png` | Mechón de hierba (decoración en bordes) |
| `PixelGrass.png` | Hierba pequeña pixel (overlay) |

---

## Trees (6 archivos) — Decoración sin colisión

| Archivo | Descripción |
|---------|-------------|
| `PixelTree1.png` | Árbol alto, copa redonda |
| `PixelTree2.png` | Variante altura media |
| `PixelTree3.png` | Variante delgada |
| `PixelTree4.png` | Copa más ancha |
| `PixelTree5.png` | Arbusto / árbol bajo |
| `PixelTree6.png` | Sapling pequeño |

---

## Objects (6 archivos) — Props de escena

| Archivo | Uso |
|---------|-----|
| `Rock_1.png` | Roca mediana |
| `Rock_2.png` | Roca pequeña |
| `Rock_3.png` | Roca grande |
| `Cave_Pillar.png` | Pilar de cueva |
| `Cave_Pillar_Small.png` | Pilar pequeño |
| `Green_Old_Block.png` | Bloque antiguo musgoso (ruina) |

---

## Examples (2 archivos) — Referencia del autor

| Archivo | Contenido |
|---------|-----------|
| `Tiles.png` | Hoja de referencia del tileset completo |
| `LightedScene1.png` | Mockup de escena iluminada (estilo industrial) |

---

## Distribución en `Scene_02_GameWorld` (nivel generado)

- **Cielo:** `Starry_Night_Big` + `GalaxyBackground` (parallax).
- **Medio:** `Layer1` → `Layer2` → `Layer3`.
- **Suelo principal:** `Grassy_Top` + `Grassy_Fill` (y `Grassy_Side` en extremos).
- **Plataformas altas:** mismos bloques césped.
- **Zona central mutada:** `Dirt_Purple_*` + `Purple_Ground_Pattern`.
- **Plataformas especiales:** `Galaxy_Block_*`.
- **Decoración:** árboles `PixelTree*`, rocas, pilares, hierba `PixelGrass`.

Generar en Unity: **`Tools → Mutation Swarm → Build Art Level (Scene_02)`**
