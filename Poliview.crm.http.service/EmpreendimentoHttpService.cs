using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IEmpreendimentoHttpService
    {
        public Task<List<Empreendimento>> Listar();
        public Task<List<Empreendimento>> ListarParaRelatorios();
        public Task<Empreendimento> ListarPorIdSP7(int idempreendimentosp7);
        public Task<Empreendimento> Update(Empreendimento mensagem);
    }
    public class EmpreendimentoHttpService : IEmpreendimentoHttpService
    {

        private readonly HttpClient _httpClient;
        public EmpreendimentoHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<List<Empreendimento>> Listar()
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/empreendimento";
                var response = await _httpClient.GetAsync(url);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
                string? values = jObject.SelectToken("objeto")?.ToString();
                if (string.IsNullOrWhiteSpace(values))
                    return new List<Empreendimento>();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empreendimento>>(values);
                if (data == null)
                    return new List<Empreendimento>();

                return data;

            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Empreendimento>> ListarParaRelatorios()
        {
            try
            {
                var url = Util.urlApiCrm(_httpClient) + $"/empreendimento/relatorio";
                var response = await _httpClient.GetAsync(url);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
                string? values = jObject.SelectToken("objeto")?.ToString();
                if (string.IsNullOrWhiteSpace(values))
                    return new List<Empreendimento>();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empreendimento>>(values);
                if (data == null)
                    return new List<Empreendimento>();

                return data;

            }
            catch
            {
                throw;
            }
        }

        public async Task<Empreendimento> ListarPorIdSP7(int idempreendimentosp7)
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/empreendimento/{idempreendimentosp7}";
                var response = await _httpClient.GetAsync(url);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
                string? values = jObject.SelectToken("objeto")?.ToString();
                if (string.IsNullOrWhiteSpace(values))
                    return new Empreendimento();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Empreendimento>(values);
                if (data == null)
                    return new Empreendimento();

                return data;

            }
            catch
            {
                throw;
            }
        }

        public async Task<Empreendimento> Update(Empreendimento empreendimento)
        {
            var url = _httpClient.BaseAddress + $"/empreendimento";
            Console.WriteLine("Alteração de empreendimento");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(empreendimento);
            Console.WriteLine(json);
            JsonContent content = JsonContent.Create(empreendimento);
            var response = await _httpClient.PutAsync(url, content);
            return empreendimento;
        }
    }
}
