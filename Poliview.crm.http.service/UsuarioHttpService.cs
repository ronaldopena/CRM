using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IUsuarioHttpService
    {
        public Task<LoginResposta> Login(LoginRequisicao obj);
        public Task<UserJwt> Userjwttoken(string token);
    }

    public class UsuarioHttpService : IUsuarioHttpService
    {
        private readonly HttpClient _httpClient;
        public UsuarioHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<LoginResposta> Login(LoginRequisicao obj)        
        {
            var url = _httpClient.BaseAddress + $"/usuario/login";
            var response = await _httpClient.PostAsJsonAsync(url, obj );
            var resp = response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new LoginResposta();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResposta>(values);
            if (data == null)
                return new LoginResposta();

            return data;
        }

        public async Task<UserJwt> Userjwttoken(string token)
        {
            var obj = new ValidarTokenRequisicao();
            obj.token = token;
            var url = _httpClient.BaseAddress + $"/usuario/userjwttoken";
            var response = await _httpClient.PostAsJsonAsync(url, obj);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new UserJwt();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<UserJwt>(values);
            if (data == null)
                return new UserJwt();

            return data;
        }
    }
}
