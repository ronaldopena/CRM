using Poliview.crm.domain;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IServicosService
    {
        List<Servicos> ListarTodos();
        List<Servicos> ListarAtivos();
        void ExecutarAtivos();
        Task<Retorno> Create(Servicos obj);
        Task<Retorno> Update(Servicos obj);
        Task<Retorno> Delete(string nomeServico);
    }
}
