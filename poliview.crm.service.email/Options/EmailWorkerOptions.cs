namespace poliview.crm.service.email.Options;

/// <summary>
/// Opções de configuração do worker de e-mail (cliente, flags de log, títulos).
/// Evita campos estáticos e leitura direta de IConfiguration nos serviços.
/// </summary>
public class EmailWorkerOptions
{
    public const string SectionName = "EmailWorker";

    public string Cliente { get; set; } = "não identificado";
    public bool VerQuery { get; set; } = true;
    public bool VerDebug { get; set; } = true;
    public bool VerErros { get; set; } = true;
    public bool VerInfos { get; set; } = true;
    public string TituloMensagemEnvio { get; set; } = "Envio de E-mail";
    public string TituloMensagemRecebimento { get; set; } = "Recebimento de E-mail";
}
