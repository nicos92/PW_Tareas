using System.Security.Cryptography;

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
            try
            {
                var result = await _protectedLocalStorage.GetAsync<List<TareaModel>>(StorageKey);
                return result.Success ? result.Value ?? [] : [];
            }
            catch (CryptographicException)
            {
                await _protectedLocalStorage.DeleteAsync(StorageKey);
                return [];
            }
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

        public async Task<PaginatedResult<TareaModel>> GetTareasPaginadasAsync(
            PaginationParams paginationParams,
            string? prioridad = null,
            string? estado = null,
            string? vencimiento = null)
        {
            var tareas = await GetTareasAsync();
            IEnumerable<TareaModel> query = tareas;

            if (!string.IsNullOrEmpty(prioridad) && Enum.TryParse<Prioridad>(prioridad, out var p))
                query = query.Where(t => t.Prioridad == p);

            if (!string.IsNullOrEmpty(estado))
                query = estado == "Completadas" ? query.Where(t => t.Completada) : query.Where(t => !t.Completada);

            if (!string.IsNullOrEmpty(vencimiento))
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                query = vencimiento switch
                {
                    "Vencidas" => query.Where(t => t.FechaVencimiento < today),
                    "VencenHoy" => query.Where(t => t.FechaVencimiento == today),
                    "ATiempo" => query.Where(t => t.FechaVencimiento > today),
                    _ => query
                };
            }

            var list = query.OrderByDescending(t => t.Id).ToList();
            var totalCount = list.Count;
            var items = list.Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                            .Take(paginationParams.PageSize).ToList();

            return new PaginatedResult<TareaModel>(
                items.AsReadOnly(), totalCount, paginationParams.Page, paginationParams.PageSize);
        }

        public async Task<EstadisticasTareas> GetEstadisticasAsync()
        {
            var tareas = await GetTareasAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);

            return new EstadisticasTareas
            {
                Total = tareas.Count,
                Completadas = tareas.Count(t => t.Completada),
                Baja = tareas.Count(t => t.Prioridad == Prioridad.Baja),
                Media = tareas.Count(t => t.Prioridad == Prioridad.Media),
                Alta = tareas.Count(t => t.Prioridad == Prioridad.Alta),
                Vencidas = tareas.Count(t => t.FechaVencimiento < today),
                VencenHoy = tareas.Count(t => t.FechaVencimiento == today),
                ATiempo = tareas.Count(t => t.FechaVencimiento > today)
            };
        }
    }
}