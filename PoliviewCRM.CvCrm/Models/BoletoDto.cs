namespace PoliviewCrm.CvCrm.Models
{
    public class BoletoDto
    {
        public string BOLETODTVENC { get; set; } = string.Empty;
        public decimal BOLETOVALOR { get; set; } = 0;
        public int RECTO { get; set; } = 0;
        public int COBRANCA { get; set; } = 0;
        public string CONTRATO { get; set; } = string.Empty;
        public string LINHADIGITAVEL { get; set; } = string.Empty;

    }

    public class BoletoRequisicao
    {
        public int EmpreendimentoSP7 { get; set; } = 0;
        public int BlocoSP7 { get; set; } = 0;
        public string UnidadeSP7 { get; set; } = string.Empty;
        public string CodigoContratoSP7 { get; set; } = string.Empty;
        public string CodigoClienteSP7 { get; set; } = string.Empty;
    }
}