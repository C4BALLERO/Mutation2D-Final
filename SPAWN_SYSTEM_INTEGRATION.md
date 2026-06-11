# Guía de Integración — Sistema de Spawn Points Parametrizables

## Resumen

He creado un sistema completo de **spawn points parametrizables** que permite:
- ✅ Asignar diferentes prefabs de enemigos a cada habitación
- ✅ Los drones flotan y disparan projectiles
- ✅ Configuración flexible por nivel mediante ScriptableObjects
- ✅ Backward compatible con el sistema antiguo

---

## Archivos Nuevos Creados

| Archivo | Ubicación | Descripción |
|---------|-----------|-------------|
| `Script_39_SpawnPoint.cs` | `Assets/_Scripts/Core/` | Componente spawn point parametrizable |
| `Script_41_EnemyRangedAttack.cs` | `Assets/_Scripts/Combat/` | Sistema de disparo para drones |
| `SO_EnemySpawnConfig.cs` | `Assets/_ScriptableObjects/` | Config de enemigos por oleada |
| `Script_02_WaveManager.cs` | Refactorizado | Ahora soporta nuevo sistema |

---

## Paso 1: Preparar Projectil de Enemigos

### En el ObjectPool:
1. **Crear SO_PoolConfig para enemigos:**
   - Asset → `Assets/_ScriptableObjects/Pools/SO_Pool_Projectile_Enemy.asset`
   - Pool Key: `"Projectile_Enemy_Basic"`
   - Initial Size: `20`
   - Max Size: `100`

2. **Duplicar el prefab de projectil del player:**
   ```
   Prefab_Projectile_Basic → Prefab_Projectile_Enemy.prefab
   ```
   - Cambiar color: Rojo/naranja (enemigos)
   - Cambiar layer: `"Projectile_Enemy"` (crear si no existe)
   - Cambiar damage default: `5` (en lugar de 10)

3. **Agregar SO_Pool a WaveManager:**
   - En `Script_02_WaveManager`, en `_poolConfigs`, agregar la nueva config

---

## Paso 2: Configurar Prefab de Drones

### En `Prefab_Enemy_Drone`:

1. **Agregar Script_41_EnemyRangedAttack:**
   ```
   Add Component → Script_41_EnemyRangedAttack
   ```

2. **Configurar en Inspector:**
   ```
   Enabled:                    true
   Fire Rate:                  2.0 (segundos entre disparos)
   Fire Range:                 8.0 (distancia máxima)
   Projectile Pool Key:        "Projectile_Enemy_Basic"
   Fire Point:                 (asignar o dejar null = usa transform)
   Use Spread Pattern:         false (true para múltiples proyectiles)
   ```

3. **Verificar Rigidbody2D:**
   ```
   Gravity Scale:              1.0+ (para flotación)
   ```

---

## Paso 3: Convertir Habitaciones a Nuevo Sistema

Para cada habitación (Scene_01_Room, etc.):

### A. Preparar GameObject de Spawn Points:

```
En la escena:
_SpawnPoints
├── sp_left_top      ← Add Script_39_SpawnPoint
├── sp_left_mid      ← Add Script_39_SpawnPoint
├── sp_left_bot      ← Add Script_39_SpawnPoint
├── sp_right_top     ← Add Script_39_SpawnPoint
├── sp_right_mid     ← Add Script_39_SpawnPoint
├── sp_right_bot     ← Add Script_39_SpawnPoint
├── sp_top_left      ← Add Script_39_SpawnPoint
├── sp_top_right     ← Add Script_39_SpawnPoint
└── p1_spawn         ← Script_39_SpawnPoint (type: Player)
```

### B. En cada Script_39_SpawnPoint:

```
Type:               Enemy (o Player para p1_spawn)
Prefab To Spawn:    Prefab_Enemy_Drone.prefab
Show Gizmo:         true
```

---

## Paso 4: Crear Configuración por Nivel

### Crear SO_EnemySpawnConfig para cada habitación:

```
Right-click Assets/_ScriptableObjects/ 
→ Create → MutationSwarm → Enemy Spawn Config
```

**Ejemplo: SO_EnemySpawnConfig_Level01.asset**

```
Enemy Entries:
  [0]
    Enemy Prefab:        Prefab_Enemy_Drone.prefab
    Spawn Point Pattern: "" (vacío = cualquier spawn)
    Weight:              1.0
    Appearance Chance:   100%
  
  [1] (opcional)
    Enemy Prefab:        Prefab_Enemy_Queen.prefab
    Spawn Point Pattern: "sp_center"
    Weight:              0.5
    Appearance Chance:   10%

Randomize Order:        true
Min Spawn Delay:        0.5
Max Spawn Delay:        1.5
```

---

## Paso 5: Asignar Config a WaveManager

En cada escena de habitación (`Scene_01_Room`, etc.):

### En WaveManager:
```
Spawn Config:  SO_EnemySpawnConfig_Level01.asset
```

**Opcional — Mantener backward compatibility:**
```
Enemy Prefab:  Prefab_Enemy_Drone.prefab (fallback)
```

---

## Resultado Esperado

✅ **Drones flotando:** 
- Gravity Scale > 0 los mantiene flotando naturalmente
- Se mueven hacia el jugador mediante MoveTowards()

✅ **Drones disparando:**
- Cada 2 segundos (configurable) disparan 1 projectil rojo
- Se autodetecta el jugador más cercano
- Los projectiles usan ObjectPool

✅ **Configuración flexible:**
- Cambiar el SO_EnemySpawnConfig por nivel
- Diferentes tipos de enemigos en diferentes habitaciones
- Ponderación aleatoria de qué tipo spawna

---

## Uso de Script_39_SpawnPoint en Código

```csharp
// Obtener spawn point manualmente
Script_39_SpawnPoint sp = GetComponent<Script_39_SpawnPoint>();

// Spawnar con prefab asignado
GameObject enemy = sp.Spawn();

// Spawnar con prefab específico
GameObject enemy = sp.SpawnWithPrefab(customPrefab);

// Cambiar prefab en runtime
sp.PrefabToSpawn = newPrefab;
```

---

## Configuración de Script_41_EnemyRangedAttack

### Parámetros principales:

```csharp
// Disparo simple
Fire Rate:              0.5f → dispara cada 0.5 seg
Fire Range:             10f  → solo dispara si ve al jugador dentro de 10u
Projectile Pool Key:    "Projectile_Enemy_Basic"

// Patrón de abanico (opcional)
Use Spread Pattern:     true
Bullets Per Shot:       3    → dispara 3 projectiles
Spread Angle:           45f  → ángulo total de dispersión
```

### Ejemplos de configuración:

**Drone básico (francotirador):**
- Fire Rate: 3.0
- Bullets Per Shot: 1
- Spread Angle: 0

**Drone con shotgun:**
- Fire Rate: 4.0
- Bullets Per Shot: 5
- Spread Angle: 60

**Drone ráfaga:**
- Fire Rate: 0.2
- Bullets Per Shot: 1
- Spread Angle: 0

---

## Solución de Problemas

### Los drones no disparan:
- [ ] ¿Script_41_EnemyRangedAttack está en el prefab?
- [ ] ¿"Enabled" está true?
- [ ] ¿Existe el pool "Projectile_Enemy_Basic"?

### Los drones se caen al suelo:
- [ ] Gravity Scale está muy alto (> 3)
- [ ] Solución: Ajustar a 1-1.5 en el Rigidbody2D

### Los projectiles no aparecen:
- [ ] ¿SO_Pool_Projectile_Enemy está asignado al ObjectPool?
- [ ] ¿Fire Range es suficientemente grande?
- [ ] Verificar que hay línea de vista con Physics2D.OverlapCircle

---

## Próximos Pasos Opcionales

1. **Añadir variantes de drones:**
   - Heavy Drone (dispara lentamente, mucho daño)
   - Fast Drone (dispara rápido, poco daño)
   - Swarm Drone (patrón de grupo)

2. **Jefe Boss con disparo:**
   - Script_16_EnemyQueen puede tener Script_41_EnemyRangedAttack

3. **Efectos visuales:**
   - VFX de disparo en Fire Point
   - Trail renderer en projectil

---

## Archivos a Revisar

- [Script_39_SpawnPoint.cs](Assets/_Scripts/Core/Script_39_SpawnPoint.cs)
- [Script_41_EnemyRangedAttack.cs](Assets/_Scripts/Combat/Script_41_EnemyRangedAttack.cs)
- [SO_EnemySpawnConfig.cs](Assets/_ScriptableObjects/SO_EnemySpawnConfig.cs)
- [Script_02_WaveManager.cs](Assets/_Scripts/Core/Script_02_WaveManager.cs) — Refactorizado
