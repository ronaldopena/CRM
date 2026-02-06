using apimobuss.Entidades;
using apimobuss.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace apimobuss.Controllers
{
    [Route("config")]
    [ApiController]
    public class testeController : ControllerBase
    {
        private string _connectionString;

        public testeController(IConfiguration config)
        {
            _connectionString = new Conexao(config).StringConexao;
        }

        [HttpGet()]
        public Object testar()
        {
            var repositorio = new TesteRepositorio(_connectionString);
            return repositorio.testar();
        }
    }
}
