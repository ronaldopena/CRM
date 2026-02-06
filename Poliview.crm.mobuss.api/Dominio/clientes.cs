namespace apimobuss.Dominio
{
    public class Clientes
    {
        public int idcliente { get; set; }
        public string cliente { get; set; } = string.Empty;
        public string cpfcnpj { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string endereco { get; set; } = string.Empty;
        public string bairro { get; set; } = string.Empty;
        public string cidade { get; set; } = string.Empty;
        public string uf { get; set; } = string.Empty;
        public string cep { get; set; } = string.Empty;
        public string ddd { get; set; } = string.Empty;
        public string telefone { get; set; } = string.Empty;
        public string celular { get; set; } = string.Empty;
        public string alteradoem { get; set; } = string.Empty;
        public DateTime datahora { get; set; }
    }
}
