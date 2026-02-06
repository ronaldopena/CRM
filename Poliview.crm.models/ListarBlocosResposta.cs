using Poliview.crm.domain;


namespace Poliview.crm.models
{
    public class ListarBlocosResposta: IRetorno
    {
        public IEnumerable<Bloco>? objeto { get; set; }
    }

}
