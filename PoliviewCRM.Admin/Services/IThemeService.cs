namespace PoliviewCRM.Admin.Services;

public interface IThemeService
{
    Task<string> GetThemeAsync();
    Task SetThemeAsync(string theme);
    Task ToggleThemeAsync();
    Task InitializeThemeAsync();
    event Action<string>? ThemeChanged;
}
