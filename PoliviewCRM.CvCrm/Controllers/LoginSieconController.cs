using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PoliviewCrm.CvCrm.Models;
using System.Text;
using RestSharp;
using System.Text.Json;

namespace PoliviewCrm.CvCrm.Controllers;

public class LoginSieconController : Controller
{
    enum Tiporelatorio
    {
        boleto = 1,
        informerendimento = 2,
        fichafinanceira = 3
    }
    private readonly ILogger<LoginSieconController> _logger;
    private readonly IConfiguration _configuration;

    public LoginSieconController(ILogger<LoginSieconController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Recebe string form-encoded: token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9...&slug=GrupoRVE&idconfiguracao=1
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Login()
    {
        // Lê o corpo bruto (form-encoded) para exibir e extrair o token
        string? bodyText = null;
        string token = string.Empty;
        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            bodyText = await reader.ReadToEndAsync();
            Request.Body.Position = 0; // reseta para não interferir em outros componentes

            if (!string.IsNullOrWhiteSpace(bodyText))
            {
                // Parse do formato form-encoded: token=valor&slug=valor&idconfiguracao=valor
                var formParams = System.Web.HttpUtility.ParseQueryString(bodyText);
                token = formParams["token"] ?? string.Empty;
                // Ignora slug e idconfiguracao conforme solicitado
            }
        }
        catch
        {
            bodyText = null;
        }

        ViewBag.BodyText = bodyText;
        ViewBag.ContentType = Request.ContentType;

        // Delegar processamento comum
        var idUnidade = Request.Query["idUnidade"].FirstOrDefault();
        await ProcessLoginAsync(token, idUnidade);

        // Renderiza a view após processar chamadas às APIs
        return View("Login");
    }

    private static List<KeyValuePair<string, string>> ExtractFlatFields(string? json)
    {
        var result = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrWhiteSpace(json)) return result;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            void AddSimple(string key, JsonElement value)
            {
                switch (value.ValueKind)
                {
                    case JsonValueKind.String:
                        result.Add(new KeyValuePair<string, string>(key, value.GetString() ?? string.Empty));
                        break;
                    case JsonValueKind.Number:
                        result.Add(new KeyValuePair<string, string>(key, value.ToString()));
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        result.Add(new KeyValuePair<string, string>(key, value.ToString()));
                        break;
                    case JsonValueKind.Null:
                        result.Add(new KeyValuePair<string, string>(key, string.Empty));
                        break;
                }
            }

            void ProcessObject(JsonElement obj, string prefix)
            {
                foreach (var prop in obj.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    var val = prop.Value;
                    switch (val.ValueKind)
                    {
                        case JsonValueKind.Object:
                            // Extrai apenas propriedades simples do primeiro nível do objeto
                            foreach (var nested in val.EnumerateObject())
                            {
                                var nestedKey = $"{key}.{nested.Name}";
                                AddSimple(nestedKey, nested.Value);
                            }
                            break;
                        case JsonValueKind.Array:
                            // Se for array de objetos, extrai campos simples do primeiro elemento
                            if (val.GetArrayLength() > 0)
                            {
                                var first = val[0];
                                if (first.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var nested in first.EnumerateObject())
                                    {
                                        var nestedKey = $"{key}[0].{nested.Name}";
                                        AddSimple(nestedKey, nested.Value);
                                    }
                                }
                                else
                                {
                                    // Array simples: junta os valores em uma string curta
                                    var items = new List<string>();
                                    foreach (var item in val.EnumerateArray()) items.Add(item.ToString());
                                    result.Add(new KeyValuePair<string, string>(key, string.Join(", ", items)));
                                }
                            }
                            else
                            {
                                result.Add(new KeyValuePair<string, string>(key, "[]"));
                            }
                            break;
                        default:
                            AddSimple(key, val);
                            break;
                    }
                }
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                ProcessObject(root, "");
            }
            else if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var first = root[0];
                if (first.ValueKind == JsonValueKind.Object)
                {
                    ProcessObject(first, "[0]");
                }
                else
                {
                    var items = new List<string>();
                    foreach (var item in root.EnumerateArray()) items.Add(item.ToString());
                    result.Add(new KeyValuePair<string, string>("[array]", string.Join(", ", items)));
                }
            }
        }
        catch
        {
            // Caso o conteúdo não seja JSON válido, apenas retorna vazio
        }

        return result;
    }

    // Helpers para mapeamento de campos específicos a partir dos JSONs das APIs
    private static string? TryGetPessoaDocumento(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in root.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "documento", StringComparison.OrdinalIgnoreCase))
                    {
                        return prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.ToString();
                    }
                }
            }
        }
        catch { }
        return null;
    }

    private static int? TryGetUnidadeIdEmpreendimento(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("empreendimento", out var emp))
            {
                if (emp.ValueKind == JsonValueKind.Object && emp.TryGetProperty("idempreendimento", out var idEmp))
                {
                    if (idEmp.ValueKind == JsonValueKind.Number && idEmp.TryGetInt32(out var v)) return v;
                    if (int.TryParse(idEmp.ToString(), out var v2)) return v2;
                }
            }
        }
        catch { }
        return null;
    }

    private static int? TryGetUnidadeIdBloco(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("bloco", out var bloco))
            {
                if (bloco.ValueKind == JsonValueKind.Object && bloco.TryGetProperty("idbloco", out var idBl))
                {
                    if (idBl.ValueKind == JsonValueKind.Number && idBl.TryGetInt32(out var v)) return v;
                    if (int.TryParse(idBl.ToString(), out var v2)) return v2;
                }
            }
        }
        catch { }
        return null;
    }

    private static string? TryGetUnidadeNome(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("nome", out var nome))
            {
                return nome.ValueKind == JsonValueKind.String ? nome.GetString() : nome.ToString();
            }
        }
        catch { }
        return null;
    }


    [HttpGet]
    [Route("")]
    public async Task<IActionResult> LoginTeste()
    {
        // Lê o corpo bruto (apenas informativo no GET de teste)
        string? bodyText = "Acesso via GET";
        string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJqdGkiOjk4MjcsInBhaW5lbCI6ImNsaWVudGUiLCJpc3MiOiJodHRwczovL3J2ZS5jdmNybS5jb20uYnIiLCJzdWIiOjIyOTIsInNsdWciOiJHcnVwb1JWRSIsInVzZXIiOjIyMCwiaWF0IjoxNzY0OTcwMjA0LCJuYmYiOjE3NjQ5NzAyMDQsImV4cCI6MTc2NDk5MTgwNH0.Oq34dinXkGZ1_PZz9tk6pqekhzlD-gsUYK0pcXKEUqubNfEQe69gCnOb6TF4PfD2ggokW4mBxXUMLVU8725AiU_d5xNtaA6XFN2rgsf6EeDolnoFIzlnDoyEXAtrfWb4cigUQ-62GnEYOVMSrvaYLZerMbyKCTCrmuH1BWnQZqA";
        ViewBag.BodyText = bodyText;
        ViewBag.ContentType = Request.ContentType;

        // idUnidade fixo para testes
        var idUnidadeTeste = "2478";
        await ProcessLoginAsync(token, idUnidadeTeste);

        // Renderiza a view após processar chamadas às APIs
        return View("Login");
    }

    // Método comum para processar login (POST e GET de teste)
    private async Task ProcessLoginAsync(string token, string? idUnidade)
    {
        // Coleta query string para debug/inspeção
        var queryDict = Request.Query.ToDictionary(kv => kv.Key, kv => string.Join(",", kv.Value.ToArray()));
        ViewBag.QueryParams = queryDict;

        ViewBag.Token = token;

        // Flag de debug e cores de UI
        var debugEnabled = _configuration.GetValue<bool>("debug");
        ViewBag.Debug = debugEnabled;
        ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
        ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
        ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";

        string? pessoaJsonContent = null;
        string? unidadeJsonContent = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            // 1) Consulta Pessoa
            try
            {
                var pessoaUrl = _configuration.GetValue<string>("CvCrmApi:ClientePessoaUrl");
                if (string.IsNullOrWhiteSpace(pessoaUrl))
                {
                    ViewBag.ApiPessoaError = "URL da API de pessoa não configurada (CvCrmApi:ClientePessoaUrl).";
                }
                else
                {
                    var pessoaClient = new RestClient(new RestClientOptions(pessoaUrl));
                    var pessoaRequest = new RestRequest("");
                    pessoaRequest.AddHeader("accept", "application/json");
                    pessoaRequest.AddHeader("authorization", $"Bearer {token}");

                    var pessoaResponse = await pessoaClient.GetAsync(pessoaRequest);
                    ViewBag.ApiPessoaStatusCode = (int)pessoaResponse.StatusCode;
                    ViewBag.ApiPessoaContentType = pessoaResponse.ContentType;
                    ViewBag.ApiPessoaResponseContent = pessoaResponse.Content;
                    pessoaJsonContent = pessoaResponse.Content;
                    ViewBag.ApiPessoaFields = ExtractFlatFields(pessoaResponse.Content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar API de Pessoa do CvCRM");
                ViewBag.ApiPessoaError = ex.Message;
            }

            // 2) Consulta Unidade (se informada)
            try
            {
                if (!string.IsNullOrWhiteSpace(idUnidade))
                {
                    var baseUrl = _configuration.GetValue<string>("CvCrmApi:ClienteUnidadeBaseUrl");
                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        ViewBag.ApiUnidadeError = "URL base da API de unidade não configurada (CvCrmApi:ClienteUnidadeBaseUrl).";
                    }
                    else
                    {
                        var unidadeUrl = baseUrl.TrimEnd('/') + "/" + idUnidade;
                        var unidadeClient = new RestClient(new RestClientOptions(unidadeUrl));
                        var unidadeRequest = new RestRequest("");
                        unidadeRequest.AddHeader("accept", "application/json");
                        unidadeRequest.AddHeader("authorization", $"Bearer {token}");

                        var unidadeResponse = await unidadeClient.GetAsync(unidadeRequest);
                        ViewBag.ApiUnidadeStatusCode = (int)unidadeResponse.StatusCode;
                        ViewBag.ApiUnidadeContentType = unidadeResponse.ContentType;
                        ViewBag.ApiUnidadeResponseContent = unidadeResponse.Content;
                        unidadeJsonContent = unidadeResponse.Content;
                        ViewBag.ApiUnidadeFields = ExtractFlatFields(unidadeResponse.Content);

                        // 3) Após consultar a unidade, chama a API SIECON CvCRM (POST) para retornar dados da unidade cadastrada no SIECON
                        try
                        {
                            var cvcrmUrl = _configuration.GetValue<string>("SieconApi:CvCrmUrl");
                            if (string.IsNullOrWhiteSpace(cvcrmUrl))
                            {
                                ViewBag.ApiSieconUnidadeError = "URL da API CvCRM não configurada (SieconApi:CvCrmUrl).";
                            }
                            else
                            {
                                // Monta o payload com mapeamento automático das respostas com fallback pela query
                                var cpfCnpj = TryGetPessoaDocumento(pessoaJsonContent) ?? (Request.Query["cpfCnpj"].FirstOrDefault() ?? string.Empty);
                                int idEmpreendimentoCvCrm = TryGetUnidadeIdEmpreendimento(unidadeJsonContent) ?? 0;
                                int idBlocoCvCrm = TryGetUnidadeIdBloco(unidadeJsonContent) ?? 0;
                                var unidadeCvCrm = TryGetUnidadeNome(unidadeJsonContent) ?? (Request.Query["UnidadeCvCrm"].FirstOrDefault() ?? idUnidade ?? string.Empty);

                                // Fallback: parâmetros da query têm precedência se informados
                                if (int.TryParse(Request.Query["idEmpreendimentoCvCrm"].FirstOrDefault(), out var idEmp)) idEmpreendimentoCvCrm = idEmp;
                                if (int.TryParse(Request.Query["idBlocoCvCrm"].FirstOrDefault(), out var idBloco)) idBlocoCvCrm = idBloco;

                                var payload = new
                                {
                                    CpfCnpj = cpfCnpj,
                                    idEmpreendimentoCvCrm = idEmpreendimentoCvCrm,
                                    idBlocoCvCrm = idBlocoCvCrm,
                                    UnidadeCvCrm = unidadeCvCrm
                                };

                                // Usa serviço genérico para POST JSON com bearer token
                                var serviceLogger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<PoliviewCrm.CvCrm.Services.ApiPostService>>()
                                                    ?? new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger<PoliviewCrm.CvCrm.Services.ApiPostService>();
                                var apiService = new PoliviewCrm.CvCrm.Services.ApiPostService(serviceLogger);
                                var cvcrmResponse = await apiService.PostJsonAsync<CvCrmDadosUnidadeSP7>(cvcrmUrl, payload, bearerToken: token);

                                ViewBag.ApiSieconUrl = cvcrmUrl;
                                ViewBag.ApiSieconAuthHeader = !string.IsNullOrWhiteSpace(token);
                                ViewBag.ApiSieconPayload = JsonSerializer.Serialize(payload);
                                ViewBag.ApiSieconUnidadeStatusCode = cvcrmResponse.StatusCode;
                                ViewBag.ApiSieconUnidadeContentType = cvcrmResponse.ContentType;
                                ViewBag.ApiSieconUnidadeResponseContent = cvcrmResponse.RawContent;
                                ViewBag.ApiSieconUnidadeFields = ExtractFlatFields(cvcrmResponse.RawContent);

                                // Disponibiliza o retorno do /cvcrm para uso posterior (ex.: Boletos)
                    // Removido: TempData["dadosunidadesp7"] não é mais utilizado. Dados fluem via query string (Base64) a partir da view de Login.
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao chamar API CvCRM (SIECON) para dados da unidade");
                            ViewBag.ApiSieconUnidadeError = ex.Message;
                        }
                    }
                }
                else
                {
                    ViewBag.ApiUnidadeInfo = "Passe idUnidade na query string para consultar os dados da unidade.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao chamar API de Unidade do CvCRM");
                ViewBag.ApiUnidadeError = ex.Message;
            }
        }
    }
}
