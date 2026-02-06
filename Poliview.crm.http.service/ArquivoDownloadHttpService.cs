using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using System.Net.Http.Json;

namespace Poliview.crm.http.services
{
    public interface IArquivoDownloadHttpService
    {
        public Task<List<ArquivoDownload>> ListAll();
        public Task<ArquivoDownload> GetArquivoPorId(int id);
        public Task<ArquivoDownload> Create(ArquivoDownload arquivo);
        public Task<ArquivoDownload> Update(ArquivoDownload arquivo);
        public Task<int> Delete(int id);
        public Task<RetornoRelatorio> RelatorioCienciaArquivo(RelatorioCienciaArquivoRequisicao obj);
        public Task<string> BaixaArquivo(BaixaArquivoDownloadRequisicao obj);
        public Task<List<GrupoMidiaArquivo>> ListarArquivosDownloadPoGrupo();
        public Task<List<GrupoMidiaArquivoDetalhe>> ListarArquivosDownloadPoGrupoDetalhe(string cpf, int idgrupomidia);
        public Task<StatusRelatorio> StatusRelatorio(string nomeArquivo);

    }

    public class ArquivoDownloadHttpService : IArquivoDownloadHttpService
    {
        private readonly HttpClient _httpClient;
        public ArquivoDownloadHttpService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<List<ArquivoDownload>> ListAll()
        {
            var url = _httpClient.BaseAddress + $"/arquivodownload";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<ArquivoDownload>();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArquivoDownload>>(values);
            if (data == null)
                return new List<ArquivoDownload>();

            return data;

        }

        public async Task<ArquivoDownload> GetArquivoPorId(int id)
        {
            var url = _httpClient.BaseAddress + $"/arquivodownload/{id}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new ArquivoDownload();

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ArquivoDownload>(values);
            if (data == null)
                return new ArquivoDownload();

            return data;

        }

        public async Task<ArquivoDownload> Create(ArquivoDownload Arquivo)
        {

            JsonContent content = JsonContent.Create(Arquivo);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(Arquivo);
            Console.WriteLine("Inclusão de Arquivo");
            Console.WriteLine(json);
            var url = _httpClient.BaseAddress + $"/arquivodownload";
            var response = await _httpClient.PostAsync(url, content);

            return Arquivo;
        }

        public async Task<ArquivoDownload> Update(ArquivoDownload Arquivo)
        {
            Console.WriteLine("Alteração de Arquivo");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(Arquivo);
            Console.WriteLine(json);
            JsonContent content = JsonContent.Create(Arquivo);
            var url = _httpClient.BaseAddress + $"/arquivodownload";
            var response = await _httpClient.PutAsync(url, content);
            return Arquivo;
        }

        public async Task<int> Delete(int id)
        {
            var url = _httpClient.BaseAddress + $"/arquivodownload/{id}";
            var response = await _httpClient.DeleteAsync(url);
            return 1;
        }

        public async Task<RetornoRelatorio> RelatorioCienciaArquivo(RelatorioCienciaArquivoRequisicao obj)
        {
            try
            {
                var url = _httpClient.BaseAddress + $"/arquivodownload/relatoriocienciadownload";
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


        public async Task<string> BaixaArquivo(BaixaArquivoDownloadRequisicao obj)
        {
            var url = _httpClient.BaseAddress + $"/arquivodownload/download";
            var response = await _httpClient.PostAsJsonAsync(url, obj);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("Arquivo")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return "";
            return values;
        }
        public async Task<List<GrupoMidiaArquivo>> ListarArquivosDownloadPoGrupo()
        {
            var url = _httpClient.BaseAddress + $"/arquivodownload/agrupamento";
            var response = await _httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<GrupoMidiaArquivo>();
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GrupoMidiaArquivo>>(values);
            if (data == null)
                return new List<GrupoMidiaArquivo>();
            return data;
        }

        public async Task<List<GrupoMidiaArquivoDetalhe>> ListarArquivosDownloadPoGrupoDetalhe(string cpf, int idgrupomidia)
        {
            var url = _httpClient.BaseAddress + $"/arquivodownload/agrupamento/{cpf}/{idgrupomidia}";
            var response = await _httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto")?.ToString();
            if (string.IsNullOrWhiteSpace(values))
                return new List<GrupoMidiaArquivoDetalhe>();
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GrupoMidiaArquivoDetalhe>>(values);
            if (data == null)
                return new List<GrupoMidiaArquivoDetalhe>();
            return data;
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
            return data;
        }

    }
}
