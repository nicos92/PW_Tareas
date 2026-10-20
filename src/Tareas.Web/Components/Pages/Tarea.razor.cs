using Microsoft.AspNetCore.Components;

using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Tareas;

namespace TareasBlazor.Components.Pages;

public partial class Tarea : ComponentBase
{
    [Parameter]
    public string id { get; set; } = string.Empty;

    [Inject]
    private TareaEstado TareaEstado { get; set; } = default!;

    private bool isLoading = true;
    private TareaDto? tarea;

    protected override async Task OnParametersSetAsync()
    {
        await TareaEstado.Inicializar();
        tarea = await TareaEstado.ObtenerPorIdAsync(id);
        isLoading = false;
    }
}
