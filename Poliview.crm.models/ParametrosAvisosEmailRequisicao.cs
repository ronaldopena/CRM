namespace Poliview.crm.models;

/// <summary>
/// DTO para atualização dos parâmetros de avisos por email.
/// </summary>
public class ParametrosAvisosEmailRequisicao
{
    public int? TamanhoMaximoAnexos { get; set; }
    public string? emailErrosAdmin { get; set; }
    public int? DiasLembrarPesquisaSatisfacao { get; set; }
    public int? qtdeAvisosLembrarPesquisa { get; set; }
    public int? documentoChamadoConcluido { get; set; }
}
