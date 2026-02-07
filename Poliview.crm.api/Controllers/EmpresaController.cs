using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("empresa")]
    [ApiController]
    public class EmpresaController : Controller
    {
        private IEmpresaService _service;

        public EmpresaController(IConfiguration configuration)
        {
            _service = new EmpresaService(configuration);
        }

        [HttpGet("")]
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

        [HttpPost("dominio")]
        public IActionResult ListarPorDominio(string dominio)
        {
            Console.WriteLine("url do dominio");
            Console.WriteLine(dominio);
            try
            {
                return Ok(_service.ListarPorDominio(dominio));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] Empresa obj)
        {
            try
            {
                Retorno retorno = await _service.Create(obj);
                return retorno.sucesso ? Ok(retorno) : BadRequest(retorno.mensagem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("")]
        public async Task<IActionResult> Update([FromBody] Empresa obj)
        {
            try
            {
                Retorno retorno = await _service.Update(obj);
                return retorno.sucesso ? Ok(retorno) : BadRequest(retorno.mensagem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Retorno retorno = await _service.Delete(id);
                return retorno.sucesso ? Ok(retorno) : BadRequest(retorno.mensagem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
