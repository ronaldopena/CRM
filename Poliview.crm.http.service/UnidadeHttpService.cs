using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;

namespace Poliview.crm.http.services
{
    public interface IUnidadeHttpService
    {
        public Task<List<Unidade>> Listar(int idEmprdSp7, int idBlocoSp7);
        public Task<List<Unidade>> ListarParaRelatorios(int idEmprdSp7, int idBlocoSp7);
    }
    public class UnidadeHttpService: IUnidadeHttpService
    {

        private readonly HttpClient _httpClient;
        public UnidadeHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }
       
        public async Task<List<Unidade>> Listar(int idEmprd, int idBloco)
        {
            var url = _httpClient.BaseAddress + $"/unidade/{idEmprd}/{idBloco}";
            var response = await _httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<Unidade>();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Unidade>>(values);
            if (data == null)
                return new List<Unidade>();

            return data;
        }

        public async Task<List<Unidade>> ListarParaRelatorios(int idEmprd, int idBloco)
        {
            var url = Util.urlApiCrm(_httpClient) + $"/unidade/{idEmprd}/{idBloco}/relatorio";
            var response = await _httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<Unidade>();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Unidade>>(values);
            if (data == null)
                return new List<Unidade>();

            return data;
        }
    }
}
