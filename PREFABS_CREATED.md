# PREFABS CREATED — Mutation Swarm 2D
**Fecha:** 2026-06-02

---

## Player

| Prefab | Ruta | Componentes | Estado |
|---|---|---|---|
| `Prefab_Player` | `Assets/_Prefabs/Player/` | Rigidbody2D, BoxCollider2D, CapsuleCollider2D, SpriteRenderer, Animator, Script_11_PlayerController, Script_12_PlayerStats | ✅ Completo — Animator controller se asigna con `Full Game Setup` |
| `Prefab_Player_Geo` | `Assets/_Prefabs/Player/` | Versión geométrica para debug | ⚠️ Debug only |

**Configuración del Player:**
- Layer: 6 (Player)
- Tag: Player
- Rigidbody2D gravity: 3
- HP: 100, MoveSpeed: 6, JumpForce: 12, DashForce: 18
- Animator: AC_Player.controller (8 estados: Idle, Walk, Jump, Fall, Dash, Attack, Hit, Die)

---

## Enemigos

| Prefab | Ruta | Script base | Estado |
|---|---|---|---|
| `Prefab_Enemy_Drone` | `Assets/_Prefabs/Enemies/` | Script_13_EnemyBase | ✅ Detector correcto (Layer 6/7) |
| `Prefab_Enemy_Boss` | `Assets/_Prefabs/Enemies/` | Script_15_EnemyBoss | ✅ 3 fases implementadas |
| `Prefab_Enemy_Queen` | `Assets/_Prefabs/Enemies/` | Script_16_EnemyQueen | ✅ Spawna drones genéticos |
| `Prefab_Enemy_Mimic` | `Assets/_Prefabs/Enemies/` | Script_17_EnemyMimic | ✅ Copia escala del player |
| `Prefab_Enemy_Parasite` | `Assets/_Prefabs/Enemies/` | Script_18_EnemyParasite | ✅ Propaga mutaciones |
| `Prefab_Enemy_Geo` | `Assets/_Prefabs/Enemies/` | Script_13_EnemyBase | Debug/placeholder |

**Configuración común de enemigos:**
- Layer: 7 (Enemy)
- _playerMask: Layer 6 (Player) ✅
- _enemyMask: Layer 7 (Enemy) ✅
- Animator: AC_Enemy_*.controller (asignado por `Full Game Setup`)

---

## Proyectiles

| Prefab | Arma asociada |
|---|---|
| `Prefab_Projectile_Basic` | Arma básica |
| `Prefab_Projectile_Geo` | Debug |
| `Prefab_Projectile_ak47` | AK-47 |
| `Prefab_Projectile_glock_p80` | Glock P80 |
| `Prefab_Projectile_mp5a3` | MP5A3 |
| `Prefab_Projectile_revolver_colt` | Revolver Colt |
| `Prefab_Projectile_bazooka_m20` | Bazooka M20 |
| `Prefab_Projectile_bazooka_thick` | Bazooka Thick |

---

## Estructuras

| Prefab | Componente | Descripción |
|---|---|---|
| `Prefab_Structure_Turret` | TurretStructure | Torreta automática |
| `Prefab_Structure_Barricade` | BarricadeStructure | Barricada defensiva |

---

## Rooms (Assets/_Prefabs/Rooms/)

| Prefab | Componente | Descripción |
|---|---|---|
| `SpawnPoint` | Script_SpawnPointGizmos | Marcador de spawn con gizmo |
| `Checkpoint` | CheckpointController | Guarda posición de respawn |
| `LevelExit` | LevelExit | Portal a siguiente sala |
| `Chest` | Chest | Cofre con curación (25 HP) |
| `PlataformaMovil` | MovingPlatform | Plataforma cinemática entre waypoints |
| `PlataformaDestructible` | DestructiblePlatform | Cae tras 1.2s de contacto |

---

## ScriptableObjects

### Weapons (Assets/_ScriptableObjects/Weapons/)
| Asset | Arma |
|---|---|
| `SO_Weapon_ak47` | AK-47 |
| `SO_Weapon_glock_p80` | Glock P80 |
| `SO_Weapon_mp5a3` | MP5A3 |
| `SO_Weapon_revolver_colt` | Revolver Colt |
| `SO_Weapon_bazooka_m20` | Bazooka M20 |
| `SO_Weapon_bazooka_thick` | Bazooka Thick |

### Upgrades (Assets/_ScriptableObjects/Upgrades/)
| Asset | Efecto |
|---|---|
| `SO_Upgrade_Drone` | Añade drone aliado |
| `SO_Upgrade_ElectricAmmo` | Munición eléctrica |
| `SO_Upgrade_ExplosiveDash` | Dash explosivo |
| `SO_Upgrade_FollowTurret` | Torreta móvil |
| `SO_Upgrade_Pierce` | Proyectiles penetrantes |
| `SO_Upgrade_Regen` | Regeneración de vida |
| `SO_Upgrade_TempShield` | Escudo temporal |
| `SO_Upgrade_Vampirism` | Vampirismo (roba HP) |

### Object Pools (Assets/_ScriptableObjects/Pools/)
8 configuraciones de pool, una por tipo de proyectil.

### Wave Config (Assets/_ScriptableObjects/Waves/)
| Asset | Config |
|---|---|
| `SO_WaveConfig_Default` | Base: 10 enemigos, +3/oleada, 1.5s→0.3s intervalo |

---

## Animator Controllers

### Player
| Controller | Ruta | Estados |
|---|---|---|
| `AC_Player` | `Assets/_Art/Animations/Player/` | Idle, Walk, Jump, Fall, Dash, Attack, Hit, Die |

**Parámetros:** IsGrounded (bool), IsRunning (bool), IsFalling (bool), Jump (trigger), Dash (trigger), Attack (trigger), Hit (trigger), Die (trigger)

### Enemies (generados por `Build Enemy Animations`)
| Controller | Ruta |
|---|---|
| `AC_Enemy_Drone` | `Assets/_Art/Animations/Enemies/` |
| `AC_Enemy_Boss` | `Assets/_Art/Animations/Enemies/` |
| `AC_Enemy_Queen` | `Assets/_Art/Animations/Enemies/` |
| `AC_Enemy_Mimic` | `Assets/_Art/Animations/Enemies/` |
| `AC_Enemy_Parasite` | `Assets/_Art/Animations/Enemies/` |

**Parámetros comunes:** IsMoving (bool), Attack (trigger), Hit (trigger), Die (trigger)
