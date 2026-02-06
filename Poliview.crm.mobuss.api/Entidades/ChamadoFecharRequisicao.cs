namespace apimobuss.Entidades
{
    public class ChamadoFecharRequisicao
    {
        public string token { get; set; } = string.Empty;
        public int idchamado { get; set; }
        public int idocorrencia { get; set; }
        public string datahorasolucao { get; set; } = string.Empty;
        public string solucao { get; set; } = string.Empty;
    }
}
