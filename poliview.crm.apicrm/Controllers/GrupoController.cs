using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("grupo")]
    [ApiController]
    public class GrupoController : Controller
    {
        private IGrupoService _service;

        public GrupoController(IConfiguration configuration)
        {
            _service = new GrupoService(configuration);
        }

        [HttpGet()]
        public IActionResult RetornaTodos()
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
    }   
}
