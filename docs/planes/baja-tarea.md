Actúa como un Desarrollador .NET Senior especializado en Clean Architecture, Blazor Server y Entity Framework Core con SQLite.

Tenemos una aplicación en desarrollo (sin datos en producción) y necesitamos cambiar la funcionalidad actual de eliminación física (hard delete) de tareas por una eliminación lógica (soft delete / dar de baja).

La solución utiliza Clean Architecture con 4 proyectos separados. Por favor, implementa los cambios siguiendo esta estructura:

1. Capa de Dominio (Domain)
Modifica la entidad de Tarea para incluir una propiedad de baja lógica, por ejemplo: public bool IsDeleted { get; set; } = false; (o public DateTime? DeletedAt { get; set; }).

2. Capa de Aplicación (Application)
Actualiza los casos de uso / handlers correspondientes para que, en lugar de invocar un método de eliminación física (Remove o Delete), actualicen el estado de la entidad (IsDeleted = true o asignen la fecha de baja) y guarden los cambios.

Asegúrate de que las consultas (Queries / Read repositories) filtren por defecto las tareas que no estén dadas de baja (WHERE IsDeleted == false), a menos que se requiera explícitamente lo contrario.

1. Capa de Infraestructura (Infrastructure)
Revisa las configuraciones de Entity Framework Core si es necesario (por ejemplo, índices o filtros globales de consulta HasQueryFilter(t => !t.IsDeleted)).

Como la base de datos está en desarrollo y no hay datos, crea una nueva migración de EF Core para reflejar el cambio en el esquema de SQLite y eliminen/vuelvan a aplicar la migración según corresponda.

1. Capa de Presentación (Blazor Server)
Actualiza los componentes de Blazor y servicios de interfaz de usuario para que el botón o acción de "Eliminar" invoque la nueva lógica de dar de baja.

Actualiza las tablas o listados para que dejen de mostrar las tareas dadas de baja.

Por favor, proporciona los cambios detallados archivo por archivo o los fragmentos de código necesarios para completar esta refactorización manteniendo los principios SOLID y Clean Architecture.
