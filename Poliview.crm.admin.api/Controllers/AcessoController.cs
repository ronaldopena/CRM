using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text;

namespace Poliview.crm.admin.api.Controllers
{
    // [Route("acesso")]    
    public class AcessoController : Controller
    {
        private readonly string _connectionString = string.Empty;
        private IConfiguration _configuration;

        public AcessoController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"] ?? string.Empty;
        }

        [HttpPost()]
        public IActionResult AcessoPost(string chaveacesso)
        {
            // var retorno = new Retorno();
            try
            {
                var ret = Listar(chaveacesso);
                return Ok(ret);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{chaveacessoBase64}")]
        public IActionResult RetornaAcessoGet(string chaveacessoBase64)
        {
            var retorno = new Retorno();

            try
            {
                byte[] bytes = Convert.FromBase64String(chaveacessoBase64);
                string chaveacesso = Encoding.UTF8.GetString(bytes);
                Console.WriteLine("Chave acesso");

                var ret = Listar(chaveacesso);
                if (ret.chaveacesso == chaveacesso)
                {
                    retorno.mensagem = "Ok";
                    retorno.sucesso = 1;
                    retorno.objeto = ret;
                }
                else
                {
                    retorno.mensagem = "chave de acesso não encontrada";
                    retorno.sucesso = 0;
                    retorno.objeto = ret;
                }
                return Ok(retorno);
                // return Ok(ret);
            }
            catch (Exception ex)
            {

                retorno.mensagem = ex.Message;
                retorno.sucesso = 0;
                retorno.objeto = null;
                //return BadRequest(retorno);
                //return BadRequest(ex.Message);
                return Ok(retorno);
            }
        }

        public Acesso Listar(string chaveacesso)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select * from OPE_CONFIG where chaveacesso='{chaveacesso}'";
            Console.WriteLine(query);
            return connection.QueryFirst<Acesso>(query);
        }

        public class Acesso
        {
            public string? urlapi { get; set; }
            public string? chaveacesso { get; set; }
            public string? principal { get; set; }
            public string? dark { get; set; }
            public string? light { get; set; }
            public string? texto { get; set; }
            public string? fundo { get; set; }
            public string? urllogo { get; set; }
            public string? urlportal { get; set; }
            public string? urlapisiecon { get; set; }
            public string? urlimgprincipal { get; set; }
            public int idempresa { get; set; }

        }

        public class Retorno
        {
            public int sucesso { get; set; }
            public string? mensagem { get; set; }
            public object? objeto { get; set; }
        }

    }

}
