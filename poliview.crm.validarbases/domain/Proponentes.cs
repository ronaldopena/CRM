namespace poliview.crm.domain
{
    public class ProponentesSiecon
    {
        public int codigo { get; set; }
        public string? contratosp7 { get; set; }
        public string? codigoclientesp7 { get; set; }
        public float percentual { get; set; }
        public int cessao { get; set; }
        public DateTime dataultimaalteracao { get; set; }
    }
    // select PROPONENTE_CDG as codigo, PROPONENTE_CTR as contrato, PROPONENTE_FORNECEDOR as codigocliente, PROPONENTE_PERCENTUAL as percentual, PROPONENTE_CESSAO as cessao, PROPONENTE_DTLIDOCRM as dataultimaalteracao from EMP_PROPONENTE

    public class ProponentesCrm
    {
        public int codigo { get; set; }
        public string? contratosp7 { get; set; }
        public string? codigoclientesp7 { get; set; }
        public int cessao { get; set; }        
    }


}
