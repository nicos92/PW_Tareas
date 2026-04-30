using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Interfaces
{
    public interface ITareaRepository
    {
        Task<List<TareaModel>> GetTareasAsync();
        Task<TareaModel?> GetTareaByIdAsync(string id);
        Task AddTareaAsync(TareaModel tarea);
        Task UpdateTareaAsync(TareaModel tarea);
        Task DeleteTareaByIdAsync(string id);
        Task ToggleTareaCompletadaAsync(string id, bool completada);
        Task<List<TareaModel>> GetTareasCompletadasAsync();
        Task<List<TareaModel>> GetTareasPendientesAsync();
        Task DeleteTareasAsync();
    }
}
