using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Tareas.Application.Configuracion;
using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Archivos;
using TareasBlazor.Components.Features.Tareas;
using TareasBlazor.Components.Shared;

namespace TareasBlazor.Components.Pages;

public partial class NuevaTarea : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private TareaEstado TareaEstado { get; set; } = default!;

    [Inject]
    private OpcionesAlmacenamiento Opciones { get; set; } = default!;

    private TareaDto tareaModel = new();
    private TareaForm? tareaForm;
    private ImageUploader? imageUploader;

    private long MaxFileSize => Opciones.MaximoBytes;

    private async Task ValidSubmit()
    {
        if (imageUploader?.TamanoExcedido == true)
        {
            return;
        }

        SubidaArchivo? subida = imageUploader?.Archivo is { } archivo
            ? SubidaArchivoFactory.Crear(archivo, MaxFileSize)
            : null;

        await TareaEstado.AgregarTareaAsync(tareaModel, subida);
        NavigationManager.NavigateTo("/listaDeTareas");
    }

    private void Cancelar()
    {
        NavigationManager.NavigateTo("/listaDeTareas");
    }
}
