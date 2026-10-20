using Tareas.Application.Dtos;

namespace TareasBlazor.Components.Pages;

public partial class NuevaTarea
{
    private TareaDto tareaModel = new();

    protected override string RutaLista => "/listaDeTareas";

    private async Task ValidSubmit()
    {
        if (ImagenInvalida)
        {
            return;
        }

        await TareaEstado.AgregarTareaAsync(tareaModel, CrearSubida());
        NavigationManager.NavigateTo(RutaLista);
    }
}
