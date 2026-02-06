using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IGrupoMidiaHttpService
    {
        public Task<List<GrupoMidia>> ListAll();
        public Task<GrupoMidia> GetGrupoMidiaPorId(int id);
        public Task<GrupoMidia> Create(GrupoMidia grupo);
        public Task<GrupoMidia> Update(GrupoMidia grupo);
        public Task<int> Delete(int id);
    }

    public class GrupoMidiaHttpService : IGrupoMidiaHttpService
    {
        private readonly HttpClient _httpClient;
        public GrupoMidiaHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<List<GrupoMidia>> ListAll()
        {
            var url = _httpClient.BaseAddress + $"/grupomidia";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<GrupoMidia>();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GrupoMidia>>(values);
            if (data == null)
                return new List<GrupoMidia>();

            return data;

        }

        public async Task<GrupoMidia> GetGrupoMidiaPorId(int id)
        {
            var url = _httpClient.BaseAddress + $"/grupomidia/{id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new GrupoMidia();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GrupoMidia>(values);
            if (data == null)
                return new GrupoMidia();

            return data;

        }

        public async Task<GrupoMidia> Create(GrupoMidia grupoMidia)
        {
            var url = _httpClient.BaseAddress + $"/grupomidia";
           JsonContent content = JsonContent.Create(grupoMidia);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(grupoMidia);
            Console.WriteLine("Inclusão de GrupoMidia");
            Console.WriteLine(json);
            var response = await _httpClient.PostAsync(url, content);
            return grupoMidia;
        }

        public async Task<GrupoMidia> Update(GrupoMidia grupoMidia)
        {
            var url = _httpClient.BaseAddress + $"/grupomidia";
            Console.WriteLine("Alteração de GrupoMidia");
            Console.WriteLine($"idGrupoMidia={grupoMidia.id}");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(grupoMidia);
            Console.WriteLine(json);
            JsonContent content = JsonContent.Create(grupoMidia);
            var response = await _httpClient.PutAsync(url, content);
            return grupoMidia;
        }

        public async Task<int> Delete(int id)
        {
            var url = _httpClient.BaseAddress + $"/grupomidia/{id}";
            var response = await _httpClient.DeleteAsync(url);
            return 1;
        }
    }
}
