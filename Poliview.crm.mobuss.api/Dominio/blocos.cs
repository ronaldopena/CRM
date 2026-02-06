namespace apimobuss.Dominio
{
    public class Blocos
    {
        public int idbloco { get; set; }
        public int idblocosiecon { get; set; }
        public int idempreendimento { get; set; }
        public int idempreendimentosiecon { get; set; }
        public string bloco { get; set; } = string.Empty;
        public string abreviatura { get; set; } = string.Empty;
        public string alteradoem { get; set; } = string.Empty;
        public DateTime datahora { get; set; }
    }
}
