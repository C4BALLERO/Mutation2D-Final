# Mutation Swarm — Contexto del Proyecto

## Datos generales
- **Nombre del juego:** Mutation Swarm
- **Motor:** Unity 2D (URP)
- **Ruta del proyecto:** `C:\Game\2Dfinal`
- **Escena principal:** `Assets/Scenes/SampleScene.unity`
- **Namespace:** `MutationSwarm`

---

## Qué es el juego
Juego de supervivencia 2D con plataformas. El jugador aguanta oleadas de enemigos que **evolucionan** según el comportamiento del jugador (si usa mucho el dash, los enemigos se vuelven más rápidos; si dispara bien, se vuelven más acorazados, etc.). Entre oleadas el jugador elige una mejora. Cuando el jugador muere, puede reintentar.

**Controles:**
- `A/D` — moverse
- `W / Espacio` — saltar (doble salto)
- `Shift` — dash
- `Ratón` — apuntar y disparar
- `B` — modo construcción de defensas
- `Esc` — pausa
- `R` — reintentar (en pantalla de muerte)
- `Enter / Space / Click` — iniciar partida desde el menú

---

## Arquitectura del proyecto

### Configuración de escena (procedural)
**IMPORTANTE:** Todos los objetos del juego son creados por código en `Assets/Scripts/Core/SceneSetup.cs`.

- En el **Editor**, se ejecuta el menú `MutationSwarm > Setup Scene` que llama a `SceneSetup.BuildScene()` y guarda la escena.
- En **Play Mode**, `SceneSetup.Start()` vuelve a ejecutar `BuildScene()` si el componente está en la escena, destruyendo los objetos previos con `DestroyImmediate` (editor) o `Destroy` (build) y recreándolos.
- Si no hay un `SceneSetup` en la escena, los objetos que ya existen desde la última configuración se usan directamente.

### Singletons (patrón estándar)
Todos los managers usan el mismo patrón:
```csharp
public static X Instance { get; private set; }
void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
}
```

**Singletons existentes:**
| Singleton | Script | Propósito |
|-----------|--------|-----------|
| `GameManager.Instance` | `Scripts/Core/GameManager.cs` | Fase del juego, número de oleada, puntuación |
| `WaveManager.Instance` | `Scripts/Systems/WaveManager.cs` | Spawneo de enemigos, lista de enemigos activos |
| `PlayerStats.Instance` | `Scripts/Player/PlayerStats.cs` | HP, DNA, upgrades del jugador |
| `PlayerController.Instance` | `Scripts/Player/PlayerController.cs` | Movimiento, dash, salto |
| `SpriteFactory.Instance` | `Scripts/Utils/SpriteFactory.cs` | Sprites procedurales (círculo, bala, plataforma, etc.) |
| `EvolutionSystem.Instance` | `Scripts/Systems/EvolutionSystem.cs` | Pesos de genes, evolución entre oleadas |
| `ParticleManager.Instance` | `Scripts/Utils/ParticleManager.cs` | Partículas/bursts visuales |
| `CameraFollow.Instance` | `Scripts/Utils/CameraFollow.cs` | Seguimiento de cámara y shake |

---

## Regla crítica: null guards en singletons
**SIEMPRE** verificar null antes de acceder a un singleton. En una sesión anterior se corrigieron **116 NullReferenceExceptions** por no tener estos guards.

```csharp
// CORRECTO
if (WaveManager.Instance == null) return;
foreach (var e in WaveManager.Instance.ActiveEnemies) { ... }

// CORRECTO (nulabilidad segura)
WaveManager.Instance?.RemoveEnemy(this);
if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.bulletsHit++;

// PELIGROSO - puede romper con NullReferenceException
if (!_spawning || GameManager.Instance.Phase != GamePhase.Playing) return;
// CORRECTO:
if (!_spawning || GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;
```

**Patrón para Update() con múltiples singletons:**
```csharp
void Update() {
    var gm = GameManager.Instance;
    var ps = PlayerStats.Instance;
    var wm = WaveManager.Instance;
    if (gm == null || ps == null || wm == null) return;
    // ... resto del Update
}
```

**Cuidado con lifetime checks después de un return:**
```csharp
// MAL: si PC es null, el objeto nunca se destruye
if (PlayerController.Instance == null) return;
if (_life <= 0f) Destroy(gameObject);  // NUNCA se ejecuta

// BIEN: primero destruir, luego el guard
if (_life <= 0f) { Destroy(gameObject); return; }
if (PlayerController.Instance == null) return;
```

---

## Scripts del proyecto

### Core
| Script | Ruta | Función |
|--------|------|---------|
| `GameManager.cs` | `Scripts/Core/` | Singleton principal. Controla `GamePhase` (Menu, Playing, Upgrade, Dead, Paused, Building), número de oleada, mejor puntuación, upgrades. Método `Restart()` recarga la escena. |
| `SceneSetup.cs` | `Scripts/Core/` | Construye toda la escena proceduralmente. Crea plataformas, prefabs, UI, managers, player. |
| `WaveStats.cs` | `Scripts/Core/` | Clase de estadísticas por oleada: bullets disparados/acertados, dash usado, tiempo en alto, defensas construidas, etc. Se usa para calcular la evolución. |
| `GeneType.cs` | `Scripts/Core/` | Enum con los tipos de genes: None, Poison, Speed, Spiny, Armored, Psychic, Corrupt. |

### Player
| Script | Ruta | Función |
|--------|------|---------|
| `PlayerController.cs` | `Scripts/Player/` | Movimiento (A/D), salto doble, dash. Requiere `Rigidbody2D` y `BoxCollider2D`. |
| `PlayerStats.cs` | `Scripts/Player/` | HP, DNA (moneda), upgrades. Método `TakeDamage()`, `Heal()`, `AddDna()`, `SpendDna()`. |
| `PlayerShooter.cs` | `Scripts/Player/` | Dispara balas hacia el ratón. Soporta upgrades: piercing, electric, fasterReload, moreDamage. |
| `PlayerVisual.cs` | `Scripts/Player/` | Anima el sprite del jugador (idle, walk, jump, dash) usando `FrameAnimator`. |

### Enemies
| Script | Ruta | Función |
|--------|------|---------|
| `EnemyBase.cs` | `Scripts/Enemies/` | Movimiento hacia el jugador, daño de contacto, muerte (drops DNA pickup, poison cloud si aplica). Soporta vuelo (Psychic), veneno (Poison), espinas (Spiny), armadura (Armored). |
| `EnemyVisual.cs` | `Scripts/Enemies/` | Anima el sprite del enemigo. 4 tipos: Dino, Mono, enemy3, Diablito (volador). |

### Systems
| Script | Ruta | Función |
|--------|------|---------|
| `WaveManager.cs` | `Scripts/Systems/` | Genera cola de spawns, instancia enemigos con stats escalados por oleada. Cuando la oleada termina, llama a `GameManager.OnWaveComplete()`. |
| `EvolutionSystem.cs` | `Scripts/Systems/` | Tabla de pesos por gen. `Evolve()` ajusta pesos según WaveStats. `RollGene()` elige un gen para el próximo spawn. |

### Defenses
| Script | Ruta | Función |
|--------|------|---------|
| `DefenseBase.cs` | `Scripts/Defenses/` | Base para Barricada, Torreta y Mina. Torreta dispara al enemigo más cercano. Mina explota al contacto. |
| `DefenseBuilder.cs` | `Scripts/Defenses/` | Con `B` entra modo construcción. Click coloca una defensa (costo 30 DNA). Teclas 1/2/3 para tipo. |

### Weapons
| Script | Ruta | Función |
|--------|------|---------|
| `Bullet.cs` | `Scripts/Weapons/` | Proyectil. Soporta piercing (atraviesa N enemigos) y electric (encadena a enemigos cercanos). |

### UI
| Script | Ruta | Función |
|--------|------|---------|
| `HUDController.cs` | `Scripts/UI/` | Actualiza todos los elementos de UI: oleada, DNA, HP, dash, gen dominante, panel de mejoras, pantalla de muerte/pausa/menú/construcción. |

### Utils
| Script | Ruta | Función |
|--------|------|---------|
| `SpriteFactory.cs` | `Scripts/Utils/` | Genera sprites procedurales en runtime: cuadrado, círculo, diamante, bala, dron, jugador, plataforma. |
| `CameraFollow.cs` | `Scripts/Utils/` | Sigue al jugador con suavizado. Límites configurables. Efecto de shake. |
| `ParticleManager.cs` | `Scripts/Utils/` | Crea bursts de partículas en posiciones del mundo. |
| `FrameAnimator.cs` | `Scripts/Utils/` | Cicla frames de Sprite[] a N fps en un SpriteRenderer. |
| `Drone.cs` | `Scripts/Utils/` | Dron que orbita al jugador y dispara al enemigo más cercano (requiere upgrade "drone"). |
| `DnaPickup.cs` | `Scripts/Utils/` | Pickup de DNA que flota, se atrae al jugador cercano y desaparece tras 8s. |
| `PoisonCloud.cs` | `Scripts/Utils/` | Nube de veneno que aparece al morir enemigos Poison. Daña al jugador si entra en rango. |
| `FogAnimator.cs` | `Scripts/Utils/` | Anima el fondo de niebla ácida. |
| `EnemyRenderer.cs` | `Scripts/Utils/` | (Renderizado auxiliar de enemigos) |

---

## Fases del juego (GamePhase)
```csharp
public enum GamePhase { Menu, Playing, Upgrade, Dead, Paused, Building }
```
- **Menu** → pantalla inicial, click/Enter para iniciar
- **Playing** → oleada activa
- **Upgrade** → entre oleadas, elegir mejora (1/2/3 o click)
- **Dead** → jugador murió, `R` para reintentar
- **Paused** → `Esc` para pausar/reanudar
- **Building** → modo construcción con `B`

---

## Upgrades disponibles
| ID | Nombre | Efecto |
|----|--------|--------|
| `piercing` | Balas Perforantes | Balas atraviesan 2 enemigos |
| `electric` | Munición Eléctrica | Cada 5ta bala encadena cercanos |
| `dashExplosive` | Dash Explosivo | Dash deja rastro de explosiones |
| `drone` | Dron Acompañante | Dron orbita y dispara |
| `regen` | Regeneración | +2 HP por segundo |
| `fastBuild` | Constructor Rápido | (reservado para expansión) |
| `moreDamage` | Más Daño | +8 daño por bala |
| `fasterReload` | Recarga Rápida | Cadencia x2.2 |
| `moreHp` | Más Vida Máxima | +40 HP máx y curación |

---

## Prefabs en escena (no son assets, son GameObjects)
Los "prefabs" son GameObjects inactivos en la escena raíz:
- `BulletPrefab` — bala del jugador/torreta
- `EnemyPrefab` — enemigo base con EnemyBase + EnemyVisual
- `BarricadePrefab` — defensa tipo barricada
- `TurretPrefab` — defensa tipo torreta
- `MinePrefab` — defensa tipo mina

---

## Jerarquía de objetos en escena
```
Main Camera (+ CameraFollow + EnemyRenderer)
Managers/
  ├── GameManager
  ├── EvolutionSystem
  ├── ParticleManager
  ├── SpriteFactory/
  └── WaveManager
Level/
  ├── Background
  ├── AcidFog
  ├── Ground
  └── Platform_1 ... Platform_8
Enemies/          ← container para enemigos instanciados
Projectiles/      ← container para balas
Defenses/         ← container para defensas construidas
BulletPrefab      ← inactivo
EnemyPrefab       ← inactivo
BarricadePrefab   ← inactivo
TurretPrefab      ← inactivo
MinePrefab        ← inactivo
Player/
  ├── PlayerStats
  ├── PlayerController
  ├── PlayerShooter
  ├── BoxCollider2D
  ├── Rigidbody2D
  ├── Visual/ (FrameAnimator + PlayerVisual + SpriteRenderer)
  ├── Muzzle
  └── Drone/ (Drone script)
Canvas/
  └── HUDController (con todas las referencias de UI asignadas)
EventSystem
```

---

## Estado del proyecto (junio 2026)
- **Compilación:** Sin errores. 0 warnings tras las correcciones.
- **Play Mode:** Funciona sin excepciones.
- **Correcciones aplicadas:** 116 NullReferenceExceptions eliminadas añadiendo null guards en 11 scripts.
- **Pendiente:** No hay bugs conocidos. El juego es completamente jugable.

---

## Cómo iniciar el proyecto
1. Abrir Unity Hub → proyecto en `C:\Game\2Dfinal`
2. Si la escena está vacía o desconfigurada: `MutationSwarm > Setup Scene` en la barra de menú de Unity
3. Presionar **Play**
4. En el menú del juego: `Enter` o click en **JUGAR**
