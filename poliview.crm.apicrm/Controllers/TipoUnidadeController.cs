
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("tipounidade")]
    [ApiController]
    public class TipoUnidadeController : Controller
    {
        private ITipoUnidadeService _service;

        public TipoUnidadeController(IConfiguration configuration)
        {
            _service = new TipoUnidadeService(configuration);
        }

        [HttpGet()]
        public IActionResult Listar()
        {
            try
            {
                return Ok(_service.Listar());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult ListarPorId(int id)
        {
            try
            {
                return Ok(_service.ListaPorId(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        public IActionResult Update(TipoUnidade obj)
        {
            try
            {                
                Console.WriteLine("alterar Tipo Unidade");
                Console.WriteLine($" id={obj.id} descricao={obj.descricao} espacocliente={obj.espacocliente}");
                return Ok(_service.Update(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
