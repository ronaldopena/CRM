using Microsoft.AspNetCore.Mvc;
using RestSharp;
using PoliviewCrm.CvCrm.Models;

namespace PoliviewCrm.CvCrm.Controllers;

public class InformeRendimentosController : Controller
{
    enum TipoRelatorio
    {
        boleto = 1,
        informerendimento = 2,
        fichafinanceira = 3
    }

    public class CvCrmDadosUnidadeSP7
    {
        public int EmpreendimentoSP7 { get; set; }
        public int BlocoSP7 { get; set; }
        public string UnidadeSP7 { get; set; }
        public string ContratoSP7 { get; set; }
        public string ClienteSP7 { get; set; }
        public string Email { get; set; }
    }
    private readonly ILogger<InformeRendimentosController> _logger;
    private readonly IConfiguration _configuration;

    public InformeRendimentosController(ILogger<InformeRendimentosController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("informe-de-rendimentos")]
    public async Task<IActionResult> Index()
    {
        // Apenas prepara a View inicial; o processamento do PDF ocorre no POST (ProcessarAcao)
        var debugEnabled = _configuration.GetValue<bool>("debug");
        var showEmail = _configuration.GetValue<bool>("showEmail");
        ViewBag.Debug = debugEnabled;
        ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
        ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
        ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";
        ViewBag.AnoAtual = DateTime.Now.Year;
        // Email do cliente derivado da query string (Base64 -> JSON) do /cvcrm
        var cvcrmJson = GetCvcrmJsonFromQuery();
        var cvcrmDados = System.Text.Json.JsonSerializer.Deserialize<CvCrmDadosUnidadeSP7>(cvcrmJson ?? "{}");
        ViewBag.Email = cvcrmDados?.Email;
        ViewBag.CodigoClienteSP7 = cvcrmDados?.ClienteSP7;
        ViewBag.ShowEmail = showEmail;
        return View("Index");
    }

    [HttpPost]
    [Route("informe-de-rendimentos")]
    public async Task<IActionResult> ProcessarAcao([FromForm] int ano, [FromForm] string action, [FromForm] string? email = null)
    {
        // Flag de debug para controlar exibição de dados na View
        var debugEnabled = _configuration.GetValue<bool>("debug");
        ViewBag.Debug = debugEnabled;

        // Validar ano
        if (ano < 2000 || ano > DateTime.Now.Year)
        {
            ViewBag.SieconError = "Ano inválido. Selecione um ano entre 2000 e o ano atual.";
            ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
            ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
            ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";
            ViewBag.AnoAtual = DateTime.Now.Year;
            return View("Index");
        }

        // Lê URL da API do Siecon
        var url = _configuration.GetValue<string>("SieconApi:GerarPdfUrl");
        if (string.IsNullOrWhiteSpace(url))
        {
            ViewBag.SieconError = "URL da API do Siecon não configurada (SieconApi:GerarPdfUrl).";
            ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
            ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
            ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";
            ViewBag.AnoAtual = DateTime.Now.Year;
            return View("Index");
        }

        try
        {

            var client = new RestClient(new RestClientOptions(url));
            ViewBag.SieconUrl = url;
            var request = new RestRequest("", Method.Post);
            request.AddHeader("accept", "application/json, application/pdf");
            request.AddHeader("Content-Type", "application/json");

            // Tenta construir o payload a partir do retorno do /cvcrm (login) vindo via query
            var cvcrmResponseJson = GetCvcrmJsonFromQuery();
            // Removido ViewBag.error: não há uso correspondente na view.
            var cvcrmDadosProc = System.Text.Json.JsonSerializer.Deserialize<CvCrmDadosUnidadeSP7>(cvcrmResponseJson ?? "{}");
            // Email efetivo: se não informado, tenta do /cvcrm (desserializado)
            var effectiveEmail = cvcrmDadosProc?.Email;
            var idcliente = cvcrmDadosProc?.ClienteSP7;

            if (string.Equals(action, "baixar", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new
                {
                    tiporelatorio = (int)TipoRelatorio.informerendimento,
                    contrato = cvcrmDadosProc.ContratoSP7,
                    cobranca = 0,
                    recebimento = 0,
                    ano = ano,
                    email = "",
                    codigoclientesp7 = cvcrmDadosProc.ClienteSP7
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);

                ViewBag.jsonText = json;
                request.AddStringBody(json!, DataFormat.Json);
                var response = await client.ExecuteAsync(request);

                // Exibir PDF na tela em vez de baixar
                if (!string.IsNullOrEmpty(response.ContentType) && response.ContentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase) && response.RawBytes != null)
                {
                    var b64 = Convert.ToBase64String(response.RawBytes);
                    ViewBag.PdfDataUrl = $"data:application/pdf;base64,{b64}";
                }
                else
                {
                    var pdfUrl = ExtractPdfUrl(response.Content);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        ViewBag.PdfUrl = pdfUrl;
                    }
                    else
                    {
                        TempData["Mensagem"] = "PDF gerado, mas não foi possível obter a URL para exibição.";
                    }
                }

                ViewBag.SieconStatusCode = (int)response.StatusCode;
                ViewBag.SieconContentType = response.ContentType;
                ViewBag.SieconResponseContent = response.Content;
            }
            else if (string.Equals(action, "email", StringComparison.OrdinalIgnoreCase))
            {
                var emailTeste = _configuration.GetValue<string>("EmailTeste");

                if (!string.IsNullOrWhiteSpace(emailTeste))
                {
                    // Ambiente de teste: sobrescreve email para evitar envios acidentais
                    email = emailTeste;
                }
                else
                {
                    email = cvcrmDadosProc.Email;
                }

                var payload = new
                {
                    tiporelatorio = (int)TipoRelatorio.informerendimento,
                    contrato = cvcrmDadosProc.ContratoSP7,
                    cobranca = 0,
                    recebimento = 0,
                    ano = ano,
                    email = email,
                    codigoclientesp7 = cvcrmDadosProc.ClienteSP7
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);

                ViewBag.jsonText = json;
                request.AddStringBody(json!, DataFormat.Json);

                if (string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.SieconError = "e-mail é obrigatório para envio.";
                    // Não mostrar PDF
                }
                else
                {
                    var response = await client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {
                        TempData["Mensagem"] = $"Informe de rendimentos de {ano} enviado com sucesso para {email}";
                    }
                    else
                    {
                        TempData["Mensagem"] = $"Erro ao enviar informe por e-mail: {response.ErrorMessage}";
                    }
                    // Redireciona para página de mensagem com botão OK
                    return RedirectToAction("Index", "Mensagem");
                }
            }
            else
            {
                ViewBag.SieconError = "Ação inválida.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar ação de Informe de Rendimentos");
            ViewBag.SieconError = ex.Message;
        }

        // Cores configuráveis
        ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
        ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
        ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";
        ViewBag.AnoAtual = DateTime.Now.Year;

        return View("Index");
    }

    // Lê o parâmetro de query "dadosunidadesp7" e tenta decodificar de Base64 para JSON.
    private string GetCvcrmJsonFromQuery()
    {
        var raw = Request.Query["dadosunidadesp7"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        try
        {
            var bytes = Convert.FromBase64String(raw);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            try { return Uri.UnescapeDataString(raw); } catch { return raw; }
        }
    }

    private static string? ExtractPdfUrl(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return ExtractPdfUrlRecursive(doc.RootElement);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractPdfUrlRecursive(System.Text.Json.JsonElement element)
    {
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.String:
                var str = element.GetString();
                if (!string.IsNullOrEmpty(str) && str.StartsWith("http", StringComparison.OrdinalIgnoreCase) && str.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
                    return str;
                break;

            case System.Text.Json.JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var result = ExtractPdfUrlRecursive(prop.Value);
                    if (!string.IsNullOrEmpty(result)) return result;
                }
                break;

            case System.Text.Json.JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var result = ExtractPdfUrlRecursive(item);
                    if (!string.IsNullOrEmpty(result)) return result;
                }
                break;
        }

        return string.Empty;
    }


        private string FindFirstValue(System.Text.Json.JsonElement element, IEnumerable<string> candidateKeys)
        {
        foreach (var key in candidateKeys)
        {
            if (TryGetPropertyIgnoreCase(element, key, out var value))
            {
                var str = ElementToString(value);
                if (!string.IsNullOrWhiteSpace(str)) return str;
            }
        }

        // Busca recursiva
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var found = FindFirstValue(prop.Value, candidateKeys);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
                break;
            case System.Text.Json.JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var found = FindFirstValue(item, candidateKeys);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
                break;
        }

            return string.Empty;
        }

    private bool TryGetPropertyIgnoreCase(System.Text.Json.JsonElement element, string propertyName, out System.Text.Json.JsonElement value)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

        private string ElementToString(System.Text.Json.JsonElement el)
        {
            switch (el.ValueKind)
            {
                case System.Text.Json.JsonValueKind.String:
                    return el.GetString() ?? string.Empty;
                case System.Text.Json.JsonValueKind.Number:
                    return el.TryGetInt64(out var l) ? l.ToString() : el.TryGetDouble(out var d) ? d.ToString(System.Globalization.CultureInfo.InvariantCulture) : el.GetRawText();
                case System.Text.Json.JsonValueKind.True:
                case System.Text.Json.JsonValueKind.False:
                    return el.GetBoolean().ToString();
                default:
                    return el.GetRawText();
            }
        }
    private string? ExtractEmailFromCvCrm(string cvcrmJson)
    {
        if (string.IsNullOrWhiteSpace(cvcrmJson)) return null;
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(cvcrmJson);
            var root = document.RootElement;
            var candidateKeys = new[] { "email", "Email", "EMAIL", "emailcliente", "EmailCliente", "EMAILCLIENTE" };
            var emailStr = FindFirstValue(root, candidateKeys);
            if (string.IsNullOrWhiteSpace(emailStr)) return null;
            var regex = new System.Text.RegularExpressions.Regex("^[^\x00-\x20@]+@[^@]+\\.[^@]+$");
            return regex.IsMatch(emailStr) ? emailStr : null;
        }
        catch
        {
        return string.Empty;
    }
    }
}