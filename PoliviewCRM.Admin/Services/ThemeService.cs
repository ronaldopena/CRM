using Microsoft.JSInterop;

namespace PoliviewCRM.Admin.Services;

public class ThemeService : IThemeService
{
    private const string ThemeKey = "theme";
    private const string DefaultTheme = "light";
    private readonly IJSRuntime _jsRuntime;

    public event Action<string>? ThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetThemeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string?>("getLocalStorageItem", ThemeKey);
            return theme ?? DefaultTheme;
        }
        catch
        {
            return DefaultTheme;
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("setLocalStorageItem", ThemeKey, theme);
            await ApplyThemeAsync(theme);
            ThemeChanged?.Invoke(theme);
        }
        catch
        {
            // Ignore errors
        }
    }

    public async Task ToggleThemeAsync()
    {
        var currentTheme = await GetThemeAsync();
        var newTheme = currentTheme == "dark" ? "light" : "dark";
        await SetThemeAsync(newTheme);
    }

    public async Task InitializeThemeAsync()
    {
        var theme = await GetThemeAsync();
        await ApplyThemeAsync(theme);
    }

    private async Task ApplyThemeAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("applyTheme", theme);
        }
        catch
        {
            // Fallback: aplicar diretamente no documento
            await _jsRuntime.InvokeVoidAsync("eval", $@"
                document.documentElement.classList.remove('dark', 'light');
                document.documentElement.classList.add('{theme}');
            ");
        }
    }
}
