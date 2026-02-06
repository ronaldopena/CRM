using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Data;
using DocumentFormat.OpenXml.Office.Word;
using ClosedXML.Excel;
using Poliview.crm.infra;

namespace Poliview.crm.services
{

    public interface IMensagemService
    {
        public ListarMensagensResposta ListAll();
        public MensagemResposta FindById(int idmensagem);
        public MensagemEmpreendimentosResposta FindEmpreendimentosById(int idmensagem);
        public MensagemResposta Create(Mensagem mensagem);
        public MensagemResposta Update(Mensagem mensagem);
        public MensagemResposta Delete(int idmensagem);
        public MensagensUsuarioResposta ListarMensagensUsuario(string cpf, int idmensagem, string config);
        public LeituraMensagemResposta LeituraMensagem(LeituraMensagemRequisicao obj);
        public RelatorioCienciaMensagemResposta RelatorioCienciaMensagem(RelatorioCienciaMensagemRequisicao obj);        
        public ListarMensagensPorGrupoDetalheResposta ListarMensagensPoGrupoDetalhe(string cpf, int idgrupomidia);
        public ListarMensagensPorGrupoResposta ListarMensagensPoGrupo(string cpf);

    }

    public class MensagemService : IMensagemService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public MensagemService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }
        public MensagemResposta FindById(int idmensagem)
        {
            var retorno = new MensagemResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query1 = $"SELECT * FROM CAD_MENSAGEM_EMPREENDIMENTOS WHERE idmensagem={idmensagem}";
                var result1 = connection.Query<MensagemEmpreendimentos>(query1);
                var empreendimentos = "";
                foreach (var item in result1)
                {
                    if (empreendimentos == "")
                        empreendimentos += item.idempreendimento.ToString();
                    else    
                        empreendimentos += "," + item.idempreendimento.ToString();
                }
                var query = $"SELECT * FROM CAD_MENSAGEM WHERE id={idmensagem}";
                var result = connection.QueryFirstOrDefault<Mensagem>(query);
                if (result != null)
                {
                    result.empreendimentos = empreendimentos;
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Mensagem não encontrada";                    
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public ListarMensagensResposta ListAll()
        {
            var retorno = new ListarMensagensResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = 
                "select " +
                "CASE MSG.variosempreendimentos  " +
                "    WHEN 1 THEN '** VARIOS **' " +
                "    WHEN 0 THEN COALESCE(EMP.NM_Empree, '** Todos **') " +
                "END as empreendimento,  " +
                "CASE MSG.variosempreendimentos  " +
                "    WHEN 1 THEN '' " +
                "    WHEN 0 THEN COALESCE(BLO.NM_Bloco,'** Todos **') " +
                "END as bloco,  " +
                "CASE MSG.variosempreendimentos  " +
                "    WHEN 1 THEN '' " +
                "    WHEN 0 THEN COALESCE(uni.NR_UnidadeSP7,'** Todos **') " +
                "END as unidade, MSG.* from CAD_MENSAGEM MSG " +
                "LEFT JOIN CAD_EMPREENDIMENTO EMP ON EMP.CD_EMPREENDIMENTO=MSG.idempreendimento " +
                "LEFT JOIN CAD_BLOCO BLO ON BLO.CD_BLOCO=MSG.idbloco " +
                "LEFT JOIN CAD_UNIDADE UNI ON UNI.CD_Unidade=MSG.idunidade ";
            
                Console.WriteLine(query);
                var result = connection.Query<Mensagem>(query);
                Console.WriteLine(result.ToString());
                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }

        }
        public MensagemEmpreendimentosResposta FindEmpreendimentosById(int idmensagem)
        {
            var retorno = new MensagemEmpreendimentosResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"SELECT * FROM CAD_MENSAGEM_EMPREENDIMENTOS WHERE idmensagem={idmensagem}";
                var result = connection.Query<MensagemEmpreendimentos>(query);

                Console.WriteLine(query);

                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Mensagem não encontrada";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public MensagensUsuarioResposta ListarMensagensUsuario(string cpf, int idmensagem, string config)            
        {
            var boletovencido = config.Substring(0, 1);
            var boletoavencer = config.Substring(1, 1);
            var aniversario = config.Substring(2, 1);
            var aniversariocontrato = config.Substring(3, 1);
            var quitacao = config.Substring(4, 1);
            var residuo = config.Substring(5, 1);

            var retorno = new MensagensUsuarioResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec dbo.CRM_Listar_Mensagens_Usuario_carrossel " +
                            $"@cpfcliente='{cpf}', @idmensagem={idmensagem}, " +
                            $"@boletovencido={boletovencido}, @boletoavencer={boletoavencer}, @aniversario={aniversario}, @aniversariocontrato={aniversariocontrato}, @quitacao={quitacao}, @residuo={residuo} ";
                var result = connection.Query<MensagemUsuario>(query);

                Console.WriteLine(query);

                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public MensagemResposta Create(Mensagem mensagem)
        {
            var retorno = new MensagemResposta();
            try
            {   
                var datainicial = String.Format("{0:dd/MM/yyyy 00:00:00}",mensagem.datainicial); 
                var datafinal = String.Format("{0:dd/MM/yyyy 23:59:59} ", mensagem.datafinal);
                var datainicialfiquepordentro = String.Format("{0:dd/MM/yyyy 00:00:00}", mensagem.datainicialfiquepordentro);
                var datafinalfiquepordentro = String.Format("{0:dd/MM/yyyy 23:59:59}", mensagem.datafinalfiquepordentro);
                using var connection = new SqlConnection(_connectionString);
                var query = $"SET DATEFORMAT DMY; insert into CAD_MENSAGEM " +
                            $"([descricao],[mensagem],[tipomensagem]," +
                            $"[urlimagem],[linkimagem],[variosempreendimentos]," +
                            $"[idempreendimento],[idbloco],[idunidade]," +
                            $"[datainicial],[datafinal],[fiquepordentro]," +
                            $"[datainicialfiquepordentro],[datafinalfiquepordentro],[idgrupomidia]) ";
                query += " values ";
                query += $"('{mensagem.descricao}','{mensagem.mensagem}',{mensagem.tipomensagem}," +
                         $"'{mensagem.urlimagem}','{mensagem.linkimagem}',{mensagem.variosempreendimentos}," +
                         $"{mensagem.idempreendimento},{mensagem.idbloco},{mensagem.idunidade}," +
                         $"'{datainicial}','{datafinal}', {mensagem.fiquepordentro}, " +
                         $"'{datainicialfiquepordentro}', '{datafinalfiquepordentro}', {mensagem.idgrupomidia} ); SELECT SCOPE_IDENTITY(); ";

                Console.WriteLine(query);

                var result = connection.QueryFirstOrDefault<int>(query);
                mensagem.id = result;

                zerarEmpreendimentosMensagem(mensagem.id);
                var empreendimentos = mensagem.empreendimentos.Split(",");
                foreach (var item in empreendimentos)
                {
                    IncluirEmpreendimentosMensagem(mensagem.id, Convert.ToInt32(item));
                }

                Console.WriteLine(result);

                if (result>0)
                {
                    mensagem.id = result;
                    retorno.mensagem = "OK";
                    retorno.objeto = mensagem;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public MensagemResposta Update(Mensagem mensagem)
        {
            var retorno = new MensagemResposta();
            try
            {
                var datainicial = String.Format("{0:dd/MM/yyyy 00:00:00}", mensagem.datainicial);
                var datafinal = String.Format("{0:dd/MM/yyyy 23:59:59} ", mensagem.datafinal);
                var datainicialfiquepordentro = String.Format("{0:dd/MM/yyyy 00:00:00}", mensagem.datainicialfiquepordentro);
                var datafinalfiquepordentro = String.Format("{0:dd/MM/yyyy 23:59:59}", mensagem.datafinalfiquepordentro);

                if (mensagem.datainicial.Year < 1900)
                    datainicial = null;

                using var connection = new SqlConnection(_connectionString);
                var query = $"SET DATEFORMAT DMY; update CAD_MENSAGEM set ";

                query += $"[descricao]='{mensagem.descricao}',";
                query += $"[mensagem]='{mensagem.mensagem}',";
                query += $"[tipomensagem]={mensagem.tipomensagem},";
                query += $"[urlimagem]='{mensagem.urlimagem}',";
                query += $"[linkimagem]='{mensagem.linkimagem}',";
                query += $"[idgrupomidia]={mensagem.idgrupomidia},";
                query += $"[variosempreendimentos]={mensagem.variosempreendimentos},";
                query += $"[idempreendimento]={mensagem.idempreendimento},";
                query += $"[idbloco]={mensagem.idbloco},";
                query += $"[idunidade]={mensagem.idunidade},";
                query += $"[datainicial]='{datainicial}',";
                query += $"[datafinal]='{datafinal}',";
                query += $"[datainicialfiquepordentro]='{datainicialfiquepordentro}',";
                query += $"[datafinalfiquepordentro]='{datafinalfiquepordentro}',";
                /*
                if (mensagem.datainicial.Year < 1900)
                    query += $"[datainicial]='{datainicial}',";
                else
                    query += $"[datainicial]=null,";

                if (mensagem.datafinal.Year < 1900)
                    query += $"[datafinal]='{datafinal}',";
                else
                    query += $"[datafinal]=null,";

                if (mensagem.datainicialfiquepordentro.Year < 1900)
                    query += $"[datainicialfiquepordentro]='{datainicialfiquepordentro}',";
                else
                    query += $"[datainicialfiquepordentro]=null,";
                
                if (mensagem.datafinalfiquepordentro.Year < 1900)
                    query += $"[datafinalfiquepordentro]='{datafinalfiquepordentro}',";
                else
                    query += $"[datafinalfiquepordentro]=null,";
                */
                query += $"[fiquepordentro]={mensagem.fiquepordentro} ";
                query += $"where [id]={mensagem.id}";

                Console.WriteLine("-------------------------------------------------------------");
                Console.WriteLine(query);
                Console.WriteLine("-------------------------------------------------------------");

                var result = connection.Execute(query);

                Console.WriteLine(mensagem.id);
                Console.WriteLine(mensagem.empreendimentos);

                zerarEmpreendimentosMensagem(mensagem.id);
                var empreendimentos = mensagem.empreendimentos.Split(",");
                foreach (var item in empreendimentos)
                {
                    IncluirEmpreendimentosMensagem(mensagem.id, Convert.ToInt32(item));
                }
                
                if (result > 0)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = mensagem;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "mensagem não encontrada";
                    retorno.objeto = mensagem;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }

        }
        public MensagemResposta Delete(int idmensagem)
        {
            var retorno = new MensagemResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"delete from CAD_MENSAGEM where id=${idmensagem} ";
                var result = connection.Execute(query);
                if (result > 0)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Mensagem não encontrada";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public LeituraMensagemResposta LeituraMensagem(LeituraMensagemRequisicao obj)
        {
            var retorno = new LeituraMensagemResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec dbo.CRM_Leitura_Mensagem @idmensagem={obj.idmensagem}, @idunidade={obj.idunidade}, @idorigem={obj.idorigem}, @cpfcnpj='{obj.cpfcnpj}'";
                var result = connection.Query(query);

                Console.WriteLine(query);

                retorno.mensagem = "OK";
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public RelatorioCienciaMensagemResposta RelatorioCienciaMensagem(RelatorioCienciaMensagemRequisicao obj)
        {
            var retorno = new RelatorioCienciaMensagemResposta();
            try
            {
                var registros = RetornaRegistrosRelCienciaMensagem(obj);

                if (registros != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = registros;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Não há registros para apresentar";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }     
        protected void zerarEmpreendimentosMensagem(int idmensagem)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"delete from CAD_MENSAGEM_EMPREENDIMENTOS where idmensagem=${idmensagem} ";
            connection.Execute(query);
        }
        protected void IncluirEmpreendimentosMensagem(int idmensagem, int idempreendimento)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"insert into CAD_MENSAGEM_EMPREENDIMENTOS (idmensagem, idempreendimento) values (${idmensagem},${idempreendimento}) ";
            connection.Execute(query);
        }
        public ListarMensagensPorGrupoResposta ListarMensagensPoGrupo(string cpf)
        {
            var retorno = new ListarMensagensPorGrupoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec [dbo].[CRM_Listar_Mensagens_Usuario] @cpfcliente='{cpf}', @idgrupomidia=0, @agrupado=1 ";
                var result = connection.Query<GrupoMidiaMensagem>(query);

                Console.WriteLine(query);

                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Não há registros para apresentar";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public ListarMensagensPorGrupoDetalheResposta ListarMensagensPoGrupoDetalhe(string cpf, int idgrupomidia)
        {
            var retorno = new ListarMensagensPorGrupoDetalheResposta();
            try
            {

                using var connection = new SqlConnection(_connectionString);
                var query = $"exec [dbo].[CRM_Listar_Mensagens_Usuario] @cpfcliente='{cpf}',@idgrupomidia={idgrupomidia}, @agrupado=0";
                var result = connection.Query<GrupoMidiaMensagemDetalhe>(query);

                Console.WriteLine(query);

                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Não há registros para apresentar";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public IEnumerable<RelatorioCienciaMensagem> RetornaRegistrosRelCienciaMensagem(RelatorioCienciaMensagemRequisicao obj)
        {
            // @datainicial datetime, @datafinal datetime, @idmensagem int, @idempreendimento int, @idbloco int, @idunidade int, @idorigem int

            Console.WriteLine(obj.datainicial.ToString());
            Console.WriteLine(obj.datafinal.ToString());

            using var connection = new SqlConnection(_connectionString);

            var query = $"SET DATEFORMAT dmy; select * from vRelatorioMensagens where 1=1 ";
            // between '{obj.datainicial.ToString("dd/MM/yyyy 00:00:00")}' and '{obj.datafinal.ToString("dd/MM/yyyy 23:59:59")}' ";
            
            if (!String.IsNullOrEmpty(obj.idmensagemList)) query += $"and idmensagem in ({obj.idmensagemList}) ";

            if (!String.IsNullOrEmpty(obj.idempreendimentoList)) query += $"and idempreendimento in ({obj.idempreendimentoList}) ";

            if (obj.idbloco > 0) query += $"and idbloco={obj.idbloco} ";

            if (obj.idunidade > 0) query += $"and idunidade={obj.idunidade} ";

            if (obj.idorigem > 0) query += $"and idorigem={obj.idorigem} ";

            Console.WriteLine(query);

            var result = connection.Query<RelatorioCienciaMensagem>(query, commandTimeout: 1200);
         
            return result.ToList();
        }
        private void SalvarExcelEnviarEmail(IEnumerable<RelatorioCienciaMensagem> dados, string caminho, string nomeArquivo, string email)
        {
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
                        worksheet.Cell(linha, i + 1).Value = (XLCellValue)props[i].GetValue(item);
                    }
                    linha++;
                }
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(caminho + nomeArquivo);
            }           
        }
    }
}
