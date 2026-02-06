using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [EnableCors()]
    [Route("configcrm")]
    [ApiController]
    public class ConfigCrmController : ControllerBase
    {
        private IConfigCrmService _service;

        public ConfigCrmController(IConfiguration configuration)
        {
            _service = new ConfigCrmService(configuration);
        }

        [HttpGet()]
        public IActionResult Config()
        {
            try
            {
                return Ok(_service.getConfigCrm());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("menus/{idempreendimento}")]
        public IActionResult Menus(int idempreendimento)
        {
            try
            {
                return Ok(_service.getMenus(idempreendimento));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
