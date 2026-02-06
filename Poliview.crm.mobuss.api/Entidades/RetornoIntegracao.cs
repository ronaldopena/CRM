namespace apimobuss.Entidades
{
    public class RetornoIntegracao
    {
        public string mensagem { get; set; } = string.Empty;
        public int sucesso { get; set; }
        public IntegracaoResposta objeto { get; set; } = new IntegracaoResposta();
    }
}
