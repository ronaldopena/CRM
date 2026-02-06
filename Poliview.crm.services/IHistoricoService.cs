using Poliview.crm.domain;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IHistoricoService
    {
        public IEnumerable<ChamadoHistorico> Listar(HistoricoChamadosRequisicao obj);
        public Boolean Incluir(HistoricoChamadosIncluirRequisicao obj);
    }
}
