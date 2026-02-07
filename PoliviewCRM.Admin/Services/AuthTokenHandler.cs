using System.Net.Http.Headers;

namespace PoliviewCRM.Admin.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IAuthTokenService _authTokenService;

    public AuthTokenHandler(IAuthTokenService authTokenService)
    {
        _authTokenService = authTokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authTokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
