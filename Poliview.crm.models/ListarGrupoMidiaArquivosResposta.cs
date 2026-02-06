using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarGrupoMidiaArquivosResposta: IRetorno
    {
        public IEnumerable<GrupoMidiaArquivo>? objeto { get; set; }
    }

    public class ListarGrupoMidiaArquivosDetalheResposta : IRetorno
    {
        public IEnumerable<GrupoMidiaArquivoDetalhe>? objeto { get; set; }
    }
}
