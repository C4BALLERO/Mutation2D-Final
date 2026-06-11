# ROOM LAYOUTS — Mutation Swarm 2D
**Versión:** 1.0 · **Fecha:** 2026-06-02

Este documento resume los layouts de todas las habitaciones.  
Para los diagramas completos y tablas de plataformas, ver [ROOMS_DESIGN.md](ROOMS_DESIGN.md).

---

## Especificaciones de Cámara

| Campo | Valor |
|---|---|
| Tipo | Ortográfica |
| Orthographic Size | 5.4 |
| Viewport | 19.2 × 10.8 unidades |
| Tile | 1 unidad (16 PPU) |
| Límites | Paredes ±9.5 · Suelo -4.0 · Techo 4.5 |

**Regla de diseño:** Todo el nivel debe ser visible sin mover la cámara.

---

## Resumen de Habitaciones

### Room_01 — Plataformas Fácil

```
Y=4.5 [==========TECHO===========]
Y=2.5         [GX high]
Y=1.5  [LH]           [RH]
Y=0.0        [Center]
      [MOV→←]
Y=-1.5 [LL]          [RL]
Y=-3.0 sp_L               sp_R
Y=-4.0 [=========SUELO==========]
```

**Elementos:** Plataforma móvil horizontal · 2 spawners · Checkpoint · Salida · Cofre en GX high

---

### Room_02 — Combate Medio

```
Y=4.5  [==========TECHO===========]
sp_TL          [GX_Top]      sp_TR
Y=1.0  [L]   [LM]  [RM]  [R]
Y=-1.0        [Purple Center]
Y=-3.0 ■CKP  p1-p4          ■EXIT
Y=-4.0  [=========SUELO==========]
sp_BL
```

**Elementos:** Plataforma central elevada · 5 spawners · Salida bloqueada hasta limpiar oleada

---

### Room_03 — Secreto Medio

```
Y=4.5  [Ceil_L]    BRECHA    [Ceil_R]
Y=4.0  [Sec_L] 🎁  🎁  [Sec_R]
Y=2.5  ■IW_L      [Plat_D]
Y=1.5        [Plat_C Purp]
Y=-0.5               [Plat_B]
Y=-1.5  [Plat_A]
Y=-3.0  ■CKP             ■EXIT
Y=-4.0  [=========SUELO==========]
```

**Elementos:** Brecha en techo → zona secreta con cofres y spawn secreto · 4 spawners · Inner wall

---

### Room_04 — Mixto Difícil

```
Y=4.5  [==========TECHO===========]
sp_TL  [Gal7]       [Gal6]  sp_TR
Y=2.5         [Plat_5]
Y=1.5  [Plat_4 Purp]  [Destr_2]
Y=0.0  [Plat_3]
Y=-0.5  [Plat_2]
Y=-2.0  [Plat_1]  [MOV→←]  [Destr_1]
Y=-3.0  ■CKP   p1-p4           sp_BL
Y=-4.0  [=====Floor 16t=========]
```

**Elementos:** 2 plataformas destructibles · 1 móvil · 6 spawners · Queen enemy · Cofres en Gal

---

### Room_05 — Plataformas Muy Difícil

```
Y=4.5 [=TECHO===sp_Center========TECHO=]
Y=3.5     [GX_Top] 🎁 ■EXIT
Y=3.0              [Plat_5_GX]
Y=2.0  [Plat_4]
Y=1.5  [MOV_V↕]        [Plat_6]
Y=0.5          [Plat_3B]
Y=0.0   [Plat_3 Purp]
Y=-0.5   [Plat_2]
Y=-1.5              [MOV_H→←]
Y=-2.0   [Plat_1]
Y=-3.0       [P_0 GX]  sp_Gap
Y=-4.0 [FL5t] ~~BRECHA~~ [FR6t]
■CKP  player_spawns
```

**Elementos:** Brecha en el suelo · 2 móviles (vertical + horizontal) · 4 spawners · Parasite enemy

---

### Room_Boss — Arena del Jefe

```
Y=4.5  [==========TECHO===========]
sp_TL      sp_TC  ■EXIT      sp_TR
Y=3.0  [GX_L3]           [GX_R3]
Y=1.5       [CL Pur] [CR Pur]
Y=0.5  [Plat_L2]       [Plat_R2]
Y=-1.5 [Plat_L1]       [Plat_R1]
       [MOV_L] [MutZone] [MOV_R]
Y=-3.0 🎁  ■CKP  p1-p4   🎁
sp_BL [===ARENA PURPLE FLOOR===] sp_BR
```

**Elementos:** 2 móviles flanqueando · Mutation Zone central · 7 spawners · Boss 3 fases · 2 cofres

---

## Prefabs de Habitaciones

Ubicación: `Assets/_Prefabs/Rooms/`

| Prefab | Componente | Descripción |
|---|---|---|
| `SpawnPoint.prefab` | Script_SpawnPointGizmos | Marcador de spawn con gizmo en editor |
| `Checkpoint.prefab` | CheckpointController | Activa al entrar → guarda posición de respawn |
| `LevelExit.prefab` | LevelExit | Portal hacia siguiente escena (puede requerir limpiar oleada) |
| `Chest.prefab` | Chest | Cura 25 HP al abrirse · animación de rebote |
| `PlataformaMovil.prefab` | MovingPlatform | Cinemática entre 2 waypoints (Rigidbody2D Kinematic) |
| `PlataformaDestructible.prefab` | DestructiblePlatform | Tiembla 1.2s → colapsa → respawnea en 4s |

---

## Reglas de Diseño de Habitaciones

1. **Una sola pantalla** — Todo visible sin mover la cámara (19.2 × 10.8 u)
2. **Sin softlock** — El jugador siempre puede llegar al suelo o a alguna plataforma
3. **Espaciado máximo entre plataformas:**
   - Horizontal sin dash: 6 unidades
   - Horizontal con dash: 8-9 unidades
   - Vertical sin doble salto: 5-6 unidades
4. **Spawners bien distribuidos** — Nunca todos en el mismo lado
5. **Checkpoint temprano** — Dentro del primer tercio izquierdo de la pantalla
6. **Salida al final del recorrido** — Generalmente derecha o arriba

---

## Cómo Generar las Habitaciones

```
1. Asegurarse que los sprites existen:
   Tools > Mutation Swarm > Import Art Package Settings

2. Construir los prefabs y escenas:
   Tools > Mutation Swarm > Build All Rooms

3. Verificar en Build Settings que Room_01..Room_Boss están incluidas
```
