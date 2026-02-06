namespace Poliview.crm.domain
{
    public class ClientesIntegracao
    {
        public string? cpfcnpj { get; set; }
        public string? codigoclientesp7 { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public string? endereco { get; set; }
        public string? bairro { get; set; }
        public string? cidade { get; set; }
        public string? estado { get; set; }
        public string? cep { get; set; }
        public string? ddd { get; set; }
        public string? telefone { get; set; }
        public string? celular { get; set; }
        public string? contato { get; set; }
        public DateTime datanascimento { get; set; }
        public DateTime datahoraultimaatualizacao { get; set; }
    }

    public class clientesConsulta
    {
        public string? email { get; set; }
        public string? ddd { get; set; }
        public string? telefone { get; set; }
        public string? celular { get; set; }
    }
}
