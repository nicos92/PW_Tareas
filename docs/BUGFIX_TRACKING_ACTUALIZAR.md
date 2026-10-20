# Bugfix: conflicto de tracking de EF Core al actualizar una tarea

## Síntoma reportado

Al modificar la fecha de una tarea y volver a guardar, la aplicación lanzaba una excepción
y el circuito de Blazor Server moría:

```text
System.InvalidOperationException: The instance of entity type 'Tarea' cannot be tracked
because another instance with the same key value for {'Id'} is already being tracked.
   at IdentityMap`1.ThrowIdentityConflict(...)
   at TareaRepository.ActualizarAsync(Tarea tarea, CancellationToken ct)  ← línea 62
   at ActualizarTareaCommand.EjecutarAsync(...)
   at TareaEstado.ActualizarTareaAsync(...)
   at EditarTarea.ValidSubmit()
   at EditForm.HandleSubmitAsync()
```

La fecha no era la causa: era sólo el motivo por el que se apretaba **Guardar** por segunda
vez. El fallo ocurría en **todo segundo guardado de la misma tarea sin recargar la página**
(también al editar tras alternar "completada" o tras crear la tarea).

## Causa raíz

`AddDbContext<TareaDbContext>` se registra *scoped* (DependencyInjection.cs) y en Blazor
Server **un circuito = un solo `DbContext` para toda la sesión**. Cualquier operación que
deje una entidad `Tarea` trackeada (estado `Unchanged` después de `SaveChanges`) sigue
occupando el change tracker mientras el usuario navega entre páginas:

| Operación | ¿Deja la entidad trackeada? |
|---|---|
| `ActualizarAsync` (un guardado previo) | Sí |
| `CambiarCompletadaAsync` (toggle en la lista) | Sí (carga con tracking) |
| `AgregarAsync` (crear la tarea) | Sí |
| `ObtenerPorIdAsync` / `ObtenerPaginadoAsync` | No (`AsNoTracking`) |
| `EliminarAsync` | No (los borrados se desenganchan al guardar) |

`ActualizarAsync` hacía `_contexto.Tareas.Update(tarea)` sobre la instancia **desconectada**
que devolvía `ObtenerPorIdAsync` (`AsNoTracking`), mientras el contexto ya trackeaba otra
instancia de la misma fila → `ThrowIdentityConflict`.

Por eso el bug "se iba" al recargar la página (F5 = circuito nuevo = `DbContext` nuevo).

```text
Sesión (circuito)
  DbContext (scoped, compartido)
    ├─ guardado #1  → Update(instancia A) → trackeada (Unchanged)   ✓
    └─ guardado #2  → ObtenerPorIdAsync() → instancia B (desconectada)
                      Update(instancia B) → choca con A             ✗ excepción
```

## Fix aplicado

`src/Tareas.Infrastructure/Repositories/TareaRepository.cs` — antes de adjuntar la instancia
nueva, se desengancha la previa si el contexto ya trackea esa fila:

```csharp
public async Task ActualizarAsync(Tarea tarea, CancellationToken ct = default)
{
    // El DbContext es scoped: en Blazor Server vive todo el circuito, y una operación
    // anterior (un guardado, un toggle, una creación) puede dejar trackeada otra
    // instancia de la misma fila. Adjuntar la nueva sin más lanzaría
    // ThrowIdentityConflict, así que se desengancha la instancia previa.
    var previa = tarea.Id != 0
        ? _contexto.Tareas.Local.FirstOrDefault(t => t.Id == tarea.Id)
        : null;
    if (previa is not null)
    {
        _contexto.Entry(previa).State = EntityState.Detached;
    }

    _contexto.Tareas.Update(tarea);
    await _contexto.SaveChangesAsync(ct);
}
```

Es un fix quirúrgico: no toca DI, comandos, dominio ni la UI, y preserva el flujo actual
(`Update` + `SaveChanges`). Alternativas descartadas: `ChangeTracker.Clear()` por operación
(descarta silenciosamente cambios pendientes futuros) e `IDbContextFactory` con contexto por
operación (fix más robusto pero de mayor alcance; no figura en los planes de arquitectura,
sólo el `IDesignTimeDbContextFactory` para migraciones).

## Tests de regresión

`tests/Tareas.Tests/Integration/TareaRepositoryActualizarTests.cs` (SQLite `:memory:`, mismo
patrón que `TareaRepositoryFiltrosTests`):

1. `Actualizar_dos_veces_en_el_mismo_contexto_no_lanza_y_persiste_el_ultimo_cambio` — el repro reportado.
2. `Actualizar_despues_de_cambiar_completada_no_lanza_conflicto` — toggle y luego editar.
3. `Actualizar_tras_AgregarAsync_no_lanza_conflicto` — crear y luego editar.

Cada test verifica dos cosas: que **no lance** la excepción y que el **último cambio quede
persistido** (reconsulta con `AsNoTracking`), para que un arreglo que sólo silencie el error
no pase desapercibido. Los tres fallaban con `ThrowIdentityConflict` antes del fix.

## Verificación

- `dotnet test`: 27/27 en verde (3 nuevos + 24 existentes).
- `dotnet build PW_Tareas.sln`: 0 warnings, 0 errores.

## Efecto colateral relacionado (fuera de alcance de este fix)

En `TareaForm.razor` el campo de fecha usa `<input type="date" @bind="Model.FechaVencimiento">`
(input nativo, cambio sin commitear respecto de `<InputDate @bind-Value>`). El `@bind` plano
**no llama a `EditContext.NotifyFieldChanged`**, por lo que la validación de la fecha
(`Required`/`FutureDate`) no se refresca en vivo: los mensajes y el estado del botón Guardar
sólo se actualizan al tocar título/descripción o al enviar. El envío sigue validando bien
(`EditForm` valida antes de `OnValidSubmit`); sólo afecta el feedback en vivo.
