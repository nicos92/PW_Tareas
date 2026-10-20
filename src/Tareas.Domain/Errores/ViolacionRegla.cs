namespace Tareas.Domain.Errores;

public sealed record ViolacionRegla(string Campo, string Mensaje);

public sealed class ViolacionReglaException : Exception
{
    public IReadOnlyList<ViolacionRegla> Violaciones { get; }

    public ViolacionReglaException(IReadOnlyList<ViolacionRegla> violaciones)
        : base(string.Join(Environment.NewLine, violaciones.Select(v => $"{v.Campo}: {v.Mensaje}")))
    {
        Violaciones = violaciones;
    }
}
