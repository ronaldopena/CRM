using Microsoft.AspNetCore.Mvc;
using PoliviewCrm.CvCrm.Models;
using PoliviewCrm.CvCrm.Services;
using RestSharp;
using System.Numerics;

namespace PoliviewCrm.CvCrm.Controllers;

public class BoletosController : Controller
{
    enum TipoRelatorio
    {
        boleto = 1,
        informerendimento = 2,
        fichafinanceira = 3
    }

    private readonly ILogger<BoletosController> _logger;
    private readonly IConfiguration _configuration;

    public BoletosController(ILogger<BoletosController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("boletos")]
    public async Task<IActionResult> Index()
    {
        // Flag de debug para controlar exibição de dados na View
        var debugEnabled = _configuration.GetValue<bool>("debug");
        var showEmail = _configuration.GetValue<bool>("showEmail");
        ViewBag.Debug = debugEnabled;
        // Cores configuráveis
        ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
        ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
        ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";

        var urlboleto = _configuration.GetValue<string>("SieconApi:UrlBoleto");
        if (string.IsNullOrWhiteSpace(urlboleto))
        {
            ViewBag.SieconError = "URL da API do Siecon não configurada (SieconApi:UrlBoleto).";
            return View("Boletos", null);
        }

        // Preenche email do cliente a partir da query string (Base64 -> JSON) do /cvcrm
        var cvcrmJson = GetCvcrmJsonFromQuery();
        CvCrmDadosUnidadeSP7? dadosunidadesp7 = null;
        try
        {
            dadosunidadesp7 = System.Text.Json.JsonSerializer.Deserialize<CvCrmDadosUnidadeSP7>(cvcrmJson ?? "{}");
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Falha ao desserializar dadosunidadesp7 a partir da query. Conteúdo: {CvCrmJson}", cvcrmJson);
            dadosunidadesp7 = null;
        }
        ViewBag.Email = dadosunidadesp7?.Email;
        ViewBag.ShowEmail = showEmail;
        // Disponibiliza o código do cliente para uso na View/JS
        ViewBag.CodigoClienteSP7 = dadosunidadesp7?.ClienteSP7;

        try
        {
            // payload
            // Se os dados da unidade não puderem ser obtidos, retorna com mensagem ao usuário
            if (dadosunidadesp7 is null)
            {
                ViewBag.SieconError = "Dados da unidade não informados ou inválidos.";
                return View("Index", new List<BoletoDto>());
            }

            var payload = new
            {
                empreendimentosp7 = (dadosunidadesp7?.EmpreendimentoSP7 ?? 0).ToString(),
                blocosp7 = (dadosunidadesp7?.BlocoSP7 ?? 0).ToString(),
                unidadesp7 = dadosunidadesp7?.UnidadeSP7 ?? string.Empty,
                codigocontratosp7 = dadosunidadesp7?.ContratoSP7 ?? string.Empty,
                codigoclientesp7 = dadosunidadesp7?.ClienteSP7 ?? string.Empty
            };

            // Usa ApiPostService para executar POST (aceitando JSON e PDF)
            var serviceLogger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<ApiPostService>>()
                                ?? new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger<ApiPostService>();
            var apiService = new ApiPostService(serviceLogger);
            var response = await apiService.PostJsonAsync(urlboleto, payload, accept: "application/json, application/pdf");
            // Deserializar a lista de boletos do response.Content
            List<BoletoDto> boletos = new List<BoletoDto>();

            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    boletos = System.Text.Json.JsonSerializer.Deserialize<List<BoletoDto>>(response.Content) ?? new List<BoletoDto>();
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Erro ao deserializar lista de boletos da API");
                    ViewBag.SieconError = "Erro ao processar dados dos boletos retornados pela API.";
                }
            }

            // Logs e metadados para debug
            ViewBag.SieconUrl = urlboleto;
            ViewBag.SieconStatusCode = response.StatusCode;
            ViewBag.SieconContentType = response.ContentType;
            ViewBag.SieconResponseContent = response.Content;

            return View("Index", boletos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar API de Boletos no Siecon");
            ViewBag.SieconError = ex.Message;

            // Retorna lista vazia em caso de erro
            return View("Index", new List<BoletoDto>());
        }
    }

    // Ações dos boletos: gerar PDF, enviar e-mail e copiar linha digitável
    [HttpPost]
    [Route("boletos/gerar-pdf")]
    public async Task<IActionResult> GerarPdf([FromForm] string numero, [FromForm] int recto, [FromForm] int cobranca, [FromForm] string contrato)
    {
        var url = _configuration.GetValue<string>("SieconApi:GerarPdfUrl");
        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["Mensagem"] = "URL da API do Siecon não configurada.";
            return RedirectToAction("Index");
        }

        try
        {
            var client = new RestClient(new RestClientOptions(url));
            var request = new RestRequest("", Method.Post);
            request.AddHeader("accept", "application/json, application/pdf");
            request.AddHeader("Content-Type", "application/json");

            var cvcrmJson = GetCvcrmJsonFromQuery();
            CvCrmDadosUnidadeSP7? cvcrmDados = null;
            try
            {
                cvcrmDados = System.Text.Json.JsonSerializer.Deserialize<CvCrmDadosUnidadeSP7>(cvcrmJson ?? "{}");
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Falha ao desserializar dadosunidadesp7 para geração de PDF. Conteúdo: {CvCrmJson}", cvcrmJson);
            }
        // Removido ViewBag.error: não há uso correspondente na view.

            // Payload para boleto
            var payload = new
            {
                tiporelatorio = (int)TipoRelatorio.boleto,
                contrato = contrato,
                cobranca = cobranca,
                recebimento = recto,
                ano = DateTime.Now.Year,
                email = "",
                codigoclientesp7 = cvcrmDados?.ClienteSP7 ?? string.Empty
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            request.AddStringBody(json, DataFormat.Json);

            var response = await client.ExecuteAsync(request);

            // Exibir PDF na tela em vez de baixar
            if (!string.IsNullOrEmpty(response.ContentType) &&
                response.ContentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase) &&
                response.RawBytes != null)
            {
                var b64 = Convert.ToBase64String(response.RawBytes);
                ViewBag.PdfDataUrl = $"data:application/pdf;base64,{b64}";
            }
            else
            {
                // Tenta extrair URL de PDF do JSON de resposta para exibir inline
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar PDF do boleto");
            TempData["Mensagem"] = $"Erro ao gerar PDF: {ex.Message}";
        }

        ViewBag.CardIconColor = _configuration.GetValue<string>("Ui:CardIconColor") ?? "#ED6F23";
        ViewBag.PageBackgroundColor = _configuration.GetValue<string>("Ui:PageBackgroundColor") ?? "#F8F8FB";
        ViewBag.CardIconBgColor = _configuration.GetValue<string>("Ui:CardIconBgColor") ?? "#FDF0E9";
        return View("Index");
    }

    [HttpPost]
    [Route("boletos/enviar-email")]
    public async Task<IActionResult> EnviarEmail([FromForm] string numero, [FromForm] int recto, [FromForm] int cobranca, [FromForm] string contrato)
    {
        var idcliente = string.Empty;
        var email = string.Empty;

        var url = _configuration.GetValue<string>("SieconApi:GerarPdfUrl");
        if (string.IsNullOrWhiteSpace(url))
        {
            TempData["Mensagem"] = "URL da API do Siecon não configurada.";
            return RedirectToAction("Index");
        }

        var cvcrmJson = GetCvcrmJsonFromQuery();
        var cvcrmDados = System.Text.Json.JsonSerializer.Deserialize<CvCrmDadosUnidadeSP7>(cvcrmJson ?? "{}");
        email = cvcrmDados?.Email ?? string.Empty;
        idcliente = cvcrmDados?.ClienteSP7 ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Mensagem"] = "e-mail é obrigatório para envio.";
            return RedirectToAction("Index");
        }

        try
        {
            var client = new RestClient(new RestClientOptions(url));
            var request = new RestRequest("", Method.Post);
            request.AddHeader("accept", "application/json, application/pdf");
            request.AddHeader("Content-Type", "application/json");

            var emailTeste = _configuration.GetValue<string>("EmailTeste");

            if (!string.IsNullOrWhiteSpace(emailTeste))
            {
                email = emailTeste;
            }
            else
            {
                email = cvcrmDados.Email ?? string.Empty;
            }

            // Payload para boleto com email
            var payload = new
            {
                tiporelatorio = (int)TipoRelatorio.boleto,
                contrato = contrato,
                cobranca = cobranca,
                recebimento = recto,
                ano = DateTime.Now.Year,
                email = email,
                codigoclientesp7 = idcliente
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            request.AddStringBody(json, DataFormat.Json);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                TempData["Mensagem"] = $"Boleto do contrato {contrato} enviado com sucesso para {email}";
            }
            else
            {
                TempData["Mensagem"] = $"Erro ao enviar boleto por e-mail: {response.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar boleto por e-mail");
            TempData["Mensagem"] = $"Erro ao enviar boleto por e-mail: {ex.Message}";
        }

        // Após envio de email, exibir página de mensagem com botão OK (retorno via history.back)
        return RedirectToAction("Index", "Mensagem");
    }

    [HttpPost]
    [Route("boletos/copiar")]
    public IActionResult Copiar([FromForm] string numero)
    {
        // Em apps web padrão, copiar para área de transferência é feito no front-end via JS.
        // Aqui apenas retornamos à tela com feedback.
        TempData["Mensagem"] = $"Linha digitável copiada para o boleto {numero}.";
        return RedirectToAction("Index");
    }

    private string ExtractPdfUrl(string responseContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return string.Empty;

            using var document = System.Text.Json.JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            // Tenta diferentes possíveis nomes de propriedades para a URL do PDF
            if (root.TryGetProperty("pdfUrl", out var pdfUrlElement))
                return pdfUrlElement.GetString() ?? string.Empty;

            if (root.TryGetProperty("url", out var urlElement))
                return urlElement.GetString() ?? string.Empty;

            if (root.TryGetProperty("downloadUrl", out var downloadUrlElement))
                return downloadUrlElement.GetString() ?? string.Empty;

            if (root.TryGetProperty("link", out var linkElement))
                return linkElement.GetString() ?? string.Empty;

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao extrair URL do PDF da resposta: {ResponseContent}", responseContent);
            return string.Empty;
        }
    }


    // Lê o parâmetro de query "dadosunidadesp7" e tenta decodificar de Base64 para JSON.
    // Se falhar, retorna o próprio conteúdo (já pode estar em JSON ou URL-encoded).
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




}
