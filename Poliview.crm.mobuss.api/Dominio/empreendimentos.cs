namespace apimobuss.Dominio
{
    public class Empreendimentos
    {
        public int idempreendimento { get; set; }
        public int idempreendimentosiecon { get; set; }
        public string empreendimento { get; set; } = string.Empty;
        public string abreviatura { get; set; } = string.Empty;
        public string endereco { get; set; } = string.Empty;
        public string bairro { get; set; } = string.Empty;
        public string cidade { get; set; } = string.Empty;
        public string uf { get; set; } = string.Empty;
        public string cep { get; set; } = string.Empty;
        public int idmunicipio { get; set; }
        public string alteradoem { get; set; } = string.Empty;
        public DateTime datahora { get; set; }
    }
}
