using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using TareasBlazor.Components.Features.Tareas;

namespace TareasBlazor.Components.Shared;

public partial class Breadcrum : ComponentBase, IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private TareaEstado TareaEstado { get; set; } = default!;

    private List<BreadcrumbSegment> BreadcrumbSegments = new();

    private sealed class BreadcrumbSegment
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public bool IsActive { get; set; }
    }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateBreadcrumbAsync();
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _ = UpdateBreadcrumbAsync();
    }

    private async Task UpdateBreadcrumbAsync()
    {
        if (BreadcrumbSegments.Count == 0 && !NavigationManager.Uri.Contains("listaDeTareas"))
        {
            return;
        }

        var uri = NavigationManager.Uri;
        var absolutePath = NavigationManager.ToBaseRelativePath(uri);

        if (string.IsNullOrEmpty(absolutePath) || absolutePath == "/")
        {
            BreadcrumbSegments = new List<BreadcrumbSegment>();
            StateHasChanged();
            return;
        }

        var segments = absolutePath.Trim('/').Split('/');
        BreadcrumbSegments = new List<BreadcrumbSegment>();
        var fullUrl = "";

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var isGuid = Guid.TryParse(segment, out _);
            var nextIsGuid = (i + 1 < segments.Length) && Guid.TryParse(segments[i + 1], out _);

            fullUrl += "/" + segment;

            if (nextIsGuid)
            {
                continue;
            }

            var isLast = i == segments.Length - 1;
            var name = segment;

            if (isGuid)
            {
                var tarea = await TareaEstado.ObtenerPorIdAsync(segment);
                name = tarea?.Titulo ?? segment;
            }

            BreadcrumbSegments.Add(new BreadcrumbSegment
            {
                Name = FormatSegmentName(name),
                Url = fullUrl,
                IsActive = isLast
            });
        }

        StateHasChanged();
    }

    private static string FormatSegmentName(string segment)
    {
        if (string.IsNullOrEmpty(segment))
            return segment;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < segment.Length; i++)
        {
            var c = segment[i];
            if (i > 0 && char.IsUpper(c))
            {
                result.Append(' ');
            }
            result.Append(c);
        }

        var formatted = result.ToString();
        return char.ToUpper(formatted[0]) + formatted[1..].ToLower();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
