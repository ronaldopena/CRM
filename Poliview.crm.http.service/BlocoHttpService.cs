using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;

namespace Poliview.crm.http.services
{
    public interface IBlocoHttpService
    {
        public Task<List<Bloco>> Listar(int idEmprdSp7);
    }
    public class BlocoHttpService: IBlocoHttpService
    {

        private readonly HttpClient _httpClient;
        public BlocoHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }
       
        public async Task<List<Bloco>> Listar(int idEmprdSp7)
        {
            var url = _httpClient.BaseAddress + $"/Bloco/{idEmprdSp7}";
            var response = await _httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<Bloco>();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Bloco>>(values);
            if (data == null)
                return new List<Bloco>();

            return data;
        }
    }
}
