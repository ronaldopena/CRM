using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("autenticacao")]
    [ApiController]
    public class AutenticacaoController : Controller
    {
        
        private IAutenticacaoService _service;
        private IConfiguration _configuration;

        public AutenticacaoController(IConfiguration configuration)
        {
            _service = new AutenticacaoService(configuration);
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult login(LoginRequisicao obj)
        {
            Console.WriteLine($"usuario: {obj.usuario}");
            Console.WriteLine($"senha: {obj.senha}");
            Console.WriteLine($"origem: {obj.origem}");

            var jwt = new Jwt();
            jwt.Subject = "baseWebApiSubject";
            jwt.Issuer = "basewebApiIssuer";
            jwt.Audience = "baseWebApiAudience";
            jwt.key = "**poliview.tecnologia.crm@2022**";
            return Ok(_service.Login(obj, jwt));
        }

    }
}
