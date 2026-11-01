using Microsoft.AspNetCore.Components;

using Tareas.Application.CasosDeUso.Consultas;
using Tareas.Application.Configuracion;
using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Archivos;
using TareasBlazor.Components.Shared;

namespace TareasBlazor.Components.Features.Tareas;

/// <summary>
/// Base de las páginas que crean o editan una tarea con <see cref="TareaForm"/> e
/// <see cref="ImageUploader"/>. Centraliza inyecciones, límite de subida, referencia a
/// <see cref="SubidaArchivoFactory"/> y navegación de cancelación.
/// </summary>
public abstract class TareaFormPageBase : ComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected TareaEstado TareaEstado { get; set; } = default!;

    [Inject]
    protected OpcionesAlmacenamiento Opciones { get; set; } = default!;

    [Inject]
    protected GetCategoriesQuery ObtenerCategorias { get; set; } = default!;

    protected TareaForm? tareaForm;
    protected ImageUploader? imageUploader;

    protected IReadOnlyList<CategoryDto> Categorias { get; private set; } = Array.Empty<CategoryDto>();

    protected long MaxFileSize => Opciones.MaximoBytes;

    protected bool ImagenInvalida =>
        imageUploader?.TamanoExcedido == true || imageUploader?.ExtensionInvalida == true;

    protected override async Task OnInitializedAsync()
    {
        Categorias = await ObtenerCategorias.EjecutarAsync();
    }

    protected SubidaArchivo? CrearSubida()
        => imageUploader?.Archivo is { } archivo
            ? SubidaArchivoFactory.Crear(archivo, MaxFileSize)
            : null;

    protected virtual void OnImagenChanged(ImageUploader.ImagenChangedEventArgs args)
        => StateHasChanged();

    protected abstract string RutaLista { get; }

    protected void Cancelar() => NavigationManager.NavigateTo(RutaLista);
}
