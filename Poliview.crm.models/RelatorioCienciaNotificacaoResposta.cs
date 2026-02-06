using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class RelatorioCienciaNotificacaoResposta : IRetorno
    {
        public IEnumerable<RelatorioCienciaNotificacao>? objeto { get; set; }
    }
}
