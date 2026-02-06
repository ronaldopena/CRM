namespace apimobuss.Entidades
{
    public class ChamadoIncluirRequisicao
    {
        public string token { get; set; } = string.Empty;
        public int idcliente { get; set; }
        public int idunidade { get; set; }
        public string descricaochamado { get; set; } = string.Empty;
    }
}
