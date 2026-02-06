using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ArquivoResposta : IRetorno
    {
        public ArquivoDownload? objeto { get; set; }
    }

    public class ArquivoEmpreendimentosResposta: IRetorno
    {
        public IEnumerable<ArquivoEmpreendimentos>? objeto { get; set; }
    }
}
