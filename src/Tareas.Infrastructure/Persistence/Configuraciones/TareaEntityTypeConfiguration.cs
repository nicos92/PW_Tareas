using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Tareas.Domain.Entidades;
using Tareas.Domain.Reglas;

namespace Tareas.Infrastructure.Persistence.Configuraciones;

/// <summary>
/// Mapeo 100% Fluent de <see cref="Tarea"/>. Los tipos de columna se eligen para que el
/// esquema resultante sea idéntico al actual salvo por <c>Categoria</c> (INTEGER → TEXT),
/// que pasa a alinearse con <c>Prioridad</c>.
/// </summary>
public sealed class TareaEntityTypeConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> entity)
    {
        entity.ToTable("Tareas");

        entity.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        entity.Property(t => t.IdPublic)
            .HasConversion<string>();
        entity.HasIndex(t => t.IdPublic)
            .IsUnique();

        entity.Property(t => t.Titulo)
            .HasMaxLength(TareaReglas.LongitudTituloMaxima);

        entity.Property(t => t.Descripcion)
            .HasMaxLength(TareaReglas.LongitudDescripcionMaxima);

        entity.Property(t => t.FechaVencimiento)
            .HasConversion(
                fecha => fecha.ToDateTime(TimeOnly.MinValue),
                fecha => DateOnly.FromDateTime(fecha));

        entity.Property(t => t.Prioridad)
            .HasConversion<string>();

        entity.Property(t => t.Categoria)
            .HasConversion<string>();
    }
}
