using Poliview.crm.domain;


namespace Poliview.crm.models
{
    public class ListarUnidadesResposta: IRetorno
    {
        public IEnumerable<Unidade>? objeto { get; set; }
    }

}
