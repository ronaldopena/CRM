namespace apimobuss.Dominio
{
    public class Log
    {
        public int id { get; set; }
        public DateTime data { get; set; }
        public int chamado { get; set; }
        public int origem { get; set; }
        public int tipo { get; set; }
        public string? mensagem { get; set; }
    }
}
