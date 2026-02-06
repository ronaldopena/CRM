using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;
using Poliview.crm.services;


namespace Poliview.crm.apicrm.Controllers
{
    [Route("mensagem")]
    [ApiController]
    public class MensagemController : ControllerBase
    {
        private IMensagemService _service;
        private IConfiguration _configuration;
        private string _connectionstring;

        private class RetornaRelatorio
        {
            public string? urlDownload { get; set; }
            public string? nomeArquivo { get; set; }
        }

        public MensagemController(IConfiguration configuration)
        {
            _service = new MensagemService(configuration);
            _configuration = configuration;
            _connectionstring = configuration["conexao"];
        }

        [HttpGet()]
        public IActionResult FindAll()
        {
            try
            {
                Console.WriteLine("request Mensagem FindAll");
                return Ok(_service.ListAll());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idmensagem}")]
        public IActionResult Find(int idmensagem)
        {
            try
            {
                Console.WriteLine("request FindByID");
                return Ok(_service.FindById(idmensagem));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{idmensagem}/empreendimentos")]
        public IActionResult FindMensagemEmpreendimentos(int idmensagem)
        {
            try
            {
                return Ok(_service.FindEmpreendimentosById(idmensagem));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{cpf}/mensagensusuario/{idmensagem}/{config}")]
        public IActionResult ListarMensagemUsuario(string cpf, int idmensagem, string config)
        {
            Console.WriteLine($"Get mensagensusuario CPF: {cpf} idmensagem: {idmensagem} config:{config}" );
            try
            {
                var ret = _service.ListarMensagensUsuario(cpf, idmensagem, config);
                Console.WriteLine(ret);
                return Ok(ret.objeto); ;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost()]
        public IActionResult Create(Mensagem obj)
        {
            try
            {
                Console.WriteLine("CREATE MENSAGEM JSON");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                // var json = new JavaScriptSerializer().Serialize(obj);
                Console.WriteLine(json);
                // Console.WriteLine("id=", obj.id);
                // Console.WriteLine("descricao=", obj.descricao);

                Console.WriteLine("Incluir mensagens");
                return Ok(_service.Create(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leitura")]
        public IActionResult LeituraMensagem(LeituraMensagemRequisicao obj)
        {
            try
            {
                Console.WriteLine("LEITURA MENSAGEM");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                // var json = new JavaScriptSerializer().Serialize(obj);
                Console.WriteLine(json);
                // Console.WriteLine("id=", obj.id);
                // Console.WriteLine("descricao=", obj.descricao);

                Console.WriteLine("LER mensagem");
                return Ok(_service.LeituraMensagem(obj));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("statusrelatorio/{relatorio}")]
        public IActionResult statusrelatorio(string relatorio)
        {
            try
            {                
                var status = Relatorio.ConsultarStatus(relatorio, _connectionstring);
                Console.WriteLine($"status do relatório {status}");
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("relatoriocienciamensagem")]
        public async Task<ActionResult<string>> RelatorioCienciaMensagem(RelatorioCienciaMensagemRequisicao obj)
        {
            try
            {
                var parametros = ParametrosService.consultar(_connectionstring);
                var nomeArquivo = Relatorio.GerarNomeRelatorio("RelatorioMensagens");
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

        private void GenerateExcelReport(string nomeArquivo, RelatorioCienciaMensagemRequisicao obj)
        {
            try
            {
                var parametros = ParametrosService.consultar(_connectionstring);
                Console.WriteLine("RelatorioCienciaMensagem");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

                var result = _service.RelatorioCienciaMensagem(obj);
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
                        worksheet.Cell(linha, coluna++).Value = "origemleitura";
                        worksheet.Cell(linha, coluna++).Value = "dataleitura";
                        worksheet.Cell(linha, coluna++).Value = "horaleitura";
                        worksheet.Cell(linha, coluna++).Value = "descricaomensagem";

                        worksheet.Cell(linha, coluna++).Value = "idempreendimento";
                        worksheet.Cell(linha, coluna++).Value = "idbloco";
                        worksheet.Cell(linha, coluna++).Value = "idunidade";
                        worksheet.Cell(linha, coluna++).Value = "idcontrato";
                        worksheet.Cell(linha, coluna++).Value = "idmensagem";
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

                            worksheet.Cell(linha, coluna++).Value = item.origemultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.dataultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.horaultimoacesso;
                            worksheet.Cell(linha, coluna++).Value = item.origemleitura;
                            worksheet.Cell(linha, coluna++).Value = item.dataleitura;
                            worksheet.Cell(linha, coluna++).Value = item.horaleitura;
                            worksheet.Cell(linha, coluna++).Value = item.descricaomensagem;

                            worksheet.Cell(linha, coluna++).Value = item.idempreendimento;
                            worksheet.Cell(linha, coluna++).Value = item.idbloco;
                            worksheet.Cell(linha, coluna++).Value = item.idunidade;
                            worksheet.Cell(linha, coluna++).Value = item.idcontrato;
                            worksheet.Cell(linha, coluna++).Value = item.idmensagem;
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

        [HttpPut()]
        public IActionResult Update(Mensagem obj)
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

        [HttpDelete("{idmensagem}")]
        public IActionResult Delete(int idmensagem)
        {
            try
            {
                Console.WriteLine("Excluir mensagens");
                return Ok(_service.Delete(idmensagem));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("agrupamento/{cpf}")]
        public IActionResult ListarAgrupamentoGruposMidiaMensagem(string cpf)
        {
            try
            {
                Console.WriteLine("Listar Grupos de midia agrupamento");
                return Ok(_service.ListarMensagensPoGrupo(cpf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("agrupamento/{cpf}/{idgrupomidia}")]
        public IActionResult ListarMensaensPorUsuarioGrupo(string cpf, int idgrupomidia)
        {
            try
            {
                Console.WriteLine($"Listar Agrupamento Grupo de midia detalhe {cpf} / {idgrupomidia}");
                return Ok(_service.ListarMensagensPoGrupoDetalhe(cpf, idgrupomidia));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}


/*
 
        public IActionResult RelatorioCienciaMensagem(RelatorioCienciaMensagemRequisicao obj)
        {
            try
            {
                var parametros = new ParametrosService(_configuration).consultar();
                Console.WriteLine("RelatorioCienciaMensagem");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                Console.WriteLine(json);

                var result = _service.RelatorioCienciaMensagem(obj);

                if (result.sucesso)
                {
                    DateTime agora = DateTime.Now;
                    string dataFormatada = agora.ToString("yyyyMMddHHmmss");
                    var caminho = parametros.caminhoPdf + @"\";
                    var nomeArquivo = "RelatorioMensagens-" + dataFormatada + ".xlsx";

                    var dados = result.objeto;

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Planilha1");

                        // Cabeçalhos
                        var props = typeof(RelatorioCienciaMensagem).GetProperties();
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