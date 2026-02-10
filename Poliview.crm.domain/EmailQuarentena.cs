namespace Poliview.crm.domain;

public class EmailQuarentena
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Remetente { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int? IdContaEmail { get; set; }
}

