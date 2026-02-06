using Poliview.crm.domain;


namespace Poliview.crm.models
{
    public class ListarEmpresasResposta: IRetorno
    {
        public IEnumerable<Empresa>? objeto { get; set; }
    }

    public class ListarEmpresaResposta : IRetorno
    {
        public Empresa? objeto { get; set; }
    }

}
