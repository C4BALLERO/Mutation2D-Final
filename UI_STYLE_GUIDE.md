# UI STYLE GUIDE — Mutation Swarm 2D
**Versión:** 1.0 · **Fecha:** 2026-06-02  
**Estilo:** Biopunk / Sci-Fi Dark Terminal

---

## Paleta de Colores

| Token | Hex | RGB | Uso |
|---|---|---|---|
| `--green-mutant` | `#2DFF7A` | `rgb(45, 255, 122)` | Confirmaciones, salud alta, evolución |
| `--cyan-tech` | `#00D4FF` | `rgb(0, 212, 255)` | Elementos interactivos, valores primarios, bordes activos |
| `--red-alert` | `#FF3030` | `rgb(255, 48, 48)` | Peligro, vida baja, alertas críticas, botón Salir |
| `--yellow-warn` | `#FFC832` | `rgb(255, 200, 50)` | Advertencias, oleada activa, record |
| `--purple-genome` | `#AA5AFF` | `rgb(170, 90, 255)` | Genes, mutaciones, datos genéticos |
| `--bg-deep` | `#04050A` | `rgb(4, 5, 10)` | Fondo de escena |
| `--bg-panel` | `#0A0E16` | `rgba(10, 14, 22, 0.96)` | Paneles, tarjetas |
| `--bg-card` | `#111520` | `rgba(17, 21, 32, 0.96)` | Tarjetas de upgrade |
| `--text-primary` | `#E4EBF5` | `rgb(228, 235, 245)` | Texto principal |
| `--text-dim` | `#8A9BAE` | `rgb(138, 155, 174)` | Texto secundario, etiquetas |
| `--border-subtle` | — | `rgba(0, 212, 255, 0.15)` | Bordes de paneles inactivos |
| `--border-active` | — | `rgba(0, 212, 255, 0.5)` | Bordes de paneles activos |
| `--border-selected` | — | `rgba(45, 255, 122, 0.5)` | Elemento seleccionado |

---

## Tipografía

Unity UI Toolkit usa la fuente del sistema por defecto. La jerarquía es:

| Nivel | Tamaño | Estilo | Color | Uso |
|---|---|---|---|---|
| Eyebrow | 9px | Normal | `--green-mutant` (60%) | Categoría sobre título |
| Title | 44–58px | Bold | `--text-primary` | Títulos de escena |
| Tagline | 13–16px | Normal | `--text-dim` | Subtítulo |
| Section | 9px | Normal | `--text-dim` (70%) | Etiqueta de sección |
| Body | 11–13px | Normal | `--text-dim` | Descripciones |
| Value | 15–22px | Bold | `--green-mutant` o `--cyan-tech` | Valores numéricos |
| Detail | 9–10px | Normal | `--text-dim` (40–60%) | Detalles técnicos |
| Hotkey | 9px | Normal | `--cyan-tech` (35%) | Atajos de teclado |

**Letter-spacing:**
- Títulos grandes: 4–6px
- Eyebrows/secciones: 2–4px
- Cuerpo: 0–1px

---

## Componentes

### Panel
```css
background-color: rgba(10, 14, 22, 0.96);
border: 1px solid rgba(0, 212, 255, 0.15);
border-radius: 4px;
padding: 16px;
```

### Botón Primario (Acción principal / Jugar)
```css
background-color: rgba(45, 255, 122, 0.15);
border: 1px solid rgba(45, 255, 122, 0.5);
color: rgb(45, 255, 122);
height: 44px; border-radius: 4px;
/* :hover → background rgba(45,255,122,0.28), border rgb(45,255,122) */
```

### Botón Secundario (Opciones)
```css
background-color: rgba(0, 212, 255, 0.08);
border: 1px solid rgba(0, 212, 255, 0.25);
color: rgba(0, 212, 255, 0.75);
/* :hover → background rgba(0,212,255,0.18) */
```

### Botón Peligro (Salir)
```css
background-color: rgba(255, 48, 48, 0.06);
border: 1px solid rgba(255, 48, 48, 0.2);
color: rgba(255, 100, 100, 0.6);
/* :hover → background rgba(255,48,48,0.18) */
```

### Tarjeta (Upgrade Card)
```css
background-color: rgba(17, 21, 32, 0.96);
border: 1px solid rgba(0, 212, 255, 0.15);
border-radius: 4px;
width: 220px; padding: 16px;
/* Seleccionada: border rgba(45,255,122,0.5), bg rgba(45,255,122,0.08) */
```

### Status Bar (activo)
```css
background-color: rgba(45, 255, 122, 0.07);
border: 1px solid rgba(45, 255, 122, 0.15);
border-radius: 3px; padding: 6px 10px;
/* Dot: 8px, border-radius 4px, color rgb(45,255,122) */
```

### Progress Bar / Loading Bar
```css
/* Container */
background-color: rgba(0, 212, 255, 0.12);
border: 1px solid rgba(0, 212, 255, 0.25);
height: 6px; border-radius: 3px;
/* Fill */
background-color: rgb(0, 212, 255);
```

---

## Espaciado

| Token | Valor | Uso |
|---|---|---|
| `--space-xs` | 4px | Margen mínimo entre elementos |
| `--space-sm` | 8px | Separación interna de componentes |
| `--space-md` | 16px | Padding de paneles |
| `--space-lg` | 24–32px | Separación entre secciones |
| `--space-xl` | 40–48px | Separación mayor (entre bloques) |

---

## Animaciones (USS Transitions)

Las transiciones CSS de Unity UI Toolkit son limitadas a: `background-color`, `border-color`, `color`, `opacity`, `translate`, `scale`, `rotate`.

**Hover estándar:**
```css
transition-property: background-color, border-color, color;
transition-duration: 0.18s;
transition-timing-function: ease-out;
```

**Animaciones complejas** (pulsado, barras, partículas) → C# vía `Update()` + manipulación directa de `element.style`.

---

## Iconografía

Los iconos usados son caracteres Unicode:
| Símbolo | Uso |
|---|---|
| `▶` | Reproducir / Iniciar |
| `⚙` | Opciones / Configuración |
| `✕` | Cerrar / Salir |
| `⬡` | Hexágono (tema biopunk) |
| `━` | Separador horizontal |
| `⚠` | Alerta / Warning |
| `//` | Prefijo de comentario técnico |
| `■ █` | Indicadores de estado |

---

## Archivos de Estilo

| Archivo | Escena | Propósito |
|---|---|---|
| `BootSplash.uss` | Scene_00_Boot | Splash screen completo |
| `MainMenu.uss` | Scene_01_MainMenu | Menú principal biopunk |
| `HUD_Main.uss` | Scene_02_GameWorld | HUD de gameplay |
| `UpgradeMenu.uss` | Scene_03_UpgradeMenu | Tarjetas de mejoras |
| `WeaponShop.uss` | Scene_02 (modal) | Shop modal |
| `BuildRadialMenu.uss` | Scene_02 (modal) | Menú radial de construcción |
| `HUD_Main_Light.uss` | — | Tema claro (override) |
| `MainMenu_Light.uss` | — | Tema claro del menú (override) |

---

## Reglas de Diseño

1. **Contraste mínimo:** Todo texto legible sobre el fondo oscuro (ratio ≥ 4.5:1)
2. **Sin ruido visual:** Los bordes son sutiles por defecto, se iluminan en hover/active
3. **Consistencia cromática:** Verde = salud/OK · Cian = datos/UI · Rojo = peligro · Amarillo = advertencia
4. **Tipografía en mayúsculas:** Para etiquetas de sección y eyebrows (3–4px letter-spacing)
5. **Sin sombras de texto** (no soportadas en Unity USS sin shader)
6. **Bordes en lugar de sombras** para separación de paneles
