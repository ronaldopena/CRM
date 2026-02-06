using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarRecursosResposta: IRetorno
    {
        public IEnumerable<Recurso>? objeto { get; set; }
    }
}
