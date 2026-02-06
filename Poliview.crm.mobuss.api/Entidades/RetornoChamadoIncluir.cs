namespace apimobuss.Entidades
{
    public class RetornoChamadoIncluir
    {
        public string mensagem { get; set; } = string.Empty;
        public int sucesso { get; set; }
        public Object? objeto { get; set; }
    }
}
