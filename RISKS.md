# RISKS — Mutation Swarm 2D
**Fecha:** 2026-06-01  
**Origen:** Auditoría de código y assets

---

## LEYENDA
- 🔴 CRÍTICO — Rompe compilación o causa crash en runtime
- 🟠 ALTO — Causa bugs visuales o de gameplay graves
- 🟡 MEDIO — Afecta funcionalidades secundarias
- 🟢 BAJO — Problemas de calidad o pulido

---

## RIESGOS CRÍTICOS

### RSK-01 — Enemigos sin componente Animator
**Nivel:** 🔴 CRÍTICO  
**Descripción:** Los 5 prefabs de enemigos (Drone, Boss, Queen, Mimic, Parasite) no tienen un componente `Animator` asignado. El código de la Fase 3 necesitará hacer `GetComponent<Animator>()` en `Initialize()`. Si se añade código de Animator antes de agregar el componente al prefab, se producirá `NullReferenceException` en runtime.  
**Impacto:** Crash en runtime al inicializar cualquier enemigo.  
**Mitigación:** Agregar null-check en todos los accesos al Animator (`_animator?.SetTrigger()`). Agregar el componente al prefab antes de modificar los scripts.

---

### RSK-02 — Animation Clips de Player posiblemente vacíos
**Nivel:** 🔴 CRÍTICO  
**Descripción:** Los 8 archivos `.anim` del player existen en disco, pero no se ha verificado si contienen frames de sprites asignados o son clips vacíos (sin keyframes). Un clip vacío en el Animator Controller no causará crash pero producirá animaciones T-pose o congeladas.  
**Impacto:** Player sin animaciones visuales en runtime.  
**Mitigación:** Abrir cada clip en Unity y verificar el contenido del Animation Window.

---

### RSK-03 — Genome.Mutate() vacío
**Nivel:** 🔴 CRÍTICO  
**Descripción:** El método `Mutate()` en `Script_08_Genome.cs:88` está vacío. El `WaveManager` o `EvolutionEngine` puede llamarlo esperando que mute el genoma; si retorna sin cambios, la evolución nunca ocurrirá y todos los enemigos tendrán el mismo genoma base wave tras wave.  
**Impacto:** Sistema de evolución completamente no funcional. El juego no escala en dificultad.  
**Mitigación:** Implementar mutación por perturbación gaussiana o por bit-flip sobre cada trait del Genome.

---

### RSK-04 — EvolutionEngine.ProcessWave() retorna lista vacía
**Nivel:** 🔴 CRÍTICO  
**Descripción:** `Script_07_EvolutionEngine.cs` tiene `ProcessWave()` retornando una lista vacía. Si el `WaveManager` usa esta lista para determinar qué genomas spawnear, no spawneará ningún enemigo.  
**Impacto:** Sin enemigos en oleadas 2+. Juego no funcional después de la primera oleada.  
**Mitigación:** Verificar que `WaveManager` tenga un fallback a genomas por defecto cuando la lista esté vacía. Implementar `ProcessWave()` antes de las pruebas de QA.

---

### RSK-05 — Genome.Crossover() clona en vez de combinar
**Nivel:** 🟠 ALTO  
**Descripción:** El método `Crossover()` retorna un clon del genoma actual en lugar de combinar genes de dos padres. Esto significa que la reproducción sexual del sistema evolutivo no funciona, y los enemigos nunca heredarán traits mixtos.  
**Impacto:** Sistema evolutivo genéticamente no funcional. Sin diversidad genética entre enemigos.  
**Mitigación:** Implementar cruzamiento real (single-point crossover o uniform crossover) entre dos genomas padre.

---

## RIESGOS ALTOS

### RSK-06 — ObjectPool.ReturnAll() vacío
**Nivel:** 🟠 ALTO  
**Descripción:** `Script_04_ObjectPool.cs` tiene el método `ReturnAll()` sin implementar (comentario: "Implementación completa en PROMPT 08"). Si se llama este método (posiblemente en cambio de escena o wave end), los proyectiles no serán devueltos al pool.  
**Impacto:** Memory leak acumulativo. Los proyectiles no serán reutilizados. Posible degradación de rendimiento en sesiones largas.  
**Mitigación:** Implementar `ReturnAll()` iterando sobre todos los objetos activos y llamando `Return()` sobre cada uno.

---

### RSK-07 — TriggerSuicideExplosion() sin daño real
**Nivel:** 🟠 ALTO  
**Descripción:** `Script_13_EnemyBase.cs:216-220` — `TriggerSuicideExplosion()` solo llama a `Die()`. No hay VFX ni daño en área al jugador. La condición `ShouldSuicideExplode()` se activa cuando `ExplosionAlMorir > 0.5f`, lo que puede ocurrir con frecuencia dado que es un trait genético.  
**Impacto:** Los enemigos con alta `ExplosionAlMorir` mueren silenciosamente sin feedback visual ni daño al jugador. El sistema de explosión suicida es efectivamente inexistente.  
**Mitigación:** Implementar `Physics2D.OverlapCircleAll()` en área para aplicar daño, más instanciar un prefab de VFX de explosión.

---

### RSK-08 — Script_15_EnemyBoss: fase única
**Nivel:** 🟠 ALTO  
**Descripción:** `Script_15_EnemyBoss.cs` define `_currentPhase` pero nunca lo incrementa. El Boss no tiene ninguna lógica de cambio de fase. El comentario en la clase indica "3 fases basadas en genomas top del historial".  
**Impacto:** El boss se comporta idéntico a un Drone de mayor tamaño durante toda la pelea.  
**Mitigación:** Implementar `OnHpThresholdCrossed()` en `TakeDamage()` para cambiar de fase cuando HP < 66% y < 33%.

---

### RSK-09 — Script_16/17/18: subclases vacías
**Nivel:** 🟠 ALTO  
**Descripción:** Queen, Mimic y Parasite solo contienen el namespace y heredan la clase base sin ningún override. Sus comentarios describen comportamientos únicos que justifican su existencia como tipos separados:
- Queen: "genera líneas evolutivas y spawnea drones genéticos"
- Mimic: "copia la habilidad del jugador más cercano"
- Parasite: "se adhiere a enemigos y propaga mutaciones"

**Impacto:** Los tres tipos de enemigos son visualmente distintos pero funcionalmente idénticos al Drone base.  
**Mitigación:** Implementar los comportamientos únicos en métodos `override` de `Update()` o estados de IA especializados.

---

### RSK-10 — AdaptivePressure sin implementar
**Nivel:** 🟠 ALTO  
**Descripción:** `Script_10_AdaptivePressure.cs` está marcado como placeholder para "PROMPT 02". Este sistema debería ajustar la presión evolutiva en tiempo real basándose en el rendimiento del jugador.  
**Impacto:** Sin ajuste dinámico de dificultad. El juego puede volverse demasiado fácil o imposible dependiendo de la habilidad del jugador.  
**Mitigación:** Implementar métricas básicas (kills/minuto, daño recibido/oleada) y usar esos valores para modular los parámetros de selección en `SelectionAlgorithm`.

---

## RIESGOS MEDIOS

### RSK-11 — LayerMask no configuradas en prefabs
**Nivel:** 🟡 MEDIO  
**Descripción:** `Script_13_EnemyBase.cs` expone `_playerMask` y `_enemyMask` como `[SerializeField]`. Si estas máscaras no están configuradas en los prefabs de enemigos, `GetNearestPlayer()` y `GetNearbyAllies()` retornarán null/vacío y el enemigo permanecerá en `IdleState` indefinidamente.  
**Impacto:** Enemigos sin comportamiento AI en runtime si las capas no están asignadas.  
**Mitigación:** Verificar que los prefabs de enemigos tienen `_playerMask` apuntando a la capa "Player" y `_enemyMask` a la capa "Enemy".

---

### RSK-12 — Script_11_PlayerController: _primaryWeapon null
**Nivel:** 🟡 MEDIO  
**Descripción:** `_primaryWeapon` se inicializa como `[SerializeField]` sin valor por defecto. Si el prefab del player no tiene un arma asignada, `HandleCombatInput()` ejecutará `_primaryWeapon?.Fire()` sin disparar nada. El trigger `Attack` se activará pero no habrá proyectil.  
**Impacto:** Player sin capacidad de ataque si el prefab no tiene arma configurada.  
**Mitigación:** Verificar que `Prefab_Player.prefab` tiene una referencia a `SO_Weapon_*.asset` como arma principal.

---

### RSK-13 — PlayerController referencia a Script_12_PlayerStats.OnDeath
**Nivel:** 🟡 MEDIO  
**Descripción:** En `Awake()`, el PlayerController se suscribe a `_stats.OnDeath += HandleDeath`. Si `Script_12_PlayerStats` no tiene el evento definido exactamente como `public event Action OnDeath`, habrá error de compilación. Esto es un punto de acoplamiento frágil.  
**Impacto:** Error de compilación si el evento cambia de nombre o firma.  
**Mitigación:** Verificar que `Script_12_PlayerStats` define `public event System.Action OnDeath`.

---

### RSK-14 — Prefab_Enemy_Geo sin script asignado claro
**Nivel:** 🟡 MEDIO  
**Descripción:** Existe `Prefab_Enemy_Geo.prefab` que no tiene un script correspondiente evidente (`Script_13_EnemyBase` es la base). Este prefab parece ser una variante geométrica del enemy base usada para desarrollo/debug.  
**Impacto:** Posible confusión en QA. No se sabe qué script usa este prefab ni si está correctamente configurado.  
**Mitigación:** Verificar el contenido del prefab y decidir si es temporal o permanente.

---

### RSK-15 — Sin sistema de pausa para animaciones
**Nivel:** 🟡 MEDIO  
**Descripción:** `Script_01_GameManager.cs` tiene estados `Paused` y `Playing`, pero no hay lógica que detenga el `Animator` de los enemigos cuando el juego está pausado. Las animaciones de Unity siguen corriendo mientras `Time.timeScale > 0`.  
**Impacto:** Animaciones de enemigos continúan durante la pausa si se usa `Time.timeScale = 0f` (estándar en Unity). Esto es en realidad el comportamiento correcto si se usa `Time.timeScale`, pero hay que verificarlo.  
**Mitigación:** Confirmar que el sistema de pausa usa `Time.timeScale = 0f`. Si usa otro mecanismo, agregar `animator.speed = 0` on pause.

---

## RIESGOS BAJOS

### RSK-16 — Sin compresión de texturas configurada
**Nivel:** 🟢 BAJO  
**Descripción:** Las .meta de los sprites muestran configuraciones de importación que pueden no estar optimizadas para builds de producción.  
**Impacto:** Builds más grandes de lo necesario.  
**Mitigación:** Revisar ajustes de compresión en Platform-specific overrides antes del build final.

### RSK-17 — Editor scripts en carpeta _Scripts en lugar de Editor/
**Nivel:** 🟢 BAJO  
**Descripción:** Los 11 editor scripts están en `Assets/_Scripts/Editor/`. Esto es correcto (Unity detecta la carpeta `Editor/`), pero mezclan scripts de runtime con scripts de editor si alguno sale de esa subcarpeta.  
**Impacto:** Ninguno si la estructura se mantiene. Risk si se reorganiza.  
**Mitigación:** No reorganizar sin verificar que los editor scripts permanecen dentro de una carpeta `Editor/`.

---

## MATRIZ DE RIESGO

| ID | Descripción | Probabilidad | Impacto | Prioridad |
|---|---|---|---|---|
| RSK-01 | Enemigos sin Animator | Alta | Crash | 🔴 P0 |
| RSK-02 | Clips Player vacíos | Media | Sin animaciones | 🔴 P0 |
| RSK-03 | Mutate() vacío | Alta | Sin evolución | 🔴 P0 |
| RSK-04 | ProcessWave() vacío | Alta | Sin enemigos | 🔴 P0 |
| RSK-05 | Crossover() clona | Alta | Sin diversidad genética | 🟠 P1 |
| RSK-06 | ReturnAll() vacío | Alta | Memory leak | 🟠 P1 |
| RSK-07 | Explosión sin daño | Alta | Mechanic rota | 🟠 P1 |
| RSK-08 | Boss sin fases | Alta | Boss trivial | 🟠 P1 |
| RSK-09 | Subclases vacías | Alta | 3 enemy types idénticos | 🟠 P1 |
| RSK-10 | AdaptivePressure | Alta | Sin dificultad dinámica | 🟠 P1 |
| RSK-11 | LayerMask no config | Media | Sin AI | 🟡 P2 |
| RSK-12 | Weapon null | Media | Sin ataque player | 🟡 P2 |
| RSK-13 | OnDeath acoplado | Baja | Error compilación | 🟡 P2 |
| RSK-14 | Prefab_Geo unclear | Baja | Confusión QA | 🟡 P2 |
| RSK-15 | Pause + animaciones | Baja | Visual bug | 🟡 P2 |
| RSK-16 | Sin compresión | Baja | Build size | 🟢 P3 |
| RSK-17 | Editor en _Scripts | Baja | Riesgo futuro | 🟢 P3 |
