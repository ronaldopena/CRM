using System.Net.Http.Headers;
using System.Net.Http.Json;
using IntegracaoMobussService.dominio;
using Newtonsoft.Json;
using IntegracaoMobussService.Dominio;
using System.Text;

namespace IntegracaoMobussService.Services
{
    public class ServiceHttp
    {
        public void MostraChamado(ChamadoMobuss chamado)
        {
            Console.WriteLine($"Nome: {chamado.nomeSolicitante} Chamado: {chamado.numSolicitacao} idLocal: {chamado.idLocal}");
        }

        public async Task<String> EnviarChamadoAsync(ChamadoMobuss chamado, ConfiguracaoCrm crmconfig)
        {
            try
            {
                var ret = "";
                HttpClient client = new HttpClient();
                string url = crmconfig.UrlApiMobuss ?? string.Empty;
                var uri = new Uri(@url);
                var company = JsonConvert.SerializeObject(chamado);
                var json = new JsonContent(chamado);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = json;
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", crmconfig.TokenApiMobuss);
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (((int)response.StatusCode) == 401)
                    {
                        ret = "Não autorizado!";
                    }
                    if (((int)response.StatusCode) == 503)
                    {
                        ret = "Serviço indisponível!";
                    }
                    else
                    {
                        RetornoErroMobuss? erro = JsonConvert.DeserializeObject<RetornoErroMobuss>(content);
                        if (erro != null)
                            ret = erro.mensagem;
                        else
                            ret = "Retorno da variável ERRO está nula!";
                    }
                }

                return ret;

            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }

        public class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }
    }
}
