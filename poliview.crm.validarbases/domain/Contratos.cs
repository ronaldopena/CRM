namespace poliview.crm.domain
{
    public class Contratos
    {
        public int codigocontrato { get; set; }
        public string? codigocontratosp7 { get; set; }
        public int codigocliente { get; set; }
        public string? codigoclientesp7 { get; set; }
        public string? cpfcnpj { get; set; }
        public string? nome { get; set; }
        public int empreendimento { get; set; }
        public int bloco { get; set; }
        public int unidade { get; set; }
        public int empreendimentosp7 { get; set; }
        public int blocosp7 { get; set; }
        public string? unidadesp7 { get; set; }
        public int status { get; set; }
        public int statusdistrato { get; set; }
        public int remanejado { get; set; }
        public string? dataultimaalteracao { get; set; }
        /*
            select c.ctr_cdg as codigo, c.ctr_fornecedor as codigocliente, c.ctr_emprd as empreendimentosp7, c.ctr_bloco as blocosp7, c.ctr_undemprd as unidadesp7, c.ctr_statusdistrato as statusdistrato,
            c.ctr_dtlidocrm as dataultimaalteracao, c.ctr_status as status, c.ctr_statusdistrato
            from emp_ctr c
        */
    }
}
