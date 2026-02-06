namespace apimobuss.Dominio
{
    public sealed class Chamado
    {
        public long idcliente { get; set; }
        public long idunidade { get; set; }
        public string descricaochamado { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
    }
}
