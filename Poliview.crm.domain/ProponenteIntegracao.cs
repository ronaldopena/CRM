namespace Poliview.crm.domain
{
    public class ProponenteIntegracao
    {
        public int codigoproponente { get; set; }
        public string contratosp7 { get; set; }
        public string codigoclientesp7 { get; set; }
        public int statusintegracao { get; set; }
        public string datacontrole { get; set; }
        public string horacontrole { get; set; }
        public string principal { get; set; }
        public DateTime? datacessao { get; set; }
        public int codigocessao { get; set; }
        public string clienteatual { get; set; }
        public string clientenovo { get; set; }
        public int statuscessao { get; set; }
        public string ativo { get; set; }
        public int bancodados { get; set; } = 1;
        public int mandante { get; set; } = 1;
        public DateTime datahoraultimaatualizacao { get; set; }
    }
}
