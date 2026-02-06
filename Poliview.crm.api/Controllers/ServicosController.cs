using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("servicos")]
    [ApiController]
    public class ServicosController : ControllerBase
    {
        private IServicosService servicosService;
        public ServicosController(IConfiguration configuration)
        {
            servicosService = new ServicosService(configuration);
        }
        [HttpGet("")]
        [Authorize]
        public IActionResult listarServicos()
        {
            try
            {
                return Ok(servicosService.ListarTodos());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpGet("ativos")]
        public IActionResult listarServicosAtivos()
        {
            try
            {
                return Ok(servicosService.ListarAtivos());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("executar")]
        public IActionResult executarServicosAtivos()
        {
            try
            {
                servicosService.ExecutarAtivos();
                return Ok("Serviços Executados");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
