using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Tareas;

namespace TareasBlazor.Components.Pages;

public partial class ListaTareas : TareaEstadoComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private static readonly string[] _columnasTabla = { "Lista", "Título", "Prioridad", "Acciones" };

    private bool _showDeleteModal;
    private TareaDto? _tareaAEliminar;

    private Task OnPrioridadChanged(string valor) => TareaEstado.SetFiltroPrioridadAsync(valor);

    private Task OnEstadoChanged(string valor) => TareaEstado.SetFiltroEstadoAsync(valor);

    private Task OnVencimientoChanged(string valor) => TareaEstado.SetFiltroVencimientoAsync(valor);

    private Task OnPageChanged(int page) => TareaEstado.IrAPaginaAsync(page);

    private void VerTarea(TareaDto tarea)
    {
        NavigationManager.NavigateTo($"/listaDeTareas/tarea/{tarea.IdPublic}");
    }

    private Task CompletarTarea(TareaDto tarea) => TareaEstado.ToggleAsync(tarea.IdPublic);

    private void EliminarTarea(TareaDto tarea)
    {
        _tareaAEliminar = tarea;
        _showDeleteModal = true;
    }

    private async Task ConfirmarEliminar()
    {
        if (_tareaAEliminar is not null)
        {
            await TareaEstado.EliminarTareaAsync(_tareaAEliminar.IdPublic);
            _tareaAEliminar = null;
        }

        _showDeleteModal = false;
    }

    private void CancelarEliminar()
    {
        _tareaAEliminar = null;
        _showDeleteModal = false;
    }

    private void IniciarEdicion(string id)
    {
        NavigationManager.NavigateTo($"/listaDeTareas/editarTarea/{id}");
    }

    private void NuevaTarea()
    {
        NavigationManager.NavigateTo("/listaDeTareas/nuevaTarea");
    }
}
