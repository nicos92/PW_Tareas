using TareasBlazor.Models;

namespace TareasBlazor.Infraestructure.Interfaces
{
    public interface ITareaRepository
    {
        /// <summary>
        /// Obtiene todas las tareas disponibles.
        /// </summary>
        /// <returns>Una lista con todas las tareas.</returns>
        Task<List<TareaModel>> GetTareasAsync();

        /// <summary>
        /// Obtiene una tarea por su identificador único.
        /// </summary>
        /// <param name="id">El identificador único de la tarea.</param>
        /// <returns>La tarea encontrada o <c>null</c> si no existe.</returns>
        Task<TareaModel?> GetTareaByIdAsync(string id);

        /// <summary>
        /// Agrega una nueva tarea al repositorio.
        /// </summary>
        /// <param name="tarea">La tarea a agregar.</param>
        Task AddTareaAsync(TareaModel tarea);

        /// <summary>
        /// Actualiza los datos de una tarea existente.
        /// </summary>
        /// <param name="tarea">La tarea con los datos actualizados.</param>
        Task UpdateTareaAsync(TareaModel tarea);

        /// <summary>
        /// Elimina una tarea por su identificador.
        /// </summary>
        /// <param name="id">El identificador único de la tarea a eliminar.</param>
        Task DeleteTareaByIdAsync(string id);

        /// <summary>
        /// Marca o desmarca una tarea como completada.
        /// </summary>
        /// <param name="id">El identificador único de la tarea.</param>
        /// <param name="completada">El nuevo estado de finalización de la tarea.</param>
        Task ToggleTareaCompletadaAsync(string id, bool completada);

        /// <summary>
        /// Obtiene solo las tareas completadas.
        /// </summary>
        /// <returns>Una lista con las tareas completadas.</returns>
        Task<List<TareaModel>> GetTareasCompletadasAsync();

        /// <summary>
        /// Obtiene solo las tareas pendientes.
        /// </summary>
        /// <returns>Una lista con las tareas pendientes.</returns>
        Task<List<TareaModel>> GetTareasPendientesAsync();

        /// <summary>
        /// Elimina todas las tareas del repositorio.
        /// </summary>
        Task DeleteTareasAsync();

        /// <summary>
        /// Obtiene las tareas de forma paginada, con filtros opcionales de prioridad, estado y vencimiento.
        /// </summary>
        /// <param name="paginationParams">Los parámetros de paginación (número de página y tamaño).</param>
        /// <param name="prioridad">Filtro opcional por prioridad de la tarea.</param>
        /// <param name="estado">Filtro opcional por estado de la tarea.</param>
        /// <param name="vencimiento">Filtro opcional por fecha de vencimiento.</param>
        /// <returns>Un resultado paginado con las tareas que cumplen los filtros.</returns>
        Task<PaginatedResult<TareaModel>> GetTareasPaginadasAsync(
            PaginationParams paginationParams,
            string? prioridad = null,
            string? estado = null,
            string? vencimiento = null);

        /// <summary>
        /// Obtiene estadísticas agregadas sobre las tareas.
        /// </summary>
        /// <returns>Un objeto con las estadísticas de las tareas.</returns>
        Task<EstadisticasTareas> GetEstadisticasAsync();
    }
}