using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class MensagemResposta : IRetorno
    {
        public Mensagem? objeto { get; set; }
    }

    public class MensagemEmpreendimentosResposta: IRetorno
    {
        public IEnumerable<MensagemEmpreendimentos>? objeto { get; set; }
    }
}
