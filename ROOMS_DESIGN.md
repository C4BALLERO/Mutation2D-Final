# ROOMS DESIGN — Mutation Swarm 2D
**Última actualización:** 2026-06-01  
**Herramienta:** `Tools > Mutation Swarm > Build All Rooms`

---

## Especificaciones Técnicas

| Campo | Valor |
|---|---|
| Cámara | Ortográfica, size = 5.4 |
| Viewport | 19.2 unidades ancho × 10.8 alto |
| Tile size | 1 unidad (16 PPU) |
| Límites del nivel | Paredes ±9.5 · Suelo -4.0 · Techo 4.5 |
| Salto máximo (JumpForce=12) | ~6 unidades verticales |
| Dash (DashForce=18, 0.15s) | ~2.7 unidades horizontales |
| Velocidad horizontal | 6 unidades/s |

---

## Room_01 — Platform Easy

**Tipo:** Room_Platform  
**Dificultad:** ⭐ Fácil  
**Spawners:** 2 (bordes izquierdo y derecho del suelo)  
**Checkpoint:** (-6, -2.8) — lado izquierdo, cerca del suelo  
**Salida:** (7, -2.8) — lado derecho  
**Coleccionable:** (0, 3.8) — cofre en la plataforma Galaxy alta central  

### Diagrama
```
X=-9.5                   X=0                    X=9.5
Y=4.5  [===================TECHO====================]
       |                                            |
Y=3.0  |                  [GX top]                 |
       |                                            |
Y=2.5  |        [GR_High]            [GL_High]     |
       |                                            |
Y=1.5  |                                            |
       |                                            |
Y=0.0  |                 [Center]                  |
       |       [MOV platform →←]                   |
Y=-1.5 |  [LL]                         [RL]        |
       |                                            |
Y=-3.0 sp_L                                   sp_R |
       |      [========SUELO========]              |
Y=-4.0 [====================================================]
```

### Plataformas
| Nombre | Posición Centro | Tamaño | Tipo | One-Way |
|---|---|---|---|---|
| Floor | (0, -4) | 18×2 | Grass | No |
| Plat_LL | (-5.5, -1.5) | 4×1 | Grass | Sí |
| Plat_Center | (0, 0) | 4×1 | Grass | Sí |
| Plat_RL | (5, -1.5) | 3×1 | Grass | Sí |
| Plat_LH | (-4, 1.5) | 3×1 | Grass | Sí |
| Plat_RH | (4, 2.5) | 3×1 | Galaxy | Sí |
| Plat_Mov | (-1, -2.5) | 3×1 | Purple | Sí (móvil) |
| Walls | ±9.5 | 1×10 | Brick | No |
| Ceiling | (0, 4.5) | 20×1 | Brick | No |

**Plataforma móvil:** Plat_Mov oscila entre X=-3.5 y X=3.5 (vel. 2 u/s)  
**Notas de diseño:** Niveles accesibles para nuevos jugadores. La plataforma móvil introduce el concepto sin obstaculizar la progresión. Todos los saltos son alcanzables con la gravedad estándar.

---

## Room_02 — Combat Medium

**Tipo:** Room_Combat  
**Dificultad:** ⭐⭐⭐ Medio  
**Spawners:** 5 (esquinas + costado izquierdo)  
**Requiere:** Limpiar la oleada para abrir la salida  
**Checkpoint:** (-7, -3) — esquina inferior izquierda  
**Salida:** (7, -3) — esquina inferior derecha (bloqueada hasta matar todos los enemigos)  
**Coleccionable:** (0, 3) — cofre en plataforma Galaxy central alta  

### Diagrama
```
X=-9.5                   X=0                    X=9.5
Y=4.5  [===================TECHO====================]
       |                                            |
Y=3.0  sp_TL            [GX_Top]             sp_TR |
       |                                            |
Y=2.5  |            [Galaxy_Top]                   |
       |                                            |
Y=1.0  |  [Plat_L]                    [Plat_R]    |
       |                                            |
Y=0.0  |  [Plat_LM]    [Plat_RM]              sp_ML|
sp_MR  |                                            |
Y=-1.0 |         [=Purple Center=]                 |
       |                                            |
Y=-3.0 ■ CKP           p1 p2 p3 p4           EXIT □|
Y=-4.0 [======Ground=================================]
```

### Plataformas
| Nombre | Posición | Tamaño | Tipo |
|---|---|---|---|
| Floor | (0, -4) | 18×2 | Grass |
| Plat_Center | (0, -1) | 6×1 | Purple |
| Plat_L | (-6, 1) | 3×1 | Grass |
| Plat_R | (6, 1) | 3×1 | Grass |
| Plat_Top | (0, 2.5) | 4×1 | Galaxy |
| Plat_LM | (-3, 0) | 3×1 | Grass |
| Plat_RM | (3, 0) | 3×1 | Grass |

**Notas de diseño:** Arena de combate. La plataforma central elevada otorga ventaja táctica. Spawners en 5 posiciones garantizan presión constante desde múltiples ángulos. El jugador debe limpiar la pantalla para desbloquear la salida.

---

## Room_03 — Secret Medium

**Tipo:** Room_Secret  
**Dificultad:** ⭐⭐⭐ Medio  
**Spawners:** 4 (incluyendo 1 spawn secreto en la brecha del techo)  
**Checkpoint:** (0, -3) — suelo central  
**Salida:** (8, -3) — derecha del suelo  
**Cofres:** 2 — (0, 4.0) en zona secreta; (-7, 3.7) en plataforma Galaxy oculta  

### Diagrama
```
X=-9.5                   X=0                    X=9.5
Y=4.5  [=====Ceil_L=====] BRECHA  sp_Secret [=Ceil_R==]
Y=4.0  |  [Plat_Sec_L]      🎁 COFRE SECRETO  [Sec_R]  |
       |                                            |
       ■ Inner_Wall                                |
Y=2.5  |             [Plat_D]                     |
       |                                           sp_TR
Y=1.5  |         [Plat_C Purple]                  |
       |                                            |
Y=0.0  |                          [Plat_B]        sp_ML
       |                                            |
Y=-1.5 |     [Plat_A]                              |
       |                                            |
Y=-3.0 sp_TL         ■ CKP               EXIT □    |
Y=-4.0 [===================SUELO==================]
```

### Descripción de la zona secreta
- El techo tiene una brecha entre X=-2 y X=+2
- Para llegar: desde `Plat_C` (altura -1.5 a +1.5) se salta hacia arriba a través de la brecha
- Dentro de la zona secreta hay cofres con vida
- El spawn `sp_Secret` (0, 4.2) hace aparecer enemigos DENTRO de la zona secreta
- El `Inner_Wall_L` en X=-5.5 crea un bolsillo izquierdo con plataformas Galaxy ocultas

### Plataformas
| Nombre | Posición | Tamaño | Tipo |
|---|---|---|---|
| Floor | (0, -4) | 18×2 | Grass |
| Ceil_L | (-6, 4.5) | 8×1 | Brick |
| Ceil_R | (6, 4.5) | 8×1 | Brick |
| Inner_Wall_L | (-5.5, 2) | 1×5 | Brick |
| Plat_A | (-4, -1.5) | 3×1 | Grass |
| Plat_B | (3, -0.5) | 4×1 | Grass |
| Plat_C | (-1, 1.5) | 3×1 | Purple |
| Plat_D | (5, 2.5) | 3×1 | Grass |
| Plat_Sec_L | (-7.5, 3.5) | 2×1 | Galaxy |
| Plat_Sec_R | (6.5, 3.8) | 2×1 | Galaxy |

---

## Room_04 — Mixed Hard

**Tipo:** Room_Mixed  
**Dificultad:** ⭐⭐⭐⭐ Difícil  
**Spawners:** 6 (todas las esquinas + centro del techo)  
**Requiere:** Limpiar la oleada  
**Checkpoint:** (0, -3) — suelo central  
**Salida:** (8, 3) — plataforma Galaxy alta derecha  
**Cofres:** 2 — (-7, 3.7) y (5.5, 3.7)  
**Enemigo:** Queen (spawna drones genéticos)  

### Diagrama
```
X=-9.5              X=0              X=9.5
Y=4.5  [===TECHO=========sp_TC========TECHO===]
       sp_TL    🎁              🎁       sp_TR
Y=3.5  [Gal_7]                      [Gal_6]EXIT□
       |                                       |
Y=2.5  |              [Plat_5]                |
       |  [Plat_4 Purp]                        |
Y=1.5  |                                       |
       |                        [Destr_2]      |
Y=1.0  |           [Plat_3]                   |
sp_ML  |                                  sp_MR|
Y=0.0  |      [Plat_2]                        |
       |                                       |
Y=-2.0 [Plat_1]     [MOV→←]  [Mut] [Destr_1] |
Y=-3.0 ■CKP       p1 p2 p3 p4                 |
sp_BL  [==floor (16 tiles)====================]
```

### Elementos especiales
- **Plataformas destructibles:** `Destr_1` (4, -1.5) y `Destr_2` (2, 1) — tiemblan 1.2s antes de caer, respawnean en 4s
- **Plataforma móvil:** oscila entre X=-3 y X=3 en Y=-3 (vel. 1.5 u/s)
- **Zona de mutación:** plataforma purple en (1, -2.5) — tema visual de riesgo

### Plataformas
| Nombre | Posición | Tamaño | Tipo |
|---|---|---|---|
| Floor | (0, -4) | 16×2 | Grass |
| Plat_1 | (-6, -2) | 3×1 | Grass |
| Plat_2 | (-2, -0.5) | 3×1 | Grass |
| Plat_3 | (3, 0) | 3×1 | Grass |
| Plat_4 | (-5, 1.5) | 3×1 | Purple |
| Plat_5 | (1, 2.5) | 4×1 | Grass |
| Plat_6 | (6, 3) | 3×1 | Galaxy |
| Plat_7 | (-7, 3.5) | 2×1 | Galaxy |
| Destr_1 | (4, -1.5) | 2×1 | Purple (destr.) |
| Destr_2 | (2, 1) | 2×1 | Purple (destr.) |
| Plat_Mov | (-1, -3) | 3×1 | Purple (móvil) |

---

## Room_05 — Platform Hard

**Tipo:** Room_Platform  
**Dificultad:** ⭐⭐⭐⭐⭐ Muy Difícil  
**Spawners:** 4 (incluyendo uno en la brecha del suelo)  
**Checkpoint:** (-8, -3) — plataforma de inicio izquierda  
**Salida:** (0, 4) — plataforma Galaxy central alta (requiere completar el recorrido)  
**Cofre:** (-1, 3.7) — en la plataforma top  
**Enemigo:** Parasite (infecta aliados = dificultad adicional)  

### Diagrama
```
X=-9.5             X=0              X=9.5
Y=4.5  [=TECHO===sp_Center============TECHO=]
Y=3.5  |    [GX_Top] 🎁 EXIT□             |
       |                     [Plat_5_GX]   |
       |  [Plat_4]                          |
Y=2.0  |                          [Plat_6] |
sp_TL  |                                sp_TR
Y=1.5  |                                   |
Y=1.0  |                  [P_3B]           |
Y=0.5  |          [P_3 Pur]               |
       |                                   |
Y=-0.5 |   [Plat_2]                        |
Y=-1.5 |                     [MOV_H→←]    |
Y=-2.0 |   [Plat_1]                        |
       |                                   |
Y=-3.0 |          [P_0 GX]                sp_Gap
       [FLOOR_L]   ~~~BRECHA~~~  [FLOOR_R] |
Y=-4.0 [=====5t=]               [====6t==]
■CKP sp_TL player_spawns
```

### Ruta de progresión prevista
```
Floor_L → Plat_1(-6,-2) → Plat_2(-3,-0.5) → Plat_3(1,0.5) 
        → Plat_4(-5,2) → Plat_Top(-1,3.5) EXIT
```
**Ruta alternativa (velocistas):**
```
Floor_L → Plat_0(0,-3) via dash sobre la brecha → Plat_3B(3,-0.5) → Plat_5(5,3) → Wall_R → top
```

### Plataformas
| Nombre | Posición | Tamaño | Tipo | Nota |
|---|---|---|---|---|
| Floor_L | (-7, -4) | 5×2 | Grass | Inicio |
| Floor_R | (6, -4) | 6×2 | Grass | Final |
| Plat_0 | (0, -3) | 2×1 | Galaxy | Requiere dash sobre brecha |
| Plat_1 | (-6, -2) | 2×1 | Grass | |
| Plat_2 | (-3, -0.5) | 2×1 | Grass | |
| Plat_3 | (1, 0.5) | 2×1 | Purple | |
| Plat_3B | (3, -0.5) | 2×1 | Grass | |
| Plat_4 | (-5, 2) | 2×1 | Grass | |
| Plat_5 | (5, 3) | 2×1 | Galaxy | |
| Plat_6 | (7, 1.5) | 2×1 | Grass | |
| Plat_Top | (-1, 3.5) | 3×1 | Galaxy | Salida |
| Mov_V | (-7, 1) | 1×1 | Purple | Vertical Y=0↔2.5 |
| Mov_H | (4, -1.5) | 2×1 | Purple | Horizontal X=3↔7 |

---

## Room_Boss — Boss Arena

**Tipo:** Room_Boss  
**Dificultad:** ⭐⭐⭐⭐⭐ Jefe  
**Spawners:** 7 (todas las esquinas + centro techo)  
**Requiere:** Derrotar al Boss (limpiar oleada)  
**Checkpoint:** (0, -3) — centro del arena  
**Salida:** (0, 4) — centro del techo (después de matar al Boss)  
**Cofres:** 2 — (-8, -3) y (8, -3) — en esquinas del arena  
**Enemigo:** Boss (3 fases con spawn de minions)  

### Diagrama
```
X=-9.5           X=0              X=9.5
Y=4.5  [============TECHO============]
       sp_TL     sp_TC  EXIT□   sp_TR
Y=3.0  |  [GX_L3]              [GX_R3]|
       |                               |
Y=1.5  |      [Plat_CL] [Plat_CR]    |
Y=0.5  |  [Plat_L2]        [Plat_R2] |
sp_ML  |                          sp_MR
Y=-1.5 |  [Plat_L1]        [Plat_R1] |
       |  [MOV_L→←]  [MutZone][MOV_R→←]|
Y=-3.0 🎁        ■CKP  p1-p4     🎁   |
sp_BL  [===BOSS ARENA PURPLE FLOOR===]sp_BR
Y=-4.0 [=================================]
```

### Plataformas
| Nombre | Posición | Tamaño | Tipo |
|---|---|---|---|
| Floor (Purple Arena) | (0, -4) | 20×2 | Purple |
| Plat_L1 | (-7, -1.5) | 3×1 | Grass |
| Plat_R1 | (7, -1.5) | 3×1 | Grass |
| Plat_L2 | (-5.5, 0.5) | 3×1 | Grass |
| Plat_R2 | (5.5, 0.5) | 3×1 | Grass |
| Plat_CL | (-2.5, 1.5) | 3×1 | Purple |
| Plat_CR | (2.5, 1.5) | 3×1 | Purple |
| Plat_L3 | (-7, 3) | 2×1 | Galaxy |
| Plat_R3 | (7, 3) | 2×1 | Galaxy |
| MutZone | (0, -2.5) | 6×1 | Purple |
| Mov_L | (-5, -2.5) | 2×1 | Galaxy (móvil) |
| Mov_R | (5, -2.5) | 2×1 | Galaxy (móvil) |

**Plataformas móviles:** Mov_L oscila X=-7.5↔-3; Mov_R oscila X=3↔7.5 (vel. 1.8 u/s)

### Fases del Boss
| Fase | HP % | Comportamiento |
|---|---|---|
| 1 | 100-66% | Normal (EnemyBase AI) |
| 2 | 65-33% | Velocidad ×1.4 · RangoVision ×1.3 · Spawna 3 minions |
| 3 | 32-0% | Velocidad máxima · Espinas +0.4 · Spawna 3 minions más |

---

## QA Checklist por Sala

| Check | R01 | R02 | R03 | R04 | R05 | RBoss |
|---|---|---|---|---|---|---|
| Todas las plataformas accesibles desde suelo | ✅ | ✅ | ✅ | ✅ | ⚠️* | ✅ |
| Sin zonas inaccesibles | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Sin softlock posible | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Spawners distribuidos | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Checkpoint presente | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Salida presente | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| WaveManager wired | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

*⚠️ Room_05: La `Plat_0` en la brecha del suelo requiere dash desde `Floor_L`. Es intencional (sala difícil).

---

## Cómo Agregar una Nueva Sala

1. En `MutationSwarmRoomBuilder.cs`, agregar un nuevo `RoomDef` al array en `BuildRoomDefinitions()`
2. Definir plataformas usando los helpers `Plat()`, `MovingPlat()`, `DestrPlat()`
3. Definir spawners usando `Spawn()` (enemigo) y `Player()` (jugador)
4. Asignar `SceneName`, `NextScene`, `EnemyPrefabName`, posiciones de checkpoint y salida
5. Ejecutar `Tools > Mutation Swarm > Build All Rooms`

---

## Prefabs disponibles en Assets/_Prefabs/Rooms/

| Prefab | Componente | Uso |
|---|---|---|
| `SpawnPoint.prefab` | Script_SpawnPointGizmos | Punto de spawn de enemigos (marcador visual) |
| `Checkpoint.prefab` | CheckpointController | Guarda posición de respawn al entrar |
| `LevelExit.prefab` | LevelExit | Portal que carga la siguiente sala |
| `Chest.prefab` | Chest | Cofre con vida (25 HP al abrir) |
| `PlataformaMovil.prefab` | MovingPlatform | Plataforma cinemática entre 2 waypoints |
| `PlataformaDestructible.prefab` | DestructiblePlatform | Cae 1.2s después de que el jugador la pise |
