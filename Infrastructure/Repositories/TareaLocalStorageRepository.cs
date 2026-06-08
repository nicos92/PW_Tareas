using Microsoft.JSInterop;

using TareasBlazor.Infraestructure.Interfaces;
using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Repositories
{
    public class TareaLocalStorageRepository(IJSRuntime jsRuntime) : ITareaRepository
    {
        private const string JsModulePath = "./js/tareas.js";

        public async Task<List<TareaModel>> GetTareasAsync()
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", JsModulePath);
            var tareas = await module.InvokeAsync<List<TareaModel>>("getTareas");
            return tareas ?? [];
        }

        public async Task<TareaModel?> GetTareaByIdAsync(string id)
        {
            var tareas = await GetTareasAsync();
            return tareas.FirstOrDefault(t => t.IdPublic == id);
        }

        public async Task AddTareaAsync(TareaModel tarea)
        {
            var tareas = await GetTareasAsync();
            tareas.Add(tarea);
            await SaveAll(tareas);
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

        public async Task DeleteTareasAsync()
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", JsModulePath);
            await module.InvokeVoidAsync("clearTareas");
        }

        private async Task SaveAll(List<TareaModel> tareas)
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", JsModulePath);
            await module.InvokeVoidAsync("saveTareas", tareas);
        }
    }
}