using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class RelatorioCienciaArquivoResposta : IRetorno
    {
        public IEnumerable<RelatorioCienciaArquivo>? objeto { get; set; }
    }
}
