using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Poliview.crm.models;
using Poliview.crm.infra;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IAutenticacaoHttpService
    {
        public Task<AutenticacaoResposta> Login(LoginRequisicao obj);
    }

    public class AutenticacaoHttpService: IAutenticacaoHttpService
    {
        private readonly HttpClient _httpClient;
        
        public AutenticacaoHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }   

        public async Task<AutenticacaoResposta> Login(LoginRequisicao obj)
        {
            if (obj.idempresa == 0) obj.idempresa = 1;

            var url = _httpClient.BaseAddress + $"/autenticacao/login";            
            var response = await _httpClient.PostAsJsonAsync(url, obj);
            var resp = response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new AutenticacaoResposta();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<AutenticacaoResposta>(values);
            if (data == null)
                return new AutenticacaoResposta();

            return data;
        }

    }
}

