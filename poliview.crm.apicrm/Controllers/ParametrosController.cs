using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Cms;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
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
                return Ok(ParametrosService.consultarEspacoCliente(cpf,_connectionString));
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
    }
}
