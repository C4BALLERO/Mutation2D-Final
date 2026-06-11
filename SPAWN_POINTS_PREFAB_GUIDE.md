# Spawn Points con Prefabs — Guía Rápida

## ✅ Qué Se Agregó

### Nuevos Componentes:
1. **SpawnPointChild.cs** — Componente para cada spawn point individual
2. **Script_SpawnPointGizmosEditor.cs** — Inspector personalizado con asignación visual

### Métodos Añadidos a Script_SpawnPointGizmos:
```csharp
SpawnAll()                      // Spawna todos los prefabs asignados
SpawnAll(GameObject prefab)     // Spawna el mismo prefab en todos
SpawnAllEnemies(GameObject)     // Solo en enemy spawn points
SpawnAllPlayers(GameObject)     // Solo en player spawn points
GetSpawnPoint(string name)      // Obtén un spawn point específico
GetAllSpawnPoints()             // Array de todos los spawn points
```

---

## 🚀 Cómo Usar (3 pasos)

### Paso 1: Agregar SpawnPointChild a cada spawn point

En tu escena (`Scene_02_GameWorld`), en **_SpawnPoints**:

```
_SpawnPoints (con Script_SpawnPointGizmos)
├── sp_TL          ← Add Component: SpawnPointChild
├── sp_TR          ← Add Component: SpawnPointChild
├── sp_ML          ← Add Component: SpawnPointChild
├── sp_MR          ← Add Component: SpawnPointChild
├── sp_BL          ← Add Component: SpawnPointChild
├── sp_BR          ← Add Component: SpawnPointChild
├── sp_TC          ← Add Component: SpawnPointChild
├── sp_BC          ← Add Component: SpawnPointChild
└── p1             ← Add Component: SpawnPointChild (marcar "Is Player Spawn")
```

### Paso 2: Asignar Prefabs en el Inspector

Selecciona **_SpawnPoints** en la jerarquía.

En el inspector aparecerá:

```
🎮 Spawn Points Configuration

📌 Asignar Prefabs a Spawn Points
├── sp_TL [ENEMY]   Prefab: Prefab_Enemy_Drone.prefab    [Spawn]
├── sp_TR [ENEMY]   Prefab: Prefab_Enemy_Drone.prefab    [Spawn]
├── sp_ML [ENEMY]   Prefab: Prefab_Enemy_Drone.prefab    [Spawn]
...
└── p1 [PLAYER]     Prefab: Prefab_Player.prefab         [Spawn]

⚡ Test Spawning
[✨ Spawn All] [🗑️ Clear Scene Spawns]
```

### Paso 3: Usar en el Código

En **Script_02_WaveManager** u otro script:

```csharp
// Obtener el manager
Script_SpawnPointGizmos spawnManager = GetComponent<Script_SpawnPointGizmos>();

// Spawnar todos
GameObject[] enemies = spawnManager.SpawnAll();

// O spawnar solo enemigos
GameObject[] enemies = spawnManager.SpawnAllEnemies(prefabDrone);

// O spawnar solo jugadores
GameObject[] players = spawnManager.SpawnAllPlayers(prefabPlayer);

// O un spawn point específico
SpawnPointChild sp = spawnManager.GetSpawnPoint("sp_TL");
GameObject enemy = sp.Spawn();
```

---

## 🎨 Configuración Visual

### Cada SpawnPointChild tiene:

```
✏️ Prefab To Spawn    → Asigna el GameObject a instanciar
✏️ Is Player Spawn    → Marca si es un spawn de jugador (verde) o enemigo (rojo)
```

### Gizmos en la escena:

```
🔴 Rojo   = Spawn point de enemigo (sin prefab)
🟡 Amarillo = Spawn point con prefab asignado
🟢 Verde  = Spawn point de jugador
🔵 Cian   = Spawn point de jugador con prefab asignado
```

---

## 💡 Ejemplos de Uso

### Ejemplo 1: Spawnar drones en todos los spawn points

```csharp
void StartWave()
{
    Script_SpawnPointGizmos spawnMgr = GetComponent<Script_SpawnPointGizmos>();
    
    // Los prefabs ya están asignados en el inspector
    GameObject[] enemies = spawnMgr.SpawnAll();
    
    foreach (GameObject enemy in enemies)
    {
        // Inicializar enemigos
        if (enemy.TryGetComponent(out Script_13_EnemyBase enemyBase))
            enemyBase.Initialize(currentGenome.Clone());
    }
}
```

### Ejemplo 2: Spawnar solo en los lados

```csharp
SpawnPointChild[] allSpawns = spawnMgr.GetAllSpawnPoints();
foreach (var sp in allSpawns)
{
    if (sp.gameObject.name.Contains("L") || sp.gameObject.name.Contains("R"))
    {
        sp.Spawn();
    }
}
```

### Ejemplo 3: Spawnar jugador

```csharp
GameObject[] players = spawnMgr.SpawnAllPlayers(prefabPlayer);
if (players.Length > 0 && players[0].TryGetComponent(out Script_11_PlayerController controller))
{
    controller.Initialize(playerIndex);
}
```

---

## 🔧 Setup para WaveManager

Actualiza **Script_02_WaveManager** para usar el nuevo sistema:

```csharp
public void SpawnEnemy()
{
    // Obtener el manager de spawn points
    Script_SpawnPointGizmos spawnMgr = FindObjectOfType<Script_SpawnPointGizmos>();
    if (spawnMgr == null)
        return;

    // Obtener un spawn point aleatorio
    SpawnPointChild[] points = spawnMgr.GetAllSpawnPoints();
    SpawnPointChild sp = points[Random.Range(0, points.Length)];
    
    if (sp.IsPlayerSpawn)
        return; // No spawnar en player spawns
    
    // Spawnar el prefab asignado
    GameObject enemyGo = sp.Spawn();
    if (enemyGo == null)
        return;
    
    EnemiesSpawned++;
    EnemiesAlive++;
    
    // Inicializar
    if (enemyGo.TryGetComponent(out Script_13_EnemyBase enemy))
    {
        Genome g = CurrentGenomePool[Random.Range(0, CurrentGenomePool.Count)];
        enemy.Initialize(g.Clone());
    }
}
```

---

## ✨ Ventajas

✅ Asignación visual de prefabs en el inspector  
✅ Sin código — solo configurar en el editor  
✅ Flexible — cada spawn point puede tener prefabs diferentes  
✅ Testing fácil — botón "Spawn All" para probar  
✅ Gizmos visuales — verde = player, rojo = enemy  
✅ Integrado con WaveManager  

---

## 🔄 Migración desde el Sistema Antiguo

Si usabas Transform[]:

```csharp
// Antes:
[SerializeField] private Transform[] _spawnPoints;
Transform sp = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
Instantiate(prefab, sp.position, Quaternion.identity);

// Ahora:
Script_SpawnPointGizmos spawnMgr = FindObjectOfType<Script_SpawnPointGizmos>();
SpawnPointChild sp = spawnMgr.GetAllSpawnPoints()[Random.Range(0, ...)];
sp.Spawn();
```

---

## 📝 Checklist de Configuración

- [ ] Agregar SpawnPointChild a cada spawn point en la escena
- [ ] Asignar prefabs en el inspector (_SpawnPoints > Spawn Points Configuration)
- [ ] Marcar "Is Player Spawn" en p1, p2, etc.
- [ ] Actualizar WaveManager para usar GetAllSpawnPoints()
- [ ] Probar con botón "✨ Spawn All" en el inspector
- [ ] Jugar la escena y verificar que spawnan correctamente
