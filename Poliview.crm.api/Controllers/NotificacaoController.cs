using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using Poliview.crm.services;

namespace poliview.crm.api.Controllers
{
    [Route("notificacao")]
    [ApiController]
    public class NotificacaoController : Controller
    {
        private INotificacaoService _service;
        private IConfiguration _configuration;
        private string _connectionstring;
        private class RetornaRelatorio
        {
            public string? urlDownload { get; set; }
            public string? nomeArquivo { get; set; }
        }

        public NotificacaoController(IConfiguration configuration)
        {
            _service = new NotificacaoService(configuration);
            _configuration = configuration;
            _connectionstring = configuration["conexao"];
        }

        [HttpGet("{id}")]
        public IActionResult Listar(string id)
        {            
            try
            {
                return Ok(_service.Listar(id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        [HttpGet("cpf/{cpf}")]
        public IActionResult ListarNotificacoes(string cpf)
        {
            try
            {
                return Ok(_service.ListarNotificacoesCliente(cpf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("jaleunotificacao")]
        public IActionResult JaLeuNotificacao(JaLeuNotificacaoRequisicao notificacao)
        {
            try
            {
                return Ok(_service.LeuNotificacao(notificacao));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("")]
        public IActionResult Salvar(Notificacao notificacao)
        {
            try
            {
                return Ok(_service.Salvar(notificacao));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leitura")]
        public IActionResult LeituraNotificacao(LeituraNotificacaoRequisicao obj)
        {
            try
            {
                Console.WriteLine("LEITURA NOTIFICAÇÃO");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                // var json = new JavaScriptSerializer().Serialize(obj);
                Console.WriteLine(json);
                // Console.WriteLine("id=", obj.id);
                // Console.WriteLine("descricao=", obj.descricao);

                Console.WriteLine("LER NOTIFICAÇÃO");
                return Ok(_service.LeituraNotificacao(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("relatoriociencianotificacao")]
        public async Task<ActionResult<string>> RelatorioCienciaNotificacao(RelatorioCienciaNotificacaoRequisicao obj)
        {
            try
            {
                var parametros = ParametrosService.consultar(_connectionstring);
                var nomeArquivo = Relatorio.GerarNomeRelatorio("RelatorioNotificacao");
                Relatorio.Incluir(nomeArquivo, _connectionstring);
                _ = Task.Run(() => GenerateExcelReport(nomeArquivo, obj));
                var retxls = new RetornaRelatorio();
                var urldownload = parametros.urlExterna + "/pdf/" + nomeArquivo;
                // var urldownload = parametros.urlExterna + "/apicrm/Download/" + nomeArquivo;
                retxls.urlDownload = urldownload;
                retxls.nomeArquivo = nomeArquivo;
                return Ok(retxls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void GenerateExcelReport(string nomeArquivo, RelatorioCienciaNotificacaoRequisicao obj)
        {
            try
            {
                var parametros = ParametrosService.consultar(_connectionstring);
                Console.WriteLine("RelatorioCienciaNotificacao");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                Console.WriteLine(json);

                var result = _service.RelatorioCienciaNotificacao(obj);
                Relatorio.SalvarStatus(nomeArquivo, "consulta dados do relatório", _connectionstring);

                if (result.sucesso)
                {

                    var caminho = parametros.caminhoPdf + @"\";
                    var dados = result.objeto;

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Planilha1");

                        int linha = 1;
                        int coluna = 1;

                        // cabeçalhos                       
                        worksheet.Cell(linha, coluna++).Value = "cliente";
                        worksheet.Cell(linha, coluna++).Value = "cpfcnpj";
                        worksheet.Cell(linha, coluna++).Value = "email";
                        worksheet.Cell(linha, coluna++).Value = "primeiroacesso";
                        worksheet.Cell(linha, coluna++).Value = "telefone";
                        worksheet.Cell(linha, coluna++).Value = "empreendimento";
                        worksheet.Cell(linha, coluna++).Value = "bloco";
                        worksheet.Cell(linha, coluna++).Value = "unidade";
                        worksheet.Cell(linha, coluna++).Value = "contrato";
                        worksheet.Cell(linha, coluna++).Value = "origemacesso";
                        worksheet.Cell(linha, coluna++).Value = "dataultimoacesso";
                        worksheet.Cell(linha, coluna++).Value = "horaultimoacesso";
                        worksheet.Cell(linha, coluna++).Value = "origemleitura";
                        worksheet.Cell(linha, coluna++).Value = "dataleitura";
                        worksheet.Cell(linha, coluna++).Value = "horaleitura";
                        worksheet.Cell(linha, coluna++).Value = "notificacao";

                        worksheet.Cell(linha, coluna++).Value = "idempreendimento";
                        worksheet.Cell(linha, coluna++).Value = "idbloco";
                        worksheet.Cell(linha, coluna++).Value = "idunidade";
                        worksheet.Cell(linha, coluna++).Value = "idcontrato";
                        worksheet.Cell(linha, coluna++).Value = "datahoraleitura";
                        worksheet.Cell(linha, coluna++).Value = "datahoraultimoacesso";


                        linha = 2;
                        foreach (var item in dados)
                        {
                            coluna = 1;
                            worksheet.Cell(linha, coluna++).Value = item.cliente;

                            worksheet.Column(coluna).Style.NumberFormat.Format = "@";
                            worksheet.Cell(linha, coluna++).Value = item.cpfcnpj;

                            worksheet.Cell(linha, coluna++).Value = item.email;
                            worksheet.Cell(linha, coluna++).Value = item.primeiroacesso;
                            worksheet.Cell(linha, coluna++).Value = item.telefone;
                            worksheet.Cell(linha, coluna++).Value = item.empreendimento;
                            worksheet.Cell(linha, coluna++).Value = item.bloco;
                            worksheet.Cell(linha, coluna++).Value = item.unidade;

                            worksheet.Column(coluna).Style.NumberFormat.Format = "@";
                            worksheet.Cell(linha, coluna++).Value = item.contrato;

                            worksheet.Cell(linha, coluna++).Value = item.origemacesso;
                            worksheet.Cell(linha, coluna++).Value = item.dataultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.horaultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.origemleitura;
                            worksheet.Cell(linha, coluna++).Value = item.dataleitura;
                            worksheet.Cell(linha, coluna++).Value = item.horaleitura;
                            worksheet.Cell(linha, coluna++).Value = item.notificacao;

                            worksheet.Cell(linha, coluna++).Value = item.idempreendimento;
                            worksheet.Cell(linha, coluna++).Value = item.idbloco;
                            worksheet.Cell(linha, coluna++).Value = item.idunidade;
                            worksheet.Cell(linha, coluna++).Value = item.idcontrato;
                            worksheet.Cell(linha, coluna++).Value = item.datahoraleitura;
                            worksheet.Cell(linha, coluna++).Value = item.datahoraultimoacesso;
                            linha++;
                        }

                        Relatorio.SalvarStatus(nomeArquivo, "exportado dados do relatório", _connectionstring);
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(caminho + nomeArquivo);
                        Relatorio.SalvarStatus(nomeArquivo, "concluído", _connectionstring, 1);
                    }
                }
                else
                {
                    Relatorio.SalvarStatus(nomeArquivo, $"Erro: {result.mensagem}", _connectionstring, -1);
                }
            }
            catch (Exception ex)
            {
                Relatorio.SalvarStatus(nomeArquivo, $"Erro: {ex.Message}", _connectionstring, -1);
            }

        }

    }
}
