using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListaOcorrenciasAberturaChamadoResposta: IRetorno
    {
        public IEnumerable<OcorrenciasAberturaChamado>? objeto { get; set; }
    }
}
