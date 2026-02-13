namespace Poliview.crm.models;

/// <summary>
/// DTO para atualização dos parâmetros de política de senhas (OPE_PARAMETRO).
/// </summary>
public class ParametrosPoliticaSenhasRequisicao
{
    public int senhaVencimentoDias { get; set; }
    public int senhaComprimento { get; set; }
    public int senhaMinimoMaiusculo { get; set; }
    public int senhaMinimoMinusculo { get; set; }
    public int senhaMinimoNumerico { get; set; }
    public int senhaMinimoAlfanumerico { get; set; }
    public int senhaTentativasLogin { get; set; }
    public int senhaCoincidir { get; set; }
    public string? senhaPadrao { get; set; }
}
