using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IAcessoSieconHttpService
    {
        public Task<Object> Dadoscliente(string cpf);
    }

    public class AcessoSieconHttpService : IAcessoSieconHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public AcessoSieconHttpService(HttpClient httpclient, IConfiguration configuration)
        {
            _httpClient = httpclient;
            _configuration = configuration;            
        }

        public async Task<Object> Dadoscliente(string cpf)
        {
            var urlApiSiecon = _configuration["urlapisiecon"];
            var url = $"{urlApiSiecon}/api/DadosCliente?cnpjCpf={cpf}&dataAssCtrDe=2000-01-01&dataAssCtrAte=2100-01-01";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new Object();
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Notificacao>(values);
            if (data == null) return new Object();
            return data;
        }
    }
}
