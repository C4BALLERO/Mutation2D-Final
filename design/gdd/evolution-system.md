# GDD — Sistema de evolución

## Overview

Tras cada oleada, el motor evolutivo genera una nueva generación de enemigos a partir de genomas (`Script_08_Genome`), selección por torneo + elitismo (`Script_09`) y presión adaptativa según el estilo del jugador (`Script_10`).

## Detailed design

### Entradas

- Muertes / daño por tipo de arma
- Tiempo en movimiento vs estático
- Uso de dash, construcción, etc.

### Salidas

- Genoma siguiente oleada: velocidad, HP, resistencias, flags de comportamiento
- Visual: color/material en `Script_13_EnemyBase`

### Límites

- Mutaciones acotadas por rangos en SO / genome base
- No romper TTK mínimo/máximo por oleada (ver balance)

## Edge cases

- Primera oleada: población inicial desde `SO_Genome_Base`
- Todos los jugadores muertos: no evolucionar; game over
- Oleada sin bajas enemigas: mantener genoma dominante con mutación leve

## Fórmulas (referencia)

- Fitness ∝ daño infligido al jugador + tiempo vivo
- Presión adaptativa: bonus a genes que contrarrestan la táctica dominante del jugador

## Implementación

`Assets/_Scripts/Evolution/`, `Assets/_ScriptableObjects/Evolution/`
