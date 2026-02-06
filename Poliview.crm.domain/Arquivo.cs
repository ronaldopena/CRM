namespace Poliview.crm.domain
{
    public class Arquivo
    {
        // IN_PADRAO, NM_ARQUIVO, DS_EXTENSAO, DT_CONTROLE, CD_USUARIO, IN_CHAMADO, BANCO
        public int id { get; set; }
        public DateTime data { get; set; }
        public string? arquivo { get; set; }
        public string? extensao { get; set; }
        public int idusuario { get; set; }
        public int idaviso { get; set; }
        public string? padrao { get; set; }
        public string? chamado { get; set; }
        public string? banco { get; set; }
        public byte[] conteudo { get; set; }
    }    
}
