using Poliview.crm.domain;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IChamadoService
    {
        public IEnumerable<Chamado> ListarChamados(ListarChamadosRequisicao obj);
        public IEnumerable<ChamadoOcorrencias> ListarOcorrenciasChamado(int idchamado);
        public Chamado ListarChamado(int idchamado);
        public IEnumerable<OcorrenciasAberturaChamado> ListarOcorrenciasParaAberturaChamado();
        public AberturaChamadoResposta SalvarChamado(SalvarChamadoRequisicao obj);
        public IEnumerable<ChamadoHistorico> ListaHistoricoChamados(HistoricoChamadosRequisicao obj);
        public IEnumerable<ChamadoHistorioEmail> ListarHistoricoEmails(HistoricoEmailsRequisicao obj);
        public IEnumerable<ChamadoAnexos> ListarAnexosChamado(AnexosChamadoRequisicao obj);
        public SalvarAnexosChamadoResposta SalvarAnexosChamado(SalvarAnexosChamadoRequisicao obj);
        public Boolean LerMensagensChamado(int idchamado);
    }
}
