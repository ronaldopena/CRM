using Poliview.crm.domain;

namespace Poliview.crm.models
{
    public class ListarEmpreendimentosResposta: IRetorno
    {
        public IEnumerable<Empreendimento>? objeto { get; set; }
    }

}
