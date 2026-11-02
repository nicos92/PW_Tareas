using Tareas.Domain.Errores;
using Tareas.Domain.Reglas;
using Tareas.Domain.ValueObjects;

namespace Tareas.Domain.Entidades;

/// <summary>
/// Entidad pura: sin anotaciones de EF Core, sin DataAnnotations y sin dependencias externas.
/// El constructor privado existe exclusivamente para la materialización de EF Core;
/// <see cref="Crear"/> y <see cref="Actualizar"/> son las únicas vías de construcción
/// y de mutación con datos del exterior, y validan todas las invariantes.
/// </summary>
public sealed class Tarea
{
    private Tarea()
    {
    }

    public int Id { get; private set; }
    public Guid IdPublic { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Completada { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public Prioridad Prioridad { get; private set; }
    public string Imagen { get; private set; } = string.Empty;
    public int? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Tarea Crear(
        string titulo,
        string descripcion,
        DateOnly? fechaVencimiento,
        Prioridad prioridad,
        int? categoryId,
        string imagen,
        DateOnly hoy)
    {
        Validar(titulo, descripcion, fechaVencimiento, prioridad, hoy);

        return new Tarea
        {
            IdPublic = Guid.NewGuid(),
            Titulo = titulo,
            Descripcion = descripcion,
            Completada = false,
            FechaVencimiento = fechaVencimiento!.Value,
            Prioridad = prioridad,
            Imagen = imagen,
            CategoryId = categoryId
        };
    }

    public void Actualizar(
        string titulo,
        string descripcion,
        DateOnly? fechaVencimiento,
        Prioridad prioridad,
        int? categoryId,
        string imagen,
        bool completada,
        DateOnly hoy)
    {
        Validar(titulo, descripcion, fechaVencimiento, prioridad, hoy);

        Titulo = titulo;
        Descripcion = descripcion;
        FechaVencimiento = fechaVencimiento!.Value;
        Prioridad = prioridad;
        CategoryId = categoryId;
        // La navegación se limpia: la relación se persiste por la FK y se recarga al consultar.
        Category = null;
        Imagen = imagen;
        Completada = completada;
    }

    public void AsignarImagen(string rutaImagen) => Imagen = rutaImagen ?? string.Empty;

    public void CambiarCompletada(bool completada) => Completada = completada;

    public void DarDeBaja()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
    }

    public void Restaurar()
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
    }

    private static void Validar(
        string titulo,
        string? descripcion,
        DateOnly? fechaVencimiento,
        Prioridad prioridad,
        DateOnly hoy)
    {
        var violaciones = TareaReglas.Validar(titulo, descripcion, fechaVencimiento, prioridad, hoy);
        if (violaciones.Count > 0)
        {
            throw new ViolacionReglaException(violaciones);
        }
    }
}
