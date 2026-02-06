using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace PoliviewCrm.CvCrm.Services;

/// <summary>
/// Serviço genérico para executar POST em uma URL com payload JSON e token opcional.
/// Reutilizável em qualquer controller/serviço do projeto.
/// </summary>
public class ApiPostService
{
    private readonly ILogger<ApiPostService> _logger;

    public ApiPostService(ILogger<ApiPostService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executa um POST com payload JSON e retorna metadados da resposta.
    /// </summary>
    public async Task<ApiResponse> PostJsonAsync(
        string url,
        object payload,
        string? bearerToken = null,
        IDictionary<string, string>? headers = null,
        string accept = "application/json")
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL inválida", nameof(url));

        var client = new RestClient(new RestClientOptions(url));
        var request = new RestRequest("", Method.Post);
        request.AddHeader("accept", accept);
        request.AddHeader("Content-Type", "application/json");

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.AddHeader("authorization", $"Bearer {bearerToken}");
        }

        if (headers != null)
        {
            foreach (var kv in headers)
            {
                request.AddHeader(kv.Key, kv.Value);
            }
        }

        request.AddJsonBody(payload);

        try
        {
            var response = await client.ExecuteAsync(request);
            return new ApiResponse
            {
                StatusCode = (int)response.StatusCode,
                ContentType = response.ContentType,
                Content = response.Content,
                IsSuccessful = response.IsSuccessful,
                RawBytes = response.RawBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar POST para {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Executa um POST com payload JSON e desserializa o corpo para o tipo T.
    /// </summary>
    public async Task<ApiResponse<T?>> PostJsonAsync<T>(
        string url,
        object payload,
        string? bearerToken = null,
        IDictionary<string, string>? headers = null,
        string accept = "application/json",
        JsonSerializerOptions? jsonOptions = null)
    {
        var baseResp = await PostJsonAsync(url, payload, bearerToken, headers, accept);

        T? data = default;
        if (!string.IsNullOrEmpty(baseResp.Content))
        {
            try
            {
                data = JsonSerializer.Deserialize<T>(baseResp.Content, jsonOptions ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Erro ao desserializar resposta para {Type}", typeof(T).Name);
            }
        }

        return new ApiResponse<T?>
        {
            StatusCode = baseResp.StatusCode,
            ContentType = baseResp.ContentType,
            RawContent = baseResp.Content,
            Data = data,
            IsSuccessful = baseResp.IsSuccessful,
            RawBytes = baseResp.RawBytes
        };
    }
}

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? Content { get; set; }
    public bool IsSuccessful { get; set; }
    public byte[]? RawBytes { get; set; }
}

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string? ContentType { get; set; }
    public string? RawContent { get; set; }
    public T? Data { get; set; }
    public bool IsSuccessful { get; set; }
    public byte[]? RawBytes { get; set; }
}

/*
Exemplo de uso:

var service = new ApiPostService(logger);
var payload = new { CpfCnpj = cpf, idEmpreendimentoCvCrm = emp, idBlocoCvCrm = bloco, UnidadeCvCrm = unidade };

// Post simples:
var resp = await service.PostJsonAsync(cvcrmUrl, payload, bearerToken);
if (resp.IsSuccessful) { var json = resp.Content; }

// Post tipado:
var respTyped = await service.PostJsonAsync<CvCrmDadosUnidadeSP7>(cvcrmUrl, payload, bearerToken);
var dados = respTyped.Data; // poderá conter Email, ContratoSP7 etc.
*/