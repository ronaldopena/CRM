using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("chamado")]
    [ApiController]
    public class ChamadoController : ControllerBase
    {
        private IChamadoService _chamadoService;

        public ChamadoController(IConfiguration configuration)
        {
            _chamadoService = new ChamadoService(configuration);
            Console.WriteLine("ChamadoController");
        }

        // erro
        [HttpGet("{idchamado}")]
        public IActionResult Chamado(int idchamado)
        {
            try
            {
                return Ok(_chamadoService.ListarChamado(idchamado));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }


        [HttpPost()]
        public IActionResult Cadastrar(SalvarChamadoRequisicao obj)
        {
            Console.WriteLine("Cadastrar " + obj.anexos);
            try
            {
                return Ok(_chamadoService.SalvarChamado(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ok
        [HttpPost("listarocorrenciasaberturachamado")]
        public IActionResult ListarOcorrenciaAberturaChamado()
        {
            try
            {
                return Ok(_chamadoService.ListarOcorrenciasParaAberturaChamado());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ocorrencia")]
        public IActionResult ListarOcorrenciaAberturaChamado2()
        {
            try
            {
                return Ok(_chamadoService.ListarOcorrenciasParaAberturaChamado());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ok
        [HttpPost("listar")]
        public IActionResult Listar(ListarChamadosRequisicao obj)
        {
            try
            {
                return Ok(_chamadoService.ListarChamados(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ok
        [HttpPost("ocorrencias")]
        public IActionResult ListarOcorrencias(ListarOcorrenciasRequisicao obj)
        {
            try
            {
                return Ok(_chamadoService.ListarOcorrenciasChamado(obj.idchamado));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("salvar")]
        public IActionResult Salvar(SalvarChamadoRequisicao obj)
        {
            Console.WriteLine("Salvar");
            Console.WriteLine(obj);

            try
            {
                return Ok(_chamadoService.SalvarChamado(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("historico")]
        public IActionResult Historico(HistoricoChamadosRequisicao obj)
        {
            try
            {
                return Ok(_chamadoService.ListaHistoricoChamados(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("historicoemail")]
        public IActionResult HistoricoEmail(HistoricoEmailsRequisicao obj)
        {
            try
            {
                return Ok(_chamadoService.ListarHistoricoEmails(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("anexos")]
        public IActionResult AnexoChamado(AnexosChamadoRequisicao obj)
        {
            try
            {
                return Ok(_chamadoService.ListarAnexosChamado(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("salvaranexos")]
        public IActionResult SalvarAnexos(SalvarAnexosChamadoRequisicao obj)
        {
            try
            {
                return Ok(_chamadoService.SalvarAnexosChamado(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("classificacao/{idchamado}")]
        public IActionResult ClassificacaoOcorrencia(int idchamado)
        {
            try
            {
                return Ok(_chamadoService.ListarChamado(idchamado));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
