namespace Poliview.crm.models;

/// <summary>
/// DTO para atualização dos parâmetros do Espaço Cliente (OPE_PARAMETRO).
/// </summary>
public class ParametrosEspacoClienteRequisicao
{
    public int habilitarEspacoCliente { get; set; }
    public int leituraobrigatoria { get; set; }
    public int empreendimentoTesteEspacoCliente { get; set; }
}
