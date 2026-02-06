using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarTipoUnidadeResposta: IRetorno
    {
        public IEnumerable<TipoUnidade>? objeto { get; set; }
    }

}
