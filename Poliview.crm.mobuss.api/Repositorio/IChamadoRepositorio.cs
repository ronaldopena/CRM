using apimobuss.Entidades;

namespace apimobuss.Repositorio
{
    public interface IChamadoRepositorio
    {
        public NovoChamado incluir(ChamadoIncluirRequisicao dados);
        public object fechar(ChamadoFecharRequisicao dados);
    }
}
