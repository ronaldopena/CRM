using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using System.Net.Http;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IMensagemHttpService
    {
        public Task<List<Mensagem>> ListAll();
        public Task<Mensagem> GetMensagemPorId(int id);
        public Task<Mensagem> Create(Mensagem mensagem);
        public Task<Mensagem> Update(Mensagem mensagem);
        public Task<int> Delete(int id);
        public Task<RetornoRelatorio> RelatorioCienciaMensagem(RelatorioCienciaMensagemRequisicao obj);
        public Task<string> LeituraMensagem(LeituraMensagemRequisicao obj);
        public Task<StatusRelatorio> StatusRelatorio(string nomeArquivo);
    }

    public class MensagemHttpService : IMensagemHttpService
    {
        private readonly HttpClient _httpClient;
        public MensagemHttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("crmHttpClient");
        }

        public async Task<List<Mensagem>> ListAll()
        {
            var url = _httpClient.BaseAddress + $"/mensagem";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<Mensagem>();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Mensagem>>(values);
            if (data == null)
                return new List<Mensagem>();

            return data;

        }

        public async Task<Mensagem> GetMensagemPorId(int id)
        {
            var url = _httpClient.BaseAddress + $"/mensagem/{id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new Mensagem();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Mensagem>(values);
            if (data == null)
                return new Mensagem();

            return data;

        }

        public async Task<Mensagem> Create(Mensagem mensagem)
        {
            var url = _httpClient.BaseAddress + $"/mensagem";
            JsonContent content = JsonContent.Create(mensagem);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(mensagem);
            Console.WriteLine("Inclusão de mensagem");
            Console.WriteLine(json);
            var response = await _httpClient.PostAsync(url, content);
            return mensagem;
        }

        public async Task<Mensagem> Update(Mensagem mensagem)
        {
            var url = _httpClient.BaseAddress + $"/mensagem";
            Console.WriteLine("Alteração de mensagem");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(mensagem);
            Console.WriteLine(json);
            JsonContent content = JsonContent.Create(mensagem);
            var response = await _httpClient.PutAsync(url, content);
            return mensagem;
        }

        public async Task<int> Delete(int id)
        {
            var url = _httpClient.BaseAddress + $"/mensagem/{id}";
            var response = await _httpClient.DeleteAsync(url);
            return 1;
        }

        public async Task<string> LeituraMensagem(LeituraMensagemRequisicao obj)
        {
            var url = _httpClient.BaseAddress + $"/mensagem/leitura";
            var response = await _httpClient.PostAsJsonAsync(url, obj);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("mensagem")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return "";
            return values;
        }
        public async Task<RetornoRelatorio> RelatorioCienciaMensagem(RelatorioCienciaMensagemRequisicao obj)
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/mensagem/relatoriocienciamensagem";
                var response = await _httpClient.PostAsJsonAsync(url, obj);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
                //string? values = jObject.SelectToken("objeto")?.ToString();
                string? values = jObject.ToString();
                if (string.IsNullOrWhiteSpace(values))
                    return new RetornoRelatorio();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<RetornoRelatorio>(values);
                if (data == null)
                    return new RetornoRelatorio();

                return data;

            }
            catch
            {
                throw;
            }
        }
        public async Task<StatusRelatorio> StatusRelatorio(string nomeArquivo)
        {
            var url = _httpClient.BaseAddress + $"/mensagem/statusrelatorio/{nomeArquivo}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            //string? values = jObject.SelectToken("objeto")?.ToString();
            string? values = jObject.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new StatusRelatorio();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<StatusRelatorio>(values);
            if (data == null)
                return new StatusRelatorio();

            Console.WriteLine($"{nomeArquivo} status {data.status}");

            return data;

        }
    }
}
