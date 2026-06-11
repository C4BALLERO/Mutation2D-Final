# GDD — Sistema de oleadas

## Overview

`Script_02_WaveManager` controla spawn por puntos, escalado coop y transición a fase de mejora (`UpgradePhase`).

## Estados

- `Combat` — spawns activos según `SO_WaveConfig`
- `UpgradePhase` — construcción (`Script_23_BuildManager`), carga aditiva `Scene_03_UpgradeMenu` (pendiente cablear)

## Edge cases

- Sin spawn points válidos: log error, no spawn
- Jugador muerto: pausar spawn o fin de oleada según diseño futuro
- Coop: escalar cantidad/HP por `playerCount`

## Balance

Revisar `Assets/_ScriptableObjects/Waves/` y skill `mutation-balance-check`.
