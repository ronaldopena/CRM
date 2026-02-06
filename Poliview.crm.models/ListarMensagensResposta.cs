using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarMensagensResposta : IRetorno
    {
        public IEnumerable<Mensagem>? objeto { get; set; }
    }
}


