using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Tareas.Application.Dtos;
using TareasBlazor.Components.Features.Categorias;

namespace TareasBlazor.Components.Pages;

public partial class Categorias : ComponentBase, IDisposable
{
    [Inject]
    private CategoriaEstado CategoriaEstado { get; set; } = default!;

    private CategoryDto _formModel = new();
    private EditForm? _editForm;
    private bool _mostrarFormulario;
    private bool _esEdicion;
    private int? _categoriaEnEdicion;
    private bool _mostrarConfirmacion;
    private CategoryDto? _categoriaAEliminar;
    private string? _mensajeError;

    protected override async Task OnInitializedAsync()
    {
        CategoriaEstado.OnChange += StateHasChanged;
        await CategoriaEstado.Inicializar();
    }

    private void Nueva()
    {
        _mensajeError = null;
        _formModel = new CategoryDto();
        _esEdicion = false;
        _categoriaEnEdicion = null;
        _mostrarFormulario = true;
    }

    private async Task Editar(CategoryDto categoria)
    {
        var dto = await CategoriaEstado.ObtenerPorIdAsync(categoria.Id);
        if (dto is null)
        {
            _mensajeError = "La categoría ya no existe.";
            await CategoriaEstado.CargarAsync();
            return;
        }

        _mensajeError = null;
        _formModel = dto;
        _esEdicion = true;
        _categoriaEnEdicion = dto.Id;
        _mostrarFormulario = true;
    }

    private async Task Guardar()
    {
        try
        {
            _mensajeError = null;

            if (_esEdicion && _categoriaEnEdicion is int id)
            {
                await CategoriaEstado.ActualizarAsync(id, _formModel);
            }
            else
            {
                await CategoriaEstado.CrearAsync(_formModel);
            }

            Cancelar();
        }
        catch (Exception ex)
        {
            _mensajeError = ex.Message;
        }
    }

    private void Cancelar()
    {
        _formModel = new CategoryDto();
        _esEdicion = false;
        _categoriaEnEdicion = null;
        _mostrarFormulario = false;
    }

    private void PedirEliminar(CategoryDto categoria)
    {
        _categoriaAEliminar = categoria;
        _mostrarConfirmacion = true;
    }

    private async Task ConfirmarEliminar()
    {
        try
        {
            _mensajeError = null;

            if (_categoriaAEliminar is not null)
            {
                await CategoriaEstado.EliminarAsync(_categoriaAEliminar.Id);
            }
        }
        catch (Exception ex)
        {
            _mensajeError = ex.Message;
        }
        finally
        {
            CancelarEliminar();
        }
    }

    private void CancelarEliminar()
    {
        _categoriaAEliminar = null;
        _mostrarConfirmacion = false;
    }

    public void Dispose()
    {
        CategoriaEstado.OnChange -= StateHasChanged;
    }
}
