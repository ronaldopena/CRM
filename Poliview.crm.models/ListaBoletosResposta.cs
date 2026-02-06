namespace Poliview.crm.models
{
    public class BoletoAPIResposta
    {
        public int Cobranca { get; set; }
        public int ItemCobranca { get; set; }
        public int CodigoParcela { get; set; }
        public decimal Valor { get; set; }
        public string? DtVenc { get; set; }
        public string? CnpjCpf { get; set; }
        public string? CodigoContrato { get; set; }
        public string? LinhaDigitavel { get; set; }
    }

    public class DetalheBoletoAPIResposta
    {
        public int Cobranca { get; set; }
        public int ItemCobranca { get; set; }
        public int CodigoParcela { get; set; }
        public decimal Valor { get; set; }
        public string? ArqBoleto { get; set; }
        public string? LinhaDigitavel { get; set; }        
    }

}
