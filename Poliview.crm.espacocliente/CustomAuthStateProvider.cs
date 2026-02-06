
using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Security.Claims;

namespace Poliview.crm.espacocliente
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ILocalStorage _localstorage;
        public CustomAuthStateProvider(ILocalStorage LocalStorage)
        {
            _localstorage = LocalStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localstorage.GetStringAsync("token");

            ClaimsIdentity identity;

            if (token!= null)
            {
                identity = new ClaimsIdentity(new[]
                    { new Claim(ClaimTypes.Authentication, token) });
            }
            else
            {
                identity = new ClaimsIdentity();
            }
            // string token = "teste.teste";
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return await Task.FromResult(state);
        }
        public async Task<Boolean> MarkUserAsAuthenticated(Usuario? usuario)
        {
            var identity = new ClaimsIdentity(new[] {
                                                        new Claim(ClaimTypes.Name, usuario.NM_USUARIO),
                                                        new Claim(ClaimTypes.Email, usuario.DS_EMAIL),
                                                        new Claim(ClaimTypes.NameIdentifier, usuario.CD_USUARIO.ToString()),
                                                    }); ;
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return true;
        }

        public async void MarkUserAsLoggedOut()
        {
            await _localstorage.RemoveAsync("token");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return;
        }

    }
}
 