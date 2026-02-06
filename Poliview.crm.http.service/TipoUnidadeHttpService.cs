using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface ITipoUnidadeHttpService
    {
        public Task<List<TipoUnidade>> Listar();
        public Task<TipoUnidade> ListarPorId(int id);
        public Task<TipoUnidade> Update(TipoUnidade obj);
    }
    public class TipoUnidadeHttpService : ITipoUnidadeHttpService
    {

        private readonly HttpClient _httpClient;
        public TipoUnidadeHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<List<TipoUnidade>> Listar()
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/tipounidade";
                var response = await _httpClient.GetAsync(url);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
                string? values = jObject.SelectToken("objeto")?.ToString();
                if (string.IsNullOrWhiteSpace(values))
                    return new List<TipoUnidade>();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TipoUnidade>>(values);
                if (data == null)
                    return new List<TipoUnidade>();

                return data;

            }
            catch
            {
                throw;
            }
        }

        public async Task<TipoUnidade> ListarPorId(int id)
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/tipounidade/{id}";
                var response = await _httpClient.GetAsync(url);
                var resp = response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(jsonResult);
                string? values = jObject.SelectToken("objeto")?.ToString();
                if (string.IsNullOrWhiteSpace(values))
                    return new TipoUnidade();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TipoUnidade>(values);
                if (data == null)
                    return new TipoUnidade();

                return data;

            }
            catch
            {
                throw;
            }
        }

        public async Task<TipoUnidade> Update(TipoUnidade obj)
        {
            var url = _httpClient.BaseAddress + $"/tipounidade";
            Console.WriteLine("Alteração de tipo unidade");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            Console.WriteLine(json);
            JsonContent content = JsonContent.Create(obj);
            var response = await _httpClient.PutAsync(url, content);
            return obj;
        }
    }
}
