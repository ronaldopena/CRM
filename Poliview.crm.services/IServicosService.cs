using Poliview.crm.domain;

namespace Poliview.crm.services
{
    public interface IServicosService
    {
        public List<Servicos> ListarTodos();
        public List<Servicos> ListarAtivos();
        public void ExecutarAtivos();
    }
}
