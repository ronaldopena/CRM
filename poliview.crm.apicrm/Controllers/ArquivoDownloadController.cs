using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using Poliview.crm.services;
using System.Web;

namespace poliview.crm.apicrm.Controllers
{
    [Route("arquivodownload")]
    [ApiController]
    public class ArquivoDownloadController : ControllerBase
    {
        private IArquivoDownloadService _service;
        private IConfiguration _configuration;
        private string _connectionstring;
        private class RetornaRelatorio
        {
            public string? urlDownload { get; set; }
            public string? nomeArquivo { get; set; }
        }

        public ArquivoDownloadController(IConfiguration configuration)
        {
            _service = new ArquivoDownloadService(configuration);
            _connectionstring = configuration["conexao"];
            _configuration = configuration;
        }

        [HttpGet()]
        public IActionResult FindAll()
        {
            try
            {
                Console.WriteLine("request Arquivo FindAll");
                return Ok(_service.ListAll());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idarquivo}")]
        public IActionResult Find(int idarquivo)
        {
            try
            {
                Console.WriteLine("request FindByID");
                return Ok(_service.FindById(idarquivo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idarquivo}/empreendimentos")]
        public IActionResult FindArquivoEmpreendimentos(int idarquivo)
        {
            try
            {
                return Ok(_service.FindEmpreendimentosById(idarquivo));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{cpf}/arquivosusuario/{idarquivo}")]
        public IActionResult ListarMensagemUsuario(string cpf, string idarquivo)
        {
            Console.WriteLine("Get ArquivosUsuario");
            try
            {
                var ret = _service.ListarArquivosUsuario(cpf, idarquivo);
                Console.WriteLine(ret);
                return Ok(ret.objeto); ;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost()]
        public IActionResult Create(ArquivoDownload obj)
        {
            try
            {
                Console.WriteLine("CREATE ARQUIVO JSON");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                // var json = new JavaScriptSerializer().Serialize(obj);
                Console.WriteLine(json);
                // Console.WriteLine("id=", obj.id);
                // Console.WriteLine("descricao=", obj.descricao);

                Console.WriteLine("Incluir Arquivo");
                return Ok(_service.Create(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("download")]
        public IActionResult DownloadArquivo(BaixaArquivoDownloadRequisicao obj)
        {
            try
            {
                Console.WriteLine("Download Arquivo JSON");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                Console.WriteLine(json);
                // Console.WriteLine("id=", obj.id);
                // Console.WriteLine("descricao=", obj.descricao);

                Console.WriteLine("Confirmação de download");
                return Ok(_service.BaixaDownloadArquivo(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("relatoriocienciadownload")]
        public IActionResult RelatorioCienciaMensagem(RelatorioCienciaArquivoRequisicao obj)
        {
            try 
            {
                var parametros = ParametrosService.consultar(_connectionstring);
                var nomeArquivo = Relatorio.GerarNomeRelatorio("RelatorioDownload");
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

        private void GenerateExcelReport(string nomeArquivo, RelatorioCienciaArquivoRequisicao obj)
        {
            try
            {
                var parametros = ParametrosService.consultar(_connectionstring);
                Console.WriteLine("RelatorioCienciaDownload");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                var result = _service.RelatorioCienciaArquivo(obj);
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
                        worksheet.Cell(linha, coluna++).Value = "origemultimoacesso";
                        worksheet.Cell(linha, coluna++).Value = "dataultimoacesso";
                        worksheet.Cell(linha, coluna++).Value = "horaultimoacesso";
                        worksheet.Cell(linha, coluna++).Value = "origemdownload";
                        worksheet.Cell(linha, coluna++).Value = "datadownload";
                        worksheet.Cell(linha, coluna++).Value = "horadownload";
                        worksheet.Cell(linha, coluna++).Value = "descricaoarquivo";

                        worksheet.Cell(linha, coluna++).Value = "idempreendimento";
                        worksheet.Cell(linha, coluna++).Value = "idbloco";
                        worksheet.Cell(linha, coluna++).Value = "idunidade";
                        worksheet.Cell(linha, coluna++).Value = "idcontrato";
                        worksheet.Cell(linha, coluna++).Value = "idarquivodownload";
                        worksheet.Cell(linha, coluna++).Value = "datahoradownload";
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

                            worksheet.Cell(linha, coluna++).Value = item.origemultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.dataultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.horaultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.origemdownload;
                            worksheet.Cell(linha, coluna++).Value = item.datadownload;
                            worksheet.Cell(linha, coluna++).Value = item.horadownload;
                            worksheet.Cell(linha, coluna++).Value = item.descricaoarquivo;

                            worksheet.Cell(linha, coluna++).Value = item.idempreendimento;
                            worksheet.Cell(linha, coluna++).Value = item.idbloco;
                            worksheet.Cell(linha, coluna++).Value = item.idunidade;
                            worksheet.Cell(linha, coluna++).Value = item.idcontrato;
                            worksheet.Cell(linha, coluna++).Value = item.idarquivodownload;
                            worksheet.Cell(linha, coluna++).Value = item.datahoradownload;
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

        /*
        [HttpPost("relatoriocienciadownload")]
        public IActionResult RelatorioCienciaMensagem(RelatorioCienciaArquivoRequisicao obj)
        {
            try
            {
                var parametros = new ParametrosService(_configuration).consultar();
                Console.WriteLine("RelatorioCienciaDownload");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);             
                Console.WriteLine(json);

                var result = _service.RelatorioCienciaArquivo(obj);

                if (result.sucesso)
                {
                    var dados = result.objeto;

                    DateTime agora = DateTime.Now;
                    string dataFormatada = agora.ToString("yyyyMMddHHmmss");
                    var caminho = parametros.caminhoPdf + @"\";

                    var nomeArquivo = "RelatorioDownloads-" + dataFormatada + ".xlsx";

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Planilha1");

                        // Cabeçalhos
                        var props = typeof(RelatorioCienciaArquivo).GetProperties();
                        for (int i = 0; i < props.Length; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = props[i].Name;
                        }

                        // Dados
                        int linha = 2;
                        foreach (var item in dados)
                        {
                            for (int i = 0; i < props.Length; i++)
                            {
                                worksheet.Cell(linha, i + 1).Value = props[i].GetValue(item);
                            }
                            linha++;
                        }
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(caminho + nomeArquivo);

                        var urldownload = parametros.urlExterna + "/pdf/" + nomeArquivo;
                        Console.WriteLine($"urldownload = {urldownload}");
                        var retxls = new RetornaRelatorio();
                        
                        retxls.urlDownload = urldownload;
                        retxls.nomeArquivo = nomeArquivo;

                        return Ok(retxls);

                    }
                }
                else
                {
                    return BadRequest(result.mensagem);
                }                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        */

        [HttpPut()]
        public IActionResult Update(ArquivoDownload obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            Console.WriteLine("UPDATE MENSAGEM JSON");
            Console.WriteLine(json);

            try
            {
                Console.WriteLine("Alterar mensagens");
                return Ok(_service.Update(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{idarquivo}")]
        public IActionResult Delete(int idarquivo)
        {
            try
            {
                Console.WriteLine("Excluir Arquivos");
                return Ok(_service.Delete(idarquivo));                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("agrupamento/{cpf}")]
        public IActionResult ListarArquivosDownloadPoGrupo(string cpf)
        {
            try
            {
                Console.WriteLine("Listar Grupos de midia agrupamento");
                return Ok(_service.ListarArquivosDownloadPorGrupo(cpf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("agrupamento/{cpf}/{idgrupomidia}")]
        public IActionResult ListarArquivosDownloadPoGrupoDetalhe(string cpf, int idgrupomidia)
        {
            try
            {
                Console.WriteLine($"Listar arquivos Grupo de midia detalhe {cpf} / {idgrupomidia}");
                return Ok(_service.ListarArquivosDownloadPorGrupoDetalhe(cpf, idgrupomidia));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("downloadfile")]
        public async Task<IActionResult> DownloadFile(string fileUrl)
        {
            var url = HttpUtility.UrlDecode(fileUrl);
            var uri = new Uri(url);
            var file = Path.GetFileName(uri.LocalPath);

            using (var httpClient = new HttpClient())
            {
                Console.WriteLine($"url={fileUrl}");
                Console.WriteLine($"file={file}");
                var response = await httpClient.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound($"Erro ao baixar arquivo: {file}. (Status code: {response.StatusCode})");
                }
                var stream = await response.Content.ReadAsStreamAsync();
                var contentType = response.Content.Headers.ContentType.ToString();

                return new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = file
                };
            }
        }

    }
}