using FluentAssertions;

using Tareas.Application.CasosDeUso.Comandos;
using Tareas.Application.CasosDeUso.Consultas;
using Tareas.Application.Dtos;
using Tareas.Application.Filtros;
using Tareas.Domain.Entidades;
using Tareas.Domain.ValueObjects;

using Xunit;

namespace Tareas.Tests.Application;

public class CasosDeUsoTests
{
    private static readonly DateOnly Hoy = new(2026, 1, 1);

    private static TareaDto Dto(string titulo = "Tarea", DateOnly? fecha = null) => new()
    {
        Titulo = titulo,
        Descripcion = "Descripción válida",
        FechaVencimiento = fecha ?? Hoy.AddDays(3),
        Prioridad = Prioridad.Media,
        Categoria = Categoria.Backend
    };

    [Fact]
    public async Task CrearTarea_genera_id_y_persiste()
    {
        var repo = new FakeTareaRepository();
        var cmd = new CrearTareaCommand(repo, new FakeArchivoStorage(), new FakeClock(Hoy));

        var id = await cmd.EjecutarAsync(Dto(), imagen: null);

        id.Should().NotBe(Guid.Empty);
        (await repo.ObtenerPorIdAsync(id)).Should().NotBeNull();
    }

    [Fact]
    public async Task CrearTarea_con_imagen_la_guarda()
    {
        var repo = new FakeTareaRepository();
        var almacenamiento = new FakeArchivoStorage();
        var cmd = new CrearTareaCommand(repo, almacenamiento, new FakeClock(Hoy));

        using var contenido = new MemoryStream(new byte[] { 1, 2, 3 });
        var id = await cmd.EjecutarAsync(Dto(), new SubidaArchivo(contenido, "foto.png"));

        almacenamiento.Guardados.Should().ContainSingle();
        (await repo.ObtenerPorIdAsync(id))!.Imagen.Should().Be("/uploads/fake.png");
    }

    [Fact]
    public async Task CrearTarea_no_escribe_disco_si_el_dto_es_invalido()
    {
        var repo = new FakeTareaRepository();
        var almacenamiento = new FakeArchivoStorage();
        var cmd = new CrearTareaCommand(repo, almacenamiento, new FakeClock(Hoy));

        var dtoInvalido = Dto(fecha: Hoy.AddDays(-5));

        var acto = async () =>
        {
            using var contenido = new MemoryStream(new byte[] { 1 });
            await cmd.EjecutarAsync(dtoInvalido, new SubidaArchivo(contenido, "foto.png"));
        };

        await acto.Should().ThrowAsync<Exception>();
        almacenamiento.Guardados.Should().BeEmpty();
    }

    [Fact]
    public async Task EliminarTarea_borra_la_entidad_y_el_archivo()
    {
        var repo = new FakeTareaRepository();
        var almacenamiento = new FakeArchivoStorage();
        var tarea = Tarea.Crear("Con imagen", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, "/uploads/vieja.png", Hoy);
        repo.Sembrar(tarea);

        var cmd = new EliminarTareaCommand(repo, almacenamiento);
        await cmd.EjecutarAsync(tarea.IdPublic);

        (await repo.ObtenerPorIdAsync(tarea.IdPublic)).Should().BeNull();
        almacenamiento.Eliminados.Should().ContainSingle().Which.Should().Be("/uploads/vieja.png");
    }

    [Fact]
    public async Task EliminarTarea_no_falla_si_el_archivo_ya_no_existe()
    {
        var repo = new FakeTareaRepository();
        var almacenamiento = new FakeArchivoStorage();
        var tarea = Tarea.Crear("Sin imagen", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);
        repo.Sembrar(tarea);

        var cmd = new EliminarTareaCommand(repo, almacenamiento);
        var acto = () => cmd.EjecutarAsync(tarea.IdPublic);

        await acto.Should().NotThrowAsync();
        almacenamiento.Eliminados.Should().BeEmpty();
    }

    [Fact]
    public async Task CambiarCompletada_resuelve_por_id_aunque_no_este_en_la_pagina()
    {
        var repo = new FakeTareaRepository();
        var tarea = Tarea.Crear("Fuera de página", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, string.Empty, Hoy);
        repo.Sembrar(tarea);

        var cmd = new CambiarCompletadaTareaCommand(repo);
        await cmd.EjecutarAsync(tarea.IdPublic);

        (await repo.ObtenerPorIdAsync(tarea.IdPublic))!.Completada.Should().BeTrue();
    }

    [Fact]
    public async Task ActualizarTarea_guarda_la_nueva_imagen_y_borra_la_anterior()
    {
        var repo = new FakeTareaRepository();
        var almacenamiento = new FakeArchivoStorage();
        var tarea = Tarea.Crear("Original", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, "/uploads/vieja.png", Hoy);
        repo.Sembrar(tarea);

        var cmd = new ActualizarTareaCommand(repo, almacenamiento, new FakeClock(Hoy));
        using var contenido = new MemoryStream(new byte[] { 1 });
        await cmd.EjecutarAsync(tarea.IdPublic, Dto(), new SubidaArchivo(contenido, "nueva.png"));

        almacenamiento.Eliminados.Should().ContainSingle().Which.Should().Be("/uploads/vieja.png");
        (await repo.ObtenerPorIdAsync(tarea.IdPublic))!.Imagen.Should().Be("/uploads/fake.png");
    }

    [Fact]
    public async Task ActualizarTarea_quita_la_imagen_si_el_dto_la_vacia()
    {
        var repo = new FakeTareaRepository();
        var almacenamiento = new FakeArchivoStorage();
        var tarea = Tarea.Crear("Original", "Descripción válida", Hoy.AddDays(2),
            Prioridad.Baja, Categoria.Backend, "/uploads/vieja.png", Hoy);
        repo.Sembrar(tarea);

        var cmd = new ActualizarTareaCommand(repo, almacenamiento, new FakeClock(Hoy));
        var dto = Dto();
        dto.Imagen = string.Empty;

        await cmd.EjecutarAsync(tarea.IdPublic, dto, imagen: null);

        (await repo.ObtenerPorIdAsync(tarea.IdPublic))!.Imagen.Should().BeEmpty();
        almacenamiento.Eliminados.Should().ContainSingle().Which.Should().Be("/uploads/vieja.png");
    }

    [Fact]
    public async Task ObtenerTareas_devuelve_dtos_paginados()
    {
        var repo = new FakeTareaRepository();
        repo.Sembrar(Tarea.Crear("Uno", "Descripción válida", Hoy.AddDays(1), Prioridad.Baja, Categoria.Backend, string.Empty, Hoy));

        var query = new ObtenerTareasQuery(repo);
        var resultado = await query.EjecutarAsync(new ParametrosPaginacion(), new FiltroTareas());

        resultado.Items.Should().HaveCount(1);
        resultado.Items[0].Titulo.Should().Be("Uno");
    }
}
