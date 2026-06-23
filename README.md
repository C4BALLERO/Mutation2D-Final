# 🧬 MUTATION SWARM 2D

Juego 2D de **supervivencia por oleadas** hecho en Unity (URP). Eres el **Agente X-7**, último soldado de la Unidad de Hazmat Biológica, atrapado en una zona industrial infestada por el virus **MUTATION-X**. Los infectados evolucionan con cada oleada... y tú también puedes mutar.

---

## 🎮 Controles

| Tecla | Acción |
|-------|--------|
| **A / D** | Moverte |
| **W / Espacio** | Saltar (doble salto disponible) |
| **Shift** | Dash (esquive rápido) |
| **Clic izquierdo** | Disparar (apuntas con el mouse) |
| **B** | Entrar / salir del **modo construcción** |
| **1 / 2 / 3** | Elegir defensa (en construcción) / mejora (al subir de oleada) |
| **F** | **¡FURIA!** (cuando la barra esté llena) |
| **Esc** | Pausa |
| **R** | Reiniciar (al morir) |

---

## ⚙️ Mecánicas principales

### 🌊 Oleadas y evolución
El juego avanza por oleadas. Los enemigos tienen **genes** (Veneno, Velocidad, Espinas, Armadura, Psíquico, Corrupto) y cada oleada hay una **MUTACIÓN DOMINANTE** que refuerza un tipo. Cuanto más sobrevives, más letal se vuelve el enjambre.

### 🔥 Combos y multiplicador
Encadena muertes **sin recibir daño** para subir tu multiplicador:
- **x2** con 5 bajas seguidas
- **x3** con 10
- **x4** con 20

El multiplicador aumenta el **ADN** que ganas. Si te golpean, el combo se reinicia.

### ⚡ Modo Furia (Overdrive)
Cada enemigo que matas llena la **barra de furia**. Cuando esté llena, pulsa **`F`** para entrar en Furia durante 5 segundos:
- 🛡️ Invencibilidad
- 🔫 Disparo rápido y más daño
- 🏃 Más velocidad
- 💥 Onda expansiva inicial que daña a los enemigos cercanos

### ☣️ Mutaciones del jugador
Los **jefes** (siempre) y los enemigos **corruptos** (a veces) sueltan **mutágeno** (orbe verde). Cada 3 mutágenos obtienes una **mutación permanente** al azar:
- **Sangre Tóxica** — los enemigos se dañan al tocarte
- **Robo de Vida** — te curas al matar
- **Frenesí** — +60% de daño cuando tienes poca vida
- **Volátil** — los enemigos explotan al morir, dañando en cadena

### ☠️ Jefes cada 5 oleadas
Cada **5 oleadas** aparece un **JEFE MUTANTE**: gigante (2.7×), con muchísima vida, resistente a las balas y capaz de lanzar bolas de fuego mientras avanza. Tiene su propia **barra de vida** en la parte superior. Al derrotarlo suelta mucho ADN y mutágeno garantizado.

### 🛠️ Defensas (modo construcción)
Pulsa **B** para construir con tu ADN:
- **Barricada** — bloquea el paso de los enemigos
- **Torreta** — dispara automáticamente al enemigo más cercano
- **Mina** — explota al contacto

### 🐔 Dron POLLO
Un dron experimental con inteligencia... de gallina. Se compra en la **tienda de mejoras** (upgrade *Dron Acompañante*). Orbita a tu alrededor y dispara a los enemigos.

### ⬆️ Mejoras entre oleadas
Al completar una oleada eliges una mejora: balas perforantes, munición eléctrica, dash explosivo, dron, regeneración, más daño, recarga rápida, más vida, etc.

---

## 🔊 Audio
Banda sonora y efectos **chiptune / 8-bit arcade** generados proceduralmente: soundtrack en bucle, gruñidos de enemigos, sonidos de muerte, disparos, daño y recompensas.

---

## ▶️ Cómo jugar
1. Ejecuta `Builds/MutationSwarm/Mutation Swarm.exe`, **o**
2. Abre el proyecto en Unity 6 y usa el menú **`MutationSwarm → Setup Scene`**, luego pulsa **Play**.

> La escena se arma de forma procedural desde `Assets/Scripts/Core/SceneSetup.cs`.

---

*Sobrevive. La humanidad cuenta contigo.*
