namespace Poliview.crm.models;

/// <summary>
/// DTO para atualização dos parâmetros de SLA.
/// </summary>
public class ParametrosSLARequisicao
{
    public int? NR_SLACritico { get; set; }
    public int? NR_SLAAlerta { get; set; }
    public bool horasUteisCalcSLA { get; set; }
}
