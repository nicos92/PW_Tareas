# Documentación de ARIA Attributes y Etiquetas Semánticas

## Introducción

Este documento explica cada atributo ARIA y etiqueta semántica aplicada a los componentes de la aplicación TareasBlazor, detallando el propósito y la razón de su uso.

---

## Índice

1. [MainLayout.razor](#mainlayoutrazor)
2. [NavMenu.razor](#navmenurazor)
3. [Home.razor](#homerazor)
4. [NuevaTarea.razor](#nuevatarerazor)
5. [EditarTarea.razor](#editartarerazor)
6. [ConfirmDialog.razor](#confirmdialograzor)
7. [FilterBar.razor](#filterbarrazor)
8. [ImageUploader.razor](#imageuploaderrazor)
9. [ListaTareas.razor](#listatareraszor)
10. [Tarea.razor](#tarerazor)
11. [TareaCard.razor](#tareacardrazor)
12. [TareaForm.razor](#tareaformrazor)
13. [TareaItem.razor](#tareaitemrazor)
14. [StatsCard.razor](#statscardrazor)
15. [Breadcrum.razor](#breadcrumrazor)

---

## MainLayout.razor

### `<aside class="sidebar" aria-label="Barra de navegación lateral">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Barra de navegación lateral"` | Las etiquetas `aria-label` en landmarks (como `<aside>`) proporcionan un nombre descriptivo a los usuarios de lectores de pantalla, permitiéndoles identificar rápidamente el propósito de la sección. Aunque `<aside>` ya es semántico, el `aria-label` añade contexto adicional sobre el contenido (navegación, no un aside genérico). |

### `<main aria-label="Contenido principal">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Contenido principal"` | Similar al `<aside>`, etiquetar el `<main>` permite a los usuarios de lectores de pantalla saltar directamente al contenido principal usando atajos de navegación por landmarks. |

### `<div id="blazor-error-ui" role="alert" aria-live="polite">`

| Atributo | Valor | Razón |
|---|---|---|
| `role` | `"alert"` | La región `alert` es una región `live` que indica contenido dinámico con información importante y urgente. Los lectores de pantalla anuncian automáticamente el contenido cuando este se actualiza. |
| `aria-live` | `"polite"` | Especifica que el navegador debe anunciar el cambio de contenido cuando el usuario esté inactivo (no interrumpe la tarea actual). `role="alert"` implícitamente añade `aria-live="assertive"` pero se deja explícito por claridad. |

¿Por qué no `aria-live="assertive"`? Porque un error en Blazor no debe interrumpir abruptamente al usuario — es información importante pero no crítica como para forzar la atención inmediata.

### `<a href="" class="reload" aria-label="Recargar página">Reload</a>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Recargar página"` | El texto visible "Reload" está en inglés, pero la aplicación está en español. El `aria-label` en español asegura que los usuarios de lectores de pantalla escuchen la acción en el idioma correcto. |

### `<a class="dismiss" aria-label="Descartar error">🗙</a>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Descartar error"` | El contenido visible es solo un caracter unicode (🗙) sin significado semántico para un lector de pantalla. El `aria-label` proporciona el texto alternativo que describe la acción del enlace. |

---

## NavMenu.razor

### `<header class="top-row ps-3 navbar navbar-dark">`

| Etiqueta semántica | Razón |
|---|---|
| `<header>` en lugar de `<div>` | `<header>` es un elemento landmark que representa la cabecera de la página o sección. Proporciona semántica de navegación a los lectores de pantalla, permitiendo identificar el área superior como cabecera en lugar de un genérico `<div>`. |

### `<a class="navbar-brand" href="" aria-label="Ir al inicio">TareasBlazor</a>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Ir al inicio"` | Describe la acción del enlace para usuarios de lectores de pantalla, indicando que el enlace lleva a la página de inicio. |

### `<input type="checkbox" title="Menú de navegación" class="navbar-toggler" aria-label="Alternar menú de navegación" />`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Alternar menú de navegación"` | Un checkbox usado como toggler no comunica su propósito visualmente. El `aria-label` indica la acción que realiza. El `title` es un fallback visual (tooltip), pero `aria-label` es lo que usan los lectores de pantalla. |
| `title` | Cambiado de "Navigation menu" a "Menú de navegación" | Traducción al español para ser consistente con el idioma de la aplicación. |

### `<nav class="flex-column" aria-label="Menú de navegación">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Menú de navegación"` | Cuando hay múltiples landmarks `<nav>`, cada uno necesita un nombre único (via `aria-label` o `aria-labelledby`) para que los usuarios puedan distinguirlos al navegar por landmarks. |

### `<ul class="navbar-nav">` y `<li class="nav-item px-3">`

| Etiqueta semántica | Razón |
|---|---|
| `<ul>` / `<li>` en lugar de `<div>` | Una lista de navegación es semánticamente una lista. Los lectores de pantalla anuncian el número de items en una lista, permitiendo a los usuarios saber cuántos enlaces hay. Esto mejora la experiencia de navegación por teclado. |

### `<span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-hidden` | `"true"` | Los iconos decorativos (Bootstrap Icons) no aportan información textual. Ocultarlos de los lectores de pantalla evita anuncios confusos como "icono casa puerta". El texto visible "Home" ya proporciona el contexto necesario. |

---

## Home.razor

### `<section class="card shadow-sm col" aria-label="Tarjetas con estadisticas de tareas">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Tarjetas con estadisticas de tareas"` | Describe el propósito de la sección para lectores de pantalla, indicando que contiene las tarjetas de estadísticas. |

### `<section class="card-body" aria-live="polite">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | Las estadísticas se actualizan dinámicamente cuando cambia el estado de las tareas (via `StateHasChanged`). `aria-live="polite"` asegura que los lectores de pantalla anuncien los cambios automáticamente sin interrumpir al usuario. |

### Corrección: `<sec>` → `<section>`

| Cambio | Razón |
|---|---|
| `<sec>` (inválido) → `<section>` (semántico) | `<sec>` no es una etiqueta HTML válida. Los lectores de pantalla no la reconocen como landmark. `<section>` es un elemento semántico que define una sección genérica del documento. |

---

## NuevaTarea.razor

### `<section class="card shadow-sm col-xl-6 col-lg-8 col-md-12" aria-label="Formulario de nueva tarea">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Formulario de nueva tarea"` | Identifica la sección como el formulario para crear una nueva tarea, permitiendo a usuarios de lectores de pantalla navegar directamente a esta sección. |

### `<h5 class="mb-0" id="nueva-tarea-heading">Nueva tarea</h5>`

| Atributo | Valor | Razón |
|---|---|---|
| `id` | `"nueva-tarea-heading"` | Proporciona un punto de anclaje para `aria-describedby` o `aria-labelledby` en el contenedor de la sección. Permite asociar el título visual con la región semánticamente. |

### `<div class="card-body" aria-live="polite">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | Los mensajes de validación del formulario aparecen dinámicamente (ValidationMessage). `aria-live="polite"` asegura que los errores sean anunciados a usuarios de lectores de pantalla cuando aparecen. |

---

## EditarTarea.razor

### `<p aria-live="polite">Cargando...</p>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | El mensaje de carga aparece dinámicamente mientras se obtienen los datos de la tarea. `aria-live="polite"` notifica al lector de pantalla que hay contenido cargándose, sin interrumpir. |

### `<section class="card shadow-sm col-xl-6 col-lg-8 col-md-12" aria-label="Formulario de edición de tarea" aria-describedby="editar-heading">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Formulario de edición de tarea"` | Identifica la sección como formulario de edición. |
| `aria-describedby` | `"editar-heading"` | Vincula la sección con su heading descriptivo. Los lectores de pantalla leerán primero el `aria-label` y luego el contenido del elemento apuntado por `aria-describedby`, proporcionando contexto adicional. |

### `<div class="card-body" aria-live="polite">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | Misma razón que en NuevaTarea.razor — los mensajes de validación en el formulario de edición también son dinámicos. |

---

## ConfirmDialog.razor

### `<div class="modal fade show d-block" tabindex="-1" ... role="dialog" aria-modal="true" aria-labelledby="confirm-dialog-title" aria-label="@Title">`

| Atributo | Valor | Razón |
|---|---|---|
| `role` | `"dialog"` | Define el elemento como un diálogo (ventana modal). Los lectores de pantalla tienen modos especiales para interactuar con diálogos (modo foco). |
| `aria-modal` | `"true"` | Indica que el contenido fuera del diálogo no está disponible para interacción. Los lectores de pantalla restringen la navegación al contenido del diálogo cuando `aria-modal="true"`. |
| `aria-labelledby` | `"confirm-dialog-title"` | Apunta al `id` del título del diálogo, haciendo que el lector de pantalla anuncie el título al abrir el modal. Esto proporciona contexto sobre el propósito del diálogo. |
| `aria-label` | `@Title` | Fallback de `aria-labelledby` — si el elemento referenciado no es accesible, se usa `aria-label`. `@Title` es el título dinámico del diálogo (ej. "Confirmar eliminación"). |

### `<h5 class="modal-title" id="confirm-dialog-title">@Title</h5>`

| Atributo | Valor | Razón |
|---|---|---|
| `id` | `"confirm-dialog-title"` | Punto de anclaje para `aria-labelledby` en el contenedor del diálogo. El lectores de pantalla anunciará el texto de este elemento como el título del diálogo. |

### `<button type="button" class="btn-close" @onclick="OnCancel" aria-label="Cerrar diálogo"></button>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Cerrar diálogo"` | El botón de cierre (btn-close de Bootstrap) no tiene texto visible — solo muestra una "X". El `aria-label` proporciona la descripción necesaria para lectores de pantalla. |

---

## FilterBar.razor

### `<fieldset class="row mb-3" aria-label="Filtros de tareas">`

| Etiqueta semántica | Razón |
|---|---|
| `<fieldset>` en lugar de `<div>` | Un grupo de filtros relacionados semánticamente debe agruparse con `<fieldset>`. Los lectores de pantalla anuncian el `<legend>` o `aria-label` al navegar al primer control dentro del fieldset. |

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Filtros de tareas"` | Proporciona un nombre descriptivo para el grupo de filtros. |

### `<legend class="visually-hidden">Filtros de tareas</legend>`

| Etiqueta semántica | Razón |
|---|---|
| `<legend>` | Elemento requerido semánticamente dentro de `<fieldset>` para describir el grupo. Bootstrap lo oculta visualmente con `visually-hidden` pero sigue siendo accesible para lectores de pantalla. |

| Clase | Razón |
|---|---|
| `visually-hidden` | Oculta el `<legend>` visualmente pero lo mantiene accesible para lectores de pantalla, ya que visualmente el título ya está implícito por los labels. |

### `<label class="form-label" for="filter-prioridad">`

### `<select id="filter-prioridad" ...>`

| Atributos | Razón |
|---|---|
| `for="filter-prioridad"` + `id="filter-prioridad"` | Asociación explícita entre label y select. Esta es la forma más robusta de asociar labels con controles de formulario. Los lectores de pantalla anuncian el label cuando el usuario enfoca el select. |

### `<select id="filter-estado" ...>`

| Atributo | Razón |
|---|---|
| `id="filter-estado"` | Misma razón que filter-prioridad — asociación label-select. |

### `<span class="text-muted" aria-live="polite" aria-atomic="true">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | El contador de tareas "Mostrando: X tareas" se actualiza dinámicamente al cambiar los filtros. `aria-live="polite"` permite al lector de pantalla anunciar el nuevo valor sin interrumpir. |
| `aria-atomic` | `"true"` | Indica que el lector de pantalla debe anunciar todo el contenido del elemento como una unidad completa, no solo las partes que cambiaron. Sin `aria-atomic`, el lector podría anunciar solo el número que cambió sin el contexto "Mostrando:". |

---

## ImageUploader.razor

### `<div class="mb-3" aria-label="Sección para subir imagen">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Sección para subir imagen"` | Describe la región como la sección de carga de imágenes. |

### `<label class="form-label" for="file-upload">`

### `<InputFile id="file-upload" ... aria-describedby="file-help-text" />`

| Atributo | Valor | Razón |
|---|---|---|
| `for` | `"file-upload"` | Asociación explícita del label con el InputFile. |
| `id` | `"file-upload"` | Mismo propósito — permite la asociación label-control. |
| `aria-describedby` | `"file-help-text"` | Apunta al texto de ayuda oculto. Los lectores de pantalla anunciarán el texto adicional después del label, proporcionando contexto sobre formatos aceptados y tamaño máximo. |

### `<div class="alert @TipoAlerta mt-2" role="status" aria-live="polite">`

| Atributo | Valor | Razón |
|---|---|---|
| `role` | `"status"` | Define la región como un "status" — contenido que proporciona información al usuario pero no requiere acción inmediata. Es una región live que anuncia cambios automáticamente. |
| `aria-live` | `"polite"` | Refuerza el comportamiento live de `role="status"`. Anuncia cambios (archivo cargado, error de tamaño, preview) sin interrumpir. |

### `<img src="@imagenBase64" alt="Vista previa de @archivoCargado.Name" .../>`

| Atributo | Valor | Razón |
|---|---|---|
| `alt` | `"Vista previa de {nombre}"` | El `alt` descriptivo proporciona contexto sobre qué imagen se está previsualizando. Incluir el nombre del archivo permite al usuario saber qué archivo se cargó. |

### `<img src="@CurrentImagen" alt="Imagen actual de la tarea" .../>`

| Atributo | Valor | Razón |
|---|---|---|
| `alt` | `"Imagen actual de la tarea"` | Describe que es la imagen existente de la tarea, no una nueva previsualización. |

### `<span id="file-help-text" class="visually-hidden">Formatos de imagen aceptados. Tamaño máximo @FormatoTamano(MaxFileSize).</span>`

| Atributo | Valor | Razón |
|---|---|---|
| `id` | `"file-help-text"` | Punto de anclaje para `aria-describedby` en el InputFile. |
| `class` | `"visually-hidden"` | Oculta visualmente el texto pero lo mantiene accesible para lectores de pantalla, que lo leerán cuando el usuario enfoque el InputFile. |

---

## ListaTareas.razor

### `<section class="card shadow-sm col-xl-6 col-lg-8 col-md-12" aria-label="Lista de tareas">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Lista de tareas"` | Identifica la sección como la lista de tareas. El valor anterior `"Sección hidden lista de tareas"` contenía la palabra "hidden" que no tiene sentido para el usuario — se corrigió. |

### `<button class="btn btn-primary btn-lg my-2" @onclick="NuevaTarea" aria-label="Crear nueva tarea">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Crear nueva tarea"` | Aunque el botón tiene texto visible "Nueva Tarea", proveer `aria-label` redundante con texto más descriptivo (verbo en infinitivo) ayuda a clarificar la acción. |

### `<i class="bi bi-plus-circle" aria-hidden="true"></i>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-hidden` | `"true"` | El icono es decorativo y no añade información. Ocultarlo previene anuncios confusos. |

### `<p class="text-muted mb-0" aria-live="polite">No hay tareas. Añade una arriba.</p>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | Este mensaje aparece dinámicamente cuando se eliminan todas las tareas o no hay resultados de filtro. `aria-live="polite"` anuncia el cambio a usuarios de lectores de pantalla. |

---

## Tarea.razor

### `<h3 class="fs-2 text-body-emphasis" aria-live="polite">Cargando...</h3>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | El mensaje de carga desaparece cuando los datos se han obtenido. `aria-live="polite"` permite al lector de pantalla anunciar el cambio cuando el contenido se actualiza (de "Cargando..." al detalle de la tarea). |

### `<a href="/listaDeTareas" class="icon-link" aria-label="Volver a la lista de tareas">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Volver a la lista de tareas"` | Describe explícitamente la acción y destino del enlace. El texto visible "Volver" es breve y el `aria-label` añade el contexto de "a la lista de tareas". |

### `<svg class="bi" aria-hidden="true">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-hidden` | `"true"` | El SVG del icono de chevron es decorativo y no debe ser anunciado por lectores de pantalla. |

---

## TareaCard.razor

### `<article class="card shadow-sm border-0 col-sm-12">`

| Etiqueta semántica | Razón |
|---|---|
| `<article>` | Cada tarjeta de tarea es un componente independiente y autocontenido. `<article>` es el elemento semántico apropiado, que los lectores de pantalla identifican como una unidad de contenido que puede distribuirse de forma independiente. |

### `<i class="bi bi-hash" aria-hidden="true"></i>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-hidden` | `"true"` | El icono de hash es decorativo. El texto visible "ID:" ya proporciona contexto. |

### `<img src="@Tarea.Imagen" alt="Imagen asociada a la tarea '@Tarea.Titulo'" .../>`

| Atributo | Valor | Razón |
|---|---|---|
| `alt` | `"Imagen asociada a la tarea '{titulo}'"` | Describe la imagen en contexto con el título de la tarea. El `alt` anterior era genérico ("Imagen de la tarea"); ahora incluye el título para dar contexto específico. |

---

## TareaForm.razor

### `<EditForm Model="Model" OnValidSubmit="OnValidSubmit" class="container" @ref="_editForm" aria-label="Formulario de tarea">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Formulario de tarea"` | Identifica el formulario para usuarios de lectores de pantalla. Los formularios son landmarks navegables y necesitan un nombre descriptivo. |

### `<ValidationSummary aria-live="polite" />`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-live` | `"polite"` | El resumen de validación aparece dinámicamente cuando hay errores de validación. `aria-live="polite"` permite al lector de pantalla anunciar los errores cuando aparecen, sin interrumpir al usuario. |

¿Por qué `aria-live` en ValidationSummary y no en cada ValidationMessage? Porque el ValidationSummary agrupa todos los errores en un solo lugar. Anunciar errores individuales podría ser redundante si el summary ya los lista. Adicionalmente, Blazor ya maneja la accesibilidad de ValidationMessage mediante `aria-invalid` en los inputs.

---

## TareaItem.razor

### `<article class="list-group-item ...">`

| Etiqueta semántica | Razón |
|---|---|
| `<article>` | Cada item de tarea en la lista es un componente independiente. `<article>` comunica que el contenido puede ser interpretado como una unidad autónoma. |

### `<input type="checkbox" ... aria-label="Marcar tarea '@Tarea.Titulo' como @(Tarea.Completada ? "pendiente" : "completada")" />`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Marcar tarea '{titulo}' como completada/pendiente"` | Un checkbox sin label visible no comunica su propósito a lectores de pantalla. El `aria-label` dinámico describe la acción (marcar como completada o pendiente) e incluye el título de la tarea para contexto. |

### `<button type="button" ... aria-label="Ver detalle de '@Tarea.Titulo'">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Ver detalle de '{titulo}'"` | El botón de ver solo contiene un icono (eye). Sin `aria-label`, los lectores de pantalla leerían "botón" sin contexto o intentarían leer el icono. |

### `<i class="bi bi-eye" aria-hidden="true"></i>`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-hidden` | `"true"` | El icono decorativo debe ocultarse de lectores de pantalla cuando el botón ya tiene `aria-label`. |

### `<button type="button" ... aria-label="Editar tarea '@Tarea.Titulo'">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Editar tarea '{titulo}'"` | El botón de editar contiene solo un icono (pencil). El `aria-label` describe la acción e incluye el título de la tarea para contexto. |

### `<button type="button" ... aria-label="Eliminar tarea '@Tarea.Titulo'">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Eliminar tarea '{titulo}'"` | El botón de eliminar contiene solo un icono (trash). El `aria-label` es crítico para que usuarios de lectores de pantalla sepan qué tarea van a eliminar, especialmente importante dado que esta acción es destructiva. |

---

## StatsCard.razor

### `<article class="card text-white @BackgroundClass" aria-label="Tarjeta de estadistica">`

| Etiqueta semántica | Razón |
|---|---|
| `<article>` | Cada tarjeta de estadística es un componente independiente. `<article>` es semánticamente correcto para contenido autocontenido. |

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"Tarjeta de estadistica"` | Describe el propósito de la tarjeta como un elemento de estadística. |

---

## Breadcrum.razor

### `<nav aria-label="breadcrumb">`

| Etiqueta semántica | Razón |
|---|---|
| `<nav>` | El breadcrumb es un elemento de navegación. `<nav>` identifica esta región como un landmark de navegación. |

| Atributo | Valor | Razón |
|---|---|---|
| `aria-label` | `"breadcrumb"` | Cuando hay múltiples landmarks `<nav>`, cada uno necesita un nombre único. El valor `"breadcrumb"` es una convención estándar que los lectores de pantalla reconocen y pueden ofrecer atajos específicos para navegar al breadcrumb. |

### `<ol class="breadcrumb">`

| Etiqueta semántica | Razón |
|---|---|
| `<ol>` (ordered list) | Un breadcrumb representa una jerarquía ordenada de páginas visitadas. `<ol>` es semánticamente correcto porque el orden importa. Los lectores de pantalla anuncian el número de items y la posición relativa. |

### `<li class="breadcrumb-item @(segment.IsActive ? "active" : "")" aria-current="@(segment.IsActive ? "page" : null)">`

| Atributo | Valor | Razón |
|---|---|---|
| `aria-current` | `"page"` (cuando está activo) | Indica que el item del breadcrumb representa la página actual. Los lectores de pantalla anuncian "página actual" junto con el texto, informando al usuario que esa es su ubicación actual. |

---

## Resumen de atributos ARIA más utilizados

| Atributo | Propósito | Cuándo usarlo |
|---|---|---|
| `aria-label` | Proporciona un nombre accesible a un elemento | Cuando el elemento no tiene texto visible o el texto visible no es descriptivo |
| `aria-live="polite"` | Anuncia cambios dinámicos sin interrumpir | Contenido que se actualiza dinámicamente (validaciones, contadores, loading) |
| `aria-live="assertive"` | Anuncia cambios dinámicos de forma urgente | Errores críticos (no usado en esta app porque ningún error requiere interrupción inmediata) |
| `aria-hidden="true"` | Oculta elementos decorativos de lectores de pantalla | Iconos, SVGs decorativos, elementos puramente visuales |
| `aria-describedby` | Añade descripción adicional a un elemento | Cuando el elemento necesita contexto extra (ej. InputFile con límite de tamaño) |
| `aria-labelledby` | Asocia un elemento con su título/etiqueta visible | Diálogos modales, secciones con headings |
| `aria-current="page"` | Indica el elemento activo en un conjunto | Breadcrumbs, navegación con página actual |
| `aria-modal="true"` | Indica que el contenido fuera del diálogo no es interactivo | Modales y diálogos |
| `aria-atomic="true"` | Indica que todo el contenido debe ser anunciado como unidad | Regiones live donde todo el texto es relevante, no solo los cambios |
| `role="alert"` | Define una región con información importante | Errores, notificaciones importantes |
| `role="dialog"` | Define un elemento como ventana de diálogo | Modales |
| `role="status"` | Define una región de estado (live region) | Barras de progreso, mensajes de estado |

## Etiquetas semánticas clave

| Etiqueta | Propósito | Beneficio SEO y accesibilidad |
|---|---|---|
| `<header>` | Cabecera de página o sección | Landmark que los motores de búsqueda identifican como contenido introductorio |
| `<nav>` | Navegación principal | Landmark para enlaces de navegación; mejora el page rank de los enlaces internos |
| `<main>` | Contenido principal | Landmark único que indica el contenido central; Google prioriza este contenido |
| `<section>` | Sección temática | Organiza el contenido en temas; mejora la estructura semántica del documento |
| `<article>` | Contenido independiente | Identifica contenido autocontenido; buscadores lo tratan como unidad de contenido |
| `<aside>` | Contenido complementario | Landmark para contenido relacionado pero no esencial |
| `<fieldset>` / `<legend>` | Grupo de controles de formulario | Agrupa campos relacionados; mejora la comprensión del formulario |
| `<ul>` / `<ol>` / `<li>` | Listas | Los buscadores entienden la relación jerárquica; mejora el SEO semántico |

## Referencias

- [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [MDN ARIA Guide](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA)
- [HTML Living Standard - ARIA](https://html.spec.whatwg.org/multipage/dom.html#aria-attributes)
- [WebAIM - ARIA Techniques](https://webaim.org/techniques/aria/)
