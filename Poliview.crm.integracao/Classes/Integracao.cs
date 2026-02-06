
namespace poliview.crm.integracao
{
    public class IntegracaoContext
    {
        private IIntegracao _integracao;

        public IntegracaoContext(IIntegracao integracao)
        {
            _integracao = integracao;
        }

        public void DefineStrategy(IIntegracao integracao)
        {
            _integracao = integracao;
        }

        public Boolean executarIntegracao()
        {
            return _integracao.Integrar();
        }

    }
}
