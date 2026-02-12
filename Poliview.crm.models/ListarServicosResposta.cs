using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarServicosResposta : IRetorno
    {
        public IEnumerable<Servicos>? objeto { get; set; }
    }
}
