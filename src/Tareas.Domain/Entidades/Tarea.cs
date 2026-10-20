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
    public Categoria Categoria { get; private set; }

    public static Tarea Crear(
        string titulo,
        string descripcion,
        DateOnly? fechaVencimiento,
        Prioridad prioridad,
        Categoria categoria,
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
            Categoria = categoria
        };
    }

    public void Actualizar(
        string titulo,
        string descripcion,
        DateOnly? fechaVencimiento,
        Prioridad prioridad,
        Categoria categoria,
        string imagen,
        bool completada,
        DateOnly hoy)
    {
        Validar(titulo, descripcion, fechaVencimiento, prioridad, hoy);

        Titulo = titulo;
        Descripcion = descripcion;
        FechaVencimiento = fechaVencimiento!.Value;
        Prioridad = prioridad;
        Categoria = categoria;
        Imagen = imagen;
        Completada = completada;
    }

    public void AsignarImagen(string rutaImagen) => Imagen = rutaImagen ?? string.Empty;

    public void CambiarCompletada(bool completada) => Completada = completada;

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
