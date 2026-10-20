using Microsoft.AspNetCore.Components;

using Tareas.Application.Dtos;
using TareasBlazor.Components.Shared;

namespace TareasBlazor.Components.Pages;

public partial class EditarTarea
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    private TareaDto? tareaModel;

    protected override string RutaLista => "/listaDeTareas";

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

    private async Task<string?> ResolverTituloTarea(string segmento)
    {
        var tarea = await TareaEstado.ObtenerPorIdAsync(segmento);
        return tarea?.Titulo;
    }

    private async Task ValidSubmit()
    {
        if (ImagenInvalida || tareaModel is null)
        {
            return;
        }

        if (!Guid.TryParse(Id, out var guid))
        {
            return;
        }

        await TareaEstado.ActualizarTareaAsync(guid, tareaModel, CrearSubida());
        NavigationManager.NavigateTo(RutaLista);
    }
}
