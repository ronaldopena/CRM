using apimobuss.Entidades;
using apimobuss.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apimobuss.Dominio;

namespace apimobuss.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class chamadoController : ControllerBase
    {
        private string _connectionString;

        public chamadoController(IConfiguration config)
        {
            _connectionString = new Conexao(config).StringConexao;
        }

        [HttpPost("incluir")]
        public ChamadoIncluirResposta incluir(ChamadoIncluirRequisicao dadoschamado)
        {
            var ret = new ChamadoIncluirResposta();
            var log = new LogRepositorio(_connectionString);
            var msg = "";

            var msgRequisicao = $"requisição de Inclusão de chamado: cliente: {dadoschamado.idcliente} idunidade: {dadoschamado.idunidade} descrição chamado: {dadoschamado.descricaochamado} ";
            log.incluir(0, msgRequisicao, tpLog.debug);

            try
            {
                var repositorio = new ChamadoRepositorio(_connectionString);

                if (!repositorio.validarProponentePrincipal(dadoschamado.idunidade,
                                                             dadoschamado.idcliente))
                {
                    msg = "O proponente informado não pertence ao " + repositorio.retornaDadosUnidade(dadoschamado.idunidade);

                    msg = msg + $" | ({dadoschamado.idunidade}:{dadoschamado.idcliente})";

                    log.incluir(0, msg, tpLog.erro);

                    ret.mensagem = msg;
                    ret.objeto = null;
                    ret.sucesso = 0;
                }
                else
                {
                    if (repositorio.TokenValido(dadoschamado.token))
                    {
                        ret.mensagem = "Ok";
                        ret.objeto = repositorio.incluir(dadoschamado);
                        ret.sucesso = 1;

                        msg = $"Inclusão chamado mobuss: chamado: {ret.objeto.idnovochamado} cliente: {dadoschamado.idcliente} idunidade: {dadoschamado.idunidade} descrição chamado: {dadoschamado.descricaochamado} ";
                        log.incluir(0, msg, tpLog.debug);

                    }
                    else
                    {
                        msg = dadoschamado.token == "" ? "Não foi passado o [TOKEN] para a API" : "[TOKEN] inválido!";
                        log.incluir(0, msg, tpLog.erro);
                        ret.mensagem = msg;
                        ret.objeto = null;
                        ret.sucesso = 0;
                    }
                }
            }
            catch (Exception e)
            {
                log.incluir(0, e.Message, tpLog.erro);
                ret.mensagem = e.Message;
                ret.objeto = null;
                ret.sucesso = 0;
            }

            return ret;
        }

        [HttpPost("fechar")]
        public Retorno fechar(ChamadoFecharRequisicao dadoschamado)
        {

            var ret = new Retorno();
            var log = new LogRepositorio(_connectionString);
            var msg = "";

            var msgRequisicao = $"requisição de fechamento de chamado: chamado: {dadoschamado.idchamado} ocorrencia: {dadoschamado.idocorrencia} data solução: {dadoschamado.datahorasolucao} Solução: {dadoschamado.solucao} ";
            log.incluir(dadoschamado.idchamado, msgRequisicao, tpLog.debug);

            try
            {
                var repositorio = new ChamadoRepositorio(_connectionString);
                if (repositorio.TokenValido(dadoschamado.token))
                {
                    msg = string.Format("chamado {0} concluído com sucesso!", dadoschamado.idchamado);
                    log.incluir(dadoschamado.idchamado, msg, tpLog.informacao);
                    ret.mensagem = msg;
                    ret.objeto = repositorio.fechar(dadoschamado);
                    ret.sucesso = 1;
                }
                else
                {
                    msg = dadoschamado.token == "" ? "Não foi passado o [TOKEN] para a API" : "[TOKEN] inválido!";
                    log.incluir(dadoschamado.idchamado, msg, tpLog.erro);
                    ret.mensagem = msg;
                    ret.objeto = null;
                    ret.sucesso = 0;
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                log.incluir(dadoschamado.idchamado, msg, tpLog.erro);
                ret.mensagem = msg;
                ret.objeto = null;
                ret.sucesso = 0;
            }

            return ret;
        }

        [HttpPost("cancelar")]
        public Retorno cancelar(ChamadoCancelarRequisicao dadoschamado)
        {

            var ret = new Retorno();
            var log = new LogRepositorio(_connectionString);
            var msg = "";

            var msgRequisicao = $"requisição de cancelamento de chamado: chamado: {dadoschamado.idchamado} ocorrencia: {dadoschamado.idocorrencia} data cancelamento: {dadoschamado.datahoracancelamento} motivo: {dadoschamado.motivo} ";
            log.incluir(dadoschamado.idchamado, msgRequisicao, tpLog.debug);

            try
            {
                var repositorio = new ChamadoRepositorio(_connectionString);
                if (repositorio.TokenValido(dadoschamado.token))
                {
                    msg = string.Format("chamado {0} cancelado com sucesso!", dadoschamado.idchamado);
                    log.incluir(dadoschamado.idchamado, msg, tpLog.informacao);
                    ret.mensagem = msg;
                    ret.objeto = repositorio.cancelar(dadoschamado);
                    ret.sucesso = 1;
                }
                else
                {
                    msg = dadoschamado.token == "" ? "Não foi passado o [TOKEN] para a API" : "[TOKEN] inválido!";
                    log.incluir(dadoschamado.idchamado, msg, tpLog.erro);
                    ret.mensagem = msg;
                    ret.objeto = null;
                    ret.sucesso = 0;
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                log.incluir(dadoschamado.idchamado, msg, tpLog.erro);
                ret.mensagem = msg;
                ret.objeto = null;
                ret.sucesso = 0;
            }

            return ret;
        }

    }
}
