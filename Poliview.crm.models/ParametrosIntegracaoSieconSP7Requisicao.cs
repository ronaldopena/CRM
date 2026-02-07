namespace Poliview.crm.models;

/// <summary>
/// DTO para atualização dos parâmetros de integração SieconSP7.
/// </summary>
public class ParametrosIntegracaoSieconSP7Requisicao
{
    public string? NM_ServidorInteg { get; set; }
    public string? NM_UsuarioInteg { get; set; }
    public string? DS_SenhaUserInteg { get; set; }
    public string? DS_PathDbInteg { get; set; }
    public string? DS_portaServidorInteg { get; set; }
}
