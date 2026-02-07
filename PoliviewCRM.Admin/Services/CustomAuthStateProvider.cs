using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace PoliviewCRM.Admin.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthTokenService _authTokenService;

    public CustomAuthStateProvider(IAuthTokenService authTokenService)
    {
        _authTokenService = authTokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authTokenService.GetTokenAsync();
        ClaimsIdentity identity;

        if (!string.IsNullOrEmpty(token))
            identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Authentication, token) }, "apiauth");
        else
            identity = new ClaimsIdentity();

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
