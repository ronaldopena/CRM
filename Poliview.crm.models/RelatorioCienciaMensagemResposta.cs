using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class RelatorioCienciaMensagemResposta : IRetorno
    {
        public IEnumerable<RelatorioCienciaMensagem>? objeto { get; set; }
    }
}
