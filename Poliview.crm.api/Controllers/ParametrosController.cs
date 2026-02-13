using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("parametros")]
    [ApiController]
    public class ParametrosController : ControllerBase
    {

        private readonly string _connectionString;
        private IConfiguration _configuration;

        public ParametrosController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }
        [HttpGet()]
        public IActionResult Config()
        {
            try
            {
                return Ok(ParametrosService.consultar(_connectionString));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Retorna o caminho completo da pasta onde a aplicação (API/Admin) está instalada.</summary>
        [HttpGet("caminho-instalacao-admin")]
        public IActionResult CaminhoInstalacaoAdmin()
        {
            try
            {
                var caminho = Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return Ok(new { caminhoInstalacaoAdmin = caminho });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("espacocliente/{cpf}")]
        public IActionResult EspacoCliente(string cpf)
        {
            try
            {
                return Ok(ParametrosService.consultarEspacoCliente(cpf, _connectionString));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("botaologin")]
        public IActionResult BotaoLogin()
        {
            try
            {
                return Ok(ParametrosService.botaoLogin(_connectionString));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // [Authorize]
        [HttpPut("integracao-sieconsp7")]
        public IActionResult AtualizarIntegracaoSieconSP7([FromBody] ParametrosIntegracaoSieconSP7Requisicao body)
        {
            try
            {
                ParametrosService.AtualizarIntegracaoSieconSP7(
                    _connectionString,
                    body.NM_ServidorInteg,
                    body.NM_UsuarioInteg,
                    body.DS_SenhaUserInteg,
                    body.DS_PathDbInteg,
                    body.DS_portaServidorInteg);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("jornadas-sla")]
        public IActionResult ListarJornadasSLA()
        {
            try
            {
                var jornadas = ParametrosService.ListarJornadasAtivasSLA(_connectionString);
                return Ok(jornadas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("jornadas-recurso")]
        public IActionResult ListarJornadasRecurso()
        {
            try
            {
                var jornadas = ParametrosService.ListarJornadasAtivasRecurso(_connectionString);
                return Ok(jornadas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //         [Authorize]
        [HttpPut("jornada")]
        public IActionResult AtualizarJornadas([FromBody] ParametrosJornadaRequisicao body)
        {
            try
            {
                ParametrosService.AtualizarJornadas(
                    _connectionString,
                    body.ID_JornadaSLA,
                    body.ID_JornadaRecurso);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //         [Authorize]
        [HttpPut("sla")]
        public IActionResult AtualizarSLA([FromBody] ParametrosSLARequisicao body)
        {
            try
            {
                ParametrosService.AtualizarSLA(
                    _connectionString,
                    body.NR_SLACritico,
                    body.NR_SLAAlerta,
                    body.horasUteisCalcSLA);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // [Authorize]
        [HttpPut("avisos-email")]
        public IActionResult AtualizarAvisosEmail([FromBody] ParametrosAvisosEmailRequisicao body)
        {
            try
            {
                ParametrosService.AtualizarAvisosEmail(
                    _connectionString,
                    body.TamanhoMaximoAnexos,
                    body.emailErrosAdmin,
                    body.DiasLembrarPesquisaSatisfacao,
                    body.qtdeAvisosLembrarPesquisa,
                    body.documentoChamadoConcluido);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("caminhos")]
        public IActionResult AtualizarCaminhos([FromBody] ParametrosCaminhosRequisicao body)
        {
            try
            {
                ParametrosService.AtualizarCaminhos(
                    _connectionString,
                    body.PastaInstalacaoCRM,
                    body.DS_PathInstallSistemaSiecon);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("espacocliente")]
        public IActionResult AtualizarEspacoCliente([FromBody] ParametrosEspacoClienteRequisicao body)
        {
            try
            {
                ParametrosService.AtualizarEspacoCliente(
                    _connectionString,
                    body.habilitarEspacoCliente,
                    body.leituraobrigatoria,
                    body.empreendimentoTesteEspacoCliente);
                return Ok(new Poliview.crm.models.Retorno { sucesso = true, mensagem = "Configuração salva com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("politica-senhas")]
        public IActionResult AtualizarPoliticaSenhas([FromBody] ParametrosPoliticaSenhasRequisicao body)
        {
            try
            {
                ParametrosService.AtualizarPoliticaSenhas(
                    _connectionString,
                    body.senhaVencimentoDias,
                    body.senhaComprimento,
                    body.senhaMinimoMaiusculo,
                    body.senhaMinimoMinusculo,
                    body.senhaMinimoNumerico,
                    body.senhaMinimoAlfanumerico,
                    body.senhaTentativasLogin,
                    body.senhaCoincidir,
                    body.senhaPadrao);
                return Ok(new Poliview.crm.models.Retorno { sucesso = true, mensagem = "Política de senhas salva com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
