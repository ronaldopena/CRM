namespace PoliviewCRM.Admin.Services;

public interface IAuthTokenService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearAsync();
}
