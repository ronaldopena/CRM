using Microsoft.AspNetCore.Mvc;

namespace Poliview.crm.apicrm.Controllers
{

    [Route("emailserver")]
    [ApiController]
    public class EmailServerController : Controller
    {
        public EmailServerController(IConfiguration configuration)
        {
        }

        [HttpPost("autorizacao")]
        public IActionResult ReceberAutorizacao()
        {

            return Ok();
        }

    }
}