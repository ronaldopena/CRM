namespace PoliviewCrm.CvCrm.Models
{
    enum PDFTipoRelatorio
    {
        Boleto = 1,
        InformeRendimentos = 2,
        FichaFinanceira = 3
    }

    public class PDFDto
    {
        public int idempreendimento { get; set; } = 0;
        public int idbloco { get; set; } = 0;
        public int idunidade { get; set; } = 0;
        public string? idclientesp7 { get; set; } = "";

    }

    public class GerarPDFDto
    {
        public int TipoRelatorio { get; set; } = (int)PDFTipoRelatorio.Boleto;
        public string? ArquivoPDF { get; set; } = "";
        public int Cobranca { get; set; } = 0;
        public string? Contrato { get; set; } = "";
        public int Ano { get; set; } = 2025;
        public int Recebimento { get; set; }
    }
}