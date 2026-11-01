using Tareas.Application.CasosDeUso.Comandos;
using Tareas.Application.CasosDeUso.Consultas;
using Tareas.Application.Dtos;

namespace TareasBlazor.Components.Features.Categorias;

/// <summary>
/// Estado de presentación del módulo de categorías. Mantiene sólo el listado y la bandera
/// de carga; toda la lógica vive en los casos de uso de Application.
/// </summary>
public sealed class CategoriaEstado
{
    private readonly GetCategoriesQuery _getCategories;
    private readonly GetCategoryByIdQuery _getCategoryById;
    private readonly CreateCategoryCommand _createCategory;
    private readonly UpdateCategoryCommand _updateCategory;
    private readonly DeleteCategoryCommand _deleteCategory;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _inicializado;

    public CategoriaEstado(
        GetCategoriesQuery getCategories,
        GetCategoryByIdQuery getCategoryById,
        CreateCategoryCommand createCategory,
        UpdateCategoryCommand updateCategory,
        DeleteCategoryCommand deleteCategory)
    {
        _getCategories = getCategories;
        _getCategoryById = getCategoryById;
        _createCategory = createCategory;
        _updateCategory = updateCategory;
        _deleteCategory = deleteCategory;
    }

    public IReadOnlyList<CategoryDto> Categorias { get; private set; } = Array.Empty<CategoryDto>();
    public bool IsLoading { get; private set; }

    public event Action? OnChange;

    public async Task Inicializar()
    {
        if (_inicializado)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_inicializado)
            {
                return;
            }

            _inicializado = true;
        }
        finally
        {
            _initLock.Release();
        }

        await CargarAsync();
    }

    public async Task CargarAsync()
    {
        IsLoading = true;
        NotificarCambio();

        Categorias = await _getCategories.EjecutarAsync();

        IsLoading = false;
        NotificarCambio();
    }

    public Task<CategoryDto?> ObtenerPorIdAsync(int id) => _getCategoryById.EjecutarAsync(id);

    public async Task CrearAsync(CategoryDto dto)
    {
        await _createCategory.EjecutarAsync(dto);
        await CargarAsync();
    }

    public async Task ActualizarAsync(int id, CategoryDto dto)
    {
        await _updateCategory.EjecutarAsync(id, dto);
        await CargarAsync();
    }

    public async Task EliminarAsync(int id)
    {
        await _deleteCategory.EjecutarAsync(id);
        await CargarAsync();
    }

    private void NotificarCambio() => OnChange?.Invoke();
}
