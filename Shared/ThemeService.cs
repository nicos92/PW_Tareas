using Microsoft.JSInterop;

namespace TareasBlazor.Shared
{
    public class ThemeService
    {
        private readonly IJSRuntime _js;

        public string CurrentTheme { get; private set; } = "light";

        public event Action? OnThemeChanged;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitializeAsync()
        {
            CurrentTheme = await _js.InvokeAsync<string>("getTheme");
            await ApplyTheme(CurrentTheme);
        }

        public async Task SetTheme(string theme)
        {
            CurrentTheme = theme;
            await ApplyTheme(theme);
            OnThemeChanged?.Invoke();
        }

        private async Task ApplyTheme(string theme)
        {
            await _js.InvokeVoidAsync("setTheme", theme);
        }
    }
}