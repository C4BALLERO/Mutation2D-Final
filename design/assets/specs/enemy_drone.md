# Spec — Enemy Drone

| Campo | Valor |
|-------|--------|
| Archivo sprite | `Assets/_Art/Sprites/Enemies/Spr_Enemy_Drone.png` |
| Prefab | `Assets/_Prefabs/Enemies/Prefab_Enemy_Drone.prefab` |
| Silueta | Cuerpo circular + antenas (art bible: enemigo base rojo) |
| PPU | 64, Point |
| Collider | Circle r=0.35, scale 1.0 |
| Script | `Script_13_EnemyBase` |
| Mutación | Tinte vía `Genome.GetMutationColor()` — no redibujar sprite |
