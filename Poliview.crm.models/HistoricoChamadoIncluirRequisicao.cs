namespace Poliview.crm.models
{
    public class HistoricoChamadosIncluirRequisicao
    {
        public int idchamado { get; set; }
        public int idocorrencia { get; set; }
        public string? descricao { get; set; }

        public int idusuario { get; set; }
        
        public int visibilidade { get; set; }

        public HistoricoChamadosIncluirRequisicao()
        {
            visibilidade = 1;
        }
    }
}