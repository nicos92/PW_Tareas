using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Repositories
{
    public class TareaProtectedLocalStorageRepository : ITareaRepository
    {
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private const string StorageKey = "tareas";

        public TareaProtectedLocalStorageRepository(ProtectedLocalStorage protectedLocalStorage)
        {
            _protectedLocalStorage = protectedLocalStorage;
        }

        public async Task<List<TareaModel>> GetTareasAsync()
        {
            var result = await _protectedLocalStorage.GetAsync<List<TareaModel>>(StorageKey);
            return result.Success ? result.Value ?? [] : [];
        }

        public async Task AddTareaAsync(TareaModel tarea)
        {
            var tareas = await GetTareasAsync();
            tareas.Add(tarea);
            await SaveAll(tareas);
        }

        public async Task DeleteTareasAsync()
        {
            await _protectedLocalStorage.SetAsync(StorageKey, new List<TareaModel>());
        }

        public async Task UpdateTareaAsync(TareaModel tarea)
        {
            var tareas = await GetTareasAsync();
            var index = tareas.FindIndex(t => t.IdPublic == tarea.IdPublic);
            if (index >= 0)
            {
                tareas[index] = tarea;
                await SaveAll(tareas);
            }
        }

        public async Task<TareaModel?> GetTareaByIdAsync(string id)
        {
            var tareas = await GetTareasAsync();
            return tareas.FirstOrDefault(t => t.IdPublic == id);
        }

        public async Task DeleteTareaByIdAsync(string id)
        {
            var tareas = await GetTareasAsync();
            var tarea = tareas.FirstOrDefault(t => t.IdPublic == id);
            if (tarea != null)
            {
                tareas.Remove(tarea);
                await SaveAll(tareas);
            }
        }

        public async Task ToggleTareaCompletadaAsync(string id, bool completada)
        {
            var tareas = await GetTareasAsync();
            var tarea = tareas.FirstOrDefault(t => t.IdPublic == id);
            if (tarea != null)
            {
                tarea.Completada = completada;
                await SaveAll(tareas);
            }
        }

        public async Task<List<TareaModel>> GetTareasCompletadasAsync()
        {
            var tareas = await GetTareasAsync();
            return [.. tareas.Where(t => t.Completada)];
        }

        public async Task<List<TareaModel>> GetTareasPendientesAsync()
        {
            var tareas = await GetTareasAsync();
            return [.. tareas.Where(t => !t.Completada)];
        }

        private async Task SaveAll(List<TareaModel> tareas)
        {
            await _protectedLocalStorage.SetAsync(StorageKey, tareas);
        }
    }
}
