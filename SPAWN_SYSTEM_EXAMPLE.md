# Ejemplo Rápido — Spawn System en Acción

## Escenario: Habitación 01 con Drones Flotantes que Disparan

### Paso 1: Estructura de Spawn Points en la Escena

```
Scene_01_Room
└── _SpawnPoints
    ├── sp_left_top (enemigo)
    ├── sp_left_mid (enemigo)
    ├── sp_left_bot (enemigo)
    ├── sp_right_top (enemigo)
    ├── sp_right_mid (enemigo)
    ├── sp_right_bot (enemigo)
    ├── sp_top_left (enemigo)
    ├── sp_top_right (enemigo)
    └── p1_spawn (player)
```

Cada uno es un **GameObject con Script_39_SpawnPoint**.

### Paso 2: Configuración en Inspector de cada Spawn Point

**Ejemplo: sp_left_top**
```
Script_39_SpawnPoint
├── Type: Enemy
├── Prefab To Spawn: Prefab_Enemy_Drone.prefab
└── Show Gizmo: true
```

**Resultado:** Gizmo rojo en la escena (visible al jugar)

---

### Paso 3: Prefab de Drone Configurado

**Prefab_Enemy_Drone.prefab**
```
Components:
├── Transform
├── SpriteRenderer (color rojo mutación)
├── Rigidbody2D
│   ├── Gravity Scale: 1.2
│   ├── Linear Damping: 0.5
│   └── Mass: 1.0
├── CircleCollider2D
├── Script_13_EnemyBase (IA base: perseguir, atacar)
├── Script_22_StatusEffects (envenenamiento)
└── Script_41_EnemyRangedAttack  ← NUEVO
    ├── Enabled: true
    ├── Fire Rate: 2.0
    ├── Fire Range: 8.0
    ├── Projectile Pool Key: "Projectile_Enemy_Basic"
    ├── Use Spread Pattern: false
    └── Fire Point: (null = usa transform)
```

---

### Paso 4: SO_EnemySpawnConfig para Nivel 1

**Asset: SO_EnemySpawnConfig_Level01.asset**

```
Enemy Entries:
[0] Drone Estándar
    Enemy Prefab: Prefab_Enemy_Drone.prefab
    Spawn Point Pattern: (vacío)
    Weight: 1.0
    Appearance Chance: 100%

Randomize Order: true
Min Spawn Delay: 0.5
Max Spawn Delay: 1.5
```

---

### Paso 5: WaveManager en Scene_01_Room

```
Script_02_WaveManager
├── Config: SO_WaveConfig_Level01.asset
├── Spawn Config: SO_EnemySpawnConfig_Level01.asset  ← NUEVO
├── Enemy Prefab: Prefab_Enemy_Drone.prefab (fallback)
├── Enemy Spawn Points: [Auto-detectado]  ← Se llena automáticamente
├── Base Enemies: 6
└── Spawn Interval: 1.2
```

---

## Flujo de Ejecución

### 1. Al iniciar la escena:
```
WaveManager.Awake()
  ↓
  Detecta Script_39_SpawnPoint en _SpawnPoints
  ↓
  _enemySpawnPoints = [sp_left_top, sp_left_mid, ...]
```

### 2. Al llamar StartWave():
```
for i = 0 to 5 (6 enemigos)
  ↓
  SpawnEnemy()
    ↓
    Selecciona spawn point aleatorio (sp_left_top)
    ↓
    Obtiene prefab de SO_EnemySpawnConfig
    ↓
    Instancia Prefab_Enemy_Drone en sp_left_top.position
    ↓
    Script_13_EnemyBase.Initialize(Genome)
    ↓
    Script_41_EnemyRangedAttack comienza a ejecutar Update()
    ↓
    Espera 1.2 segundos (spawn interval)
```

### 3. Durante la oleada:
```
Cada drone (corrutina independiente):
  ↓
  Update() → Script_41_EnemyRangedAttack.Update()
    ↓
    ¿Ve al jugador dentro de 8 unidades?
      Sí:
        ↓
        ¿Pasaron 2 segundos desde último disparo?
          Sí:
            ↓
            Dispara 1 projectile en dirección al jugador
            ↓
            Projectile usa ObjectPool ("Projectile_Enemy_Basic")
            ↓
            Vuela 3 segundos o hasta impactar
      No:
        ↓
        Continúa esperando/persiguiendo
```

---

## Resultado Visual

```
En la escena:
- 6 drones rojos flotando (gravityScale > 0)
- Cada uno sigue al jugador
- Cada 2 segundos dispara 1 bala roja
- Las balas persiguen al jugador durante 3 segundos
- Al impactar, el drone toma daño y vuelve al pool
```

---

## Configuraciones Alternativas

### Variante 1: Drones Variadps (diferentes tipos)

**SO_EnemySpawnConfig_Level01_Advanced.asset**
```
Enemy Entries:
[0] Drone Standard (70% chance)
    Enemy Prefab: Prefab_Enemy_Drone.prefab
    Weight: 1.0
    Appearance Chance: 70%

[1] Drone Heavy (20% chance)
    Enemy Prefab: Prefab_Enemy_Drone_Heavy.prefab  (lento, mucho daño)
    Weight: 0.3
    Appearance Chance: 20%

[2] Drone Fast (10% chance)
    Enemy Prefab: Prefab_Enemy_Drone_Fast.prefab (rápido, poco daño)
    Weight: 0.5
    Appearance Chance: 10%
```

### Variante 2: Spawn Points Específicos

```
Enemy Entries:
[0] Drone Left (solo spawn points de la izquierda)
    Enemy Prefab: Prefab_Enemy_Drone.prefab
    Spawn Point Pattern: "sp_left"
    Weight: 1.0

[1] Drone Right (solo spawn points de la derecha)
    Enemy Prefab: Prefab_Enemy_Drone_Ranged.prefab
    Spawn Point Pattern: "sp_right"
    Weight: 1.0
```

---

## Debugging

### ¿Los drones no aparecen?
```csharp
// En WaveManager (inspector) busca:
Debug.Log($"Spawn points encontrados: {_enemySpawnPoints.Length}");
Debug.Log($"Config spawn: {_spawnConfig}");
```

### ¿Los drones flotan al piso?
```
En Prefab_Enemy_Drone, ajusta:
Rigidbody2D.Gravity Scale: 1.5 (aumentar)
Rigidbody2D.Linear Damping: 0.2 (disminuir)
```

### ¿No disparan?
```
Verifica en Script_41_EnemyRangedAttack:
- Enabled: true
- Fire Range: >= 5
- Pool Key: "Projectile_Enemy_Basic" existe en ObjectPool
```

---

## Ventajas del Sistema

✅ **Reutilizable:** Mismo sistema para todas las habitaciones  
✅ **Flexible:** Cambiar enemigos editando SO_EnemySpawnConfig  
✅ **Escalable:** Agregar nuevos tipos de drones fácilmente  
✅ **Performance:** Object Pool para projectiles  
✅ **Modular:** Componentes independientes (SpawnPoint, RangedAttack, etc.)

