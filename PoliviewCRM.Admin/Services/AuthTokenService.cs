using Microsoft.JSInterop;

namespace PoliviewCRM.Admin.Services;

public class AuthTokenService : IAuthTokenService
{
    private const string TokenKey = "token";
    private readonly IJSRuntime _jsRuntime;

    public AuthTokenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("getLocalStorageItem", TokenKey);
    }

    public async Task SetTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("setLocalStorageItem", TokenKey, token);
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("removeLocalStorageItem", TokenKey);
    }
}
