using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarGrupoMidiaResposta: IRetorno
    {
        public IEnumerable<GrupoMidia>? objeto { get; set; }
    }
}
