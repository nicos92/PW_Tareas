# Requerimiento de arquitectura

## Requerimiento original

Revisar el código buscando inconsistencias para luego pasar el proyecto a una arquitectura
limpia, separando el front end del backend y creando nuevas librerías de datos.

## Arquitectura implementada

Se optó por **Clean Architecture por proyectos dentro de la misma solución, sin API HTTP**:
la separación es por capas (librerías), no por frontera de red.

```
PW_Tareas.sln
├── src/
│   ├── Tareas.Domain/         net8.0 · SIN dependencias
│   ├── Tareas.Application/    net8.0 → sólo Domain
│   ├── Tareas.Infrastructure/ net8.0 → Application (EF Core 8.0.26)
│   └── Tareas.Web/            net8.0 → Application + Infrastructure   ← FRONT END
└── tests/
    └── Tareas.Tests/          xUnit + FluentAssertions
```

**Regla de dependencia:** `Web → Application → Domain` e `Infrastructure → Application`.
**Domain no depende de nadie.** La UI nunca ve entidades de dominio, `DbContext` ni strings
de filtro: sólo DTOs.

### Responsabilidades

| Capa | Contenido |
|---|---|
| **Domain** | Entidad `Tarea`, `TareaReglas` (invariantes), value objects `Prioridad`/`Categoria`, `ViolacionRegla`. |
| **Application** | Puertos (`ITareaRepository`, `IArchivoStorage`, `ISystemClock`), casos de uso (consultas/comandos), DTOs, filtros tipados y atributos de validación de formulario. |
| **Infrastructure** | `TareaDbContext` + configuraciones Fluent + migraciones, `TareaRepository`, `ArchivoStorage`, `SystemClock`, resolución de la ruta de la BD y composición de servicios. |
| **Web** | Blazor Server: páginas, componentes, `TareaEstado` (estado de UI) y `Program.cs` como composition root. |

### Cambios estructurales clave

- `IdPublic` pasa de `string` a `Guid` (columna sigue siendo `TEXT`, conversión explícita).
- `Categoria` pasa de `INTEGER` a `TEXT` (único cambio de esquema real; migración con
  conversión `CASE` de los valores existentes).
- Los strings mágicos de filtro (`"Completadas"`, `"Vencidas"`, …) se reemplazan por los
  enums tipados `EstadoFiltro` y `VencimientoFiltro`.
- El acceso a archivos (guardar/eliminar) se centraliza en `IArchivoStorage`, con validación
  de contención de ruta y límite de tamaño configurable.
- La cadena de conexión deja de estar hardcodeada: `ConnectionStrings:Tareas` → `DATA_DIR`
  → `%LOCALAPPDATA%/PW_Tareas/tareas.db`.

Detalle completo de la migración en [PLAN_LIMPIA_ARQUITECTURA.md](PLAN_LIMPIA_ARQUITECTURA.md).
