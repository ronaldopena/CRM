using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarMensagensPorGrupoResposta : IRetorno
    {
        public IEnumerable<GrupoMidiaMensagem>? objeto { get; set; }
    }

    public class ListarMensagensPorGrupoDetalheResposta : IRetorno
    {
        public IEnumerable<GrupoMidiaMensagemDetalhe>? objeto { get; set; }
    }
}


