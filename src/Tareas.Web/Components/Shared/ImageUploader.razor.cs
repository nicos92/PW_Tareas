using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace TareasBlazor.Components.Shared;

public partial class ImageUploader : ComponentBase
{
    [Parameter] public string? CurrentImagen { get; set; }
    [Parameter] public EventCallback<ImagenChangedEventArgs> OnImagenChanged { get; set; }
    [Parameter] public long MaxFileSize { get; set; } = 1024 * 1024;

    private IBrowserFile? archivoCargado;
    private string? imagenBase64;
    private bool tamanoExcedido;
    private int inputFileKey;

    private string TipoAlerta => tamanoExcedido ? "alert-danger" : "alert-success";

    public IBrowserFile? Archivo => archivoCargado;
    public bool TamanoExcedido => tamanoExcedido;

    private async Task CargarArchivo(InputFileChangeEventArgs e)
    {
        archivoCargado = e.File;
        tamanoExcedido = archivoCargado.Size > MaxFileSize;
        imagenBase64 = null;

        if (!tamanoExcedido)
        {
            using var stream = archivoCargado.OpenReadStream(MaxFileSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            imagenBase64 = $"data:{archivoCargado.ContentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
        }

        await OnImagenChanged.InvokeAsync(new ImagenChangedEventArgs
        {
            Archivo = archivoCargado,
            ImagenBase64 = imagenBase64,
            TamanoExcedido = tamanoExcedido
        });

        StateHasChanged();
    }

    private async Task Limpiar()
    {
        archivoCargado = null;
        imagenBase64 = null;
        tamanoExcedido = false;
        inputFileKey++;

        await OnImagenChanged.InvokeAsync(new ImagenChangedEventArgs
        {
            Archivo = null,
            ImagenBase64 = null,
            TamanoExcedido = false,
            LimpiarImagenExistente = true
        });

        StateHasChanged();
    }

    private string FormatoTamano(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    public sealed class ImagenChangedEventArgs
    {
        public IBrowserFile? Archivo { get; set; }
        public string? ImagenBase64 { get; set; }
        public bool TamanoExcedido { get; set; }
        public bool LimpiarImagenExistente { get; set; }
    }
}
