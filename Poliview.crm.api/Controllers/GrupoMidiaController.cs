using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.services;

namespace poliview.crm.api.Controllers
{
    [Route("grupomidia")]
    [ApiController]
    public class GrupoMidiaController : Controller
    {
        private IGrupoMidiaService _service;

        public GrupoMidiaController(IConfiguration configuration)
        {
            _service = new GrupoMidiaService(configuration);
        }

        [HttpGet()]
        public IActionResult FindAll()
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

        [HttpGet("{idgrupomidia}")]
        public IActionResult FindById(int idgrupomidia)
        {
            try
            {
                return Ok(_service.FindById(idgrupomidia));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost()]
        public IActionResult Create(GrupoMidia obj)
        {
            try
            {
                return Ok(_service.Create(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        public IActionResult Update(GrupoMidia obj)
        {
            try
            {
                return Ok(_service.Update(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{idgrupomidia}")]
        public IActionResult Delete(int idgrupomidia)
        {
            try
            {
                return Ok(_service.Delete(idgrupomidia));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }   
}
