---
name: mutation-balance-check
description: Revisa balance de oleadas, genomas, daño y economía de materiales en Mutation Swarm. Usar tras cambiar SO de Waves, Evolution, Combat o upgrades.
---

# Balance check — Mutation Swarm

Adaptado de CCGS `/balance-check`.

## Dominios

| Dominio | Archivos |
|---------|----------|
| Oleadas | `Assets/_ScriptableObjects/Waves/`, `Script_02_WaveManager` |
| Evolución | `Assets/_ScriptableObjects/Evolution/`, `Script_07`–`Script_10`, `design/gdd/evolution-system.md` |
| Combate | `Script_21_DamageSystem`, `Script_19_Projectile`, armas |
| Economía build | `Script_23_BuildManager`, SO estructuras |

## Análisis

1. Leer GDD del sistema en `design/gdd/`
2. Extraer valores numéricos de SO y código
3. Comprobar:
   - TTK enemigo por oleada no cae a 0 ni sube sin límite
   - Genomas no generan resistencia total a la táctica del jugador
   - Escalado coop no duplica dificultad de forma rota
   - Materiales de build: faucets/sinks entre oleadas

## Informe

- Tabla de outliers (valor, archivo, recomendación)
- Estrategias degeneradas (ej. solo torretas, solo dash)
- Cambios sugeridos como SO, no hardcode

No modificar assets sin aprobación del usuario.
