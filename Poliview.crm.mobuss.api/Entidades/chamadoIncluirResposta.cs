namespace apimobuss.Entidades
{
    public class ChamadoIncluirResposta
    {
        public string? mensagem { get; set; }
        public int sucesso { get; set; }
        public NovoChamado? objeto { get; set; }
    }

    public class NovoChamado
    {
        public int idnovochamado { get; set; }
    }
}
