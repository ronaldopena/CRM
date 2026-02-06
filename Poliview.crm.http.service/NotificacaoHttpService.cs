using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface INotificacaoHttpService
    {
        public Task<Notificacao> ListarAsync(string id);
        public Task<NotificacaoResposta> SalvarAsync(Notificacao notificacao);
        public Task<RetornoRelatorio> RelatorioCienciaNotificacao(RelatorioCienciaNotificacaoRequisicao obj);
        public Task<string> LeituraNotificacao(LeituraNotificacaoRequisicao obj);
        public Task<StatusRelatorio> StatusRelatorio(string nomeArquivo);
    }

    public class NotificacaodHttpService : INotificacaoHttpService
    {
        private readonly HttpClient _httpClient;
        public NotificacaodHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<string> LeituraNotificacao(LeituraNotificacaoRequisicao obj)
        {
            var url = _httpClient.BaseAddress + $"/notificacao/leitura";
            var response = await _httpClient.PostAsJsonAsync(url, obj);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("mensagem")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return "";
            return values;
        }

        public async Task<Notificacao> ListarAsync(string id)
        {
            var url = _httpClient.BaseAddress + $"/notificacao/{id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new Notificacao();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Notificacao>(values);
            if (data == null) return new Notificacao();

            return data;
        }

        public async Task<RetornoRelatorio> RelatorioCienciaNotificacao(RelatorioCienciaNotificacaoRequisicao obj)
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/notificacao/relatoriociencianotificacao";
                var response = await _httpClient.PostAsJsonAsync(url, obj);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
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

        public async Task<NotificacaoResposta> SalvarAsync(Notificacao notificacao)
        {
            var url = _httpClient.BaseAddress + $"/notificacao";
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(notificacao);
            Console.WriteLine(json);
            JsonContent content = JsonContent.Create(notificacao);
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.ToString();
            if (string.IsNullOrWhiteSpace(values)) return new NotificacaoResposta();
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificacaoResposta>(values);
            if (data == null) return new NotificacaoResposta();
            return data;
        }

        public async Task<StatusRelatorio> StatusRelatorio(string nomeArquivo)
        {
            var url = _httpClient.BaseAddress + $"/mensagem/statusrelatorio";
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

            return data;

        }

    }
}
