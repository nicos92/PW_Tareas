## [1.0.2] - 2026-11-02

### ♻️ Refactor

- Agrega componentes reutilizables y desacopla los existentes

### 🐛 Bug Fixes

- Compacta StatsPanel para que no desborde la altura del viewport
- Desengancha la instancia previa en ActualizarAsync para evitar el conflicto de tracking
- Notifica el cambio de fecha en TareaForm para revalidar y habilitar Guardar
- Cae a una carpeta temporal cuando la ruta de la base no admite escritura

### 👷 CI

- Incorpora validacion de commits y pipeline de despliegue (#17)
- Migra el despliegue en cloud run a workload identity federation
- Despliega cloud run con un tag inmutable
- Espera la sincronizacion del mirror antes de desplegar

### 💼 Other

- Ignora _site de documentacion
- Agrega docker ignore
- Agrega docker file
- Actualiza readme
- Actualiza program para la creacion de la base de datos
- Agrega documentacion para el despligue en hugging face spaces

### 📚 Documentation

- Actualiza change log v1.0.0 a v1.0.1
- Actualiza CHANGELOG  a v1.0.2
- Agrega inventario de componentes reutilizables de Tareas.Web
- Documenta el bugfix del conflicto de tracking al actualizar una tarea
- Agrega la guia de despliegue en Google Cloud Run
- Documenta las variables de entorno y el despliegue en GCP
- Actualiza la auditoria de la unidad 4
- Actualiza la auditoria con el despliegue en cloud run

### 🔧 Chore

- Habilita el procesamiento de forwarded headers en el contenedor

### 🚀 Features

- Aplica localizacion es-AR a la app y al campo de fecha del formulario
- Agrega la ruta fisica de adjuntos separada de la carpeta publica

### 🚫 Removed

- Migra el proyecto a Clean Architecture por capas

### 🧪 Tests

- Agrega regresiones del conflicto de tracking al actualizar una tarea
- Agrega regresiones de la ruta de base y del almacenamiento de adjuntos

## [1.0.1] - 2026-09-07

### 🚀 Features

- Agrega archivo base para git-cliff para CHANGELOG automatico

### 🐛 Bug Fixes

- Elimina del seguimiento de git la carpeta obj que se genera automaticamente

## [0.1.0] - 2026-08-24

### 🐛 Bug Fixes

- Simplifica la vista de las paginas de la paginacion
- Elimina del seguimiento a la base de datos sqlite

## [1.0.0] - 2026-06-14

### 💼 Other

- Nuevos componentes y mejoras UX
