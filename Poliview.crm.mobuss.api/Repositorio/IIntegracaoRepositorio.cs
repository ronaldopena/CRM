using apimobuss.Dominio;
using apimobuss.Entidades;

namespace apimobuss.Repositorio
{
    public interface IIntegracaoRepositorio
    {
        public IntegracaoResposta integracao(DateTime dataUltimaIntegracao);
    }
}
