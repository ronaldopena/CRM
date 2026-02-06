using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarGruposResposta: IRetorno
    {
        public IEnumerable<Grupo>? objeto { get; set; }
    }
}
