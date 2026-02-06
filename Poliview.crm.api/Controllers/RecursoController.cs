using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("recurso")]
    [ApiController]
    public class RecursoController : Controller
    {
        private IRecursoService _service;

        public RecursoController(IConfiguration configuration)
        {
            _service = new RecursoService(configuration);
        }

        [HttpGet()]
        public IActionResult RetornaTodos()
        {
            try
            {
                return Ok(_service.ListAll());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idgrupo}")]
        public IActionResult RetornaPorId(int idgrupo)
        {
            try
            {
                return Ok(_service.GetByIdGrupo(idgrupo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }   
}
