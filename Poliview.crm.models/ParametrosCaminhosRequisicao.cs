namespace Poliview.crm.models;

/// <summary>
/// DTO para atualização dos parâmetros de caminhos (OPE_PARAMETRO).
/// </summary>
public class ParametrosCaminhosRequisicao
{
    /// <summary>Pasta de instalação do CRM (PastaInstalacaoCRM).</summary>
    public string? PastaInstalacaoCRM { get; set; }

    /// <summary>Pasta de instalação do SIECON (DS_PathInstallSistemaSiecon).</summary>
    public string? DS_PathInstallSistemaSiecon { get; set; }
}
