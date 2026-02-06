using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarArquivosResposta : IRetorno
    {
        public IEnumerable<ArquivoDownload>? objeto { get; set; }
    }
}


