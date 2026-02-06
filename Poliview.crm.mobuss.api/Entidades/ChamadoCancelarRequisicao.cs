namespace apimobuss.Entidades
{
    public class ChamadoCancelarRequisicao
    {
        public string token { get; set; } = string.Empty;
        public int idchamado { get; set; }
        public int idocorrencia { get; set; }
        public string datahoracancelamento { get; set; } = string.Empty;
        public string motivo { get; set; } = string.Empty;
    }
}
