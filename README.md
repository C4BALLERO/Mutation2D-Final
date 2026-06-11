# Mutation Swarm 2D (Unity 6000.3.8f1)

**Documentación completa del proyecto:** [`docs/PROJECT_DOCUMENTATION.md`](docs/PROJECT_DOCUMENTATION.md)

Survival horde shooter cooperativo en una sola pantalla 2D estática.  
Los enemigos evolucionan entre oleadas con un sistema genético (`Genome`) que responde a la estrategia del jugador.

## Historia breve

En el sector de investigación **Argos-9**, una colonia de organismos sintéticos escapó del laboratorio principal.  
El enjambre muta en tiempo real, hereda rasgos de los supervivientes y aprende de cada táctica humana.  
Tu escuadrón (1–4 jugadores) debe resistir oleadas, adaptarse más rápido que la plaga y sobrevivir a la **Reina Evolutiva**.

## Escenas base (nomenclatura)

- `Scene_00_Boot`: inicializa singletons (`GameManager`, `InputManager`, `SaveManager`, `AudioManager`).
- `Scene_01_MainMenu`: pantalla de inicio, selección de jugadores, opciones.
- `Scene_02_GameWorld`: arena fija principal (juego real).
- `Scene_03_UpgradeMenu`: escena aditiva de mejoras entre oleadas.

## Scripts principales (resumen rápido)

### Core
- `Script_01_GameManager`: estado global de la partida, coop, game over.
- `Script_02_WaveManager`: control de oleadas y fases.
- `Script_03_EventBus`: eventos globales tipados (`Action<T>`).
- `Script_04_ObjectPool`: reutilización de enemigos/proyectiles/VFX.
- `Script_05_SaveManager`: PlayerPrefs + JSON.
- `Script_06_InputManager`: base de input 1–4 jugadores.

### Evolution
- `Script_07_EvolutionEngine`: procesa datos de combate y crea siguiente generación.
- `Script_08_Genome`: genes mutables del enemigo.
- `Script_09_SelectionAlgorithm`: torneo + elitismo.
- `Script_10_AdaptivePressure`: contraestrategia por comportamiento del jugador.

### Entities
- `Script_11_PlayerController`: movimiento avanzado (dash, coyote time, jump buffer, wall jump) y combate.
- `Script_12_PlayerStats`: vida y atributos del jugador.
- `Script_13_EnemyBase`: aplica genome, color/material mutación, combate y muerte.
- `Script_14_EnemyStateMachine`: estados `Idle`, `Pursue`, `Attack`, `Flee`, `Swarm`.
- `Script_15_EnemyBoss`, `Script_16_EnemyQueen`, `Script_17_EnemyMimic`, `Script_18_EnemyParasite`.

### Combat
- `Script_19_Projectile`: proyectil pooled.
- `Script_20_WeaponBase`: arma base.
- `Script_21_DamageSystem`: cálculo de daño/resistencias.
- `Script_22_StatusEffects`: poison/freeze/burn/stun.

### Building
- `Script_23_BuildManager`: construcción en `UpgradePhase`, validación de posición, preview.
- `Script_24_StructureBase`: clase base estructuras.
- `TurretStructure`: torreta automática (escaneo + disparo pooled).
- `BarricadeStructure`: barricada de bloqueo con HP.

### UI
- `Script_25_HUDController`: HUD principal + toasts de mutación + panel evolución.
- `Script_26_UpgradeMenuUI`: cartas de mejora con timer y navegación gamepad/teclado.
- `Script_27_EvolutionDisplayUI`: panel genético de la horda.
- `HUD_Main.uxml` + `HUD_Main.uss`: layout/estilo del HUD.
- `BuildRadialMenu.uxml` + `BuildRadialMenu.uss`: menú radial de construcción.
- `UpgradeMenu.uxml`: cartas de upgrade.

## Prefabs que puedes crear rápido en Unity

### `Prefab_Player`
Componentes requeridos:
- `Rigidbody2D` (Gravity Scale `3`, Collision Detection `Continuous`)
- `BoxCollider2D`
- `CapsuleCollider2D` (`IsTrigger = true`)
- `Animator`
- `SpriteRenderer`
- `Script_12_PlayerStats`
- `Script_11_PlayerController`

Jerarquía sugerida:
- `Prefab_Player`
  - `GroundCheck` (Empty)
  - `FirePoint` (Empty)
  - `Weapon_Primary`
  - `Weapon_Secondary`
  - `VFX_Dash` (ParticleSystem)

### `Prefab_Enemy_Drone`
- `Rigidbody2D`
- Collider 2D
- `SpriteRenderer`
- `Script_13_EnemyBase`
- `Script_22_StatusEffects`

### `Prefab_Structure_Turret`
- Collider 2D
- `TurretStructure`
- `FirePoint` (Empty)

### `Prefab_Structure_Barricade`
- `BoxCollider2D`
- `BarricadeStructure`
- Canvas World Space (opcional para barra de vida)

## UI de inicio (Main Menu sugerido)

En `Scene_01_MainMenu` crea un `UIDocument` con:
- Botón **Jugar**
- Selector **1–4 jugadores**
- Botón **Opciones**
- Botón **Salir**
- Panel de texto con “Historia breve” (sección de arriba)

Al pulsar **Jugar**:
1. `Script_01_GameManager.StartGameSession(playerCount)`
2. Cargar `Scene_02_GameWorld`

## Notas de implementación importantes

- Detección enemiga con `Physics2D.OverlapCircle` (sin triggers para visión).
- Comunicación de UI y gameplay por `Script_03_EventBus` (sin `FindObjectOfType`).
- Construcción habilitada solo durante `WaveState.UpgradePhase`.
- Materiales de construcción no persisten entre sesiones (`ResetMaterialsForNewSession`).

## UI Kenney + nivel geométrico jugable

Pack: `Assets/_Art/kenney_ui-pack-space-expansion` (SVG → sprites claros **Blue / Yellow / Green / Extra**).

En Unity ejecuta:

**`Tools → Mutation Swarm → Build Kenney UI + Playable Geometric Level`**

Genera:
- **Menú** (`Scene_01_MainMenu`) con botones uGUI Kenney (panel cristal, botones azul/verde).
- **Nivel jugable** (`Scene_02_GameWorld`) fondo claro, plataformas Kenney amarillas.
- **Jugador:** cuadrado azul (`Prefab_Player_Geo`).
- **Enemigos:** círculos rojos (`Prefab_Enemy_Geo`) + oleada automática al iniciar.
- **Proyectiles:** círculos amarillos con daño y pool `Projectile_Geo`.

Controles: WASD / flechas, espacio salto, shift dash, clic / RT disparar.

## Pack de armas (Guns V1.01)

Carpeta copiada en `Assets/_Art/GunsPack/`.

En Unity:

**`Tools → Mutation Swarm → Import Guns Pack + Weapon Shop`**

Crea 6 armas (Glock, Revolver, MP5, AK-47, Bazooka x2), proyectiles en pool, ScriptableObjects y la **tienda** en `Scene_02`.

### Tienda entre oleadas

- Se abre tras las oleadas **2, 5, 8, 11, 14** y cada **3 oleadas** (3, 6, 9…).
- Pagas con **materiales** (los que ganas al matar enemigos).
- Compra y equipa; el sprite del arma se ve en el jugador.
- **Continuar oleada** o **Esc** para seguir.

Configura oleadas en `Script_39_WeaponShopManager` → `_shopAfterWaves`.

## Jugador — Power armor Argos-9

Tu imagen de referencia se guarda en `Assets/_Art/Sprites/Player/Spr_Player_ArgosArmor_source.png`.

Menú Unity:

**`Tools → Mutation Swarm → Build Player Sprite (Argos Armor)`**

Genera spritesheet **sin fondo** (`Spr_Player_ArgosArmor_sheet.png`), animaciones **Idle / Walk** (brazos y piernas), y `AC_Player` con estados: Idle, Walk, Fall, Jump, Dash, Attack, Hit, Die. Actualiza `Prefab_Player` + `Prefab_Player_Geo`.

## Sprites de enemigos (art bible)

Menú Unity:

**`Tools → Mutation Swarm → Build Enemy Sprites (Art Bible)`**

Genera en `Assets/_Art/Sprites/Enemies/` y prefabs en `Assets/_Prefabs/Enemies/`:

| Enemigo | Silueta | Prefab |
|---------|---------|--------|
| Drone | Círculo + antenas | `Prefab_Enemy_Drone` |
| Boss | Cuadrado grande | `Prefab_Enemy_Boss` |
| Queen | Disco + corona | `Prefab_Enemy_Queen` |
| Mimic | Cuadrado “glitch” | `Prefab_Enemy_Mimic` |
| Parasite | Triángulo pequeño | `Prefab_Enemy_Parasite` |

Specs: `design/assets/specs/enemy_*.md`. El tinte de mutación sigue en runtime (`Genome.GetMutationColor`).

Asigna `Prefab_Enemy_Drone` en `WaveManager` → `_enemyPrefab`. Boss/Queen/Mimic/Parasite para oleadas especiales.

## Paquete de arte (`Assets/_Art/Materials`)

Catálogo detallado: [`Assets/_Data/ART_PACKAGE_CATALOG.md`](Assets/_Data/ART_PACKAGE_CATALOG.md)

En Unity:

1. **`Tools → Mutation Swarm → Import Art Package Settings`** — configura todas las PNG como sprites (PPU 16, Point).
2. **`Tools → Mutation Swarm → Build Art Level (Scene_02)`** — genera la arena con parallax, plataformas de césped, zona púrpura, bloques galaxia, árboles y rocas.

## Generación automática (escenas + prefabs)

Al abrir el proyecto en Unity, aparecerá un diálogo para generar todo automáticamente.  
También puedes ejecutarlo manualmente:

**`Tools → Mutation Swarm → Setup Complete Project`**

Esto crea:

- Escenas: `Scene_00_Boot`, `Scene_01_MainMenu`, `Scene_02_GameWorld`, `Scene_03_UpgradeMenu`
- Prefabs: jugador, enemigo drone, proyectil, torreta, barricada
- ScriptableObjects: oleadas, pool, estructuras, upgrades
- Capas físicas: `Player`, `Enemy`, `Projectile_*`, `Platform`, `BuildSurface`, `Structure`
- Build Settings con las 4 escenas en orden

### Flujo de juego

1. `Scene_00_Boot` carga singletons y pasa a menú.
2. `Scene_01_MainMenu` selecciona jugadores y llama `StartGameSession()`.
3. `Scene_02_GameWorld` es la arena principal.
4. `Scene_03_UpgradeMenu` se carga en modo aditivo entre oleadas (pendiente conectar en `WaveManager`).

## Pack de armas + tienda entre oleadas

El pack **Guns Update V1.01** vive en `Assets/_Art/GunsPack/`. Para generar datos, proyectiles y la tienda en escena:

**`Tools → Mutation Swarm → Import Guns Pack + Weapon Shop`**

Crea 6 armas (`SO_Weapon_*` en `Assets/_ScriptableObjects/Combat/Weapons/`), prefabs de proyectil y el objeto `_WeaponShop` en `Scene_02_GameWorld`.

| Arma | Coste (materiales) |
|------|-------------------|
| Glock P80 | 0 (inicial) |
| Revolver Colt 45 | 18 |
| MP5A3 | 35 |
| AK-47 | 50 |
| Bazooka M20 | 75 |
| Thick Bazooka | 110 |

Tras las oleadas **2, 5, 8, 11, 14** y cada **3ª oleada**, se abre la tienda (pausa con `timeScale = 0`). Compras con **materiales de construcción** (los mismos que sueltas los enemigos). Pulsa **Continuar** o **Esc** para cerrar y arrancar la siguiente oleada.

El jugador usa `Script_38_PlayerLoadout` (Glock al inicio). El HUD muestra el arma equipada vía `WeaponEquippedEvent`.

## AI Studio (CCGS + Cursor)

Este repo integra una adaptación de [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) para trabajar en **Cursor**:

- **`AGENTS.md`** — contexto del proyecto, convenciones y roles
- **`.cursor/rules/`** — estándares por carpeta (gameplay, UI, editor…)
- **`.cursor/skills/`** — `mutation-start`, `mutation-build`, `mutation-balance-check`, **`mutation-visual-art`** (personajes/sprites/Animator), etc.
- **`design/`** — GDDs y pilares (evolución, oleadas)
- **`docs/CCGS-INTEGRATION.md`** — qué se importó y cómo actualizar el template

En el chat de Cursor, el agente puede usar esas skills cuando pidas build, balance o auditoría del proyecto.

## Próximos pasos recomendados

1. Ejecutar **Setup Complete Project** en Unity si aún no lo hiciste.
2. Conectar `WaveManager` con `BuildManager.StartBuildPhase()` y carga aditiva de `Scene_03_UpgradeMenu`.
3. Reemplazar sprites placeholder por arte final en `Assets/_Art/Sprites`.
4. Añadir Animator Controller al `Prefab_Player`.
cr