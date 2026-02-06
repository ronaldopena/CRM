using apimobuss.Entidades;
using apimobuss.Dominio;

namespace apimobuss.Repositorio
{
    public interface ILogRepositorio
    {
        public void incluir(int chamado, string mensgem, tpLog tipo);
    }
}
