namespace TareasBlazor.Models
{
    public class EstadisticasTareas
    {
        public int Total { get; set; }
        public int Completadas { get; set; }
        public int Pendientes => Total - Completadas;
        public int Baja { get; set; }
        public int Media { get; set; }
        public int Alta { get; set; }
        public int Vencidas { get; set; }
        public int VencenHoy { get; set; }
        public int ATiempo { get; set; }
    }
}
