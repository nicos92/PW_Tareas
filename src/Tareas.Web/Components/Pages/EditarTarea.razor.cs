using Microsoft.AspNetCore.Components;

using Tareas.Application.Configuracion;
using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Archivos;
using TareasBlazor.Components.Features.Tareas;
using TareasBlazor.Components.Shared;

namespace TareasBlazor.Components.Pages;

public partial class EditarTarea : ComponentBase
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private TareaEstado TareaEstado { get; set; } = default!;

    [Inject]
    private OpcionesAlmacenamiento Opciones { get; set; } = default!;

    private TareaDto? tareaModel;
    private TareaForm? tareaForm;
    private ImageUploader? imageUploader;

    private long MaxFileSize => Opciones.MaximoBytes;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && tareaModel is null)
        {
            await TareaEstado.Inicializar();
            tareaModel = await TareaEstado.ObtenerPorIdAsync(Id);
            StateHasChanged();
        }
    }

    private void OnImagenChanged(ImageUploader.ImagenChangedEventArgs args)
    {
        if (args.LimpiarImagenExistente && tareaModel is not null)
        {
            tareaModel.Imagen = string.Empty;
        }
    }

    private async Task ValidSubmit()
    {
        if (imageUploader?.TamanoExcedido == true || tareaModel is null)
        {
            return;
        }

        if (!Guid.TryParse(Id, out var guid))
        {
            return;
        }

        SubidaArchivo? subida = imageUploader?.Archivo is { } archivo
            ? SubidaArchivoFactory.Crear(archivo, MaxFileSize)
            : null;

        await TareaEstado.ActualizarTareaAsync(guid, tareaModel, subida);
        NavigationManager.NavigateTo("/listaDeTareas");
    }

    private void Cancelar()
    {
        NavigationManager.NavigateTo("/listaDeTareas");
    }
}
