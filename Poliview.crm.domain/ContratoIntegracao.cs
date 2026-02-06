namespace Poliview.crm.domain
{
    public class ContratoIntegracao
    {
        public int codigocontrato { get; set; }
        public string contratosp7 { get; set; }
        public string codigoclientesp7 { get; set; }
        public int codigoempresasp7 { get; set; }
        public int codigoempreendimentosp7 { get; set; }
        public int codigoblocosp7 { get; set; }
        public string codigounidadesp7 { get; set; }
        public int statuscontratosp7 { get; set; }
        public int statusdistrato { get; set; }
        public int statusremanejado { get; set; }
        public int statusintegracao { get; set; }
        public string cpfresponsavel { get; set; }
        public string nomeresponsavel { get; set; }
        public string telefoneresponsavel { get; set; }
        public string celularresponsavel { get; set; }
        public string emailresponsavel { get; set; }
        public DateTime datavendaresponsavel { get; set; }
        public DateTime datahoraultimaatualizacao { get; set; }
    }
}
