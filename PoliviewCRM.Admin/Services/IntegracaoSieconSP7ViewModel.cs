using System.Text.Json.Serialization;

namespace PoliviewCRM.Admin.Services;

/// <summary>
/// ViewModel para a tela Integração SieconSP7. Nomes das propriedades alinhados ao JSON da API (camelCase).
/// </summary>
public class IntegracaoSieconSP7ViewModel
{
    [JsonPropertyName("nM_ServidorInteg")]
    public string? Servidor { get; set; }

    [JsonPropertyName("nM_UsuarioInteg")]
    public string? Usuario { get; set; }

    [JsonPropertyName("dS_SenhaUserInteg")]
    public string? Senha { get; set; }

    [JsonPropertyName("dS_PathDbInteg")]
    public string? CaminhoServidor { get; set; }

    [JsonPropertyName("dS_portaServidorInteg")]
    public string? PortaServidor { get; set; }
}
