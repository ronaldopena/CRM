using System.Text.Json.Serialization;

namespace PoliviewCRM.Admin.Services;

/// <summary>
/// Resposta de erro da API (ex.: 400 Bad Request).
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
