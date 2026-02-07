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
    }
}
