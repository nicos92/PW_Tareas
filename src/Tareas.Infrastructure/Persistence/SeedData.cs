using Microsoft.EntityFrameworkCore;

using Tareas.Domain.Entidades;

namespace Tareas.Infrastructure.Persistence;

/// <summary>
/// Datos iniciales de la aplicación. El inicializador es idempotente: sólo crea las
/// categorías que todavía no existen, sin pisar ni duplicar las que ya están en la base.
/// </summary>
public static class SeedData
{
    private static readonly (string Nombre, string Descripcion)[] CategoriasPorDefecto =
    {
        ("Trabajo", "Tareas laborales y proyectos"),
        ("Personal", "Tareas personales y del hogar"),
        ("Estudio", "Tareas académicas y de formación"),
        ("Salud", "Turnos médicos y actividad física")
    };

    public static async Task InicializarAsync(
        TareaDbContext contexto,
        CancellationToken ct = default)
    {
        var existentes = await contexto.Categorias
            .AsNoTracking()
            .Select(c => c.Name)
            .ToListAsync(ct);

        var normalizadas = existentes
            .Select(nombre => nombre.Trim().ToUpperInvariant())
            .ToHashSet();

        var nuevas = CategoriasPorDefecto
            .Where(categoria => normalizadas.Add(categoria.Nombre.ToUpperInvariant()))
            .Select(categoria => Category.Crear(categoria.Nombre, categoria.Descripcion))
            .ToList();

        if (nuevas.Count == 0)
        {
            return;
        }

        contexto.Categorias.AddRange(nuevas);
        await contexto.SaveChangesAsync(ct);
    }
}
