# Auditoría de Código — UNIDAD 2: Interfaz Visual

**Proyecto:** `PW_Tareas` · **Capa UI:** `src/Tareas.Web` · **Framework:** .NET 8 (Blazor Web App, `InteractiveServer`) · **Bootstrap:** 5.x local (`wwwroot/bootstrap/bootstrap.min.css`).

---

## Estado de Cumplimiento - UNIDAD 2

### 1. HTML5 semántico en todos los layouts
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Ruta de la carpeta:** `src/Tareas.Web/Components/Layout/`, `src/Tareas.Web/Components/Shared/`, `src/Tareas.Web/Components/Pages/` y `src/Tareas.Web/Components/App.razor`
  - **Archivos y líneas:**
    - `Layout/MainLayout.razor` (Líneas 3-17) — estructura semántica de página: `<div class="page">` (Línea 3), componente `SkipLink` (Línea 4), `<aside class="sidebar" aria-label="Barra de navegación lateral">` (Líneas 6-8), `<main id="main-content" aria-label="Contenido principal">` (Líneas 10-14) y dentro `<section class="content px-4">` (Línea 11).
    - `Layout/NavMenu.razor` (Líneas 2-40) — `<header class="top-row ...">` (Líneas 2-6), `<nav class="flex-column" aria-label="Menú de navegación">` (Línea 14) con `<ul class="navbar-nav">` (Línea 15).
    - `Layout/NavScroll.razor` (Líneas 2-3) — `<nav>` + `<ul>` para el menú fuera del lienzo (offcanvas).
    - `Layout/StatsPanel.razor` (Líneas 3-26) — `<aside class="stats-panel ..." aria-label="Panel de estadísticas">` (Línea 3) con listas `<ul>` (Líneas 15, 22).
    - `Layout/SkipLink.razor` (Líneas 1-3) — enlace de salto accesible hacia `#main-content`.
    - `Shared/Breadcrum.razor` (Líneas 3-4) — `<nav aria-label="breadcrumb">` + `<ol class="breadcrumb">`.
    - `Shared/CardSection.razor` (Línea 1) — `<section class="card ...">` como contenedor semántico reutilizable.
    - `Shared/StatsCard.razor` (Línea 1) y `Shared/TareaCard.razor` (Línea 10) — `<article>` para contenido autocontenido.
    - `Shared/FilterBar.razor` (Línea 5) — `<fieldset>` + `<legend>` para agrupar controles de filtro.
    - `Pages/Tarea.razor` (Línea 6) — `<section class="container py-4">`.
    - `Components/App.razor` (Líneas 1-26) — documento HTML5 completo: `<!DOCTYPE html>`, `<html lang="es-AR">`, `<head>`, `<body>`.
  - **Observación:** no existe un `<footer>` de página. No es obligatorio según el enunciado ("etiquetas como…"), pero si el diseño requiere pie de página, conviene agregarlo en `MainLayout.razor` después del `<main>` (p. ej. `<footer class="footer" aria-label="Pie de página">…</footer>`).

---

### 2. CSS Isolation en al menos 3 componentes
- **Estado: CUMPLE** (hay **5** archivos `.razor.css`, superando el mínimo de 3).
- **Evidencia / Ausencia:**
  - **Ruta de la carpeta:** `src/Tareas.Web/Components/Layout/` y `src/Tareas.Web/Components/Pages/`
  - **Archivos de estilos aislados encontrados:**
    - `Layout/MainLayout.razor.css` (Líneas 1-99) — estilos de `.page`, `main`, `.sidebar`, `.top-row`, `#blazor-error-ui`; media queries en Líneas 40-80.
    - `Layout/NavMenu.razor.css` (Líneas 1-87) — estilos de `.navbar-toggler`, `.nav-item`, `.nav-scrollable`; media query en Líneas 74-87.
    - `Layout/StatsPanel.razor.css` (Líneas 1-63) — estilos de `.stats-panel`; media queries en Líneas 46-63.
    - `Layout/SkipLink.razor.css` (Líneas 1-14) — estilos de `.skip-link` y su estado `:focus`.
    - `Pages/Configuracion.razor.css` (Líneas 1-19) — estilos de `.btn-themelight` / `.btn-themedark` y sus `:hover`.
  - **Evidencia de aislamiento activo:**
    - `Components/App.razor` (Línea 10) — `<link rel="stylesheet" href="Tareas.Web.styles.css" />`, bundle generado por el SDK que incluye el CSS con ámbito de cada componente.
    - Uso de `::deep` para atravesar el ámbito hacia componentes hijos: `MainLayout.razor.css` (Líneas 23-37, 45-47, 69-73), `NavMenu.razor.css` (Líneas 47-65) y `StatsPanel.razor.css` (Líneas 28-44).

---

### 3. Diseño responsive con Bootstrap 5 (mobile-first)
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Archivos y líneas:**
    - `Components/App.razor` (Línea 8) — carga local de Bootstrap 5 (`bootstrap/bootstrap.min.css`); (Línea 12) iconos; (Línea 21) bundle JS de Bootstrap.
    - `Pages/Home.razor` (Líneas 12-23) — `<ul class="row">` con tres `col-md-4` (Líneas 13, 16, 20): una columna en móvil, tres desde `md`.
    - `Pages/Tarea.razor` (Líneas 13-19) — `row justify-content-center` y `col-12 col-md-10 col-lg-8`: ancho completo en móvil y progresivamente más angosto en tablet/escritorio.
    - `Shared/TareaCard.razor` (Línea 13 y Línea 42) — `d-flex flex-column flex-sm-row`, `align-items-start align-items-sm-center`, `justify-content-between`: apilado en móvil y horizontal desde `sm`.
    - `Layout/StatsPanel.razor` (Línea 3) — `d-none d-lg-block`: panel oculto en pantallas pequeñas y visible desde `lg`.
    - `Shared/FilterBar.razor` (Línea 5) — `row` como rejilla de filtros; `div class="col d-flex align-items-end"` (Línea 24).
    - `Shared/SelectFilter.razor` (Línea 24) — `col-lg-4` por defecto: apilado en móvil, cuatro columnas desde `lg`, componiendo la rejilla de `FilterBar`.
    - `Shared/CardSection.razor` (Línea 23) — `CssClass = "col-12"` por defecto dentro de rejillas.
    - `Pages/ListaTareas.razor` (Línea 7) — `col-12 mb-3`; (Línea 49) `container d-flex justify-content-center`.
    - `Pages/Categorias.razor` (Línea 14) — `col-12 mb-3`; (Línea 79) `d-flex gap-1`.
    - `Pages/Configuracion.razor` (Línea 4) — `container py-4`; (Línea 12) `d-flex gap-3`.
    - `Shared/ImageUploader.razor` (Línea 19) — `container-fluid`; `Pages/Tarea.razor` (Línea 6) — `container py-4`.
    - `Shared/TableSkeleton.razor` (Línea 17) — `placeholder col-6`; `Shared/LoadingSpinner.razor` (Línea 1) — `d-flex justify-content-center`.
    - `Shared/TareaCard.razor` (Línea 36) — `img-fluid` para imágenes fluidas.
  - **Detalle de clases mobile-first implementadas:**
    - Rejilla: `row`, `col`, `col-12`, `col-md-4`, `col-md-10`, `col-lg-8`, `col-lg-4` (comportamiento de una columna por defecto y expansión ascendente en breakpoints `md`/`lg`).
    - Visibilidad: `d-none d-lg-block` (StatsPanel) para ocultar en móvil y mostrar en escritorio.
    - Flex responsive: `flex-column flex-sm-row`, `align-items-start align-items-sm-center` (TareaCard), `justify-content-center`, `justify-content-between`, `gap-*`.
    - Contenedores: `container`, `container-fluid`, utilidades de espaciado `p-*`, `m-*`, `py-*`.
    - **Media queries propias coherentes con mobile-first:** `MainLayout.razor.css` (Líneas 40-80) parte de `flex-direction: column` y cambia a `row` en `min-width: 641px`; `NavMenu.razor.css` (Líneas 74-87) muestra el menú colapsable solo bajo 641px; `StatsPanel.razor.css` (Líneas 46-63) diferencia escritorio (`min-width: 641px`) y móvil (`max-width: 640.98px`).

---

### 4. JS Interop
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Inyección/Invocación en C#:**
    - `Components/Features/Tema/ThemeService.cs` — `private readonly IJSRuntime _js;` (Línea 10), inyección por constructor (Líneas 16-19), **C# → JS** con `_js.InvokeAsync<string>("getTheme")` (Línea 23) y `_js.InvokeVoidAsync("setTheme", theme)` (Línea 36).
    - `Components/Pages/Configuracion.razor` — `@inject ThemeService ThemeService` (Línea 2) y llamadas a `ThemeService.SetTheme("dark"/"light")` (Líneas 13 y 16), que terminan invocando la función JS.
    - `Program.cs` (Línea 24) — registro DI `builder.Services.AddScoped<ThemeService>();`.
    - `Components/App.razor` (Línea 22) — `<script src="js/theme.js"></script>`, necesario para exponer las funciones globales antes de la invocación.
  - **Archivo de script JavaScript:** `src/Tareas.Web/wwwroot/js/theme.js`
    - Líneas 9-12 — `window.setTheme = function (theme) { ... }` (recibe la llamada desde C#, aplica `data-bs-theme` y persiste en `localStorage`).
    - Líneas 14-16 — `window.getTheme = function () { ... }` (retorna el tema almacenado a C#).
    - Líneas 4-7 y 18 — `applyStoredTheme()` al cargar la página.
    - Líneas 20-38 — integración con la navegación mejorada de Blazor (`Blazor.addEventListener('enhancedload', ...)`).
  - **JS adicional (no vía IJSRuntime):** `Layout/NavScroll.razor` (Línea 1) usa `onclick="document.querySelector('.navbar-toggler').click()"` para cerrar el offcanvas; es JS embebido en el marcado y refuerza el uso de JavaScript, pero el requisito se cumple formalmente con el flujo `ThemeService` ↔ `theme.js`.
  - **Observación:** `ThemeService.InitializeAsync()` (Líneas 21-25), que ejecuta la ruta C# → JS `getTheme`, no se invoca desde ningún componente; el flujo activo es `Configuracion.razor` → `SetTheme` → `InvokeVoidAsync("setTheme")` → `theme.js:9-12`. Si se desea leer el tema persistido al iniciar, conviene llamar a `InitializeAsync()` desde un componente de arranque (p. ej. `MainLayout` o `Configuracion` en `OnInitializedAsync`).

---

## Conclusión

Los 4 requisitos de la **UNIDAD 2 – Interfaz Visual** se cumplen:

1. **HTML5 semántico:** `header`, `nav`, `main`, `aside`, `section`, `article`, `fieldset`, `ol`/`ul` presentes en layout y componentes (sin `<footer>`, no exigido).
2. **CSS Isolation:** 5 archivos `.razor.css` (mínimo 3) con bundle `Tareas.Web.styles.css` y uso de `::deep`.
3. **Responsive Bootstrap 5:** rejilla `row`/`col-*`, breakpoints `sm`/`md`/`lg`, visibilidad `d-none d-lg-block` y media queries mobile-first.
4. **JS Interop:** `ThemeService` inyecta `IJSRuntime` e invoca `getTheme`/`setTheme` en `wwwroot/js/theme.js`, con el script cargado desde `App.razor`.
