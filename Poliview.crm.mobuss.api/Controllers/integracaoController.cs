using apimobuss.Entidades;
using apimobuss.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apimobuss.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class integracaoController : ControllerBase
    {
        private string _connectionString;

        public integracaoController(IConfiguration config)
        {
            _connectionString = new Conexao(config).StringConexao;
        }


        [HttpPost()]
        public RetornoIntegracao integracao(IntegracaoRequisicao dados)
        {
            var ret = new RetornoIntegracao();
            var log = new LogRepositorio(_connectionString);

            DateTime data = convertData(dados.dataultimaintegracao);
            log.incluir(0, "solicitação de integração de dados. Data ultima integração: " + dados.dataultimaintegracao, tpLog.informacao);

            try
            {
                var repositorio = new IntegracaoRepositorio(_connectionString);
                if (repositorio.TokenValido(dados.token))
                {
                    ret.mensagem = "Ok";
                    ret.objeto = repositorio.integracao(data);
                    ret.sucesso = 1;
                }
                else
                {
                    ret.mensagem = dados.token == "" ? "Não foi passado o [TOKEN] para a API" : "[TOKEN] inválido!";
                    ret.objeto = new IntegracaoResposta();
                    ret.sucesso = 0;
                }
            }
            catch (Exception e)
            {
                ret.mensagem = e.Message;
                ret.objeto = new IntegracaoResposta();
                ret.sucesso = 0;
            }

            return ret;
        }

        private DateTime convertData(string data)
        {
            var dataaux = data.Substring(6, 4) + "-" +
                          data.Substring(3, 2) + "-" +
                          data.Substring(0, 2) + " " +
                          data.Substring(11, 4);

            return Convert.ToDateTime(dataaux);
        }

    }
}
